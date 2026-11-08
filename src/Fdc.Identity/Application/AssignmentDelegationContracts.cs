using System.Collections.Immutable;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Application;

public sealed record UserRoleProposal(Guid UserId, Guid RoleId, DateTimeOffset From, DateTimeOffset Until,
    long ExpectedUserGrantVersion, long ExpectedRoleVersion, string Reason);
public interface IRoleAssignmentConflictPolicy
{
    // Approved capability/SoD rules, not role-name comparisons. Must execute against current server state.
    Task DemandCompatibleAsync(UserRoleProposal proposal, ImmutableArray<RoleAssignment> currentAssignments, CancellationToken ct);
}
public sealed class PendingRoleAssignmentConflictPolicy : IRoleAssignmentConflictPolicy
{
    public Task DemandCompatibleAsync(UserRoleProposal proposal, ImmutableArray<RoleAssignment> currentAssignments, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D04: role assignment conflict/delegation capability policy");
}
public sealed record AdministrativeDelegationProposal(Guid UserRoleId, string TargetPopulation,
    ImmutableArray<string> Functions, DateTimeOffset From, DateTimeOffset Until, long ExpectedVersion, string Reason);
public interface IAdministrativeDelegationPolicy
{
    // Separate administrative envelope: a StationGrant must never be accepted as a substitute.
    Task DemandProposalAsync(AdministrativeDelegationProposal proposal, CancellationToken ct);
}
public sealed class PendingAdministrativeDelegationPolicy : IAdministrativeDelegationPolicy
{
    public Task DemandProposalAsync(AdministrativeDelegationProposal proposal, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D04/D06: independent administrative delegation scope/custody");
}
