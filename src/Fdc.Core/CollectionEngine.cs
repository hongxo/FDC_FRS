using System.IO.Compression;
using System.Text.Json;

namespace Fdc.Core;

public sealed class CollectionEngine(CollectorOptions options, Store store, IIedFactory factory)
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> locks = new();
    private readonly SemaphoreSlim capacity = new(options.MaxConcurrentIeds);
    public async Task<IReadOnlyList<RemoteFile>> ListAsync(Ied ied, CancellationToken ct)
    {
        var gate = locks.GetOrAdd(ied.Id, _ => new(1)); await gate.WaitAsync(ct);
        try
        {
            await capacity.WaitAsync(ct);
            try { return await Task.Run(() => { using var session = factory.Connect(ied); return session.List(ied.Directory); }, ct); }
            finally { capacity.Release(); }
        }
        finally { gate.Release(); }
    }
    public async Task<int> CollectAsync(Ied ied, string? requestedPath, CancellationToken ct)
    {
        var gate = locks.GetOrAdd(ied.Id, _ => new(1)); await gate.WaitAsync(ct);
        try
        {
            await capacity.WaitAsync(ct);
            try { return await Task.Run(() => Collect(ied, requestedPath, ct), ct); }
            finally { capacity.Release(); }
        }
        finally { gate.Release(); }
    }
    private int Collect(Ied ied, string? requestedPath, CancellationToken ct)
    {
        using var session = factory.Connect(ied); var files = session.List(ied.Directory);
        var count = 0; var failures = new List<string>();
        var cfgs = files.Where(x => Path.GetExtension(x.Path).Equals(".cfg", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (requestedPath != null && !cfgs.Any(x => x.Path == requestedPath))
            throw new InvalidDataException("Chỉ tải lại đường dẫn CFG có trong danh mục IED đã cấu hình.");
        foreach (var cfg in cfgs.Where(x => requestedPath == null || x.Path == requestedPath))
        {
            ct.ThrowIfCancellationRequested();
            var stem = Path.ChangeExtension(cfg.Path, null);
            var members = files.Where(x => string.Equals(Path.ChangeExtension(x.Path, null), stem, StringComparison.OrdinalIgnoreCase)
                && new[] { ".cfg", ".dat", ".hdr", ".inf" }.Contains(Path.GetExtension(x.Path).ToLowerInvariant())).OrderBy(x => x.Path, StringComparer.Ordinal).ToArray();
            var dat = members.SingleOrDefault(x => Path.GetExtension(x.Path).Equals(".dat", StringComparison.OrdinalIgnoreCase));
            if (dat == null) continue; // The relay may still be writing its record.
            var source = options.StationId + "/" + ied.Id + "/" + cfg.Path;
            var fingerprint = Identity.Hash(string.Join('\n', members.Select(x => $"{x.Path}|{x.Size}|{x.Modified:O}")));
            // Unknown-size / unknown-time vendors are deliberately re-read and deduplicated by content.
            var reliableMetadata = members.All(x => x.Size > 0 && x.Modified != null);
            if (requestedPath == null && reliableMetadata && store.Unchanged(source, fingerprint)) continue;
            var stage = Path.Combine(options.DataRoot, "staging", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stage);
            try
            {
                foreach (var file in members)
                {
                    if (file.Size > options.MaxFileBytes) throw new InvalidDataException("File vượt giới hạn dung lượng.");
                    var local = Path.Combine(stage, Identity.FileName(file.Path));
                    session.Download(file.Path, local, options.MaxFileBytes, ct);
                    if (file.Size > 0 && new FileInfo(local).Length != file.Size) throw new InvalidDataException("File thay đổi hoặc chưa tải đủ.");
                }
                var trigger = Comtrade.Validate(Path.Combine(stage, Identity.FileName(cfg.Path)), Path.Combine(stage, Identity.FileName(dat.Path)));
                var digests = members.Select(x =>
                {
                    var name = Identity.FileName(x.Path); var path = Path.Combine(stage, name);
                    return new FileDigest(name, new FileInfo(path).Length, Identity.FileHash(path));
                }).ToArray();
                // Include content: relay filenames and directory timestamps can be reused or unknown.
                var id = Identity.Hash(options.StationId + "\n" + ied.Id + "\n" + cfg.Path + "\n" + string.Join('\n', digests.Select(x => x.Name + ":" + x.Sha256)));
                var existing = store.Find(id);
                if (existing != null && requestedPath == null)
                { if (reliableMetadata) store.Observe(source, fingerprint); continue; }
                var manifest = new Manifest(id, options.StationId, ied.Id, ied.Bay, ied.Manufacturer, ied.Model, ied.Firmware,
                    cfg.Path, DateTimeOffset.UtcNow, trigger, "IED CFG; timezone may be unspecified", digests);
                var json = existing?.ManifestJson ?? JsonSerializer.Serialize(manifest);
                File.WriteAllText(Path.Combine(stage, "manifest.json"), json);
                var bundles = Path.Combine(options.DataRoot, "bundles"); Directory.CreateDirectory(bundles);
                var bundle = Path.Combine(bundles, id + ".zip"); var temp = bundle + "." + Guid.NewGuid().ToString("N") + ".part";
                try { ZipFile.CreateFromDirectory(stage, temp); File.Move(temp, bundle, true); }
                finally { if (File.Exists(temp)) File.Delete(temp); }
                store.Add(manifest, bundle, json);
                if (requestedPath != null) store.Requeue(id);
                if (reliableMetadata) store.Observe(source, fingerprint);
                count++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { failures.Add($"{cfg.Path}: {ex.Message}"); }
            finally { Directory.Delete(stage, true); }
        }
        if (failures.Count > 0) throw new IOException($"Đã thu {count} bản ghi; lỗi: {string.Join("; ", failures)}");
        return count;
    }
}
