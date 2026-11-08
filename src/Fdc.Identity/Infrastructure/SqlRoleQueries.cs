using Fdc.Identity.Application;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

// Called only AFTER RoleManagementService resolves the current authoritative read scope.
// There is intentionally no "null scope means all roles" overload.
public sealed class SqlRoleQueries(IamDbContext db) : IRoleQueries
{
    public async Task<RolePage> ListAsync(RoleReadScope scope, string? afterCode, int limit, CancellationToken ct)
    {
        if (limit is < 1 or > 100 || afterCode?.Length > 80) throw new ArgumentException("Invalid page bounds.");
        var ids = scope.RoleIds.ToArray();
        var query = db.Set<RoleRow>().AsNoTracking().Where(r => ids.Contains(r.Id));
        if (afterCode is not null) query = query.Where(r => r.RoleCode.CompareTo(afterCode) > 0);
        var rows = await query.OrderBy(r => r.RoleCode).Take(limit + 1)
            .Select(r => new RoleView(r.Id, r.RoleCode, r.RoleName, r.Description, r.IsSystemSeed, r.IsActive, r.Version)).ToListAsync(ct);
        var hasMore = rows.Count > limit;
        return new(rows.Take(limit).ToArray(), hasMore ? rows[limit - 1].Code : null);
    }
    public async Task<RoleView?> DetailAsync(RoleReadScope scope, Guid roleId, CancellationToken ct)
    {
        if (!scope.RoleIds.Contains(roleId)) return null;
        return await db.Set<RoleRow>().AsNoTracking().Where(r => r.Id == roleId)
            .Select(r => new RoleView(r.Id, r.RoleCode, r.RoleName, r.Description, r.IsSystemSeed, r.IsActive, r.Version)).SingleOrDefaultAsync(ct);
    }
    public async Task<IReadOnlyList<string>> PermissionsAsync(RoleReadScope scope, Guid roleId, CancellationToken ct)
    {
        if (!scope.RoleIds.Contains(roleId)) return [];
        return await (from rp in db.Set<RolePermissionRow>().AsNoTracking()
                      join p in db.Set<PermissionRow>().AsNoTracking() on rp.PermissionId equals p.Id
                      where rp.RoleId == roleId && p.IsApproved && p.IsImplemented
                      orderby p.PermissionCode select p.PermissionCode).ToListAsync(ct);
    }
}
