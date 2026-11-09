using System.Collections.Immutable;
using System.Data;
using Fdc.Identity.Application;

namespace Fdc.Identity.Tests;

public sealed class AuthorityCommitIntentTests
{
    private static readonly Guid Actor = Guid.NewGuid(), Target = Guid.NewGuid();
    private static AuthorityCommitIntent Intent() => new(Actor, IdentityOperation.UserRoleRevoke,
        "TEST-TARGET", new string('A', 64), AuthorityImpact.BoundedUserMutation, [Target]);
    private static SqlCommitGatePlan Plan() => new("TEST.ONLY", false,
        [new(Actor, false), new(Target, true)], 1000, IsolationLevel.Serializable);

    [Fact] public void Missing_affected_user_cannot_use_actor_only_plan() =>
        Assert.Throws<InvalidOperationException>(() => Intent().DemandCoveredBy(Plan() with { Users = [new(Actor, false)] }));
    [Fact] public void Shared_target_gate_is_not_enough_for_mutation() =>
        Assert.Throws<InvalidOperationException>(() => Intent().DemandCoveredBy(Plan() with { Users = [new(Actor, false), new(Target, false)] }));
    [Fact] public void Global_impact_requires_exclusive_policy_from_start() =>
        Assert.Throws<InvalidOperationException>(() => (Intent() with { Impact = AuthorityImpact.PolicyMutation }).DemandCoveredBy(Plan()));
    [Fact] public void Policy_plan_still_needs_every_explicit_target_exclusive() =>
        Assert.Throws<InvalidOperationException>(() => (Intent() with { Impact = AuthorityImpact.PolicyMutation }).DemandCoveredBy(Plan() with { ExclusivePolicy = true, Users = [new(Actor, false)] }));
    [Fact] public void Exact_coverage_normalizes_strongest_duplicate_without_granting_authority()
    {
        var result = Intent().DemandCoveredBy(Plan() with { Users = [new(Actor, false), new(Target, false), new(Target, true)] });
        Assert.Equal(2, result.Users.Length);
        Assert.True(result.Users.Single(x => x.UserId == Target).Exclusive);
    }
    [Theory] [InlineData("actor")] [InlineData("operation")] [InlineData("impact")]
    [InlineData("target")] [InlineData("digest")] [InlineData("affected")]
    public void Incomplete_or_unknown_intent_denied(string field)
    {
        var i = Intent();
        i = field switch
        {
            "actor" => i with { ActorId = Guid.Empty },
            "operation" => i with { Operation = (IdentityOperation)int.MaxValue },
            "impact" => i with { Impact = (AuthorityImpact)int.MaxValue },
            "target" => i with { TargetReference = " " },
            "digest" => i with { RevisionDigest = "client-checkbox" },
            "affected" => i with { AffectedUsers = ImmutableHashSet<Guid>.Empty },
            _ => throw new InvalidOperationException()
        };
        Assert.Throws<ArgumentException>(() => i.DemandCoveredBy(Plan()));
    }
}
