using System.Text.Json;
using Fdc.Core;

namespace Fdc.Collector.Pilot;

public sealed record PilotCollectedRecord(Manifest Manifest, PilotPackage Package, ComtradePreview Preview,
    EvidenceKind EvidenceKind, string ChecksumScope);

// Explicit single-pass operation, never an auto-starting worker or network scanner.
public sealed record AcquisitionQueuePolicy(TimeSpan LeaseDuration, TimeSpan InitialBackoff, TimeSpan MaximumBackoff);

public sealed class PilotAcquisition(PilotTransferQueue queue, string absoluteSpoolRoot,
    ComtradeLimits parserLimits, string cfgEncoding, long minimumFreeBytes, long maximumFileBytes,
    TimeProvider clock, AcquisitionQueuePolicy queuePolicy, Func<long>? availableBytes = null)
{
    public IReadOnlyList<PilotCollectedRecord> Collect(IIedFileAcquisitionAdapter adapter, IedTarget target,
        int maximumAttempts, CancellationToken ct)
    {
        if (!Path.IsPathFullyQualified(absoluteSpoolRoot) || minimumFreeBytes < 0 || maximumFileBytes is < 1 or > 104857600 || maximumAttempts is < 1 or > 10 ||
            !Identity.SafeId(target.Station) || !Identity.SafeId(target.IedId)) throw new ArgumentException("Explicit bounded acquisition required.");
        if (queuePolicy.LeaseDuration <= TimeSpan.Zero || queuePolicy.LeaseDuration > TimeSpan.FromHours(1) ||
            queuePolicy.InitialBackoff <= TimeSpan.Zero || queuePolicy.MaximumBackoff < queuePolicy.InitialBackoff || queuePolicy.MaximumBackoff > TimeSpan.FromDays(1))
            throw new ArgumentException("Explicit bounded queue policy required.");
        parserLimits.Validate(); SafeDirectory(absoluteSpoolRoot);
        var capability = adapter.ProbeCapabilities(ct);
        if (capability.EvidenceKind != adapter.EvidenceKind) throw new InvalidDataException("Capability evidence kind mismatch.");
        queue.SaveCapability(target, capability);
        if (capability.FileService == CapabilityState.UNSUPPORTED_FILE_SERVICE || capability.FileService == CapabilityState.ACCESS_DENIED)
            return []; // No automatic FTP/SFTP/HTTP fallback.
        var files = adapter.ListFiles(ct);
        if (files.Count > 10000) throw new InvalidDataException("Discovery bound exceeded.");
        var allowed = files.Where(f => new[] { ".cfg", ".dat", ".hdr", ".inf" }.Contains(Path.GetExtension(f.RemotePath).ToLowerInvariant())).ToArray();
        var groups = allowed.GroupBy(f => f.RemotePath[..^Path.GetExtension(f.RemotePath).Length], StringComparer.OrdinalIgnoreCase);
        var collected = new List<PilotCollectedRecord>();
        foreach (var group in groups)
        {
            ct.ThrowIfCancellationRequested();
            var members = group.OrderBy(f => f.RemotePath, StringComparer.Ordinal).ToArray();
            if (members.GroupBy(f => Path.GetExtension(f.RemotePath), StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                throw new InvalidDataException("Ambiguous case-colliding file group.");
            var cfg = members.SingleOrDefault(f => Path.GetExtension(f.RemotePath).Equals(".cfg", StringComparison.OrdinalIgnoreCase));
            var dat = members.SingleOrDefault(f => Path.GetExtension(f.RemotePath).Equals(".dat", StringComparison.OrdinalIgnoreCase));
            if (cfg is null || dat is null) continue; // Not a complete group; never COMTRADE_CONFIRMED.
            if (members.Any(f => f.Size < 0 || f.Size > maximumFileBytes)) throw new InvalidDataException("File size bound.");
            var needed = checked(members.Sum(f => f.Size) + minimumFreeBytes);
            long Free() => availableBytes?.Invoke() ?? new DriveInfo(Path.GetPathRoot(absoluteSpoolRoot)!).AvailableFreeSpace;
            if (Free() < needed) throw new IOException("COLLECTOR_DISK_RESERVE");
            var job = queue.Discover(target.Station, target.IedId, cfg.RemotePath, Guid.NewGuid(), maximumAttempts);
            var lease = queue.Claim("Download", queuePolicy.LeaseDuration, job);
            // A separately running worker may have another pending job. Do not apply this group's bytes to it.
            if (lease is null || lease.Id != job) throw new InvalidOperationException("A pending discovery requires recovery before this single-pass group.");
            var stageRoot = Path.Combine(absoluteSpoolRoot, "staging"); SafeDirectory(stageRoot);
            var stage = Path.Combine(stageRoot, Guid.NewGuid().ToString("N") + ".part"); SafeDirectory(stage);
            try
            {
                var digests = new List<FileDigest>();
                foreach (var member in members)
                {
                    ct.ThrowIfCancellationRequested();
                    if (Free() < member.Size + minimumFreeBytes) throw new IOException("COLLECTOR_DISK_RESERVE");
                    var name = AcquisitionSafety.FileName(member.RemotePath); var local = Path.Combine(stage, name);
                    using (var output = new FileStream(local, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    { adapter.DownloadFile(member.RemotePath, output, ct); output.Flush(true); }
                    var valid = AcquisitionSafety.ValidateTransfer(local, member.Size);
                    digests.Add(new(name, valid.Bytes, valid.Sha256));
                }
                var after = adapter.ListFiles(ct).ToDictionary(f => f.RemotePath, StringComparer.Ordinal);
                if (members.Any(before => !after.TryGetValue(before.RemotePath, out var current) || current.Size != before.Size || current.Modified != before.Modified))
                    throw new InvalidDataException("Group source metadata changed during acquisition.");
                var preview = BoundedComtradeReader.Read(Path.Combine(stage, AcquisitionSafety.FileName(cfg.RemotePath)),
                    Path.Combine(stage, AcquisitionSafety.FileName(dat.RemotePath)), parserLimits, cfgEncoding, ct);
                var id = Identity.Hash(target.Station + "\n" + target.IedId + "\n" + cfg.RemotePath + "\n" + string.Join('\n', digests.Select(f => f.Name + ":" + f.Sha256)));
                var manifest = new Manifest(id, target.Station, target.IedId, "", target.Vendor ?? "", target.Model ?? "", target.Firmware ?? "",
                    cfg.RemotePath, clock.GetUtcNow(), preview.TriggerTimeRaw, "IED CFG; timezone unspecified unless separately verified", digests);
                var recordRoot = Path.Combine(absoluteSpoolRoot, "records"); SafeDirectory(recordRoot);
                var directory = Path.Combine(recordRoot, id);
                var json = JsonSerializer.Serialize(manifest);
                if (Directory.Exists(directory))
                {
                    // Filename reuse/content duplicate: retain the first immutable metadata and original bytes.
                    SafeDirectory(directory);
                    var originalPath = Path.Combine(directory, "manifest.json");
                    if (new FileInfo(originalPath).Length > 65536 || IsReparse(originalPath)) throw new InvalidDataException("Unsafe stored manifest.");
                    json = File.ReadAllText(originalPath);
                    var original = JsonSerializer.Deserialize<Manifest>(json) ?? throw new InvalidDataException("Missing immutable manifest.");
                    if (original.Id != id || original.Files.Count != digests.Count || original.Files.Where((f, i) => f != digests[i]).Any()) throw new InvalidDataException("Conflicting existing package.");
                    foreach (var file in original.Files)
                    {
                        var stored = Path.Combine(directory, AcquisitionSafety.FileName(file.Name));
                        if (IsReparse(stored)) throw new InvalidDataException("Unsafe stored file.");
                        AcquisitionSafety.ValidateTransfer(stored, file.Size, file.Sha256);
                    }
                    manifest = original;
                }
                else
                {
                    using (var output = new FileStream(Path.Combine(stage, "manifest.json"), FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    { var data = System.Text.Encoding.UTF8.GetBytes(json); output.Write(data); output.Flush(true); }
                    Directory.Move(stage, directory); // Durable file promotion precedes SQL; orphan is visible/recoverable on repeat.
                }
                var filesDigest = Identity.Hash(string.Join('\n', manifest.Files.Select(f => f.Name + "|" + f.Size + "|" + f.Sha256)));
                var package = new PilotPackage(id, json, Identity.Hash(json), directory, filesDigest);
                queue.CompleteDownload(lease, package, maximumAttempts);
                queue.SaveCapability(target, capability with { FileDownload = CapabilityState.SUCCESS, Comtrade = CapabilityState.SUCCESS,
                    ObservedAt = clock.GetUtcNow(), Diagnostic = "Local size/SHA256/parser verification completed; no independent source checksum or model/firmware verification implied." });
                collected.Add(new(manifest, package, preview, adapter.EvidenceKind, "Internal SHA256/size verified; no independent original checksum supplied."));
            }
            catch
            {
                // Preserve partial files for diagnosis/reconciliation, bounded by disk reserve; never touch IED files.
                try { queue.Retry(lease, queuePolicy.InitialBackoff, queuePolicy.MaximumBackoff, "ACQUISITION_FAILED"); }
                catch (InvalidOperationException) { /* A stale worker cannot overwrite a newer lease. */ }
                throw;
            }
        }
        return collected;
    }
    private static bool IsReparse(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    private static void SafeDirectory(string path)
    {
        Directory.CreateDirectory(path);
        for (var directory = new DirectoryInfo(Path.GetFullPath(path)); directory is not null; directory = directory.Parent)
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Reparse spool path forbidden.");
    }
}
