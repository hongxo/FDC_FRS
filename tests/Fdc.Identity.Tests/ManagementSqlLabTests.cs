using System.Collections.Immutable;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

[Collection("IAM database lab")]
public sealed class ManagementSqlLabTests
{
    [SqlLabFact]
    public async Task Sql_management_scope_precedes_paging_and_cross_subject_grants_are_hidden()
    {
        await using var db = Context(); await AssertTarget(db);
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var now = DateTimeOffset.UtcNow;
            T Row<T>(T row) where T : IamRow { row.Id = Guid.NewGuid(); row.CreatedAt = now; return row; }
            var p1 = Row(new PersonReferenceRow { PersonKey = "test-visible" }); var p2 = Row(new PersonReferenceRow { PersonKey = "test-hidden" });
            var u1 = Row(new UserRow { PersonReferenceId = p1.Id, NormalizedUsername = "Z-VISIBLE", Active = true });
            var u2 = Row(new UserRow { PersonReferenceId = p2.Id, NormalizedUsername = "A-HIDDEN", Active = true });
            var r1 = Row(new RoleRow { RoleCode = "synthetic-visible", RoleName = "Custom Visible" });
            var r2 = Row(new RoleRow { RoleCode = "synthetic-hidden", RoleName = "Custom Hidden" });
            var a1 = Row(new UserRoleRow { UserId = u1.Id, RoleId = r1.Id, Active = true, ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(1), ApprovalReference = "synthetic-rollback" });
            var a2 = Row(new UserRoleRow { UserId = u2.Id, RoleId = r2.Id, Active = true, ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(1), ApprovalReference = "synthetic-rollback" });
            var corporation = Row(new CorporationRow { Code = "TEST-CORP" });
            var unit = Row(new ManagementUnitRow { CorporationId = corporation.Id, Code = "TEST-UNIT" });
            var station = Row(new StationRow { ManagementUnitId = unit.Id, Code = "TEST-STATION", HierarchyVersion = 1 });
            var scopeRow = Row(new ScopeRow { Kind = "Station", StationId = station.Id });
            var g1 = Row(new StationGrantRow { UserRoleId = a1.Id, ScopeId = scopeRow.Id, Active = true, ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(1), HierarchyVersion = 1, ApprovalReference = "synthetic-rollback" });
            var g2 = Row(new StationGrantRow { UserRoleId = a2.Id, ScopeId = scopeRow.Id, Active = true, ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(1), HierarchyVersion = 1, ApprovalReference = "synthetic-rollback" });
            var permission = Row(new PermissionRow { PermissionCode = "synthetic.view", Module = "test", Action = "view", ScopeRequirement = "Station", SoDPolicy = "test", RiskLevel = "test", CatalogVersion = 1, IsApproved = true, IsImplemented = true, ApprovalReference = "synthetic-rollback" });
            var function = Row(new GrantFunctionRow { StationGrantId = g1.Id, PermissionId = permission.Id });
            var e1 = Row(new SecurityEventRow { ActorUserId = u1.Id, EventCode = "test.visible", OutcomeCode = "test", CorrelationId = "synthetic", PolicyVersion = 1, RestoreEpoch = Guid.NewGuid() });
            var e2 = Row(new SecurityEventRow { ActorUserId = u2.Id, EventCode = "test.hidden", OutcomeCode = "test", CorrelationId = "synthetic", PolicyVersion = 1, RestoreEpoch = Guid.NewGuid() });
            db.AddRange(p1, p2, u1, u2, r1, r2, a1, a2, corporation, unit, station, scopeRow, g1, g2, permission, function, e1, e2);
            await db.SaveChangesAsync();
            var visibility = new ManagementReadScope([u1.Id], [r1.Id], [a1.Id], [g1.Id], [e1.Id], [], ImmutableHashSet.Create(StringComparer.Ordinal, permission.PermissionCode));
            var queries = new SqlManagementQueries(db);
            var page = await queries.UsersAsync(visibility, null, null, new(0, 1), default);
            Assert.Equal(u1.Id, Assert.Single(page.Items).Id); Assert.False(page.HasMore);
            Assert.Empty((await queries.UsersAsync(visibility, "HIDDEN", null, new(0, 1), default)).Items);
            Assert.Null(await queries.UserAsync(visibility, u2.Id, default));
            Assert.Equal(a1.Id, Assert.Single((await queries.UserRolesAsync(visibility, u1.Id, new(0, 10), default)).Items).Id);
            Assert.Empty((await queries.GrantsAsync(visibility, u1.Id, a2.Id, new(0, 10), default)).Items);
            // Even if a stale/wrong scope provider lists both IDs, subject/assignment SQL join must still reject mixing them.
            var wrong = visibility with { UserRoleIds = [a1.Id, a2.Id], RoleIds = [r1.Id, r2.Id], GrantIds = [g1.Id, g2.Id] };
            Assert.Empty((await queries.GrantsAsync(wrong, u1.Id, a2.Id, new(0, 10), default)).Items);
            Assert.Equal(g1.Id, Assert.Single((await queries.GrantsAsync(visibility, u1.Id, a1.Id, new(0, 10), default)).Items).Id);
            Assert.Equal(permission.PermissionCode, Assert.Single(await queries.GrantFunctionsAsync(visibility, u1.Id, a1.Id, g1.Id, default)).PermissionCode);
            Assert.Equal(e1.Id, Assert.Single((await queries.AuditAsync(visibility, null, new(0, 1), default)).Items).Id);
            Assert.Null(await queries.AuditEventAsync(visibility, e2.Id, default));
            Assert.Equal(permission.PermissionCode, Assert.Single((await queries.PermissionsAsync(visibility, new(0, 10), default)).Items).Code);
            Assert.Empty((await queries.UsersAsync(ManagementReadTests.Empty, null, null, new(0, 1), default)).Items);
        }
        finally { await tx.RollbackAsync(); }
        Assert.False(await db.Set<UserRow>().AsNoTracking().AnyAsync());
        Assert.False(await db.Set<SecurityEventRow>().AsNoTracking().AnyAsync());
        Assert.False(await db.Set<StationGrantRow>().AsNoTracking().AnyAsync());
    }
    private static IamDbContext Context()
    {
        var value = Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION") ?? throw new InvalidOperationException("Lab connection required.");
        var options = new SqlConnectionStringBuilder(value);
        if (options.DataSource != "lpc:." || options.InitialCatalog != "FdcIam_DevelopmentLab") throw new InvalidOperationException("Unexpected lab connection.");
        return new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(value).Options);
    }
    private static async Task AssertTarget(IamDbContext db)
    {
        var ok = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND DB_NAME()=N'FdcIam_DevelopmentLab' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Environment' AND CONVERT(nvarchar(128),value)=N'DevelopmentLab') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
        Assert.Equal(1, ok); Assert.False(await db.Set<UserRow>().AnyAsync());
    }
}
