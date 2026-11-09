using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Fdc.Collector.Pilot;
using Fdc.Core;

if (args.Length > 0 && args[0].StartsWith("inventory-", StringComparison.Ordinal))
{
    InventoryCommands.Run(args);
    return;
}

// Offline-only product harness: no socket listener, IED endpoint, credential or fallback principal.
if (args.Length != 2 || args[0] != "offline-demo")
    throw new ArgumentException("Usage: offline-demo <new-output-directory>. Real IED probe requires a separately approved run profile; not enabled here.");
var root = Path.GetFullPath(args[1]);
if (Directory.Exists(root) && Directory.EnumerateFileSystemEntries(root).Any()) throw new InvalidOperationException("Never overwrite an existing run.");
Directory.CreateDirectory(root);
var source = Path.Combine(root, "fixture-source"); Directory.CreateDirectory(source);
File.WriteAllText(Path.Combine(source, "synthetic.cfg"), "SYNTHETIC,OFFLINE_IED,2013\n2,1A,1D\n1,Ia,A,,A,1,0,0,-32768,32767,1,1,P\n1,Trip,,,0\n50\n1\n1000,3\n01/01/2026,00:00:00.000000\n01/01/2026,00:00:00.001000\nASCII\n1\n");
File.WriteAllText(Path.Combine(source, "synthetic.dat"), "1,0,1,0\n2,1000,2,1\n3,2000,3,0\n");
var target = new IedTarget("OFFLINE_STATION", "OFFLINE_IED", IPAddress.Loopback, 1, "/", "OFFLINE_FIXTURE", null, null);
var storeRoot = Path.Combine(root, "collector"); _ = new Store(storeRoot);
var queue = new PilotTransferQueue(storeRoot, TimeProvider.System);
using var adapter = new OfflineFixtureFiles(source);
var limits = new ComtradeLimits(1048576, 10485760, 32, 32, 100000, 200);
var policy = new AcquisitionQueuePolicy(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));
var watch = Stopwatch.StartNew();
var records = new PilotAcquisition(queue, Path.Combine(root, "spool"), limits, "UTF-8", 1048576, 10485760, TimeProvider.System, policy).Collect(adapter, target, 3, default);
watch.Stop();
var data = new OfflineDashboardData("OFFLINE_FIXTURE", DateTimeOffset.UtcNow, records, adapter.ListFiles(default),
    queue.Status("Download", 100).Select(j => (object)new { j.Id, j.State, j.Attempts }).ToArray(),
    queue.Status("Upload", 100).Select(j => (object)new { j.Id, j.State, j.Attempts }).ToArray(),
    new DriveInfo(Path.GetPathRoot(root)!).AvailableFreeSpace);
OfflineDashboard.Write(Path.Combine(root, "dashboard"), data);
File.WriteAllText(Path.Combine(root, "run.json"), JsonSerializer.Serialize(new
{
    EvidenceKind = "OFFLINE_FIXTURE", Records = records.Count, AcquisitionMs = watch.Elapsed.TotalMilliseconds,
    RealIedConnections = 0, CenterUploads = 0, Acknowledgments = 0,
    Limits = "Synthetic file acquisition/SQLite/parser/viewer only. No MMS, real IED, mTLS upload or SCADA acceptance. No deploy-time retry/poll defaults selected."
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("OFFLINE_FIXTURE: " + records.Count + " record(s) locally acquired; dashboard/run.json created. No real IED or Center connection.");

internal sealed class OfflineFixtureFiles(string directory) : IIedFileAcquisitionAdapter
{
    public EvidenceKind EvidenceKind => EvidenceKind.OFFLINE_FIXTURE;
    public CapabilityObservation ProbeCapabilities(CancellationToken ct) => new(CapabilityState.NOT_TESTED, CapabilityState.NOT_TESTED, CapabilityState.SUCCESS,
        CapabilityState.NOT_TESTED, CapabilityState.NOT_TESTED, EvidenceKind, DateTimeOffset.UtcNow, "Local synthetic fixtures; no network.");
    public IReadOnlyList<IedFileMetadata> ListFiles(CancellationToken ct) => Directory.GetFiles(directory).Select(f => new IedFileMetadata("/" + Path.GetFileName(f), new FileInfo(f).Length, File.GetLastWriteTimeUtc(f))).ToArray();
    public IedFileMetadata GetFileMetadata(string path, CancellationToken ct) => ListFiles(ct).Single(f => f.RemotePath == path);
    public void DownloadFile(string path, Stream destination, CancellationToken ct, IProgress<long>? progress = null)
    {
        ct.ThrowIfCancellationRequested(); _ = GetFileMetadata(path, ct);
        using var input = File.OpenRead(Path.Combine(directory, AcquisitionSafety.FileName(path))); input.CopyTo(destination); progress?.Report(input.Length);
    }
    public string GetVendorDiagnostics() => "OFFLINE_FIXTURE: no hardware/vendor claim";
    public void Dispose() { }
}
