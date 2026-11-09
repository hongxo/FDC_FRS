using System.Collections.Frozen;
using System.Collections.Immutable;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Application;

public enum IamFeature
{
    AUTH_LOGIN, AUTH_MFA, AUTH_RECOVERY, AUTH_SESSION,
    IAM_USERS_READ, IAM_USERS_MUTATE, IAM_ROLES_READ, IAM_ROLES_MUTATE,
    IAM_PERMISSIONS_READ, IAM_PERMISSIONS_MUTATE, IAM_STATION_GRANTS_READ,
    IAM_STATION_GRANTS_MUTATE, IAM_APPROVAL, IAM_AUDIT_READ, IAM_AUDIT_ARCHIVE
}

public enum FeatureActivationStatus
{
    DISABLED, IMPLEMENTED_NOT_AUTHORIZED, APPROVED_NOT_ACTIVATED, ACTIVE, EXPIRED, REVOKED, BLOCKED_BY_POLICY
}

public sealed record FeatureActivationSnapshot(
    IamFeature FeatureKey,
    string ApprovalScope,
    long ApprovalVersion,
    PolicyReleaseArtifact Approval,
    AuthorityVersion CurrentAuthority,
    bool Enabled,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? ActivatedBy,
    Guid? ActivationAuditId);

public interface IFeatureActivationReader
{
    // A trusted authority must bind the signed release to this exact feature, scope and deployment.
    // Every update to activation or approval metadata must advance ApprovalVersion.
    // Read current versions, revocation and committed activation audit together from the writable authority.
    Task<FeatureActivationSnapshot?> ReadAsync(IamFeature feature, CancellationToken ct);
}

public sealed class PendingFeatureActivationReader : IFeatureActivationReader
{
    public Task<FeatureActivationSnapshot?> ReadAsync(IamFeature feature, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult<FeatureActivationSnapshot?>(null);
    }
}

// A release enables a code path. It never supplies session, RBAC, object-scope or transaction authority.
public sealed class FeatureActivationGate(IFeatureActivationReader reader, PolicyReleaseValidator validator, TimeProvider clock)
{
    private static readonly FrozenDictionary<IamFeature, ImmutableHashSet<string>> Decisions =
        new Dictionary<IamFeature, string[]>
        {
            [IamFeature.AUTH_LOGIN] = ["D01", "D02", "D03", "D05", "D06", "D09", "D10", "D12"],
            [IamFeature.AUTH_MFA] = ["D01", "D03", "D05", "D06", "D09", "D10", "D12"],
            [IamFeature.AUTH_RECOVERY] = ["D01", "D02", "D03", "D05", "D06", "D09", "D10", "D12"],
            [IamFeature.AUTH_SESSION] = ["D01", "D05", "D09", "D10", "D12"],
            [IamFeature.IAM_USERS_READ] = ["D01", "D04", "D05", "D12"],
            [IamFeature.IAM_USERS_MUTATE] = ["D01", "D04", "D05", "D09", "D10", "D12"],
            [IamFeature.IAM_ROLES_READ] = ["D01", "D04", "D05", "D12"],
            [IamFeature.IAM_ROLES_MUTATE] = ["D01", "D04", "D05", "D09", "D10", "D12"],
            [IamFeature.IAM_PERMISSIONS_READ] = ["D01", "D04", "D05", "D12"],
            [IamFeature.IAM_PERMISSIONS_MUTATE] = ["D01", "D04", "D05", "D09", "D10", "D12"],
            [IamFeature.IAM_STATION_GRANTS_READ] = ["D01", "D04", "D05", "D12"],
            [IamFeature.IAM_STATION_GRANTS_MUTATE] = ["D01", "D04", "D05", "D08", "D09", "D10", "D12"],
            [IamFeature.IAM_APPROVAL] = ["D01", "D04", "D05", "D08", "D09", "D10", "D12"],
            [IamFeature.IAM_AUDIT_READ] = ["D01", "D04", "D05", "D10", "D12"],
            [IamFeature.IAM_AUDIT_ARCHIVE] = ["D09", "D10", "E06", "D12"]
        }.ToFrozenDictionary(entry => entry.Key, entry => entry.Value.ToImmutableHashSet(StringComparer.Ordinal));

