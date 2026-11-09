using System.Net;
using Fdc.Center.Persistence;
using Fdc.Collector.Pilot;
using Fdc.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Center.Tests;

public sealed partial class CenterDatabaseSqlTests
{
    [CenterFoundationFact]
    public async Task OFFLINE_FIXTURE_collector_sqlite_staging_to_canonical_sql_receipt_restart_and_replay()
    {
        await using var f = await Fixture.Create();
        try
        {
            var source = await f.Package();
            var edge = Path.Combine(f.Root, "pilot-edge");
            var queue = new PilotTransferQueue(edge, TimeProvider.System);
            using var adapter = new OfflineSqlFixtureFiles(Path.Combine(f.Root, "records", source.Manifest.Id));
            var target = new IedTarget(f.StationCode, "IED-SYNTHETIC", IPAddress.Loopback, 1, "/", null, null, null);
            var record = new PilotAcquisition(queue, Path.Combine(edge, "spool"), new(1048576, 1048576, 32, 32, 10000, 100),
                "UTF-8", 1024, 1048576, TimeProvider.System, new(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1)))
                .Collect(adapter, target, 3, default).Single();
            var center = Path.Combine(f.Root, "pilot-intake");
            var limits = new PilotIntakeLimits(1048576, 4194304, 1024, 10, TimeSpan.FromMinutes(5), new(1048576, 1048576, 32, 32, 10000, 100), "UTF-8", 12);
            await using var db = f.Db();
            var authority = new FixtureMachine(f);
            PilotFileIntake Intake() => new(center, limits, authority, new SqlRecordIngestion(db, authority, TimeProvider.System), TimeProvider.System, true);
            var intake = Intake(); var ticket = await intake.BeginAsync(record.Manifest, "SYNTHETIC-PILOT", default);
            Assert.Equal(record.Manifest.Id, ticket.ManifestId);
            await Assert.ThrowsAsync<InvalidDataException>(() => intake.PutFileAsync(ticket.Id, "test.dat", new MemoryStream([1]), default));
            Assert.False(await db.Set<DisturbanceRecord>().AnyAsync());
            foreach (var file in record.Manifest.Files)
            {
                await using var input = File.OpenRead(Path.Combine(record.Package.Directory, file.Name));
                await intake.PutFileAsync(ticket.Id, file.Name, input, default);
            }
            // Restart coordinator from persisted staging context, not an in-memory session dictionary.
            var ack = await Intake().CompleteAsync(ticket.Id, default);
            Assert.False(ack.Duplicate); Assert.Equal(record.Package.ManifestSha256, ack.ManifestSha256); Assert.Equal(record.Package.FilesDigest, ack.FilesDigest);
            await using var replayDb = f.Db();
            var restarted = new PilotFileIntake(center, limits, authority, new SqlRecordIngestion(replayDb, authority, TimeProvider.System), TimeProvider.System, true);
            var replay = await restarted.CompleteAsync(ticket.Id, default); Assert.True(replay.Duplicate); Assert.Equal(ack.RecordId, replay.RecordId);
            Assert.Single(await db.Set<DisturbanceRecord>().AsNoTracking().ToListAsync()); Assert.Equal(2, await db.Set<RecordFile>().CountAsync()); Assert.Single(await db.Set<ProcessingOutbox>().ToListAsync());
            var upload = queue.Claim("Upload", TimeSpan.FromMinutes(1))!;
            queue.Acknowledge(upload, new(ack.ManifestId, ack.RecordId, ack.ManifestSha256, ack.FilesDigest, ack.CorrelationId), new DirectSqlFixtureReceiptTrust());
            Assert.Equal("Acknowledged", queue.Status("Upload", 10).Single().State);
            Assert.True(File.Exists(Path.Combine(record.Package.Directory, "test.dat"))); // No local delete before or after ACK without retention release.
            var denied = new PilotFileIntake(center, limits, new PendingMachineRecordAuthority(), new SqlRecordIngestion(db, new PendingMachineRecordAuthority(), TimeProvider.System), TimeProvider.System, true);
            await Assert.ThrowsAsync<Fdc.Identity.Application.OwnerDecisionRequiredException>(() => denied.StatusAsync(ticket.Id, default));
        }
        finally { SqliteConnection.ClearAllPools(); }
    }

    private sealed class DirectSqlFixtureReceiptTrust : ICenterReceiptTrust
    {
        public void DemandVerified(PilotAck ack) { if (ack.CorrelationId != "SYNTHETIC-PILOT") throw new UnauthorizedAccessException(); }
        // Test-only receipt from the actual direct SQL call above; not mTLS/production trust.
    }
    private sealed class OfflineSqlFixtureFiles(string directory) : IIedFileAcquisitionAdapter
    {
        public EvidenceKind EvidenceKind => EvidenceKind.OFFLINE_FIXTURE;
        public CapabilityObservation ProbeCapabilities(CancellationToken ct) => new(CapabilityState.NOT_TESTED, CapabilityState.NOT_TESTED, CapabilityState.SUCCESS, CapabilityState.NOT_TESTED, CapabilityState.NOT_TESTED, EvidenceKind, DateTimeOffset.UtcNow, "OFFLINE_FIXTURE; no MMS/device test.");
        public IReadOnlyList<IedFileMetadata> ListFiles(CancellationToken ct) => Directory.GetFiles(directory)
            .Where(p => Path.GetExtension(p) is ".cfg" or ".dat")
            .Select(p => new IedFileMetadata("/" + Path.GetFileName(p), new FileInfo(p).Length, File.GetLastWriteTimeUtc(p))).ToArray();
        public IedFileMetadata GetFileMetadata(string path, CancellationToken ct) => ListFiles(ct).Single(f => f.RemotePath == path);
        public void DownloadFile(string path, Stream destination, CancellationToken ct, IProgress<long>? progress = null)
        { ct.ThrowIfCancellationRequested(); using var input = File.OpenRead(Path.Combine(directory, AcquisitionSafety.FileName(path))); input.CopyTo(destination); }
        public string GetVendorDiagnostics() => "OFFLINE_FIXTURE, no vendor/device assertion";
        public void Dispose() { }
    }
}
