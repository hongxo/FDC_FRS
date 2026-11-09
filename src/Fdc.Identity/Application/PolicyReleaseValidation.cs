using System.Collections.Immutable;
using System.Security.Cryptography;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Application;

// A candidate input contract, not an approval format or an authorization grant.
// The verifier must obtain trust from independent owner evidence, not a JSON APPROVED flag.
public sealed record PolicyReleaseArtifact(string ReleaseId, string Environment,
    ImmutableHashSet<string> Decisions, AuthorityVersion Authority,
    DateTimeOffset NotBefore, DateTimeOffset ExpiresAt,
    ImmutableArray<byte> Payload, string Sha256, string EvidenceReference);

public interface IPolicyReleaseEvidenceVerifier
{
    // Verify signature/trusted owner/scope AND the exact artifact hash, decisions, environment,
    // authority versions and validity interval. A stored path or self-asserted hash is not evidence.
    Task<bool> VerifyAsync(PolicyReleaseArtifact artifact, CancellationToken ct);
}

public sealed class PendingPolicyReleaseEvidenceVerifier : IPolicyReleaseEvidenceVerifier
{
    public Task<bool> VerifyAsync(PolicyReleaseArtifact artifact, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(false);
    }
}

public sealed class PolicyReleaseValidator(IPolicyReleaseEvidenceVerifier evidence, TimeProvider clock)
{
    // Pure validation building block, deliberately not registered as an effective policy provider.
    // Callers must supply required decisions/current versions from trusted server context.
    public async Task DemandValidAsync(PolicyReleaseArtifact artifact, string environment,
        IReadOnlySet<string> requiredDecisions, AuthorityVersion current, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(requiredDecisions);
        var now = clock.GetUtcNow();
        if (string.IsNullOrWhiteSpace(environment) || string.IsNullOrWhiteSpace(artifact.ReleaseId) ||
            artifact.Environment != environment || requiredDecisions.Count == 0 ||
            artifact.Decisions is null || !requiredDecisions.All(required =>
                artifact.Decisions.Any(value => string.Equals(required, value, StringComparison.Ordinal))) ||
            artifact.Authority != current || current.Policy <= 0 || current.Catalog <= 0 ||
            current.Hierarchy <= 0 || current.RestoreEpoch == Guid.Empty ||
            now < artifact.NotBefore || now >= artifact.ExpiresAt || artifact.NotBefore >= artifact.ExpiresAt ||
            artifact.Payload.IsDefaultOrEmpty || string.IsNullOrWhiteSpace(artifact.EvidenceReference) ||
            artifact.Sha256 is not { Length: 64 } ||
            !artifact.Sha256.All(char.IsAsciiHexDigit))
            throw new OwnerDecisionRequiredException("Incomplete, stale or out-of-scope policy release");
        var digest = Convert.ToHexString(SHA256.HashData(artifact.Payload.AsSpan()));
        if (!string.Equals(digest, artifact.Sha256, StringComparison.OrdinalIgnoreCase) ||
            !await evidence.VerifyAsync(artifact, ct))
            throw new OwnerDecisionRequiredException("Trusted evidence for the exact policy release is required");
        ct.ThrowIfCancellationRequested();
        var verifiedAt = clock.GetUtcNow();
        if (verifiedAt < artifact.NotBefore || verifiedAt >= artifact.ExpiresAt)
            throw new OwnerDecisionRequiredException("Policy release validity changed during evidence verification");
        // Validation is not an ALLOW, does not seed/configure anything and cannot bypass pending providers.
    }
}
