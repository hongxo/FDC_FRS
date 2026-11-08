using Fdc.Identity.Domain;
using Fdc.Identity.Security;

namespace Fdc.Identity.Application;

public interface ICurrentAuthorityReader
{
    // Server-resolved object ID and session handle digest only, never client-supplied roles/stations/MFA flags.
    Task<(AuthorizationSnapshot Snapshot, TrustedResource? Resource)?> ReadAsync(string sessionDigest, string? objectId, CancellationToken ct);
}
public sealed class AuthorityUnavailableException : Exception;
public sealed class OwnerDecisionRequiredException(string decision) : Exception($"Owner disposition required: {decision}");
public sealed class AuthorizationService(ICurrentAuthorityReader reader, PermissionEvaluator evaluator, TimeProvider clock)
{
    public async Task<AccessDecision> DemandAsync(string sessionDigest, string permissionCode, CredentialKind credential,
        string? objectId, string? administrativeTarget, WorkflowContext? workflow, CancellationToken ct)
    {
        if (credential != CredentialKind.HumanSession) return AccessDecision.Deny("HUMAN_SCHEME_REQUIRED");
        if (string.IsNullOrWhiteSpace(sessionDigest)) return AccessDecision.Deny("SESSION_REQUIRED");
        try
        {
            var current = await reader.ReadAsync(sessionDigest, objectId, ct);
            if (current is null) return AccessDecision.Deny("AUTHORITY_UNAVAILABLE");
            return evaluator.Evaluate(current.Value.Snapshot,
                new(permissionCode, credential, current.Value.Resource, administrativeTarget, workflow), clock.GetUtcNow());
        }
        catch (AuthorityUnavailableException) { return AccessDecision.Deny("AUTHORITY_UNAVAILABLE"); }
    }
}

// This contract must hold D09's serialization boundary through authorization, mutation and audit commit.
public interface IAuthorizedCommitBoundary
{
    Task<T> CommitAsync<T>(Guid actorUserId, Func<CancellationToken, Task<T>> recheckMutateAndAudit, CancellationToken ct);
}
public sealed class PendingD09CommitBoundary : IAuthorizedCommitBoundary
{
    public Task<T> CommitAsync<T>(Guid actorUserId, Func<CancellationToken, Task<T>> recheckMutateAndAudit, CancellationToken ct)
        => throw new OwnerDecisionRequiredException("D09: commit/revoke serialization");
}
