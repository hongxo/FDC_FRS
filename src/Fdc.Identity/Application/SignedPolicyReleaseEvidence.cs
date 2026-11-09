using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;

namespace Fdc.Identity.Application;

public sealed record ApprovalAttestation(string DecisionId, string Option, string ApproverId,
    string KeyId, ImmutableArray<byte> Signature);

public sealed record SignedPolicyEvidence(string Repository, string Ref, string CommitSha, string Scope,
    IamFeature Feature, long ApprovalVersion, string ActivatedBy, Guid ActivationAuditId,
    DateTimeOffset EffectiveFrom, DateTimeOffset ActivationExpiresAt, ImmutableArray<ApprovalAttestation> Attestations);

public sealed record ReviewedApprovalTarget(string Repository, string Ref, string CommitSha, string Scope,
    string Environment, ImmutableDictionary<string, string> ExpectedOptions);

public sealed record TrustedApproverKey(string ApproverId, ImmutableArray<byte> PublicKey,
    ImmutableHashSet<(string DecisionId, string Option)> Authority,
    DateTimeOffset ValidFrom, DateTimeOffset ValidUntil, bool Revoked);

public interface IApprovalTrustReader
{
    // Read current named approver authority and revocation from an independently controlled trust source.
    // The supplied key ID is only a lookup key; it cannot introduce trust.
    Task<TrustedApproverKey?> ReadAsync(string keyId, CancellationToken ct);
}

// Candidate protocol. No trust roots, deployment binding or approvals are registered by the Center host.
public sealed class SignedPolicyReleaseEvidenceVerifier(
    ReviewedApprovalTarget target, IApprovalTrustReader trust, TimeProvider clock) : IPolicyReleaseEvidenceVerifier
{
    public async Task<bool> VerifyAsync(PolicyReleaseArtifact artifact, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var signed = artifact.SignedEvidence;
        var verifiedAt = clock.GetUtcNow();
        if (artifact.Payload.IsDefaultOrEmpty || artifact.Sha256 is not { Length: 64 } ||
            !string.Equals(artifact.Sha256, Convert.ToHexString(SHA256.HashData(artifact.Payload.AsSpan())), StringComparison.OrdinalIgnoreCase) ||
            verifiedAt < artifact.NotBefore || verifiedAt >= artifact.ExpiresAt || artifact.Authority is null)
            return false;
        if (signed is null || signed.Repository != target.Repository || signed.Ref != target.Ref ||
            signed.CommitSha != target.CommitSha || signed.CommitSha is not { Length: 40 } ||
            !signed.CommitSha.All(char.IsAsciiHexDigit) || signed.Scope != target.Scope ||
            artifact.Environment != target.Environment || !Enum.IsDefined(signed.Feature) ||
            string.IsNullOrWhiteSpace(signed.Scope) || string.IsNullOrWhiteSpace(signed.ActivatedBy) ||
            signed.ActivationAuditId == Guid.Empty || signed.ApprovalVersion < 1 ||
            signed.EffectiveFrom < artifact.NotBefore || signed.ActivationExpiresAt > artifact.ExpiresAt ||
            signed.EffectiveFrom >= signed.ActivationExpiresAt || signed.Attestations.IsDefaultOrEmpty ||
            signed.Attestations.Length > 32 || artifact.Decisions is null ||
            signed.Attestations.Length != artifact.Decisions.Count)
            return false;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var approval in signed.Attestations)
        {
            if (approval is null || string.IsNullOrWhiteSpace(approval.DecisionId) ||
                !seen.Add(approval.DecisionId) || !artifact.Decisions.Any(id => id == approval.DecisionId) ||
                !target.ExpectedOptions.Any(option => option.Key == approval.DecisionId && option.Value == approval.Option) ||
                string.IsNullOrWhiteSpace(approval.ApproverId) || string.IsNullOrWhiteSpace(approval.KeyId) ||
                approval.ApproverId == signed.ActivatedBy || approval.Signature.Length != 64)
                return false;
            var key = await trust.ReadAsync(approval.KeyId, ct);
            ct.ThrowIfCancellationRequested();
            var now = clock.GetUtcNow();
            if (key is null || key.Revoked || key.ApproverId != approval.ApproverId ||
                now < key.ValidFrom || now >= key.ValidUntil || key.Authority is null ||
                !key.Authority.Contains((approval.DecisionId, approval.Option)) ||
                key.PublicKey.IsDefaultOrEmpty || key.PublicKey.Length > 256)
                return false;
            try
            {
                using var publicKey = ECDsa.Create();
                publicKey.ImportSubjectPublicKeyInfo(key.PublicKey.AsSpan(), out var consumed);
                if (consumed != key.PublicKey.Length || publicKey.ExportParameters(false).Curve.Oid.Value != "1.2.840.10045.3.1.7" ||
                    !publicKey.VerifyData(SigningBytes(artifact, signed, approval), approval.Signature.AsSpan(),
                        HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation))
                    return false;
            }
            catch (CryptographicException) { return false; }
        }
        return clock.GetUtcNow() < artifact.ExpiresAt;
    }

    // v1 canonical encoding. Ordered fields and invariant UTC ticks; signature bytes are excluded.
    public static byte[] SigningBytes(PolicyReleaseArtifact artifact, SignedPolicyEvidence signed, ApprovalAttestation approval) =>
        JsonSerializer.SerializeToUtf8Bytes(new
        {
            Protocol = "Fdc.Iam.FeatureActivation/v1",
            signed.Repository, signed.Ref, signed.CommitSha, signed.Scope, Feature = signed.Feature.ToString(),
            signed.ApprovalVersion, signed.ActivatedBy, signed.ActivationAuditId,
            EffectiveFrom = signed.EffectiveFrom.UtcTicks, ActivationExpiresAt = signed.ActivationExpiresAt.UtcTicks,
            artifact.ReleaseId, artifact.Environment, artifact.EvidenceReference, PayloadSha256 = artifact.Sha256,
            Decisions = artifact.Decisions.Order(StringComparer.Ordinal).ToArray(),
            artifact.Authority.Policy, artifact.Authority.Catalog, artifact.Authority.Hierarchy, artifact.Authority.RestoreEpoch,
            NotBefore = artifact.NotBefore.UtcTicks, ExpiresAt = artifact.ExpiresAt.UtcTicks,
            approval.DecisionId, approval.Option, approval.ApproverId, approval.KeyId
        });
}
