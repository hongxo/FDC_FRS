using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Security;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

public interface IReviewedPermissionPolicy
{
    // Only a reviewed server mapping may supply MFA/SoD/scope semantics missing from the draft catalog.
    PermissionDescriptor? Resolve(PermissionRow permission, AuthorityVersion authority);
}
public sealed class PendingPermissionPolicy : IReviewedPermissionPolicy
{
    public PermissionDescriptor? Resolve(PermissionRow permission, AuthorityVersion authority) => null;
}
public interface ITrustedResourceResolver
{
    // Resolve actual object ownership on the same database/transaction, never infer from a client station ID.
    Task<TrustedResource?> ResolveAsync(IamDbContext db, string objectId, AuthorityVersion authority, CancellationToken ct);
}

// Coherent read-only snapshot, not D09's mutation/revoke commit gate. Not wired into the current hosts.
public sealed class SqlCurrentAuthorityReader(IDbContextFactory<IamDbContext> factory,
    IReviewedPermissionPolicy policy, ITrustedResourceResolver resources, TimeProvider clock) : ICurrentAuthorityReader
{
    public async Task<(AuthorizationSnapshot Snapshot, TrustedResource? Resource)?> ReadAsync(string sessionDigest, string? objectId, CancellationToken ct)
    {
        if (!SqlSessionAuthorityReader.IsDigest(sessionDigest)) return null;
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            await SqlSessionAuthorityReader.RequireWritablePrimaryAsync(db, ct);
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var current = await SqlSessionAuthorityReader.ReadCoreAsync(db, sessionDigest, ct);
            if (current is null || !SessionValidity.IsCurrent(current.User, current.Session, current.Authority, clock.GetUtcNow())) return null;
            var assignments = await db.Set<UserRoleRow>().AsNoTracking().Where(x => x.UserId == current.User.Id).ToListAsync(ct);
            var assignmentIds = assignments.Select(x => x.Id).ToArray();
            var roleIds = assignments.Select(x => x.RoleId).Distinct().ToArray();
            var roles = await db.Set<RoleRow>().AsNoTracking().Where(x => roleIds.Contains(x.Id)).ToListAsync(ct);
            var links = await db.Set<RolePermissionRow>().AsNoTracking().Where(x => roleIds.Contains(x.RoleId)).ToListAsync(ct);
            var permissionIds = links.Select(x => x.PermissionId).Distinct().ToArray();
            var permissions = await db.Set<PermissionRow>().AsNoTracking().Where(x => permissionIds.Contains(x.Id)).ToListAsync(ct);
            var grants = await db.Set<StationGrantRow>().AsNoTracking().Where(x => assignmentIds.Contains(x.UserRoleId)).ToListAsync(ct);
            var grantIds = grants.Select(x => x.Id).ToArray();
            var scopeIds = grants.Select(x => x.ScopeId).Distinct().ToArray();
            var scopes = await db.Set<ScopeRow>().AsNoTracking().Where(x => scopeIds.Contains(x.Id)).ToListAsync(ct);
            var functions = await db.Set<GrantFunctionRow>().AsNoTracking().Where(x => grantIds.Contains(x.StationGrantId)).ToListAsync(ct);
            var delegations = await db.Set<AdministrativeDelegationRow>().AsNoTracking().Where(x => assignmentIds.Contains(x.UserRoleId)).ToListAsync(ct);
            var codes = permissions.ToDictionary(x => x.Id, x => x.PermissionCode);
            var descriptors = permissions.Select(p => policy.Resolve(p, current.Authority))
                .Where(p => p is not null && p.CatalogVersion == current.Authority.Catalog &&
                    permissions.Any(row => row.PermissionCode == p.Code && row.IsApproved && row.IsImplemented && row.CatalogVersion == p.CatalogVersion))
                .Cast<PermissionDescriptor>().ToImmutableArray();
            var typedGrants = new List<StationGrant>();
            foreach (var grant in grants)
            {
                var scope = scopes.Single(x => x.Id == grant.ScopeId);
                var kind = scope.Kind switch { "Station" => ScopeKind.Station, "ManagementUnit" => ScopeKind.ManagementUnit, "Corporation" => ScopeKind.Corporation, _ => (ScopeKind?)null };
                var id = kind switch { ScopeKind.Station => scope.StationId, ScopeKind.ManagementUnit => scope.ManagementUnitId, ScopeKind.Corporation => scope.CorporationId, _ => null };
                if (kind is { } k && id is { } target)
                    typedGrants.Add(new(grant.Id, grant.UserRoleId, k, target.ToString("D"), grant.Active, grant.ValidFrom, grant.ValidUntil, grant.HierarchyVersion));
            }
            var snapshot = new AuthorizationSnapshot(current.User, current.Session, current.Authority,
                roles.Select(r => new RoleDefinition(r.Id, r.RoleCode, r.RoleName, r.Description, r.IsSystemSeed, r.IsActive, r.Version, r.ValidUntil)).ToImmutableArray(),
                assignments.Select(a => new RoleAssignment(a.Id, a.UserId, a.RoleId, a.Active, a.ValidFrom, a.ValidUntil)).ToImmutableArray(), descriptors,
                links.Where(x => codes.ContainsKey(x.PermissionId)).Select(x => new RolePermission(x.RoleId, codes[x.PermissionId])).ToImmutableArray(),
                typedGrants.ToImmutableArray(),
                functions.Where(x => codes.ContainsKey(x.PermissionId)).Select(x => new GrantFunction(x.StationGrantId, codes[x.PermissionId])).ToImmutableArray(),
                delegations.Where(x => codes.ContainsKey(x.PermissionId)).Select(x => new AdministrativeDelegation(x.UserRoleId, x.TargetPopulation, codes[x.PermissionId], x.Active, x.ValidFrom, x.ValidUntil)).ToImmutableArray());
            var resource = objectId is null ? null : await resources.ResolveAsync(db, objectId, current.Authority, ct);
            await tx.CommitAsync(ct);
            return (snapshot, resource);
        }
        catch (DbException) { throw new AuthorityUnavailableException(); }
    }
}
