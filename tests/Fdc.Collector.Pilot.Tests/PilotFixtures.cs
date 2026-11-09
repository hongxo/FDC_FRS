using System.Net;
using Fdc.Collector.Pilot;
using Fdc.Core;
using Microsoft.Data.Sqlite;

namespace Fdc.Collector.Pilot.Tests;

internal sealed class PilotFiles : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "FdcPilot-" + Guid.NewGuid().ToString("N"));
    public string Source => Path.Combine(Root, "source");
    public string Cfg => Path.Combine(Source, "event.cfg");
    public string Dat => Path.Combine(Source, "event.dat");
    public static ComtradeLimits Limits => new(1048576, 10485760, 32, 32, 100000, 20);
    public static AcquisitionQueuePolicy QueuePolicy => new(TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
    public IedTarget Target => new("SYNTHETIC_STATION", "SYNTHETIC_IED", IPAddress.Loopback, 18102, "/", null, null, null);
    public PilotFiles(string format = "ASCII")
    {
        Directory.CreateDirectory(Source);
        File.WriteAllText(Cfg, Config(format));
        if (format == "ASCII") File.WriteAllText(Dat, "1,0,1,0\n2,1000,2,1\n3,2000,3,0\n");
        else
        {
            using var writer = new BinaryWriter(File.Create(Dat));
            for (var i = 0; i < 3; i++)
            {
                writer.Write((uint)(i + 1)); writer.Write((uint)(i * 1000));
                if (format == "BINARY") writer.Write((short)(i + 1));
                else if (format == "BINARY32") writer.Write(16777217 + i);
                else writer.Write(i + 1.5f);
                writer.Write((ushort)(i % 2));
            }
        }
    }
    public static string Config(string format) => $"SYNTHETIC,IED,2013\n2,1A,1D\n1,Ia,A,,A,2,1,0,-32768,32767,100,1,S\n1,Trip,,,0\n50\n1\n1000,3\n01/01/2026,00:00:00.000000\n01/01/2026,00:00:00.001000\n{format}\n2\n";
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        var full = Path.GetFullPath(Root);
        if (!full.StartsWith(Path.Combine(Path.GetTempPath(), "FdcPilot-"), StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Cleanup escaped test root.");
        Directory.Delete(full, true);
    }
}
internal sealed class OfflineFiles(string source, bool partial = false) : IIedFileAcquisitionAdapter
{
    public EvidenceKind EvidenceKind => EvidenceKind.OFFLINE_FIXTURE;
    public CapabilityObservation ProbeCapabilities(CancellationToken ct) => new(CapabilityState.NOT_TESTED, CapabilityState.NOT_TESTED, CapabilityState.SUCCESS, CapabilityState.NOT_TESTED, CapabilityState.NOT_TESTED, EvidenceKind, DateTimeOffset.UtcNow, "OFFLINE_FIXTURE; no MMS/real device.");
    public IReadOnlyList<IedFileMetadata> ListFiles(CancellationToken ct) => Directory.GetFiles(source).Select(p => new IedFileMetadata("/" + Path.GetFileName(p), new FileInfo(p).Length, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))).ToArray();
    public IedFileMetadata GetFileMetadata(string path, CancellationToken ct) => ListFiles(ct).Single(x => x.RemotePath == path);
    public void DownloadFile(string path, Stream destination, CancellationToken ct, IProgress<long>? progress = null)
    {
        ct.ThrowIfCancellationRequested(); var file = Path.Combine(source, AcquisitionSafety.FileName(path));
        using var input = File.OpenRead(file);
        if (partial) { destination.WriteByte((byte)input.ReadByte()); return; }
        input.CopyTo(destination); progress?.Report(input.Length);
    }
    public string GetVendorDiagnostics() => "OFFLINE_FIXTURE; no vendor interoperability conclusion";
    public void Dispose() { }
}
internal sealed class MutableClock : TimeProvider
{
    public DateTimeOffset Current = new(2026, 11, 9, 0, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Current;
}
