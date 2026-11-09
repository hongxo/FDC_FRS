using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using Fdc.Collector.Pilot;

namespace Fdc.Collector.Pilot.Tests;

public sealed class NativeMmsSimulationTests
{
    [NativeSimulationFact]
    public async Task Pinned_C_abi_real_MMS_read_only_roundtrip_on_loopback_is_SIMULATED_not_real_IED()
    {
        using var files = new PilotFiles();
        var executable = Environment.GetEnvironmentVariable("FDC_PILOT_SIMULATOR")!;
        var library = Environment.GetEnvironmentVariable("FDC_PILOT_NATIVE_LIBRARY")!;
        var hash = Environment.GetEnvironmentVariable("FDC_PILOT_NATIVE_SHA256")!;
        var simulatorHash = Environment.GetEnvironmentVariable("FDC_PILOT_SIMULATOR_SHA256")!;
        Assert.Equal(simulatorHash, Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(executable))));
        var reservation = new TcpListener(IPAddress.Loopback, 0); reservation.Start();
        var port = ((IPEndPoint)reservation.LocalEndpoint).Port; reservation.Stop();
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add(port.ToString()); start.ArgumentList.Add(files.Source + Path.DirectorySeparatorChar);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Simulator process not started.");
        try
        {
            var banner = await process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Contains("127.0.0.1:" + port, banner);
            var now = DateTimeOffset.UtcNow;
            var target = files.Target with { Port = port };
            var profile = new LabReadProfile("SIMULATED-TEST-ONLY", "NOT-A-REAL-LAB-APPROVAL", now.AddMinutes(-1), now.AddMinutes(3),
                ImmutableHashSet.Create(AcquisitionSafety.Endpoint(target)), 1000, 1000, 100, 1048576, 10485760, TimeSpan.FromMilliseconds(1));
            var timer = Stopwatch.StartNew();
            using var adapter = new MmsReadOnlyAdapter(library, hash, target, profile, new LoopbackOnlyFixtureAuthority(), TimeProvider.System, EvidenceKind.SIMULATED, default);
            var connectMs = timer.Elapsed.TotalMilliseconds;
            var capability = adapter.ProbeCapabilities(default);
            Assert.Equal(EvidenceKind.SIMULATED, capability.EvidenceKind);
            Assert.Equal(CapabilityState.SUCCESS, capability.DirectoryList);
            Assert.Equal(CapabilityState.NOT_TESTED, capability.FileDownload);
            Assert.Throws<InvalidOperationException>(() => new MmsReadOnlyAdapter(library, hash, target, profile, new LoopbackOnlyFixtureAuthority(), TimeProvider.System, EvidenceKind.SIMULATED, default));
            timer.Restart(); var list = adapter.ListFiles(default); var listingMs = timer.Elapsed.TotalMilliseconds;
            Assert.Contains(list, f => f.RemotePath == "/event.cfg"); Assert.Contains(list, f => f.RemotePath == "/event.dat");
            using var output = new MemoryStream(); timer.Restart(); adapter.DownloadFile("/event.dat", output, default); var downloadMs = timer.Elapsed.TotalMilliseconds;
            Assert.Equal(File.ReadAllBytes(files.Dat), output.ToArray());
            using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => adapter.DownloadFile("/event.dat", new MemoryStream(), cancelled.Token));
            Assert.Throws<InvalidDataException>(() => adapter.DownloadFile("/../event.dat", new MemoryStream(), default));
            var q = new PilotTransferQueue(files.Root, TimeProvider.System);
            timer.Restart();
            var collected = new PilotAcquisition(q, Path.Combine(files.Root, "spool"), PilotFiles.Limits, "UTF-8", 1024, 1048576,
                TimeProvider.System, PilotFiles.QueuePolicy).Collect(adapter, target, 2, default).Single();
            var pipelineMs = timer.Elapsed.TotalMilliseconds;
            Assert.Equal(EvidenceKind.SIMULATED, collected.EvidenceKind); Assert.Equal(3, collected.Preview.Samples);
            Assert.Equal("Ready", q.Status("Upload", 10).Single().State);
            var evidence = Environment.GetEnvironmentVariable("FDC_PILOT_EVIDENCE")!;
            Directory.CreateDirectory(evidence);
            await File.WriteAllTextAsync(Path.Combine(evidence, "native-simulated.json"), JsonSerializer.Serialize(new
            {
                EvidenceKind = "SIMULATED", Device = "local libiec61850 file-service simulator, NOT ABB/Siemens/Schneider hardware",
                NativeSha256 = hash, SimulatorSha256 = simulatorHash, SourceCommit = "6e7a603c771d6124cd3cafea7ec38b7e0f2e7aaf",
                ConnectMs = connectMs, ListingMs = listingMs, DownloadMs = downloadMs, DownloadBytes = output.Length,
                DownloadBytesPerSecond = output.Length / (downloadMs / 1000), MmsToSqliteReadyMs = pipelineMs,
                SourceByteComparison = "Exact match to known synthetic fixture", RealIedConnections = 0,
                Limits = "One loopback transfer; no per-vendor benchmark, SCADA coexistence, full Center upload or fleet capacity conclusion"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        finally { if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); } }
    }
    private sealed class LoopbackOnlyFixtureAuthority : ILabReadAuthorization
    {
        public void Demand(IedTarget target, LabReadProfile profile, DateTimeOffset now)
        {
            if (!IPAddress.IsLoopback(target.Address) || profile.ReleaseId != "SIMULATED-TEST-ONLY") throw new InvalidOperationException("No real endpoint permitted by test fixture.");
            AcquisitionSafety.Validate(target, profile, now);
        }
    }
}
internal sealed class NativeSimulationFactAttribute : FactAttribute
{
    public NativeSimulationFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("FDC_PILOT_SIMULATED") != "1" ||
            new[] { "FDC_PILOT_SIMULATOR", "FDC_PILOT_SIMULATOR_SHA256", "FDC_PILOT_NATIVE_LIBRARY", "FDC_PILOT_NATIVE_SHA256", "FDC_PILOT_EVIDENCE" }
                .Any(name => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))))
            Skip = "Explicit SIMULATED loopback opt-in and pinned local native/simulator/evidence paths required.";
    }
}
