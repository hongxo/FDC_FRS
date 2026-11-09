using Fdc.Collector.Pilot;
using Fdc.Core;

namespace Fdc.Collector.Pilot.Tests;

public sealed class QueueAndAcquisitionTests
{
    [Fact] public void Empty_schema_rollback_keeps_legacy_store_and_reapply_is_idempotent()
    {
        using var f = new PilotFiles(); var legacy = new Store(f.Root);
        var q = new PilotTransferQueue(f.Root, TimeProvider.System); q.RollbackEmptyPilotSchema();
        Assert.Empty(legacy.Records());
        var reapplied = new PilotTransferQueue(f.Root, TimeProvider.System);
        Assert.Empty(reapplied.Status("Download", 10)); Assert.Empty(reapplied.Status("Upload", 10));
    }
    [Fact] public void Rollback_with_queued_data_is_denied_without_deleting_it()
    {
        using var f = new PilotFiles(); var q = new PilotTransferQueue(f.Root, TimeProvider.System);
        var id = q.Discover("TEST", "IED", "/event.cfg", Guid.NewGuid(), 2);
        Assert.Throws<InvalidOperationException>(() => q.RollbackEmptyPilotSchema());
        Assert.Equal(id, q.Status("Download", 10).Single().Id);
    }
    [Fact] public void Existing_store_coexists_and_expired_lease_is_fenced_after_restart()
    {
        using var f = new PilotFiles(); var clock = new MutableClock();
        var legacy = new Store(f.Root); var q = new PilotTransferQueue(f.Root, clock);
        var id = q.Discover("TEST", "IED", "/event.cfg", Guid.NewGuid(), 3);
        var first = q.Claim("Download", TimeSpan.FromSeconds(2))!;
        Assert.Null(q.Claim("Download", TimeSpan.FromSeconds(2)));
        clock.Current += TimeSpan.FromSeconds(3);
        var restarted = new PilotTransferQueue(f.Root, clock); var second = restarted.Claim("Download", TimeSpan.FromSeconds(2))!;
        Assert.Equal(id, second.Id); Assert.NotEqual(first.Token, second.Token); Assert.Equal(2, second.Attempt);
        Assert.Throws<InvalidOperationException>(() => q.Retry(first, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), "OLD_WORKER"));
        restarted.Retry(second, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), "INTERRUPTED");
        Assert.Empty(legacy.Records()); // Pilot migration never rewrites existing Collector records.
    }
    [Fact] public void Retry_is_finite_backoff_and_no_sensitive_error_payload()
    {
        using var f = new PilotFiles(); var clock = new MutableClock(); var q = new PilotTransferQueue(f.Root, clock);
        q.Discover("TEST", "IED", "/event.cfg", Guid.NewGuid(), 2);
        var a = q.Claim("Download", TimeSpan.FromMinutes(1))!;
        Assert.Throws<ArgumentException>(() => q.Retry(a, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), "RAW connection secret"));
        q.Retry(a, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), "IO_ERROR"); Assert.Null(q.Claim("Download", TimeSpan.FromSeconds(5)));
        clock.Current += TimeSpan.FromSeconds(1); var b = q.Claim("Download", TimeSpan.FromSeconds(5))!;
        q.Retry(b, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), "IO_ERROR"); clock.Current += TimeSpan.FromDays(1);
        Assert.Null(q.Claim("Download", TimeSpan.FromSeconds(5))); Assert.Equal("Failed", q.Status("Download", 10).Single().State);
    }
    [Fact] public async Task Concurrent_claims_do_not_duplicate_a_lease()
    {
        using var f = new PilotFiles(); var q = new PilotTransferQueue(f.Root, TimeProvider.System);
        q.Discover("TEST", "IED", "/event.cfg", Guid.NewGuid(), 2);
        var claims = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => q.Claim("Download", TimeSpan.FromMinutes(1)))));
        Assert.Single(claims, c => c is not null);
    }
    [Fact] public void Offline_acquisition_preserves_bytes_deduplicates_content_and_requires_verified_ack()
    {
        using var f = new PilotFiles(); var q = new PilotTransferQueue(f.Root, TimeProvider.System);
        using var adapter = new OfflineFiles(f.Source);
        var engine = new PilotAcquisition(q, Path.Combine(f.Root, "spool"), PilotFiles.Limits, "UTF-8", 1024, 1048576, TimeProvider.System, PilotFiles.QueuePolicy);
        var first = engine.Collect(adapter, f.Target, 3, default).Single();
        var second = engine.Collect(adapter, f.Target, 3, default).Single();
        Assert.Equal(first.Manifest.Id, second.Manifest.Id); Assert.Equal(first.Package.ManifestJson, second.Package.ManifestJson);
        Assert.Single(q.Status("Upload", 10)); Assert.Equal(2, q.Status("Download", 10).Count);
        Assert.Equal(File.ReadAllBytes(f.Cfg), File.ReadAllBytes(Path.Combine(first.Package.Directory, "event.cfg")));
        Assert.Equal(File.ReadAllBytes(f.Dat), File.ReadAllBytes(Path.Combine(first.Package.Directory, "event.dat")));
        Assert.Equal(EvidenceKind.OFFLINE_FIXTURE, first.EvidenceKind);
        var lease = q.Claim("Upload", TimeSpan.FromMinutes(1))!;
        var ack = new PilotAck(lease.Id, Guid.NewGuid(), first.Package.ManifestSha256, first.Package.FilesDigest, "SYNTHETIC-ACK");
        Assert.Throws<InvalidOperationException>(() => q.Acknowledge(lease, ack, new PendingCenterReceiptTrust()));
        Assert.Throws<InvalidDataException>(() => q.Acknowledge(lease, ack with { FilesDigest = new string('0', 64) }, new FixtureReceiptTrust()));
        q.Acknowledge(lease, ack, new FixtureReceiptTrust());
        Assert.Equal("Acknowledged", q.Status("Upload", 10).Single().State); Assert.True(File.Exists(Path.Combine(first.Package.Directory, "event.dat")));
        Assert.Null(q.Claim("Upload", TimeSpan.FromMinutes(1)));
    }
    [Fact] public void Partial_file_and_low_disk_never_promote_or_acknowledge()
    {
        using var f = new PilotFiles(); var q = new PilotTransferQueue(f.Root, TimeProvider.System);
        using var partial = new OfflineFiles(f.Source, true);
        var spool = Path.Combine(f.Root, "spool");
        var engine = new PilotAcquisition(q, spool, PilotFiles.Limits, "UTF-8", 1024, 1048576, TimeProvider.System, PilotFiles.QueuePolicy);
        Assert.Throws<InvalidDataException>(() => engine.Collect(partial, f.Target, 2, default));
        Assert.Empty(q.Status("Upload", 10)); Assert.True(Directory.GetFiles(Path.Combine(spool, "staging"), "*", SearchOption.AllDirectories).Length > 0);
        var guarded = new PilotAcquisition(q, spool, PilotFiles.Limits, "UTF-8", 1024, 1048576, TimeProvider.System, PilotFiles.QueuePolicy, () => 1);
        Assert.Throws<IOException>(() => guarded.Collect(partial, f.Target, 2, default)); Assert.Empty(q.Status("Upload", 10));
    }
    [Fact] public void Reused_filename_changed_bytes_creates_new_content_record()
    {
        using var f = new PilotFiles(); var q = new PilotTransferQueue(f.Root, TimeProvider.System); using var adapter = new OfflineFiles(f.Source);
        var engine = new PilotAcquisition(q, Path.Combine(f.Root, "spool"), PilotFiles.Limits, "UTF-8", 1024, 1048576, TimeProvider.System, PilotFiles.QueuePolicy);
        var first = engine.Collect(adapter, f.Target, 3, default).Single();
        File.WriteAllText(f.Dat, File.ReadAllText(f.Dat).Replace("2,1000,2,1", "2,1000,9,1"));
        var changed = engine.Collect(adapter, f.Target, 3, default).Single(); Assert.NotEqual(first.Manifest.Id, changed.Manifest.Id);
    }
    private sealed class FixtureReceiptTrust : ICenterReceiptTrust { public void DemandVerified(PilotAck ack) { } } // Only tests; never host DI or real mTLS evidence.
}
