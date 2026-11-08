using System.IO.Compression;
using System.Text.Json;

namespace Fdc.Core;

public sealed class BundleReceiver(string root, Store store)
{
    private readonly SemaphoreSlim gate = new(1);
    public async Task<Receipt> ReceiveAsync(string archive, string station, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        var stage = Path.Combine(root, "staging", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        try
        {
            using var zip = ZipFile.OpenRead(archive);
            if (zip.Entries.Count is < 3 or > 20 || zip.Entries.Sum(x => x.Length) > 300 * 1024 * 1024)
                throw new InvalidDataException("Gói vượt giới hạn dung lượng hoặc số file.");
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in zip.Entries)
            {
                if (entry.FullName != Identity.FileName(entry.FullName) || !names.Add(entry.FullName))
                    throw new InvalidDataException("Gói có đường dẫn hoặc tên file trùng không hợp lệ.");
                if (entry.FullName == "manifest.json" && entry.Length > 64 * 1024) throw new InvalidDataException("Manifest quá lớn.");
                entry.ExtractToFile(Path.Combine(stage, entry.FullName));
            }
            var json = await File.ReadAllTextAsync(Path.Combine(stage, "manifest.json"), ct);
            var manifest = JsonSerializer.Deserialize<Manifest>(json) ?? throw new InvalidDataException("Manifest rỗng.");
            if (manifest.StationId != station || !Identity.SafeId(manifest.StationId) || !Identity.SafeId(manifest.IedId)
                || manifest.Id.Length != 64 || !manifest.Id.All(char.IsAsciiHexDigit)) throw new InvalidDataException("Sai định danh manifest.");
            if (manifest.Files.Count != names.Count - 1 || manifest.Files.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != manifest.Files.Count)
                throw new InvalidDataException("Danh sách file manifest không khớp.");
            foreach (var file in manifest.Files)
            {
                if (file.Name != Identity.FileName(file.Name) || file.Name == "manifest.json") throw new InvalidDataException("Tên file không hợp lệ.");
                var path = Path.Combine(stage, file.Name);
                if (!File.Exists(path) || new FileInfo(path).Length != file.Size || Identity.FileHash(path) != file.Sha256)
                    throw new InvalidDataException("Kích thước hoặc SHA-256 không khớp.");
            }
            var cfg = manifest.Files.Single(x => x.Name.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase));
            var dat = manifest.Files.Single(x => x.Name.EndsWith(".dat", StringComparison.OrdinalIgnoreCase));
            Comtrade.Validate(Path.Combine(stage, cfg.Name), Path.Combine(stage, dat.Name));
            var expectedId = Identity.Hash(manifest.StationId + "\n" + manifest.IedId + "\n" + manifest.RemoteIdentity + "\n" +
                string.Join('\n', manifest.Files.Select(x => x.Name + ":" + x.Sha256)));
            if (expectedId != manifest.Id) throw new InvalidDataException("Mã bản ghi không khớp nội dung.");
            var targetDirectory = Path.Combine(root, "records", station, manifest.IedId); Directory.CreateDirectory(targetDirectory);
            var target = Path.Combine(targetDirectory, manifest.Id + ".zip");
            File.Copy(archive, target + ".part", true); File.Move(target + ".part", target, true);
            store.Add(manifest, target, json); store.Delivered(manifest.Id);
            return new(manifest.Id, Identity.Hash(json), "Accepted");
        }
        finally { Directory.Delete(stage, true); gate.Release(); }
    }
}
