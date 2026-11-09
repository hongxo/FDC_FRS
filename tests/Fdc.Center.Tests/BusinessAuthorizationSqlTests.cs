using Fdc.Center.Persistence;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Center.Tests;

public sealed partial class CenterDatabaseSqlTests
{
    [CenterFoundationFact]
    public async Task Business_reads_scope_aggregate_and_objects_and_recheck_revocation()
    {
        await using var fixture = await Fixture.Create();
        await using var center = fixture.Db();
        await using var tx = await center.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
            .UseSqlServer(center.Database.GetDbConnection()).Options);
        await iam.Database.UseTransactionAsync(tx.GetDbTransaction());
        var now = DateTimeOffset.UtcNow;
        var epoch = Guid.NewGuid();
        var digest = OpaqueProofs.Digest(OpaqueProofs.Create(), "session");
        var hidden = new IedDevice { StationId = fixture.OtherStation, Code = "HIDDEN-SYNTHETIC", DisplayName = "Hidden", IsSynthetic = true };
        center.Add(hidden);
        await center.SaveChangesAsync();
        T Row<T>(T row) where T : IamRow { row.Id = Guid.NewGuid(); row.CreatedAt = now; return row; }
        var person = Row(new PersonReferenceRow { PersonKey = "a6-synthetic" });
        var user = Row(new UserRow { PersonReferenceId = person.Id, NormalizedUsername = "A6-SYNTHETIC", Active = true });
        var authority = Row(new AuthorityVersionRow { PolicyConfigured = true, RestoreEpoch = epoch });
        var session = Row(new SessionRow { UserId = user.Id, HandleDigest = digest, IdleUntil = now.AddMinutes(5), AbsoluteUntil = now.AddMinutes(10),
            SecurityVersion = 1, GrantVersion = 1, PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = epoch });
        var role = Row(new RoleRow { RoleCode = "a6-synthetic", RoleName = "Fixture", IsActive = true });
        var assignment = Row(new UserRoleRow { UserId = user.Id, RoleId = role.Id, Active = true, ValidFrom = now.AddMinutes(-1),
            ValidUntil = now.AddMinutes(5), ApprovalReference = "synthetic" });
        var scope = Row(new ScopeRow { Kind = "Station", StationId = fixture.Station });
        var grant = Row(new StationGrantRow { UserRoleId = assignment.Id, ScopeId = scope.Id, Active = true, ValidFrom = now.AddMinutes(-1),
            ValidUntil = now.AddMinutes(5), HierarchyVersion = 1, ApprovalReference = "synthetic" });
        var permission = Row(new PermissionRow { PermissionCode = "a6.synthetic.read", Module = "fixture", Action = "read",
            ScopeRequirement = "Station", SoDPolicy = "fixture", RiskLevel = "fixture", CatalogVersion = 1,
            IsApproved = true, IsImplemented = true, ApprovalReference = "synthetic" });
        var link = Row(new RolePermissionRow { RoleId = role.Id, PermissionId = permission.Id });
        var function = Row(new GrantFunctionRow { StationGrantId = grant.Id, PermissionId = permission.Id });
        try
        {
            iam.AddRange(person, user, authority, session, role, assignment, scope, grant, permission, link, function);
            await iam.SaveChangesAsync();
            var service = new BusinessAuthorization(center, new TestOnlyBusinessMapping(), new TestOnlyPermissionPolicy(),
                new UnusedObjectResolver(), new PermissionEvaluator(), TimeProvider.System);
            Assert.Equal(1, await service.DashboardAsync(digest, default));
            Assert.Equal(fixture.Station, Assert.Single(await service.ListAsync(digest, BusinessReadOperation.Station, null, 0, 50, default)).Id);
            Assert.NotNull(await service.DetailAsync(digest, BusinessReadOperation.Station, fixture.Station, default));
            Assert.Null(await service.DetailAsync(digest, BusinessReadOperation.Station, fixture.OtherStation, default));
            Assert.Equal(fixture.Ied, Assert.Single(await service.ListAsync(digest, BusinessReadOperation.Ied, null, 0, 1, default)).Id);
            Assert.NotNull(await service.DetailAsync(digest, BusinessReadOperation.Ied, fixture.Ied, default));
            Assert.Null(await service.DetailAsync(digest, BusinessReadOperation.Ied, hidden.Id, default));
            Assert.Empty(await service.ListAsync(digest, BusinessReadOperation.Ied, fixture.OtherStation, 0, 50, default));
            Assert.Equal(fixture.Collector, Assert.Single(await service.ListAsync(digest, BusinessReadOperation.Collector, null, 0, 50, default)).Id);
            var record = new DisturbanceRecord { StationId = fixture.Station, IedId = fixture.Ied, CollectorId = fixture.Collector,
                ManifestId = new string('a', 64), ManifestSha256 = new string('b', 64), RemoteIdentity = "A6-SYNTHETIC",
                CollectedAtUtc = now, TimeSource = "fixture", CorrelationId = "A6-SYNTHETIC", IsSynthetic = true };
            center.Add(record);
            await center.SaveChangesAsync();
            Assert.Equal(record.Id, Assert.Single(await service.ListAsync(digest, BusinessReadOperation.Record, null, 0, 50, default)).Id);

            var pending = new BusinessAuthorization(center, new PendingBusinessOperationPolicy(), new TestOnlyPermissionPolicy(),
                new UnusedObjectResolver(), new PermissionEvaluator(), TimeProvider.System);
            await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => pending.DashboardAsync(digest, default));

            grant.Active = false;
            await iam.SaveChangesAsync();
            Assert.Equal(0, await service.DashboardAsync(digest, default));
            Assert.Null(await service.DetailAsync(digest, BusinessReadOperation.Record, record.Id, default));
            Assert.Null(await service.DetailAsync(digest, BusinessReadOperation.Ied, fixture.Ied, default));
        }
        finally
        {
            await tx.RollbackAsync();
        }
    }

    private sealed class TestOnlyBusinessMapping : IReviewedBusinessOperationPolicy
    {
        public string DemandPermission(BusinessReadOperation operation, AuthorityVersion version) => "a6.synthetic.read";
    }

    private sealed class TestOnlyPermissionPolicy : IReviewedPermissionPolicy
    {
        public PermissionDescriptor? Resolve(PermissionRow row, AuthorityVersion version) =>
            row.PermissionCode == "a6.synthetic.read" ? new(row.PermissionCode, true, true, version.Catalog, false, null) : null;
    }

    private sealed class UnusedObjectResolver : ITrustedResourceResolver
    {
        public Task<TrustedResource?> ResolveAsync(IamDbContext db, string objectId, AuthorityVersion authority, CancellationToken ct) =>
            throw new InvalidOperationException("Scope evaluation uses canonical station ownership.");
    }
}
