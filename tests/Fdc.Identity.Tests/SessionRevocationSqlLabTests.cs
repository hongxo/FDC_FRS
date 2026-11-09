using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Identity.Tests;

// Real SQL DML tests in a rollback-only fixture. Policy and outer boundary are TEST-ONLY, not live approval/MFA.
[Collection("IAM database lab")]
public sealed class SessionRevocationSqlLabTests
{
    [SqlLabFact] public async Task Credential_reader_and_authentication_complete_isolated_sql_session_and_rollback()
    {
        await using var db = await Context();
        var reader = new SqlPasswordCredentialReader(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync("FIXTURE-CREDENTIAL", default));
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var person = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = now, PersonKey = "fixture-credential" };
            var user = new UserRow { Id = Guid.NewGuid(), CreatedAt = now, PersonReferenceId = person.Id,
                NormalizedUsername = "FIXTURE-CREDENTIAL", Active = true };
            var proofs = new PasswordProofs(1, 256);
            var input = OpaqueProofs.Create();
            var hash = proofs.Hash(input);
            db.AddRange(person, user, new PasswordCredentialRow { Id = Guid.NewGuid(), CreatedAt = now,
                UserId = user.Id, EncodedHash = hash, AlgorithmVersion = "fixture" });
            await db.SaveChangesAsync();
            Assert.Null(await reader.ReadAsync("FIXTURE-NOT-PRESENT", default));
            var credential = await reader.ReadAsync(user.NormalizedUsername, default);
            Assert.NotNull(credential);
            Assert.Equal(user.Id, credential.UserId);
            Assert.True(credential.Active);
            Assert.NotEqual(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed, proofs.Verify(credential.EncodedHash, input));
            Assert.DoesNotContain(hash, credential.ToString());
            Assert.False(await db.Set<SessionRow>().AnyAsync());
            var login = new AuthenticationService(new IsolatedLoginPolicy(), new IsolatedSqlLogin(db, reader),
                proofs, TimeProvider.System);
            var rejected = await login.LoginAsync(user.NormalizedUsername, OpaqueProofs.Create(), "fixture", default);
            Assert.Equal(LoginOutcome.Rejected, rejected.Outcome);
            Assert.Null(rejected.Proof);
            Assert.False(await db.Set<SessionRow>().AnyAsync());
            var result = await login.LoginAsync(user.NormalizedUsername, input, "fixture", default);
            Assert.Equal(LoginOutcome.SessionCreated, result.Outcome);
            Assert.Equal(OpaqueProofs.Digest(result.Proof!, "session"), await db.Set<SessionRow>()
                .Select(row => row.HandleDigest).SingleAsync());
            Assert.Equal(1, await db.Set<AuditOutboxRow>().CountAsync());
        }
        finally { await tx.RollbackAsync(); }
        Assert.False(await db.Set<PasswordCredentialRow>().AsNoTracking().AnyAsync());
        Assert.False(await db.Set<SessionRow>().AsNoTracking().AnyAsync());
        Assert.False(await db.Set<AuditOutboxRow>().AsNoTracking().AnyAsync());
    }

    // Explicit synthetic policy and rollback-only SQL adapter. Neither is registered in the product host.
    private sealed class IsolatedLoginPolicy : IAuthenticationPolicyProvider
    {
        public Task<AuthenticationPolicy> GetAsync(CancellationToken ct) => Task.FromResult(new AuthenticationPolicy(
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), 1, 1, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), "fixture"));
    }

    private sealed class IsolatedSqlLogin(IamDbContext db, SqlPasswordCredentialReader reader) : ILoginTransactionStore, ILoginTransaction
    {
        public Task<LoginResult> ExecuteAsync(Func<ILoginTransaction, CancellationToken, Task<LoginResult>> action, CancellationToken ct) =>
            db.Database.CurrentTransaction is not null ? action(this, ct) : throw new InvalidOperationException("Fixture transaction required.");
        public async Task<CredentialCandidate?> ReadForUpdateAsync(string username, CancellationToken ct) =>
            await reader.ReadAsync(username, ct) is { } row ? new(row.UserId, row.Active, row.EncodedHash, false) : null;
        public Task<bool> IsSourceLimitedAsync(string source, AuthenticationPolicy policy, CancellationToken ct) => Task.FromResult(false);
        public Task RecordFailureAsync(Guid? userId, string source, AuthenticationPolicy policy, CancellationToken ct) => Task.CompletedTask;
        public async Task<LoginResult> CompletePasswordProofAsync(SessionIssue proposed, AuthenticationPolicy policy, string? upgradedHash, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;
            var epoch = Guid.NewGuid();
            var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = proposed.UserId,
                EventCode = "fixture.login", OutcomeCode = "fixture", CorrelationId = "isolated-login", PolicyVersion = 1, RestoreEpoch = epoch };
            db.AddRange(new SessionRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = proposed.UserId,
                HandleDigest = proposed.Digest, IdleUntil = proposed.IdleUntil, AbsoluteUntil = proposed.AbsoluteUntil,
                SecurityVersion = 1, GrantVersion = 1, PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = epoch },
                audit, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
            await db.SaveChangesAsync(ct);
            return new(LoginOutcome.SessionCreated);
        }
    }

    [SqlLabFact] public async Task Revoke_current_then_everywhere_invalidates_only_current_subject_sessions_and_challenges()
    {
        await using var db = await Context(); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var fixture = await Seed(db); var service = Service(db);
            var first = await service.RevokeAsync(fixture.First.HandleDigest, SessionRevocationOperation.Current, default);
            Assert.True(first.Revoked); Assert.Equal(1, first.SessionsChanged); Assert.Equal(1, first.SecurityVersion);
            Assert.True((await FreshSession(db, fixture.First.Id)).Revoked);
            Assert.False((await FreshSession(db, fixture.Second.Id)).Revoked); Assert.False((await FreshSession(db, fixture.Other.Id)).Revoked);
            Assert.False((await service.RevokeAsync(fixture.First.HandleDigest, SessionRevocationOperation.Current, default)).Revoked);
            Assert.Equal(1, await db.Set<SecurityEventRow>().CountAsync());
            var all = await service.RevokeAsync(fixture.Second.HandleDigest, SessionRevocationOperation.Everywhere, default);
            Assert.True(all.Revoked); Assert.Equal(1, all.SessionsChanged); Assert.Equal(2, all.SecurityVersion);
            Assert.True((await FreshSession(db, fixture.Second.Id)).Revoked); Assert.False((await FreshSession(db, fixture.Other.Id)).Revoked);
            Assert.NotNull(await db.Set<ChallengeRow>().AsNoTracking().Select(x => x.ConsumedAt).SingleAsync());
            Assert.Equal(1, await db.Set<UserRow>().AsNoTracking().Where(x => x.Id == fixture.Other.UserId).Select(x => x.SecurityVersion).SingleAsync());
            var correlations = await (from entry in db.Set<AuditOutboxRow>().AsNoTracking()
                                      join securityEvent in db.Set<SecurityEventRow>().AsNoTracking() on entry.EventId equals securityEvent.Id
                                      select securityEvent.CorrelationId).ToListAsync();
            Assert.Equal(2, correlations.Count);
            Assert.All(correlations, value => Assert.Equal("synthetic-revocation-test", value));
            var reader = new SqlSessionAuthorityReader(new SharedTransactionFactory(db));
            var denied = await reader.ReadAsync(fixture.Second.HandleDigest, default); Assert.NotNull(denied);
            Assert.False(SessionValidity.IsCurrent(denied.User, denied.Session, denied.Authority, DateTimeOffset.UtcNow));
            var allowed = await reader.ReadAsync(fixture.Other.HandleDigest, default); Assert.NotNull(allowed);
            Assert.True(SessionValidity.IsCurrent(allowed.User, allowed.Session, allowed.Authority, DateTimeOffset.UtcNow));
        }
        finally { await tx.RollbackAsync(); }
        Assert.False(await db.Set<UserRow>().AsNoTracking().AnyAsync()); Assert.False(await db.Set<SessionRow>().AsNoTracking().AnyAsync());
    }
    [SqlLabFact] public async Task Required_outbox_failure_rolls_back_revoke_security_version_and_challenge_invalidation()
    {
        await using var db = await Context(); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var fixture = await Seed(db);
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE [ops].[AuditOutbox] WITH NOCHECK ADD CONSTRAINT [CK_Test_RevokeOutbox] CHECK ([Fence]<0);");
            await Assert.ThrowsAsync<AuthorityUnavailableException>(() => Service(db).RevokeAsync(fixture.First.HandleDigest, SessionRevocationOperation.Everywhere, default));
            Assert.False((await FreshSession(db, fixture.First.Id)).Revoked); Assert.False((await FreshSession(db, fixture.Second.Id)).Revoked);
            Assert.Equal(1, await db.Set<UserRow>().AsNoTracking().Where(x => x.Id == fixture.First.UserId).Select(x => x.SecurityVersion).SingleAsync());
            Assert.Null(await db.Set<ChallengeRow>().AsNoTracking().Select(x => x.ConsumedAt).SingleAsync());
            Assert.False(await db.Set<SecurityEventRow>().AsNoTracking().AnyAsync()); Assert.False(await db.Set<AuditOutboxRow>().AsNoTracking().AnyAsync());
        }
        finally { await tx.RollbackAsync(); }
    }
    [SqlLabFact] public async Task Unknown_or_stale_session_never_selects_an_arbitrary_user_to_revoke()
    {
        await using var db = await Context(); await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var fixture = await Seed(db);
            Assert.False((await Service(db).RevokeAsync(new string('A', 64), SessionRevocationOperation.Everywhere, default)).Revoked);
            var user = await db.Set<UserRow>().SingleAsync(x => x.Id == fixture.First.UserId); user.SecurityVersion++; await db.SaveChangesAsync();
            Assert.False((await Service(db).RevokeAsync(fixture.First.HandleDigest, SessionRevocationOperation.Everywhere, default)).Revoked);
            Assert.False(await db.Set<SessionRow>().AsNoTracking().AnyAsync(x => x.Revoked)); Assert.False(await db.Set<SecurityEventRow>().AnyAsync());
        }
        finally { await tx.RollbackAsync(); }
    }
    private static SqlSessionRevocationService Service(IamDbContext db) => new(db, new FixtureBoundary(), new FixturePolicy(), TimeProvider.System);
    private static Task<SessionRow> FreshSession(IamDbContext db, Guid id) => db.Set<SessionRow>().AsNoTracking().SingleAsync(x => x.Id == id);
    private sealed class FixtureBoundary : IAuthorizedCommitBoundary
    { public Task<T> CommitAsync<T>(Guid actorUserId, Func<CancellationToken, Task<T>> action, CancellationToken ct) => action(ct); }
    private sealed class FixturePolicy : ISessionRevocationPolicy
    {
        public Task DemandConfiguredAsync(SessionRevocationOperation operation, CancellationToken ct) => Task.CompletedTask;
        public Task<string> DemandAsync(SessionRevocationOperation operation, CurrentHumanSession session, CancellationToken ct) => Task.FromResult("synthetic-revocation-test");
    }
    private sealed class SharedTransactionFactory(IamDbContext db) : IDbContextFactory<IamDbContext>
    {
        public IamDbContext CreateDbContext()
        {
            var context = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(db.Database.GetDbConnection()).Options);
            context.Database.UseTransaction(db.Database.CurrentTransaction!.GetDbTransaction()); return context;
        }
    }
    private static async Task<(SessionRow First, SessionRow Second, SessionRow Other)> Seed(IamDbContext db)
    {
        var now = DateTimeOffset.UtcNow; var epoch = Guid.NewGuid(); var users = new List<UserRow>();
        foreach (var label in new[] { "own", "other" })
        {
            var person = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = now, PersonKey = "revoke-fixture-" + label };
            var user = new UserRow { Id = Guid.NewGuid(), CreatedAt = now, PersonReferenceId = person.Id, NormalizedUsername = "REVOKE-FIXTURE-" + label, Active = true };
            db.AddRange(person, user); users.Add(user);
        }
        db.Add(new AuthorityVersionRow { Id = Guid.NewGuid(), CreatedAt = now, PolicyConfigured = true, RestoreEpoch = epoch });
        SessionRow Session(Guid userId) => new() { Id = Guid.NewGuid(), CreatedAt = now, UserId = userId, HandleDigest = OpaqueProofs.Digest(OpaqueProofs.Create(), "session"),
            IdleUntil = now.AddMinutes(5), AbsoluteUntil = now.AddMinutes(10), SecurityVersion = 1, GrantVersion = 1, PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = epoch };
        var first = Session(users[0].Id); var second = Session(users[0].Id); var other = Session(users[1].Id);
        db.AddRange(first, second, other, new ChallengeRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = users[0].Id,
            ProofDigest = OpaqueProofs.Digest(OpaqueProofs.Create(), "challenge"), Purpose = "synthetic", ExpiresAt = now.AddMinutes(5) });
        await db.SaveChangesAsync(); return (first, second, other);
    }
    private static async Task<IamDbContext> Context()
    {
        var connection = Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION") ?? throw new InvalidOperationException("Lab required.");
        var settings = new SqlConnectionStringBuilder(connection);
        if (settings.DataSource != "lpc:." || settings.InitialCatalog != "FdcIam_DevelopmentLab") throw new InvalidOperationException("Unexpected target.");
        var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
        try
        {
            var valid = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND DB_NAME()=N'FdcIam_DevelopmentLab' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) THEN 1 ELSE 0 END AS [Value]").SingleAsync();
            Assert.Equal(1, valid); Assert.False(await db.Set<UserRow>().AnyAsync()); Assert.False(await db.Set<AuthorityVersionRow>().AnyAsync(x => x.AuthorityKey == "Center")); return db;
        }
        catch { await db.DisposeAsync(); throw; }
    }
}
