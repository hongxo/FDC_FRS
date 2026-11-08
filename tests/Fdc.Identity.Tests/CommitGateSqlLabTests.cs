using System.Data;
using System.Diagnostics;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

// Candidate gate protocol tests using committed, non-authoritative probe rows with unique keys, removed in finally.
// No session/user/role/grant is created. Test plan provider does not become a runtime D09 approval.
public sealed class CommitGateSqlLabTests
{
    [SqlLabFact] public async Task Mutation_holds_gate_revoke_waits_then_observes_committed_mutation()
    {
        await using var fixture = await Probe.CreateAsync();
        await using var mutationDb = fixture.Context(); await using var revokeDb = fixture.Context();
        await revokeDb.Database.OpenConnectionAsync();
        var revokeSpid = await revokeDb.Database.SqlQueryRaw<int>("SELECT CONVERT(int,@@SPID) AS [Value]").SingleAsync();
        var actor = Guid.NewGuid(); var entered = NewSignal(); var release = NewSignal();
        var mutation = new SqlAuthorityCommitBoundary(mutationDb, fixture.Plan(actor, false)).CommitAsync(actor, async ct =>
        {
            Assert.Equal(1, await mutationDb.Set<AuthorityVersionRow>().Where(x => x.Id == fixture.Id).Select(x => x.PolicyVersion).SingleAsync(ct));
            entered.SetResult(); await release.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
            await mutationDb.Set<AuthorityVersionRow>().Where(x => x.Id == fixture.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.CatalogVersion, 2), ct);
            return "mutation";
        }, default);
        Task<int>? revoke = null;
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            revoke = new SqlAuthorityCommitBoundary(revokeDb, fixture.Plan(actor, true)).CommitAsync(actor, async ct =>
            {
                var committedMutation = await revokeDb.Set<AuthorityVersionRow>().AsNoTracking().SingleAsync(x => x.Id == fixture.Id, ct);
                Assert.Equal(2, committedMutation.CatalogVersion);
                await revokeDb.Set<AuthorityVersionRow>().Where(x => x.Id == fixture.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.PolicyVersion, 2), ct);
                return 2;
            }, default);
            // Actual SQL WAIT state is the barrier evidence, not a timing/sleep assumption.
            await fixture.RequireWaitingApplicationLock(revokeSpid);
            release.TrySetResult();
            Assert.Equal("mutation", await mutation.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Equal(2, await revoke.WaitAsync(TimeSpan.FromSeconds(10)));
            await fixture.EvidenceAsync("P41-GATE-01", "SQL application lock WAIT observed; revoke read mutation value2 only after mutation commit");
        }
        finally
        {
            release.TrySetResult();
            await Drain(mutation); if (revoke is not null) await Drain(revoke);
        }
    }
    [SqlLabFact] public async Task Committed_revoke_is_read_before_next_protected_mutation()
    {
        await using var fixture = await Probe.CreateAsync();
        await using var revokeDb = fixture.Context(); await using var mutationDb = fixture.Context(); var actor = Guid.NewGuid();
        await new SqlAuthorityCommitBoundary(revokeDb, fixture.Plan(actor, true)).CommitAsync(actor, async ct =>
        {
            await revokeDb.Set<AuthorityVersionRow>().Where(x => x.Id == fixture.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.PolicyVersion, 2), ct); return 1;
        }, default);
        var allowed = await new SqlAuthorityCommitBoundary(mutationDb, fixture.Plan(actor, false)).CommitAsync(actor, async ct =>
        {
            var version = await mutationDb.Set<AuthorityVersionRow>().AsNoTracking().Where(x => x.Id == fixture.Id).Select(x => x.PolicyVersion).SingleAsync(ct);
            if (version != 1) return false;
            await mutationDb.Set<AuthorityVersionRow>().Where(x => x.Id == fixture.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.CatalogVersion, 2), ct); return true;
        }, default);
        Assert.False(allowed);
        await using var inspect = fixture.Context();
        Assert.Equal(1, await inspect.Set<AuthorityVersionRow>().Where(x => x.Id == fixture.Id).Select(x => x.CatalogVersion).SingleAsync());
        await fixture.EvidenceAsync("P41-GATE-02", "Committed probe revoke version2 read inside next gate; mutation denied; mutation counter stayed1");
    }
    [SqlLabFact] public async Task Gate_timeout_never_invokes_protected_callback()
    {
        await using var fixture = await Probe.CreateAsync();
        await using var holderDb = fixture.Context(); await using var waiterDb = fixture.Context(); var actor = Guid.NewGuid();
        var entered = NewSignal(); var release = NewSignal(); var called = false;
        var holder = new SqlAuthorityCommitBoundary(holderDb, fixture.Plan(actor, true)).CommitAsync(actor, async ct => { entered.SetResult(); await release.Task.WaitAsync(TimeSpan.FromSeconds(10), ct); return 1; }, default);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Assert.ThrowsAsync<AuthorityUnavailableException>(() => new SqlAuthorityCommitBoundary(waiterDb, fixture.Plan(actor, true, 200)).CommitAsync(actor, _ => { called = true; return Task.FromResult(1); }, default));
            Assert.False(called); Assert.Null(waiterDb.Database.CurrentTransaction);
            await fixture.EvidenceAsync("P41-GATE-03", "Incompatible lock timed out; callback false; transaction disposed");
        }
        finally { release.TrySetResult(); await Drain(holder); }
    }
    [SqlLabFact] public async Task Callback_failure_rolls_back_database_and_discards_tracked_changes()
    {
        await using var fixture = await Probe.CreateAsync(); await using var db = fixture.Context(); var actor = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SqlAuthorityCommitBoundary(db, fixture.Plan(actor, true)).CommitAsync<int>(actor, async ct =>
        {
            var row = await db.Set<AuthorityVersionRow>().SingleAsync(x => x.Id == fixture.Id, ct); row.CatalogVersion = 2;
            await db.SaveChangesAsync(ct); throw new InvalidOperationException("Synthetic callback failure.");
        }, default));
        Assert.Empty(db.ChangeTracker.Entries()); Assert.Null(db.Database.CurrentTransaction);
        await using var inspect = fixture.Context(); Assert.Equal(1, await inspect.Set<AuthorityVersionRow>().Where(x => x.Id == fixture.Id).Select(x => x.CatalogVersion).SingleAsync());
        await fixture.EvidenceAsync("P41-GATE-04", "Flushed probe mutation rolled back; database counter1; tracker cleared");
    }
    [SqlLabFact] public async Task Boundary_refuses_nested_transactions()
    {
        await using var fixture = await Probe.CreateAsync(); await using var db = fixture.Context(); await using var outer = await db.Database.BeginTransactionAsync();
        var actor = Guid.NewGuid(); var called = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SqlAuthorityCommitBoundary(db, fixture.Plan(actor, true)).CommitAsync(actor, _ => { called = true; return Task.FromResult(1); }, default));
        Assert.False(called); await outer.RollbackAsync();
    }
    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static async Task Drain(Task task) { try { await task.WaitAsync(TimeSpan.FromSeconds(12)); } catch { /* Assertion path retains original test failure. */ } }
    private sealed class PlanProvider(SqlCommitGatePlan plan) : ISqlCommitGatePlanProvider
    { public Task<SqlCommitGatePlan> ResolveAsync(Guid actorId, CancellationToken ct) => Task.FromResult(plan); }
    private sealed class Probe(string connection) : IAsyncDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        private string Key => "GateProbe-" + Id.ToString("N");
        private string Prefix => "FdcTest." + Id.ToString("N");
        public IamDbContext Context() => new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
        public ISqlCommitGatePlanProvider Plan(Guid actor, bool exclusive, int timeout = 5000) =>
            new PlanProvider(new(Prefix, false, [new(actor, exclusive)], timeout, IsolationLevel.ReadCommitted));
        public static async Task<Probe> CreateAsync()
        {
            var connection = Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION") ?? throw new InvalidOperationException("Lab connection required.");
            var options = new SqlConnectionStringBuilder(connection);
            if (options.DataSource != "lpc:." || options.InitialCatalog != "FdcIam_DevelopmentLab") throw new InvalidOperationException("Unexpected target.");
            var fixture = new Probe(connection); await using var db = fixture.Context();
            var valid = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND DB_NAME()=N'FdcIam_DevelopmentLab' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Environment' AND CONVERT(nvarchar(128),value)=N'DevelopmentLab') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
            Assert.Equal(1, valid);
            db.Add(new AuthorityVersionRow { Id = fixture.Id, CreatedAt = DateTimeOffset.UtcNow, AuthorityKey = fixture.Key, PolicyConfigured = false, RestoreEpoch = Guid.NewGuid() });
            await db.SaveChangesAsync(); return fixture;
        }
        public async Task RequireWaitingApplicationLock(int sessionId)
        {
            await using var db = Context(); var timer = Stopwatch.StartNew();
            while (timer.Elapsed < TimeSpan.FromSeconds(4))
            {
                var waiting = await db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.dm_tran_locks WHERE request_session_id={sessionId} AND resource_type=N'APPLICATION' AND request_status=N'WAIT'").SingleAsync();
                if (waiting > 0) return;
                await Task.Delay(20); // Polling real lock state; elapsed sleep is never evidence of ordering.
            }
            throw new Xunit.Sdk.XunitException("Expected SQL APPLICATION WAIT state not observed.");
        }
        public async Task EvidenceAsync(string id, string result)
        {
            var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../docs/auth-d12-backend-implementation/phase-4-1/evidence"));
            Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(Path.Combine(directory, id + ".json"), System.Text.Json.JsonSerializer.Serialize(new { TestId = id,
                SourceCommit = Environment.GetEnvironmentVariable("FDC_IAM_SOURCE_SHA"), TimestampUtc = DateTimeOffset.UtcNow, Result = "PASS", Observation = result,
                Scope = "Candidate gate and synthetic non-authoritative probe; not full D09/IAM acceptance" }));
        }
        public async ValueTask DisposeAsync()
        {
            await using var db = Context();
            await db.Set<AuthorityVersionRow>().Where(x => x.Id == Id && x.AuthorityKey == Key && !x.PolicyConfigured).ExecuteDeleteAsync();
            Assert.False(await db.Set<AuthorityVersionRow>().AnyAsync(x => x.Id == Id));
        }
    }
}