    public async Task<FeatureActivationStatus> EvaluateAsync(IamFeature feature, string environment, string scope,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!Decisions.TryGetValue(feature, out var decisions) ||
            environment is not ("Development" or "Lab" or "Testing") || string.IsNullOrWhiteSpace(scope))
            return FeatureActivationStatus.BLOCKED_BY_POLICY;

        try
        {
            var snapshot = await reader.ReadAsync(feature, ct);
            if (snapshot is null || snapshot.FeatureKey != feature ||
                !string.Equals(snapshot.ApprovalScope, scope, StringComparison.Ordinal) ||
                snapshot.ApprovalVersion < 1 || snapshot.Approval is null || snapshot.CurrentAuthority is null)
                return FeatureActivationStatus.BLOCKED_BY_POLICY;
            var now = clock.GetUtcNow();
            if (snapshot.RevokedAt is not null) return FeatureActivationStatus.REVOKED;
            if (now >= snapshot.ExpiresAt || now >= snapshot.Approval.ExpiresAt) return FeatureActivationStatus.EXPIRED;
            if (snapshot.EffectiveFrom < snapshot.Approval.NotBefore || snapshot.ExpiresAt > snapshot.Approval.ExpiresAt ||
                snapshot.EffectiveFrom >= snapshot.ExpiresAt)
                return FeatureActivationStatus.BLOCKED_BY_POLICY;

            await validator.DemandValidAsync(snapshot.Approval, environment, decisions, snapshot.CurrentAuthority, ct);
            // Re-read from authority after external evidence verification to fence revision or revocation changes.
            var current = await reader.ReadAsync(feature, ct);
            ct.ThrowIfCancellationRequested();
            if (current is null || current.FeatureKey != feature) return FeatureActivationStatus.BLOCKED_BY_POLICY;
            if (current.RevokedAt is not null) return FeatureActivationStatus.REVOKED;
            if (current.ApprovalVersion != snapshot.ApprovalVersion || current.CurrentAuthority != snapshot.CurrentAuthority ||
                current.ApprovalScope != snapshot.ApprovalScope || current.Approval is null ||
                current.Approval.ReleaseId != snapshot.Approval.ReleaseId || current.Approval.Sha256 != snapshot.Approval.Sha256 ||
                current.Enabled != snapshot.Enabled || current.EffectiveFrom != snapshot.EffectiveFrom ||
                current.ExpiresAt != snapshot.ExpiresAt || current.ActivatedBy != snapshot.ActivatedBy ||
                current.ActivationAuditId != snapshot.ActivationAuditId)
                return FeatureActivationStatus.BLOCKED_BY_POLICY;

            now = clock.GetUtcNow();
            if (now >= current.ExpiresAt) return FeatureActivationStatus.EXPIRED;
            if (!current.Enabled || now < current.EffectiveFrom) return FeatureActivationStatus.APPROVED_NOT_ACTIVATED;
            return !string.IsNullOrWhiteSpace(current.ActivatedBy) &&
                current.ActivationAuditId is Guid auditId && auditId != Guid.Empty
                ? FeatureActivationStatus.ACTIVE : FeatureActivationStatus.BLOCKED_BY_POLICY;
        }
        catch (Exception error) when (error is OwnerDecisionRequiredException or AuthorityUnavailableException or UnauthorizedAccessException)
        {
            return FeatureActivationStatus.BLOCKED_BY_POLICY;
        }
    }

    public async Task DemandActiveAsync(IamFeature feature, string environment, string scope, CancellationToken ct)
    {
        if (await EvaluateAsync(feature, environment, scope, ct) != FeatureActivationStatus.ACTIVE)
            throw new OwnerDecisionRequiredException("IAM feature activation is blocked by policy.");
    }
}
