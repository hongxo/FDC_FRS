using System.Collections.Immutable;
using System.Net;
using System.Security.Cryptography;

namespace Fdc.Collector.Pilot;

public enum EvidenceKind { OFFLINE_FIXTURE, SIMULATED, REAL_LAB_IED }
public enum CapabilityState { NOT_TESTED, UNKNOWN, SUCCESS, UNSUPPORTED_FILE_SERVICE, ACCESS_DENIED, FAILED }
public sealed record IedTarget(string Station, string IedId, IPAddress Address, int Port,
    string Directory, string? Vendor, string? Model, string? Firmware);
public sealed record LabReadProfile(string ReleaseId, string EvidenceReference, DateTimeOffset From,
    DateTimeOffset Until, ImmutableHashSet<string> Endpoints, int ConnectTimeoutMs, int RequestTimeoutMs,
    int MaximumEntries, long MaximumFileBytes, long BytesPerSecond, TimeSpan MinimumOperationInterval);
public interface ILabReadAuthorization
{
    // Must verify independent Lab-owner evidence for exact endpoints, read operations, time and limits.
    // Existence of an inventory, a boolean config flag or this interface is not approval.
    void Demand(IedTarget target, LabReadProfile profile, DateTimeOffset now);
}
public sealed class PendingLabReadAuthorization : ILabReadAuthorization
{
    public void Demand(IedTarget target, LabReadProfile profile, DateTimeOffset now) =>
        throw new InvalidOperationException("LAB_READ_SCOPE_REQUIRED: exact confirmed run window, endpoints and limits required.");
}
public sealed record IedFileMetadata(string RemotePath, long Size, DateTimeOffset? Modified);
public sealed record CapabilityObservation(CapabilityState MmsConnected, CapabilityState FileService,
    CapabilityState DirectoryList, CapabilityState FileDownload, CapabilityState Comtrade,
    EvidenceKind EvidenceKind, DateTimeOffset ObservedAt, string Diagnostic);
public sealed record TransferValidation(long Bytes, string Sha256, bool ExpectedSizeMatched,
    bool? SourceChecksumMatched); // null means no independent IED/original checksum, never implied true.
public interface IIedFileAcquisitionAdapter : IDisposable
{
    EvidenceKind EvidenceKind { get; }
    CapabilityObservation ProbeCapabilities(CancellationToken ct);
    IReadOnlyList<IedFileMetadata> ListFiles(CancellationToken ct);
    IedFileMetadata GetFileMetadata(string remotePath, CancellationToken ct);
    void DownloadFile(string remotePath, Stream destination, CancellationToken ct, IProgress<long>? progress = null);
    string GetVendorDiagnostics();
}
public sealed class IedFileServiceException(int code) : IOException($"MMS_FILE_SERVICE_ERROR_{code}")
{
    public int Code { get; } = code;
    public CapabilityState State => Code switch { 4 => CapabilityState.UNSUPPORTED_FILE_SERVICE, 21 => CapabilityState.ACCESS_DENIED, _ => CapabilityState.FAILED };
}
public static class AcquisitionSafety
{
    public static string Endpoint(IedTarget target) => target.Address + ":" + target.Port;
    public static void Validate(IedTarget target, LabReadProfile profile, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(profile.ReleaseId) || string.IsNullOrWhiteSpace(profile.EvidenceReference) ||
            now < profile.From || now >= profile.Until || profile.From >= profile.Until ||
            profile.Endpoints is null || !profile.Endpoints.Contains(Endpoint(target)) ||
            target.Port is < 1 or > 65535 || target.Address.Equals(IPAddress.Any) || target.Address.Equals(IPAddress.IPv6Any) || target.Address.Equals(IPAddress.Broadcast) ||
            target.Address.IsIPv6Multicast || target.Address.GetAddressBytes()[0] is >= 224 and <= 239 ||
            profile.ConnectTimeoutMs is < 1 or > 60000 || profile.RequestTimeoutMs is < 1 or > 120000 ||
            profile.MaximumEntries is < 1 or > 10000 || profile.MaximumFileBytes is < 1 or > 104857600 ||
            profile.BytesPerSecond <= 0 || profile.MinimumOperationInterval <= TimeSpan.Zero || profile.MinimumOperationInterval > TimeSpan.FromHours(1) ||
            string.IsNullOrWhiteSpace(target.Station) || string.IsNullOrWhiteSpace(target.IedId))
            throw new InvalidOperationException("Invalid or out-of-scope Lab read profile.");
        ValidateRemotePath(target.Directory, true);
    }
    public static void ValidateRemotePath(string path, bool directory = false)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > 1024 || path.Any(char.IsControl) ||
            path.Contains('\\') || path.Contains(':') || path.Contains('%') || path.Contains('*') || path.Contains('?') ||
            path.Split('/').Any(p => p is "." or "..") || (!directory && path.EndsWith('/')))
            throw new InvalidDataException("Unsafe remote path; unsupported naming requires reviewed adapter profile.");
    }
    public static string FileName(string path)
    {
        ValidateRemotePath(path);
        var name = path.Split('/')[^1];
        if (name.Length is < 1 or > 200 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            name.EndsWith('.') || name.EndsWith(' ') || new[] { "CON", "PRN", "AUX", "NUL", "COM1", "LPT1" }.Contains(Path.GetFileNameWithoutExtension(name), StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException("Unsafe local filename.");
        return name;
    }
    public static TransferValidation ValidateTransfer(string path, long expectedSize, string? sourceSha256 = null)
    {
        using var file = File.OpenRead(path);
        if (expectedSize < 0 || file.Length != expectedSize) throw new InvalidDataException("Incomplete or changed source file.");
        var hash = Convert.ToHexStringLower(SHA256.HashData(file));
        if (sourceSha256 is not null && !string.Equals(hash, sourceSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Independent source checksum mismatch.");
        return new(file.Length, hash, true, sourceSha256 is null ? null : true);
    }
}
