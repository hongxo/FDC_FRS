using System.Collections.Immutable;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

// SQL DML/integrity tests with explicitly test-only authority/gate fixtures. Not runtime policy or D09 race acceptance.
[Collection("IAM database lab")]
public sealed class RoleCrudSqlLabTests
{
    [SqlProfileFact] public async Task P4_scoped_user_query_profile_observation_only()
    {
        await using var db = Context(); await Target(db); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await Fixtures(db);
            var visible = (await db.Set<UserRow>().Select(x => x.Id).ToListAsync()).ToImmutableHashSet();
            var scope = new ManagementReadScope(visible, [], [], [], [], [], ImmutableHashSet<string>.Empty);
            var queries = new SqlManagementQueries(db);
            for (var i = 0; i < 10; i++) Assert.Equal(3, (await queries.UsersAsync(scope, null, null, new(0, 50), default)).Items.Count);
            var durations = new List<double>(); var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpuBefore = process.TotalProcessorTime; var started = DateTimeOffset.UtcNow;
            var total = System.Diagnostics.Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
            {
                var sample = System.Diagnostics.Stopwatch.StartNew();
                Assert.Equal(3, (await queries.UsersAsync(scope, null, null, new(0, 50), default)).Items.Count);
                durations.Add(sample.Elapsed.TotalMilliseconds);
            }
            total.Stop(); process.Refresh(); var ordered = durations.Order().ToArray();
            double P(double percentile) => ordered[(int)Math.Ceiling(percentile * ordered.Length) - 1];
            var observation = new { TestId = "P4-PERF-01", SourceCommit = Environment.GetEnvironmentVariable("FDC_IAM_SOURCE_SHA"), StartedUtc = started,
                EndedUtc = DateTimeOffset.UtcNow, Workload = "100 sequential scoped SQL user-list reads; 10 warmup; 3 synthetic users; one connection/transaction; no HTTP/login/authority-resolution cost",
                P50Milliseconds = P(.50), P95Milliseconds = P(.95), P99Milliseconds = P(.99), ThroughputPerSecond = 100 / total.Elapsed.TotalSeconds,
                ErrorRate = 0.0, ClientCpuMilliseconds = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds, ClientWorkingSetBytes = process.WorkingSet64,
                ServerCpu = "NOT_MEASURED", DiskNetworkPoolDeadlocks = "NOT_MEASURED", AcceptanceThreshold = "PENDING", SamplesMilliseconds = durations };
            var evidence = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../docs/auth-d12-backend-implementation/phase-4/evidence"));
            Directory.CreateDirectory(evidence);
            await File.WriteAllTextAsync(Path.Combine(evidence, "scoped-query-profile.json"), System.Text.Json.JsonSerializer.Serialize(observation, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        }
        finally { await tx.RollbackAsync(); }
    }
    [SqlLabFact] public async Task P4_D001_request_target_must_match_immutable_payload_target()
    {
        await using var db = Context(); await Target(db); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var auth = await Fixtures(db); var service = new SqlRoleCrudService(db, new FixtureBoundary(), auth, TimeProvider.System);
            var now = DateTimeOffset.UtcNow;
            var intended = new RoleRow { Id = Guid.NewGuid(), CreatedAt = now, RoleCode = "intended", RoleName = "Intended" };
            var unrelated = new RoleRow { Id = Guid.NewGuid(), CreatedAt = now, RoleCode = "unrelated", RoleName = "Unrelated" };
            db.AddRange(intended, unrelated); await db.SaveChangesAsync();
            var actor = await auth.DemandAsync(RoleMutationOperation.SubmitRequest, default);
            var payload = new RoleChangePayload(RoleChangeKind.Rename, intended.Id, null, "Changed", 1, 1, [], [], "synthetic target-binding test");
            var proof = new RoleChangeRequest(actor.PersonKey, payload, 1, now.AddMinutes(5));
            // Insert-only malformed fixture: the existing update trigger intentionally cannot repair/prevent this initial mismatch.
            var request = new RoleChangeRequestRow { Id = proof.Id, CreatedAt = now, RequesterUserId = actor.UserId,
                RoleId = unrelated.Id, Revision = 1, PayloadJson = proof.PayloadJson, Digest = proof.Digest,
                IdempotencyKey = "target-binding-test", BaseRoleVersion = 1, PolicyVersion = 1, CatalogVersion = 1, ExpiresAt = proof.ExpiresAt };
            db.Add(request); await db.SaveChangesAsync(); await Approve(db, auth, request.Id);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyReviewedAsync(request.Id, 1, request.Digest, request.RowVersion, default));
            Assert.Equal("Unrelated", await db.Set<RoleRow>().AsNoTracking().Where(x => x.Id == unrelated.Id).Select(x => x.RoleName).SingleAsync());
            Assert.Equal(0, await db.Set<RoleHistoryRow>().CountAsync());
        }
        finally { await tx.RollbackAsync(); }
    }
    [SqlLabFact] public async Task P4_D002_idempotent_apply_result_uses_original_history_version()
    {
        await using var db = Context(); await Target(db); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var auth = await Fixtures(db); var service = new SqlRoleCrudService(db, new FixtureBoundary(), auth, TimeProvider.System);
            var created = await service.SubmitAsync(new(RoleChangeKind.Create, null, "replay-role", "Original", 0, 1, [], [], "synthetic"), "replay-create", default);
            var first = await Apply(db, service, auth, created);
            await Apply(db, service, auth, await service.SubmitAsync(new(RoleChangeKind.Rename, first.RoleId, null, "Later revision", 1, 1, [], [], "synthetic"), "later-rename", default));
            var request = await db.Set<RoleChangeRequestRow>().SingleAsync(x => x.Id == created.RequestId);
            var replay = await service.ApplyReviewedAsync(request.Id, request.Revision, request.Digest, request.RowVersion, default);
            Assert.True(replay.Replayed); Assert.Equal(first.RoleVersion, replay.RoleVersion);
            Assert.Equal(2, await db.Set<RoleHistoryRow>().CountAsync());
        }
        finally { await tx.RollbackAsync(); }
    }
    [SqlLabFact] public async Task Real_sql_role_create_rename_activate_permissions_clone_deactivate_and_replay()
    {
        await using var db = Context(); await Target(db); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var auth = await Fixtures(db); var service = new SqlRoleCrudService(db, new FixtureBoundary(), auth, TimeProvider.System);
            var payload = new RoleChangePayload(RoleChangeKind.Create, null, "sql-custom-role", "Custom SQL Role", 0, 1, [], [], "synthetic reason");
            var accepted = await service.SubmitAsync(payload, "create-request", default);
            Assert.Equal("Pending", accepted.ApprovalStatus); Assert.Equal("NotApplied", accepted.EffectiveStatus);
            Assert.False(await db.Set<RoleRow>().AnyAsync());
            Assert.True((await service.SubmitAsync(payload, "create-request", default)).Replayed);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitAsync(payload with { Name = "different" }, "create-request", default));
            var result = await Apply(db, service, auth, accepted);
            var role = await db.Set<RoleRow>().SingleAsync(); Assert.False(role.IsActive); Assert.False(role.IsSystemSeed);
            var row = await db.Set<RoleChangeRequestRow>().SingleAsync(x => x.Id == accepted.RequestId);
            Assert.True((await service.ApplyReviewedAsync(row.Id, row.Revision, row.Digest, row.RowVersion, default)).Replayed);
            Assert.Equal(1, await db.Set<RoleRow>().CountAsync());
            async Task<RoleApplyResult> Change(RoleChangeKind kind, string key, string? name = null, ImmutableArray<string> add = default)
            {
                var request = await service.SubmitAsync(new(kind, role.Id, null, name, role.Version, 1, add.IsDefault ? [] : add, [], "synthetic reason"), key, default);
                return await Apply(db, service, auth, request);
            }
            await Change(RoleChangeKind.Rename, "rename-request", "Renamed SQL Role"); Assert.Equal("Renamed SQL Role", role.RoleName);
            await Change(RoleChangeKind.Activate, "activate-request"); Assert.True(role.IsActive);
            await Change(RoleChangeKind.PermissionDiff, "permission-request", add: ["station.view"]); Assert.Equal(1, await db.Set<RolePermissionRow>().CountAsync());
            var clone = await service.SubmitAsync(new(RoleChangeKind.Clone, role.Id, "cloned-sql-role", "Clone", role.Version, 1, [], [], "synthetic reason"), "clone-request", default);
            var cloneResult = await Apply(db, service, auth, clone);
            var clonedRole = await db.Set<RoleRow>().SingleAsync(r => r.Id == cloneResult.RoleId);
            Assert.False(clonedRole.IsActive); Assert.NotEqual(role.Id, clonedRole.Id);
            Assert.Equal(1, await db.Set<RolePermissionRow>().CountAsync(x => x.RoleId == clonedRole.Id));
            Assert.False(await db.Set<UserRoleRow>().AnyAsync(x => x.RoleId == clonedRole.Id));
            var targetPerson = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, PersonKey = "sql-test-target" };
            var target = new UserRow { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, PersonReferenceId = targetPerson.Id, NormalizedUsername = "SQL-TEST-TARGET", Active = true };
            db.AddRange(targetPerson, target, new UserRoleRow { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, UserId = target.Id,
                RoleId = role.Id, Active = true, ValidFrom = DateTimeOffset.UtcNow.AddMinutes(-1), ValidUntil = DateTimeOffset.UtcNow.AddHours(1), ApprovalReference = "rollback-only-fixture" });
            await db.SaveChangesAsync();
            await Change(RoleChangeKind.Deactivate, "deactivate-request"); Assert.False(role.IsActive);
            Assert.Equal(2, target.GrantVersion);
            Assert.Equal(6, await db.Set<RoleHistoryRow>().CountAsync());
            Assert.Equal(12, await db.Set<SecurityEventRow>().CountAsync()); Assert.Equal(12, await db.Set<AuditOutboxRow>().CountAsync());
            Assert.Equal(7, (await db.Set<AuthorityVersionRow>().SingleAsync()).PolicyVersion);
        }
        finally { await tx.RollbackAsync(); }
        Assert.False(await db.Set<RoleRow>().AsNoTracking().AnyAsync()); Assert.False(await db.Set<UserRow>().AsNoTracking().AnyAsync());
    }
    [SqlLabFact] public async Task Real_sql_stale_role_or_request_version_and_duplicate_code_cannot_apply()
    {
        await using var db = Context(); await Target(db); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var auth = await Fixtures(db); var service = new SqlRoleCrudService(db, new FixtureBoundary(), auth, TimeProvider.System);
            var create = new RoleChangePayload(RoleChangeKind.Create, null, "unique-role", "Original", 0, 1, [], [], "synthetic");
            await Apply(db, service, auth, await service.SubmitAsync(create, "initial-create", default));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitAsync(create, "duplicate-create", default));
            var role = await db.Set<RoleRow>().SingleAsync();
            var rename = await service.SubmitAsync(new(RoleChangeKind.Rename, role.Id, null, "Must not replace", role.Version, 1, [], [], "synthetic"), "stale-rename", default);
            await Approve(db, auth, rename.RequestId);
            var request = await db.Set<RoleChangeRequestRow>().SingleAsync(x => x.Id == rename.RequestId);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyReviewedAsync(request.Id, request.Revision, request.Digest, new byte[8], default));
            role.Version++; await db.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyReviewedAsync(request.Id, request.Revision, request.Digest, request.RowVersion, default));
            Assert.Equal("Original", role.RoleName); Assert.Equal("NotApplied", request.EffectiveStatus);
            Assert.Equal(1, await db.Set<RoleHistoryRow>().CountAsync()); Assert.Equal(2, (await db.Set<AuthorityVersionRow>().SingleAsync()).PolicyVersion);
        }
        finally { await tx.RollbackAsync(); }
        Assert.False(await db.Set<RoleRow>().AsNoTracking().AnyAsync());
    }
    [SqlLabFact] public async Task Real_sql_outbox_failure_rolls_back_effective_role_and_success_audit()
    {
        await using var db = Context(); await Target(db); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var auth = await Fixtures(db); var service = new SqlRoleCrudService(db, new FixtureBoundary(), auth, TimeProvider.System);
            var accepted = await service.SubmitAsync(new(RoleChangeKind.Create, null, "fail-role", "Must not exist", 0, 1, [], [], "synthetic reason"), "failure-request", default);
            await Approve(db, auth, accepted.RequestId);
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE [ops].[AuditOutbox] WITH NOCHECK ADD CONSTRAINT [CK_Test_RejectNewOutbox] CHECK ([Fence] < 0);");
            var row = await db.Set<RoleChangeRequestRow>().SingleAsync();
            await Assert.ThrowsAsync<DbUpdateException>(() => service.ApplyReviewedAsync(row.Id, row.Revision, row.Digest, row.RowVersion, default));
            Assert.False(await db.Set<RoleRow>().AsNoTracking().AnyAsync());
            Assert.Equal("NotApplied", await db.Set<RoleChangeRequestRow>().AsNoTracking().Select(x => x.EffectiveStatus).SingleAsync());
            Assert.Equal(0, await db.Set<SecurityEventRow>().AsNoTracking().CountAsync(x => x.EventCode == "iam.role.apply"));
            Assert.Equal(1, await db.Set<AuthorityVersionRow>().AsNoTracking().Select(x => x.PolicyVersion).SingleAsync());
        }
        finally { await tx.RollbackAsync(); }
        Assert.False(await db.Set<RoleRow>().AsNoTracking().AnyAsync());
    }
    private static async Task<RoleApplyResult> Apply(IamDbContext db, SqlRoleCrudService service, FixtureAuthority auth, RoleRequestAccepted accepted)
    {
        await Approve(db, auth, accepted.RequestId);
        var row = await db.Set<RoleChangeRequestRow>().SingleAsync(x => x.Id == accepted.RequestId);
        return await service.ApplyReviewedAsync(row.Id, row.Revision, row.Digest, row.RowVersion, default);
    }
    private static async Task Approve(IamDbContext db, FixtureAuthority auth, Guid id)
    {
        var row = await db.Set<RoleChangeRequestRow>().SingleAsync(x => x.Id == id); row.ApprovalStatus = "Approved";
        foreach (var user in new[] { auth.Reviewer, auth.Approver }) db.Add(new RoleChangeApprovalRow { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow,
            RequestId = row.Id, Revision = row.Revision, ApproverUserId = user.Id, ApproverPersonReferenceId = user.PersonReferenceId,
            RequestDigest = row.Digest, Decision = user.Id == auth.Reviewer.Id ? "Reviewed" : "Approved" });
        await db.SaveChangesAsync();
    }
    private static async Task<FixtureAuthority> Fixtures(IamDbContext db)
    {
        var now = DateTimeOffset.UtcNow; var users = new List<UserRow>();
        foreach (var suffix in new[] { "requester", "reviewer", "approver" })
        {
            var person = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = now, PersonKey = "sql-test-" + suffix };
            var user = new UserRow { Id = Guid.NewGuid(), CreatedAt = now, PersonReferenceId = person.Id, NormalizedUsername = "SQL-TEST-" + suffix, Active = true };
            db.AddRange(person, user); users.Add(user);
        }
        db.Add(new AuthorityVersionRow { Id = Guid.NewGuid(), CreatedAt = now, PolicyConfigured = true, RestoreEpoch = Guid.NewGuid() });
        db.Add(new PermissionRow { Id = Guid.NewGuid(), CreatedAt = now, PermissionCode = "station.view", Module = "synthetic", Action = "view", ScopeRequirement = "Station",
            SoDPolicy = "test", RiskLevel = "test", IsApproved = true, IsImplemented = true, CatalogVersion = 1, ApprovalReference = "rollback-only-fixture" });
        await db.SaveChangesAsync(); return new(db, users[0], users[1], users[2]);
    }
    private sealed class FixtureAuthority(IamDbContext db, UserRow requester, UserRow reviewer, UserRow approver) : IRoleMutationAuthority
    {
        public UserRow Reviewer => reviewer; public UserRow Approver => approver;
        public async Task<RoleMutationActor> DemandAsync(RoleMutationOperation operation, CancellationToken ct)
        {
            var version = await db.Set<AuthorityVersionRow>().SingleAsync(ct);
            var user = operation == RoleMutationOperation.SubmitRequest ? requester : approver;
            var person = await db.Set<PersonReferenceRow>().SingleAsync(p => p.Id == user.PersonReferenceId, ct);
            return new(user.Id, person.PersonKey, version.PolicyVersion, version.CatalogVersion, DateTimeOffset.UtcNow.AddMinutes(5), ImmutableHashSet.Create(StringComparer.Ordinal, "station.view"), "synthetic-correlation");
        }
        public async Task DemandReviewedRequestAsync(RoleMutationActor actor, RoleChangeRequestRow row, CancellationToken ct)
        {
            var approvals = await db.Set<RoleChangeApprovalRow>().Where(x => x.RequestId == row.Id && x.Revision == row.Revision && x.RequestDigest == row.Digest).ToListAsync(ct);
            Assert.Equal(2, approvals.Select(x => x.ApproverPersonReferenceId).Distinct().Count());
            Assert.DoesNotContain(approvals, x => x.ApproverPersonReferenceId == requester.PersonReferenceId);
            Assert.Equal(approver.Id, actor.UserId);
        }
    }
    private sealed class FixtureBoundary : IAuthorizedCommitBoundary
    {
        public Task<T> CommitAsync<T>(Guid actorUserId, Func<CancellationToken, Task<T>> action, CancellationToken ct) => action(ct);
    }
    private static IamDbContext Context()
    {
        var connection = Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION") ?? throw new InvalidOperationException("Lab connection required.");
        var options = new SqlConnectionStringBuilder(connection);
        if (options.DataSource != "lpc:." || options.InitialCatalog != "FdcIam_DevelopmentLab") throw new InvalidOperationException("Unexpected lab.");
        return new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
    }
    private static async Task Target(IamDbContext db)
    {
        var ok = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND DB_NAME()=N'FdcIam_DevelopmentLab' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) THEN 1 ELSE 0 END AS [Value]").SingleAsync();
        Assert.Equal(1, ok); Assert.False(await db.Set<UserRow>().AnyAsync()); Assert.False(await db.Set<RoleRow>().AnyAsync());
    }
}

public sealed class SqlProfileFactAttribute : FactAttribute
{
    public SqlProfileFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("FDC_IAM_CONFIRMED_LAB") != "FdcIam_DevelopmentLab" || Environment.GetEnvironmentVariable("FDC_IAM_RUN_PROFILE") != "1")
            Skip = "Explicit local lab and observation-only profile required.";
    }
}
