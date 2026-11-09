using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Identity.Tests;

public sealed class SqlLabFactAttribute : FactAttribute
{
    public SqlLabFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("FDC_IAM_CONFIRMED_LAB") != "FdcIam_DevelopmentLab")
            Skip = "Explicit disposable local lab environment not supplied.";
    }
}

// Synthetic rows are written only inside a rolled-back transaction, never installed as users/seeds.
[Collection("IAM database lab")]
public sealed class Phase2SqlLabTests
{
    [SqlLabFact]
    public async Task Sql_session_reader_sees_current_versions_disable_and_revoke()
    {
        await using var db = Context();
        await AssertTarget(db);
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var person = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = now, PersonKey = "phase2-test-" + Guid.NewGuid().ToString("N") };
            var user = new UserRow { Id = Guid.NewGuid(), CreatedAt = now, PersonReferenceId = person.Id, NormalizedUsername = "PHASE2-" + Guid.NewGuid().ToString("N"), Active = true };
            var authority = new AuthorityVersionRow { Id = Guid.NewGuid(), CreatedAt = now, PolicyConfigured = true, RestoreEpoch = Guid.NewGuid() };
            Assert.False(await db.Set<AuthorityVersionRow>().AnyAsync(x => x.AuthorityKey == "Center"));
            var proof = OpaqueProofs.Create();
            var session = new SessionRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = user.Id, HandleDigest = OpaqueProofs.Digest(proof, "session"), IdleUntil = now.AddMinutes(2), AbsoluteUntil = now.AddMinutes(5), SecurityVersion = 1, GrantVersion = 1, PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = authority.RestoreEpoch };
            db.AddRange(person, user, authority, session); await db.SaveChangesAsync();
            var reader = new SqlSessionAuthorityReader(new TransactionFactory(db, tx));
            var initial = await reader.ReadAsync(session.HandleDigest, default);
            Assert.NotNull(initial); Assert.True(SessionValidity.IsCurrent(initial.User, initial.Session, initial.Authority, now));
            user.GrantVersion++; await db.SaveChangesAsync();
            var grantChanged = await reader.ReadAsync(session.HandleDigest, default);
            Assert.NotNull(grantChanged); Assert.False(SessionValidity.IsCurrent(grantChanged.User, grantChanged.Session, grantChanged.Authority, now));
            user.GrantVersion = 1; user.Active = false; await db.SaveChangesAsync();
            var disabled = await reader.ReadAsync(session.HandleDigest, default);
            Assert.NotNull(disabled); Assert.False(SessionValidity.IsCurrent(disabled.User, disabled.Session, disabled.Authority, now));
            user.Active = true; session.Revoked = true; await db.SaveChangesAsync();
            var revoked = await reader.ReadAsync(session.HandleDigest, default);
            Assert.NotNull(revoked); Assert.False(SessionValidity.IsCurrent(revoked.User, revoked.Session, revoked.Authority, now));
            authority.RestoreEpoch = Guid.NewGuid(); await db.SaveChangesAsync();
            var restored = await reader.ReadAsync(session.HandleDigest, default);
            Assert.NotNull(restored); Assert.NotEqual(restored.Authority.RestoreEpoch, restored.Session.Version.RestoreEpoch);
        }
        finally { await tx.RollbackAsync(); }
        Assert.False(await db.Set<UserRow>().AsNoTracking().AnyAsync());
        Assert.False(await db.Set<SessionRow>().AsNoTracking().AnyAsync());
        Assert.False(await db.Set<AuthorityVersionRow>().AsNoTracking().AnyAsync());
    }

    [SqlLabFact]
    public async Task Sql_authority_reader_denies_missing_session_in_real_empty_lab()
    {
        await using var db = Context(); await AssertTarget(db);
        var reader = new SqlCurrentAuthorityReader(new Factory(), new PendingPermissionPolicy(), new NoResources(), TimeProvider.System);
        Assert.Null(await reader.ReadAsync(new string('A', 64), null, default));
    }

    private static IamDbContext Context()
    {
        var connection = Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION") ?? throw new InvalidOperationException("Lab connection not provided.");
        var builder = new SqlConnectionStringBuilder(connection);
        if (builder.DataSource != "lpc:." || builder.InitialCatalog != "FdcIam_DevelopmentLab") throw new InvalidOperationException("Unexpected lab connection.");
        return new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
    }
    private static async Task AssertTarget(IamDbContext db)
    {
        var allowed = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND DB_NAME()=N'FdcIam_DevelopmentLab' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Environment' AND CONVERT(nvarchar(128),value)=N'DevelopmentLab') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
        Assert.Equal(1, allowed);
        Assert.False(await db.Set<UserRow>().AnyAsync());
    }
    private sealed class Factory : IDbContextFactory<IamDbContext> { public IamDbContext CreateDbContext() => Context(); }
    private sealed class TransactionFactory(IamDbContext owner, IDbContextTransaction transaction) : IDbContextFactory<IamDbContext>
    {
        public IamDbContext CreateDbContext()
        {
            var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(owner.Database.GetDbConnection()).Options);
            db.Database.UseTransaction(transaction.GetDbTransaction()); return db;
        }
    }
    private sealed class NoResources : ITrustedResourceResolver
    {
        public Task<Fdc.Identity.Domain.TrustedResource?> ResolveAsync(IamDbContext db, string objectId, Fdc.Identity.Domain.AuthorityVersion authority, CancellationToken ct)
            => throw new InvalidOperationException("Missing session must stop resource resolution.");
    }
}
