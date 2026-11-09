using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Security;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

// Candidate building block: borrows the caller's context/transaction, never begins, saves,
// commits or disposes it. Caller still must acquire the reviewed D09 gates before reading,
// recheck all actors/targets and persist effect/audit/outbox in this same transaction.
// An active transaction alone is NOT proof of D09 serialization. Not registered as a writer.
public sealed class SqlTransactionalAuthorityReader(
    IReviewedPermissionPolicy policy, ITrustedResourceResolver resources, TimeProvider clock)
{
    public async Task<(AuthorizationSnapshot Snapshot, TrustedResource? Resource)?> ReadAsync(
        IamDbContext db, string sessionDigest, string? objectId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("An existing authority transaction is required.");
        if (db.ChangeTracker.Entries().Any(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Flush or discard pending changes before the authority recheck.");
        if (!SqlSessionAuthorityReader.IsDigest(sessionDigest)) return null;
        try
        {
            await SqlSessionAuthorityReader.RequireWritablePrimaryAsync(db, ct);
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
            return (snapshot, resource);
        }
        catch (Exception error) when (SqlAuthorityErrors.IsUnavailable(error)) { throw new AuthorityUnavailableException(); }
    }
}
