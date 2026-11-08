using System.Collections.Immutable;
using Fdc.Identity.Infrastructure;

namespace Fdc.Identity.Application;

public enum RoleMutationOperation { SubmitRequest, ApplyReviewedRequest }
public sealed record RoleMutationActor(Guid UserId, string PersonKey, long PolicyVersion, long CatalogVersion,
    DateTimeOffset RequestExpiresAt, ImmutableHashSet<string> SupportedPermissions, string CorrelationId);
public interface IRoleMutationAuthority
{
    // Server session, fixed operation mapping, current primary authority, MFA/delegation and no-self-grant.
    Task<RoleMutationActor> DemandAsync(RoleMutationOperation operation, CancellationToken ct);
    // Re-resolve requester/reviewer/approver persons and authority, immutable revision, beneficiary impact and MFA.
    // Persisted ApprovalStatus alone is insufficient; this check runs inside the same D09 transaction.
    Task DemandReviewedRequestAsync(RoleMutationActor actor, RoleChangeRequestRow request, CancellationToken ct);
}
public sealed class PendingRoleMutationAuthority : IRoleMutationAuthority
{
    public Task<RoleMutationActor> DemandAsync(RoleMutationOperation operation, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D04/D05/D09: role mutation mapping, current session and commit authority");
    public Task DemandReviewedRequestAsync(RoleMutationActor actor, RoleChangeRequestRow request, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D03/D04/D09: independent current review, MFA and beneficiary impact");
}
public sealed record RoleRequestAccepted(Guid RequestId, long Revision, string Digest, string ApprovalStatus, string EffectiveStatus, bool Replayed);
public sealed record RoleApplyResult(Guid RequestId, Guid RoleId, long RoleVersion, bool Replayed);
