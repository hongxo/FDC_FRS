using Fdc.Identity.Application;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

// Requires a freshly authorized server scope. Scope predicates precede filtering, paging and projection in SQL.
public sealed class SqlManagementQueries(IamDbContext db) : IManagementQueries
{
    public async Task<ManagementPage<UserView>> UsersAsync(ManagementReadScope scope, string? search, bool? active, PageWindow page, CancellationToken ct)
    {
        page.Validate();
        if (search?.Length > 80 || search?.Any(char.IsControl) == true) throw new ArgumentException("Invalid search.");
        var ids = scope.UserIds.ToArray();
        var query = db.Set<UserRow>().AsNoTracking().Where(x => ids.Contains(x.Id));
        if (!string.IsNullOrEmpty(search)) query = query.Where(x => x.NormalizedUsername.Contains(search));
        if (active is { } status) query = query.Where(x => x.Active == status);
        var rows = await query.OrderBy(x => x.NormalizedUsername).Skip(page.Offset).Take(page.Limit + 1)
            .Select(x => new UserView(x.Id, x.NormalizedUsername, x.Active, x.Version)).ToListAsync(ct);
        return Page(rows, page);
    }
    public Task<UserView?> UserAsync(ManagementReadScope scope, Guid id, CancellationToken ct)
    {
        if (!scope.UserIds.Contains(id)) return Task.FromResult<UserView?>(null);
        return db.Set<UserRow>().AsNoTracking().Where(x => x.Id == id)
            .Select(x => new UserView(x.Id, x.NormalizedUsername, x.Active, x.Version)).SingleOrDefaultAsync(ct);
    }
    public async Task<ManagementPage<UserRoleView>> UserRolesAsync(ManagementReadScope scope, Guid userId, PageWindow page, CancellationToken ct)
    {
        page.Validate(); if (!scope.UserIds.Contains(userId)) return new([], false);
        var roles = scope.RoleIds.ToArray(); var assignments = scope.UserRoleIds.ToArray();
        var query = from a in db.Set<UserRoleRow>().AsNoTracking()
                    join r in db.Set<RoleRow>().AsNoTracking() on a.RoleId equals r.Id
                    where a.UserId == userId && assignments.Contains(a.Id) && roles.Contains(r.Id)
                    orderby a.CreatedAt, a.Id
                    select new UserRoleView(a.Id, a.UserId, a.RoleId, r.RoleName, a.Active, a.ValidFrom, a.ValidUntil, a.Version);
        return Page(await query.Skip(page.Offset).Take(page.Limit + 1).ToListAsync(ct), page);
    }
    private IQueryable<StationGrantRow> VisibleGrants(ManagementReadScope scope, Guid userId, Guid userRoleId)
    {
        var grants = scope.GrantIds.ToArray(); var roles = scope.RoleIds.ToArray();
        // All subject, assignment and role checks remain in SQL; a supplied ID is a filter, not authority.
        return from g in db.Set<StationGrantRow>().AsNoTracking()
               join a in db.Set<UserRoleRow>().AsNoTracking() on g.UserRoleId equals a.Id
               where g.UserRoleId == userRoleId && a.UserId == userId && roles.Contains(a.RoleId) && grants.Contains(g.Id)
               select g;
    }
    public async Task<ManagementPage<GrantView>> GrantsAsync(ManagementReadScope scope, Guid userId, Guid userRoleId, PageWindow page, CancellationToken ct)
    {
        page.Validate(); if (!scope.UserIds.Contains(userId) || !scope.UserRoleIds.Contains(userRoleId)) return new([], false);
        var query = from g in VisibleGrants(scope, userId, userRoleId)
                    join s in db.Set<ScopeRow>().AsNoTracking() on g.ScopeId equals s.Id
                    orderby g.CreatedAt, g.Id
                    select new GrantView(g.Id, g.UserRoleId, s.Id, s.Kind, s.CorporationId, s.ManagementUnitId, s.StationId,
                        g.Active, g.ValidFrom, g.ValidUntil, g.Version, g.HierarchyVersion);
        return Page(await query.Skip(page.Offset).Take(page.Limit + 1).ToListAsync(ct), page);
    }
    public async Task<IReadOnlyList<GrantFunctionView>> GrantFunctionsAsync(ManagementReadScope scope, Guid userId, Guid userRoleId, Guid grantId, CancellationToken ct)
    {
        if (!scope.UserIds.Contains(userId) || !scope.UserRoleIds.Contains(userRoleId) || !scope.GrantIds.Contains(grantId)) return [];
        var codes = scope.PermissionCodes.ToArray();
        return await (from g in VisibleGrants(scope, userId, userRoleId)
                      join f in db.Set<GrantFunctionRow>().AsNoTracking() on g.Id equals f.StationGrantId
                      join p in db.Set<PermissionRow>().AsNoTracking() on f.PermissionId equals p.Id
                      where g.Id == grantId && codes.Contains(p.PermissionCode) && p.IsApproved && p.IsImplemented
                      orderby p.PermissionCode select new GrantFunctionView(g.Id, p.PermissionCode)).ToListAsync(ct);
    }
    public async Task<ManagementPage<PermissionView>> PermissionsAsync(ManagementReadScope scope, PageWindow page, CancellationToken ct)
    {
        page.Validate(); var codes = scope.PermissionCodes.ToArray();
        var rows = await db.Set<PermissionRow>().AsNoTracking().Where(p => codes.Contains(p.PermissionCode) && p.IsApproved && p.IsImplemented)
            .OrderBy(p => p.PermissionCode).Skip(page.Offset).Take(page.Limit + 1)
            .Select(p => new PermissionView(p.PermissionCode, p.Module, p.Action, p.ScopeRequirement, p.RiskLevel, p.CatalogVersion)).ToListAsync(ct);
        return Page(rows, page);
    }
    public async Task<ManagementPage<AuditView>> AuditAsync(ManagementReadScope scope, string? action, PageWindow page, CancellationToken ct)
    {
        page.Validate(); if (action?.Length > 80 || action?.Any(char.IsControl) == true) throw new ArgumentException("Invalid action.");
        var ids = scope.EventIds.ToArray();
        var query = db.Set<SecurityEventRow>().AsNoTracking().Where(e => ids.Contains(e.Id));
        if (!string.IsNullOrEmpty(action)) query = query.Where(e => e.EventCode == action);
        var rows = await query.OrderByDescending(e => e.CreatedAt).ThenBy(e => e.Id).Skip(page.Offset).Take(page.Limit + 1)
            .Select(e => new AuditView(e.Id, e.ActorUserId, e.EventCode, e.OutcomeCode, e.CreatedAt, e.CorrelationId)).ToListAsync(ct);
        return Page(rows, page);
    }
    public Task<AuditView?> AuditEventAsync(ManagementReadScope scope, Guid id, CancellationToken ct)
    {
        if (!scope.EventIds.Contains(id)) return Task.FromResult<AuditView?>(null);
        return db.Set<SecurityEventRow>().AsNoTracking().Where(e => e.Id == id)
            .Select(e => new AuditView(e.Id, e.ActorUserId, e.EventCode, e.OutcomeCode, e.CreatedAt, e.CorrelationId)).SingleOrDefaultAsync(ct);
    }
    public Task<ChangeRequestView?> RequestAsync(ManagementReadScope scope, Guid id, CancellationToken ct)
    {
        if (!scope.RequestIds.Contains(id)) return Task.FromResult<ChangeRequestView?>(null);
        return db.Set<RoleChangeRequestRow>().AsNoTracking().Where(x => x.Id == id)
            .Select(x => new ChangeRequestView(x.Id, x.RequesterUserId, x.RoleId, x.Revision, x.Version, x.ApprovalStatus, x.EffectiveStatus, x.ExpiresAt)).SingleOrDefaultAsync(ct);
    }
    private static ManagementPage<T> Page<T>(List<T> rows, PageWindow page) => new(rows.Take(page.Limit).ToArray(), rows.Count > page.Limit);
}
