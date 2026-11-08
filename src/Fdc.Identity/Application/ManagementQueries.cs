using System.Collections.Immutable;

namespace Fdc.Identity.Application;

public enum ManagementReadOperation { Users, UserDetail, UserRoles, StationGrants, PermissionCatalog, SecurityAudit, SecurityAuditDetail, ChangeRequest }
// Server-derived visibility only; empty means no rows, never all. Resolve afresh for each service call.
public sealed record ManagementReadScope(ImmutableHashSet<Guid> UserIds, ImmutableHashSet<Guid> RoleIds,
    ImmutableHashSet<Guid> UserRoleIds, ImmutableHashSet<Guid> GrantIds, ImmutableHashSet<Guid> EventIds,
    ImmutableHashSet<Guid> RequestIds, ImmutableHashSet<string> PermissionCodes);
public interface IManagementReadAuthority
{
    Task<ManagementReadScope> DemandAsync(ManagementReadOperation operation, CancellationToken ct);
}
public sealed class PendingManagementReadAuthority : IManagementReadAuthority
{
    public Task<ManagementReadScope> DemandAsync(ManagementReadOperation operation, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D04/D05: management read mapping, delegation and current Human session");
}
public sealed record ManagementPage<T>(IReadOnlyList<T> Items, bool HasMore);
public sealed record UserView(Guid Id, string Username, bool Active, long Version);
public sealed record UserRoleView(Guid Id, Guid UserId, Guid RoleId, string RoleName, bool Active,
    DateTimeOffset ValidFrom, DateTimeOffset ValidUntil, long Version);
public sealed record GrantView(Guid Id, Guid UserRoleId, Guid ScopeId, string ScopeKind, Guid? CorporationId,
    Guid? ManagementUnitId, Guid? StationId, bool Active, DateTimeOffset ValidFrom, DateTimeOffset ValidUntil,
    long Version, long HierarchyVersion);
public sealed record GrantFunctionView(Guid GrantId, string PermissionCode);
public sealed record PermissionView(string Code, string Module, string Action, string Scope, string Risk, long CatalogVersion);
public sealed record AuditView(Guid Id, Guid? ActorId, string Action, string Result, DateTimeOffset Timestamp, string CorrelationId);
public sealed record ChangeRequestView(Guid Id, Guid RequesterId, Guid? RoleId, long Revision, long Version,
    string ApprovalStatus, string EffectiveStatus, DateTimeOffset ExpiresAt);
public readonly record struct PageWindow
{
    public int Offset { get; }
    public int Limit { get; }
    public PageWindow(int offset, int limit)
    {
        if (offset is < 0 or > 10000 || limit is < 1 or > 100) throw new ArgumentException("Invalid page bounds.");
        Offset = offset; Limit = limit;
    }
    public void Validate() { if (Limit is < 1 or > 100 || Offset is < 0 or > 10000) throw new ArgumentException("Invalid page bounds."); }
}
public interface IManagementQueries
{
    Task<ManagementPage<UserView>> UsersAsync(ManagementReadScope scope, string? search, bool? active, PageWindow page, CancellationToken ct);
    Task<UserView?> UserAsync(ManagementReadScope scope, Guid id, CancellationToken ct);
    Task<ManagementPage<UserRoleView>> UserRolesAsync(ManagementReadScope scope, Guid userId, PageWindow page, CancellationToken ct);
    Task<ManagementPage<GrantView>> GrantsAsync(ManagementReadScope scope, Guid userId, Guid userRoleId, PageWindow page, CancellationToken ct);
    Task<IReadOnlyList<GrantFunctionView>> GrantFunctionsAsync(ManagementReadScope scope, Guid userId, Guid userRoleId, Guid grantId, CancellationToken ct);
    Task<ManagementPage<PermissionView>> PermissionsAsync(ManagementReadScope scope, PageWindow page, CancellationToken ct);
    Task<ManagementPage<AuditView>> AuditAsync(ManagementReadScope scope, string? action, PageWindow page, CancellationToken ct);
    Task<AuditView?> AuditEventAsync(ManagementReadScope scope, Guid id, CancellationToken ct);
    Task<ChangeRequestView?> RequestAsync(ManagementReadScope scope, Guid id, CancellationToken ct);
}
public sealed class ManagementQueryService(IManagementReadAuthority authority, IManagementQueries queries)
{
    public async Task<ManagementPage<UserView>> UsersAsync(string? search, bool? active, PageWindow page, CancellationToken ct)
    {
        page.Validate(); ValidateFilter(search);
        return await queries.UsersAsync(await authority.DemandAsync(ManagementReadOperation.Users, ct), search, active, page, ct);
    }
    public async Task<UserView?> UserAsync(Guid id, CancellationToken ct) =>
        await queries.UserAsync(await authority.DemandAsync(ManagementReadOperation.UserDetail, ct), id, ct);
    public async Task<ManagementPage<UserRoleView>> RolesAsync(Guid userId, PageWindow page, CancellationToken ct)
    {
        page.Validate(); return await queries.UserRolesAsync(await authority.DemandAsync(ManagementReadOperation.UserRoles, ct), userId, page, ct);
    }
    public async Task<ManagementPage<GrantView>> GrantsAsync(Guid userId, Guid userRoleId, PageWindow page, CancellationToken ct)
    {
        page.Validate(); return await queries.GrantsAsync(await authority.DemandAsync(ManagementReadOperation.StationGrants, ct), userId, userRoleId, page, ct);
    }
    public async Task<ManagementPage<PermissionView>> PermissionsAsync(PageWindow page, CancellationToken ct)
    {
        page.Validate(); return await queries.PermissionsAsync(await authority.DemandAsync(ManagementReadOperation.PermissionCatalog, ct), page, ct);
    }
    public async Task<ManagementPage<AuditView>> AuditAsync(string? action, PageWindow page, CancellationToken ct)
    {
        page.Validate(); ValidateFilter(action);
        return await queries.AuditAsync(await authority.DemandAsync(ManagementReadOperation.SecurityAudit, ct), action, page, ct);
    }
    public async Task<AuditView?> AuditEventAsync(Guid id, CancellationToken ct) =>
        await queries.AuditEventAsync(await authority.DemandAsync(ManagementReadOperation.SecurityAuditDetail, ct), id, ct);
    public async Task<ChangeRequestView?> RequestAsync(Guid id, CancellationToken ct) =>
        await queries.RequestAsync(await authority.DemandAsync(ManagementReadOperation.ChangeRequest, ct), id, ct);
    private static void ValidateFilter(string? text)
    { if (text?.Length > 80 || text?.Any(char.IsControl) == true) throw new ArgumentException("Invalid filter."); }
}
