using Fdc.Identity.Application;

namespace Fdc.Identity.Tests;

public sealed class AssignmentPolicyTests
{
    [Fact] public async Task Unapproved_assignment_conflict_policy_does_not_grant()
    {
        var now = DateTimeOffset.UtcNow;
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new PendingRoleAssignmentConflictPolicy().DemandCompatibleAsync(
            new(Guid.NewGuid(), Guid.NewGuid(), now, now.AddHours(1), 1, 1, "synthetic"), [], default));
    }
    [Fact] public async Task Station_scope_is_not_administrative_delegation()
    {
        var now = DateTimeOffset.UtcNow;
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new PendingAdministrativeDelegationPolicy().DemandProposalAsync(
            new(Guid.NewGuid(), "station-like-text-does-not-grant-global", ["synthetic.permission"], now, now.AddHours(1), 1, "synthetic"), default));
    }
}
