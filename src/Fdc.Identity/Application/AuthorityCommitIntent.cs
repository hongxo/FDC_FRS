using System.Collections.Immutable;

namespace Fdc.Identity.Application;

public enum AuthorityImpact { BoundedUserMutation, PolicyMutation }

// Internal candidate contract only: never bind from HTTP JSON. Server resolution of affected
// users must occur under the policy gate before accepting a bounded impact set.
// Not wired into the pending D09 provider or any effective writer.
public sealed record AuthorityCommitIntent(Guid ActorId, IdentityOperation Operation,
    string TargetReference, string RevisionDigest, AuthorityImpact Impact,
    ImmutableHashSet<Guid> AffectedUsers)
{
    public SqlCommitGatePlan DemandCoveredBy(SqlCommitGatePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (ActorId == Guid.Empty || !Enum.IsDefined(Operation) || !Enum.IsDefined(Impact) ||
            string.IsNullOrWhiteSpace(TargetReference) ||
            RevisionDigest is not { Length: 64 } || !RevisionDigest.All(char.IsAsciiHexDigit) ||
            AffectedUsers is null || AffectedUsers.Contains(Guid.Empty) ||
            (Impact == AuthorityImpact.BoundedUserMutation && AffectedUsers.Count == 0))
            throw new ArgumentException("Complete server-derived authority intent required.");
        var normalized = plan.Normalize(ActorId);
        if (Impact == AuthorityImpact.PolicyMutation && !normalized.ExclusivePolicy)
            throw new InvalidOperationException("Policy impact requires an exclusive policy gate.");
        if (AffectedUsers.Any(id => !normalized.Users.Any(x => x.UserId == id && x.Exclusive)))
            throw new InvalidOperationException("Every affected user requires an exclusive gate from the start.");
        return normalized;
    }
}
