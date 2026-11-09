using System.Security.Cryptography;
using System.Text.Json;
using Fdc.Core;

namespace Fdc.Center.Persistence;

public sealed record PilotIntakeLimits(long MaximumFileBytes, long MaximumPackageBytes,
    long MinimumFreeBytes, int MaximumStagingSessions, TimeSpan SessionLifetime, ComtradeLimits ParserLimits, string CfgEncoding, int MaximumFileAttempts);

// Filesystem-backed staging, canonical existing SQL writer/outbox at commit. NOT registered or
// mapped by the main host while D01/D07 are pending. No SQL table or effective identity seeded.
public sealed class PilotFileIntake(string absoluteStorageRoot, PilotIntakeLimits limits,
    IMachineRecordAuthority authority, SqlRecordIngestion ingestion, TimeProvider clock, bool syntheticLabData)
{
    private sealed record Session(Manifest Manifest, string ManifestJson, string ManifestSha256,
        MachineRecordBinding Binding, DateTimeOffset ExpiresAt, string CorrelationId);

    public async Task<PilotUploadTicket> BeginAsync(Manifest input, string correlationId, CancellationToken ct)
    {
        ValidateLimits(); ValidateManifest(input);
        if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > 200 || correlationId.Any(char.IsControl)) throw new ArgumentException("Bounded server correlation required.");
        var binding = await authority.DemandAsync(input.StationId, input.IedId, ct);
        var manifest = input with { Files = input.Files.ToArray() };
        var json = JsonSerializer.Serialize(manifest);
        if (System.Text.Encoding.UTF8.GetByteCount(json) > 65536) throw new InvalidDataException("Manifest64KiB bound.");
        SafeDirectory(absoluteStorageRoot); var incoming = Path.Combine(absoluteStorageRoot, "incoming"); SafeDirectory(incoming);
        using var admission = ExclusiveFile(Path.Combine(incoming, "admission.lock"));
        if (Directory.EnumerateDirectories(incoming).Take(limits.MaximumStagingSessions).Count() >= limits.MaximumStagingSessions)
            throw new IOException("INGESTION_BACKPRESSURE:staging reconciliation required.");
        if (new DriveInfo(Path.GetPathRoot(absoluteStorageRoot)!).AvailableFreeSpace < checked(manifest.Files.Sum(f => f.Size) + limits.MinimumFreeBytes))
            throw new IOException("CENTER_DISK_RESERVE");
        var id = Guid.NewGuid(); var root = Path.Combine(incoming, id.ToString("N")); SafeDirectory(root); SafeDirectory(Path.Combine(root, "verified")); SafeDirectory(Path.Combine(root, "partial"));
        var session = new Session(manifest, json, Fdc.Core.Identity.Hash(json), binding, clock.GetUtcNow() + limits.SessionLifetime, correlationId);
        await using (var stream = new FileStream(Path.Combine(root, "session.json"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
        { await JsonSerializer.SerializeAsync(stream, session, cancellationToken: ct); await stream.FlushAsync(ct); stream.Flush(true); }
        return new(id, manifest.Id, session.ManifestSha256, session.ExpiresAt);
    }
    public async Task PutFileAsync(Guid ticket, string name, Stream source, CancellationToken ct)
    {
        var session = await Current(ticket, ct); ValidateName(name);
        using var uploadLock = ExclusiveFile(Path.Combine(TicketRoot(ticket), "upload.lock"));
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var remaining = session.ExpiresAt - clock.GetUtcNow();
        if (remaining <= TimeSpan.Zero) throw new UnauthorizedAccessException("Expired upload window.");
        budget.CancelAfter(remaining); ct = budget.Token;
        var file = session.Manifest.Files.SingleOrDefault(f => f.Name == name) ?? throw new UnauthorizedAccessException("File is not declared by ticket.");
        var root = TicketRoot(ticket); var partial = Path.Combine(root, "partial", Guid.NewGuid().ToString("N") + ".part");
        if (Directory.EnumerateFiles(Path.Combine(root, "partial")).Take(limits.MaximumFileAttempts).Count() >= limits.MaximumFileAttempts)
            throw new IOException("INGESTION_BACKPRESSURE:partial attempt reconciliation required.");
        var final = Path.Combine(root, "verified", name);
        await using (var output = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
        {
            var buffer = new byte[65536]; long total = 0;
            while (true)
            {
                var count = await source.ReadAsync(buffer, ct); if (count == 0) break;
                if (count > file.Size - total) throw new InvalidDataException("Upload exceeds declared file size.");
                if (new DriveInfo(Path.GetPathRoot(absoluteStorageRoot)!).AvailableFreeSpace < count + limits.MinimumFreeBytes) throw new IOException("CENTER_DISK_RESERVE");
                await output.WriteAsync(buffer.AsMemory(0, count), ct); total += count;
            }
            if (total != file.Size) throw new InvalidDataException("Partial upload; restart whole file, not byte-resume.");
            await output.FlushAsync(ct); output.Flush(true);
        }
        await VerifyFile(partial, file, ct);
        _ = await Current(ticket, ct); // Expiry/registry binding may have changed during body transfer.
        try { File.Move(partial, final, false); }
        catch (IOException) when (File.Exists(final)) { await VerifyFile(final, file, ct); } // Safe duplicate; immutable original, partial retained for reconciliation.
    }
    public async Task<PilotIngestionAck> CompleteAsync(Guid ticket, CancellationToken ct)
    {
        var session = await Current(ticket, ct); var incoming = Path.Combine(TicketRoot(ticket), "verified");
        using var uploadLock = ExclusiveFile(Path.Combine(TicketRoot(ticket), "upload.lock"));
        var records = Path.Combine(absoluteStorageRoot, "records"); SafeDirectory(records);
        var final = Path.Combine(records, session.Manifest.Id);
        if (!Directory.Exists(final))
        {
            SafeDirectory(incoming);
            foreach (var file in session.Manifest.Files) await VerifyFile(Path.Combine(incoming, file.Name), file, ct);
            var cfg = session.Manifest.Files.Single(f => Path.GetExtension(f.Name).Equals(".cfg", StringComparison.OrdinalIgnoreCase));
            var dat = session.Manifest.Files.Single(f => Path.GetExtension(f.Name).Equals(".dat", StringComparison.OrdinalIgnoreCase));
            _ = BoundedComtradeReader.Read(Path.Combine(incoming, cfg.Name), Path.Combine(incoming, dat.Name), limits.ParserLimits, limits.CfgEncoding, ct);
            var manifestPath = Path.Combine(incoming, "manifest.json");
            if (!File.Exists(manifestPath))
                await using (var output = new FileStream(manifestPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
                { await output.WriteAsync(System.Text.Encoding.UTF8.GetBytes(session.ManifestJson), ct); await output.FlushAsync(ct); output.Flush(true); }
            try { Directory.Move(incoming, final); }
            catch (IOException) when (Directory.Exists(final)) { /* Another writer promoted same content; revalidate below. */ }
        }
        SafeDirectory(final);
        var storedManifest = Path.Combine(final, "manifest.json");
        if (new FileInfo(storedManifest).Length > 65536 || IsReparse(storedManifest) || await File.ReadAllTextAsync(storedManifest, ct) != session.ManifestJson)
            throw new InvalidDataException("Conflicting immutable manifest replay.");
        var verified = await VerifiedRecordPackage.VerifyAsync(absoluteStorageRoot, session.Manifest, ct);
        var cfgFile = session.Manifest.Files.Single(f => Path.GetExtension(f.Name).Equals(".cfg", StringComparison.OrdinalIgnoreCase));
        var datFile = session.Manifest.Files.Single(f => Path.GetExtension(f.Name).Equals(".dat", StringComparison.OrdinalIgnoreCase));
        var preview = BoundedComtradeReader.Read(Path.Combine(final, cfgFile.Name), Path.Combine(final, datFile.Name), limits.ParserLimits, limits.CfgEncoding, ct);
        if (session.Manifest.TriggerTimeRaw is not null && session.Manifest.TriggerTimeRaw != preview.TriggerTimeRaw) throw new InvalidDataException("Trigger metadata mismatch.");
        _ = await Current(ticket, ct);
        var receipt = await ingestion.AcceptAsync(verified, session.CorrelationId, syntheticLabData, ct);
        var filesDigest = Fdc.Core.Identity.Hash(string.Join('\n', session.Manifest.Files.Select(f => f.Name + "|" + f.Size + "|" + f.Sha256)));
        return new(receipt.RecordId, session.Manifest.Id, session.ManifestSha256, filesDigest, receipt.Duplicate, session.CorrelationId);
    }
    public async Task<PilotUploadTicket> StatusAsync(Guid ticket, CancellationToken ct)
    {
        var s = await Current(ticket, ct); return new(ticket, s.Manifest.Id, s.ManifestSha256, s.ExpiresAt);
    }
    private async Task<Session> Current(Guid ticket, CancellationToken ct)
    {
        ValidateLimits(); var root = TicketRoot(ticket); SafeExistingDirectory(root);
        var path = Path.Combine(root, "session.json");
        if (new FileInfo(path).Length > 131072 || IsReparse(path)) throw new InvalidDataException("Unsafe ticket metadata.");
        await using var input = File.OpenRead(path);
        var session = await JsonSerializer.DeserializeAsync<Session>(input, cancellationToken: ct) ?? throw new InvalidDataException("Missing ticket.");
        if (clock.GetUtcNow() >= session.ExpiresAt) throw new UnauthorizedAccessException("Expired ticket.");
        ValidateManifest(session.Manifest);
        if (session.ManifestJson != JsonSerializer.Serialize(session.Manifest) || session.ManifestSha256 != Fdc.Core.Identity.Hash(session.ManifestJson)) throw new InvalidDataException("Corrupt ticket.");
        var current = await authority.DemandAsync(session.Manifest.StationId, session.Manifest.IedId, ct);
        if (current != session.Binding) throw new UnauthorizedAccessException("Collector/station binding changed.");
        return session;
    }
    private string TicketRoot(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("Ticket ID required.");
        return Path.Combine(absoluteStorageRoot, "incoming", id.ToString("N"));
    }
    private void ValidateLimits()
    {
        limits.ParserLimits.Validate();
        if (!Path.IsPathFullyQualified(absoluteStorageRoot) || limits.MaximumFileBytes is < 1 or > 104857600 ||
            limits.MaximumPackageBytes < limits.MaximumFileBytes || limits.MaximumPackageBytes > 419430400 ||
            limits.MinimumFreeBytes < 0 || limits.MaximumStagingSessions is < 1 or > 10000 ||
            limits.SessionLifetime <= TimeSpan.Zero || limits.SessionLifetime > TimeSpan.FromHours(24) || limits.MaximumFileAttempts is < 1 or > 100) throw new ArgumentException("Explicit bounded Lab intake limits required.");
    }
    private void ValidateManifest(Manifest m)
    {
        if (m is null || string.IsNullOrWhiteSpace(m.StationId) || string.IsNullOrWhiteSpace(m.IedId) ||
            m.Id is null || m.RemoteIdentity is null || m.Files is null || string.IsNullOrWhiteSpace(m.TimeSource)) throw new InvalidDataException("Missing manifest fields.");
        if (m.TimeSource.Length > 200 || !Fdc.Core.Identity.SafeId(m.StationId) || !Fdc.Core.Identity.SafeId(m.IedId) || m.Files.Count is < 2 or > 4 ||
            m.Id.Length != 64 || m.Id.Any(c => !char.IsAsciiHexDigit(c) || char.IsUpper(c)) || m.RemoteIdentity.Length > 1024 ||
            m.Files.Any(f => f is null || string.IsNullOrWhiteSpace(f.Name) || f.Sha256 is null || f.Size < 0 || f.Size > limits.MaximumFileBytes ||
                (Path.GetExtension(f.Name).Equals(".cfg", StringComparison.OrdinalIgnoreCase) && f.Size > limits.ParserLimits.MaxCfgBytes) ||
                f.Sha256.Length != 64 || f.Sha256.Any(c => !char.IsAsciiHexDigit(c) || char.IsUpper(c))) ||
            m.Files.Sum(f => f.Size) > limits.MaximumPackageBytes) throw new InvalidDataException("Manifest identity/size bound.");
        foreach (var f in m.Files) ValidateName(f.Name);
        var extensions = m.Files.Select(f => Path.GetExtension(f.Name).ToLowerInvariant()).ToArray();
        if (extensions.Distinct().Count() != extensions.Length || !extensions.Contains(".cfg") || !extensions.Contains(".dat") ||
            m.Files.Select(f => Path.GetFileNameWithoutExtension(f.Name)).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 1)
            throw new InvalidDataException("Ambiguous/missing CFG/DAT group.");
        var id = Fdc.Core.Identity.Hash(m.StationId + "\n" + m.IedId + "\n" + m.RemoteIdentity + "\n" + string.Join('\n', m.Files.Select(f => f.Name + ":" + f.Sha256)));
        if (id != m.Id) throw new InvalidDataException("Manifest content identity mismatch.");
    }
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name != Fdc.Core.Identity.FileName(name) || name.Contains(':') || name.Contains('%') || name.Any(char.IsControl) ||
            Path.GetExtension(name).ToLowerInvariant() is not (".cfg" or ".dat" or ".hdr" or ".inf")) throw new InvalidDataException("Unsafe file name.");
    }
    private static async Task VerifyFile(string path, FileDigest expected, CancellationToken ct)
    {
        if (IsReparse(path)) throw new InvalidDataException("Reparse file forbidden.");
        await using var input = File.OpenRead(path);
        if (input.Length != expected.Size || Convert.ToHexStringLower(await SHA256.HashDataAsync(input, ct)) != expected.Sha256) throw new InvalidDataException("File integrity mismatch.");
    }
    private static bool IsReparse(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    private static FileStream ExclusiveFile(string path)
    {
        if (File.Exists(path) && IsReparse(path)) throw new InvalidDataException("Reparse lock file forbidden.");
        return new(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }
    private static void SafeDirectory(string path) { Directory.CreateDirectory(path); SafeExistingDirectory(path); }
    private static void SafeExistingDirectory(string path)
    {
        for (var d = new DirectoryInfo(Path.GetFullPath(path)); d is not null; d = d.Parent)
            if (!d.Exists || (d.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Unsafe storage directory.");
    }
}
