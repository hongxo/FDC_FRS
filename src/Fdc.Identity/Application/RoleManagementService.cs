using System.Collections.Immutable;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Application;

public sealed record RoleReadScope(ImmutableArray<Guid> RoleIds, ImmutableArray<Guid> VisibleUserIds);
public interface IRoleAdministrationAuthority
{
    // Must resolve a REAL current Human session and the owner-approved operation mapping/delegation.
    Task<RoleReadScope> DemandReadAsync(CancellationToken ct);
}
public sealed class PendingD04RoleAuthority : IRoleAdministrationAuthority
{
    public Task<RoleReadScope> DemandReadAsync(CancellationToken ct)
        => throw new OwnerDecisionRequiredException("D04/D05: role administration mapping and current Human session");
}
public sealed record RoleView(Guid Id, string Code, string Name, string Description, bool IsSystemSeed, bool Active, long Version);
public sealed record RolePage(IReadOnlyList<RoleView> Items, string? NextCode);
public interface IRoleQueries
{
    Task<RolePage> ListAsync(RoleReadScope scope, string? afterCode, int limit, CancellationToken ct);
    Task<RoleView?> DetailAsync(RoleReadScope scope, Guid roleId, CancellationToken ct);
    Task<IReadOnlyList<string>> PermissionsAsync(RoleReadScope scope, Guid roleId, CancellationToken ct);
}
public sealed class RoleManagementService(IRoleAdministrationAuthority authority, IRoleQueries queries)
{
    public async Task<RolePage> ListAsync(string? afterCode, int limit, CancellationToken ct)
    {
        if (limit is < 1 or > 100 || afterCode?.Length > 80) throw new ArgumentException("Invalid page bounds.");
        return await queries.ListAsync(await authority.DemandReadAsync(ct), afterCode, limit, ct);
    }
    public async Task<RoleView?> DetailAsync(Guid roleId, CancellationToken ct)
        => await queries.DetailAsync(await authority.DemandReadAsync(ct), roleId, ct);
    public async Task<IReadOnlyList<string>> PermissionsAsync(Guid roleId, CancellationToken ct)
        => await queries.PermissionsAsync(await authority.DemandReadAsync(ct), roleId, ct);
}
