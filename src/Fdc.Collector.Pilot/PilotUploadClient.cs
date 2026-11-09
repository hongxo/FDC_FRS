using System.Net;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Fdc.Core;

namespace Fdc.Collector.Pilot;

public sealed record PilotCenterProfile(Uri Origin, string ServerLeafSha256, string ReleaseId, string EvidenceReference,
    DateTimeOffset From, DateTimeOffset Until, TimeSpan Timeout, bool SimulatedLoopback);
public interface IPilotCenterTransferAuthorization { void Demand(PilotCenterProfile profile, Manifest manifest, DateTimeOffset now); }
public sealed class PendingPilotCenterTransferAuthorization : IPilotCenterTransferAuthorization
{
    public void Demand(PilotCenterProfile profile, Manifest manifest, DateTimeOffset now) => throw new InvalidOperationException("D01_D07_RELEASE_REQUIRED: reviewed Center mTLS/station identity profile is not configured.");
}

// Explicit opt-in client, not registered in the operating Collector. No API key/cookie/redirect,
// no unbounded retry and no implicit FTP/SFTP fallback. Caller owns certificate lifecycle.
public sealed class PilotUploadClient : IDisposable
{
    private readonly PilotCenterProfile profile;
    private readonly IPilotCenterTransferAuthorization authorization;
    private readonly TimeProvider clock;
    private readonly HttpClient http;
    private readonly string spoolRoot;
    public PilotUploadClient(PilotCenterProfile profile, X509Certificate2 collectorCertificate, string absoluteSpoolRoot,
        IPilotCenterTransferAuthorization authorization, TimeProvider clock)
    {
        this.profile = profile; this.authorization = authorization; this.clock = clock; spoolRoot = Path.GetFullPath(absoluteSpoolRoot);
        if (!Path.IsPathFullyQualified(absoluteSpoolRoot) || !profile.Origin.IsAbsoluteUri || profile.Origin.Scheme != "https" ||
            profile.Origin.AbsolutePath != "/" || profile.Origin.Query.Length != 0 || profile.Origin.Fragment.Length != 0 || profile.Origin.UserInfo.Length != 0 ||
            profile.ServerLeafSha256.Length != 64 || profile.ServerLeafSha256.Any(c => !char.IsAsciiHexDigit(c)) ||
            profile.Timeout <= TimeSpan.Zero || profile.Timeout > TimeSpan.FromMinutes(30) ||
            (profile.SimulatedLoopback && (!IPAddress.TryParse(profile.Origin.Host, out var ip) || !IPAddress.IsLoopback(ip))) ||
            !collectorCertificate.HasPrivateKey || collectorCertificate.NotBefore.ToUniversalTime() > clock.GetUtcNow().UtcDateTime ||
            collectorCertificate.NotAfter.ToUniversalTime() <= clock.GetUtcNow().UtcDateTime ||
            !collectorCertificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().Any(e => e.EnhancedKeyUsages.Cast<Oid>().Any(o => o.Value == "1.3.6.1.5.5.7.3.2")))
            throw new ArgumentException("Explicit valid HTTPS/mTLS profile and certificate required.");
        var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false, CheckCertificateRevocationList = !profile.SimulatedLoopback };
        handler.ClientCertificates.Add(collectorCertificate);
        handler.ServerCertificateCustomValidationCallback = (_, peer, _, errors) => peer is not null &&
            peer.NotBefore.ToUniversalTime() <= clock.GetUtcNow().UtcDateTime && peer.NotAfter.ToUniversalTime() > clock.GetUtcNow().UtcDateTime &&
            string.Equals(peer.GetCertHashString(HashAlgorithmName.SHA256), profile.ServerLeafSha256, StringComparison.OrdinalIgnoreCase) &&
            (profile.SimulatedLoopback || errors == SslPolicyErrors.None);
        http = new(handler) { BaseAddress = profile.Origin, Timeout = profile.Timeout };
    }
    public async Task<PilotAck> UploadAsync(PilotPackage package, CancellationToken ct)
    {
        if (package.ManifestJson.Length > 65536 || Identity.Hash(package.ManifestJson) != package.ManifestSha256) throw new InvalidDataException("Invalid immutable manifest.");
        var manifest = JsonSerializer.Deserialize<Manifest>(package.ManifestJson) ?? throw new InvalidDataException();
        Demand(manifest);
        if (manifest.Id != package.Id || manifest.Files is null || manifest.Files.Count is < 2 or > 4 ||
            Path.GetFullPath(package.Directory) != Path.Combine(spoolRoot, "records", package.Id)) throw new InvalidDataException("Package outside approved spool.");
        for (var d = new DirectoryInfo(package.Directory); d is not null; d = d.Parent)
            if ((d.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Reparse spool forbidden.");
        foreach (var file in manifest.Files)
        {
            if (AcquisitionSafety.FileName(file.Name) != file.Name) throw new InvalidDataException("Unsafe package filename.");
            var path = Path.Combine(package.Directory, file.Name);
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Reparse upload file forbidden.");
            AcquisitionSafety.ValidateTransfer(path, file.Size, file.Sha256);
        }
        using var initiation = new HttpRequestMessage(HttpMethod.Post, "api/pilot/v1/ingestion/") { Content = JsonContent.Create(manifest) };
        using var initiated = await http.SendAsync(initiation, HttpCompletionOption.ResponseHeadersRead, ct);
        if (initiated.StatusCode != HttpStatusCode.Created) throw new IOException("INGESTION_INITIATION_REJECTED");
        var ticket = await Read<PilotUploadTicket>(initiated, ct);
        if (ticket.Id == Guid.Empty || ticket.ManifestId != package.Id || ticket.ManifestSha256 != package.ManifestSha256 || clock.GetUtcNow() >= ticket.ExpiresAt) throw new InvalidDataException("Ticket identity mismatch.");
        foreach (var file in manifest.Files)
        {
            Demand(manifest);
            await using var input = File.OpenRead(Path.Combine(package.Directory, file.Name));
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/pilot/v1/ingestion/{ticket.Id:D}/files/{Uri.EscapeDataString(file.Name)}") { Content = new StreamContent(input) };
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.StatusCode != HttpStatusCode.NoContent) throw new IOException("INGESTION_FILE_REJECTED");
        }
        Demand(manifest);
        using var completion = new HttpRequestMessage(HttpMethod.Post, $"api/pilot/v1/ingestion/{ticket.Id:D}/complete");
        using var completed = await http.SendAsync(completion, HttpCompletionOption.ResponseHeadersRead, ct);
        if (completed.StatusCode != HttpStatusCode.OK) throw new IOException("INGESTION_COMMIT_NOT_ACKNOWLEDGED");
        var ack = await Read<PilotIngestionAck>(completed, ct);
        if (ack.ManifestId != package.Id || ack.ManifestSha256 != package.ManifestSha256 || ack.FilesDigest != package.FilesDigest ||
            ack.RecordId == Guid.Empty || string.IsNullOrWhiteSpace(ack.CorrelationId) || ack.CorrelationId.Length > 200) throw new InvalidDataException("ACK integrity mismatch.");
        Demand(manifest); // Revoked local run profile must not create a new cleanup decision.
        return new(ack.ManifestId, ack.RecordId, ack.ManifestSha256, ack.FilesDigest, ack.CorrelationId);
    }
    public async Task UploadAndAcknowledgeAsync(PilotTransferQueue queue, QueueLease lease, CancellationToken ct)
    {
        if (lease.Kind != "Upload") throw new ArgumentException("Upload lease required.");
        var ack = await UploadAsync(queue.Package(lease.Id), ct);
        queue.Acknowledge(lease, ack, new VerifiedThisResponse(ack)); // Only reachable after actual pinned mTLS/response checks.
    }
    private void Demand(Manifest manifest)
    {
        var now = clock.GetUtcNow();
        if (string.IsNullOrWhiteSpace(profile.ReleaseId) || string.IsNullOrWhiteSpace(profile.EvidenceReference) || now < profile.From || now >= profile.Until)
            throw new InvalidOperationException("Expired/missing Center transfer scope.");
        authorization.Demand(profile, manifest, now);
    }
    private static async Task<T> Read<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/json") throw new InvalidDataException("JSON receipt required.");
        await using var source = await response.Content.ReadAsStreamAsync(ct); using var bytes = new MemoryStream(); var buffer = new byte[4096];
        while (true) { var n = await source.ReadAsync(buffer, ct); if (n == 0) break; if (bytes.Length + n > 65536) throw new InvalidDataException("Receipt size bound."); bytes.Write(buffer, 0, n); }
        return JsonSerializer.Deserialize<T>(bytes.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new InvalidDataException("Missing receipt.");
    }
    private sealed class VerifiedThisResponse(PilotAck expected) : ICenterReceiptTrust
    { public void DemandVerified(PilotAck actual) { if (actual != expected) throw new InvalidDataException("Unverified ACK."); } }
    public void Dispose() => http.Dispose();
}
