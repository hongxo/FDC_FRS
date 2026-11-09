using System.Collections.Immutable;
using System.Security.Cryptography;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Tests;

public sealed class PolicyReleaseValidationTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-11-09T00:00:00Z");
    private static readonly AuthorityVersion Version = new(1, 1, 1, Guid.NewGuid());
    private static readonly ImmutableHashSet<string> Required = ["D04", "D09"];
    private static PolicyReleaseArtifact Artifact()
    {
        ImmutableArray<byte> payload = [1, 2, 3];
        return new("TEST-ONLY", "Testing", Required, Version, Now.AddMinutes(-1), Now.AddMinutes(1),
            payload, Convert.ToHexString(SHA256.HashData(payload.AsSpan())), "TEST-NOT-OWNER-APPROVAL");
    }
    private static PolicyReleaseValidator Validator(IPolicyReleaseEvidenceVerifier? verifier = null) =>
        new(verifier ?? new PendingPolicyReleaseEvidenceVerifier(), new Clock());

    [Fact] public async Task Complete_metadata_and_hash_never_replace_independent_evidence() =>
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => Validator().DemandValidAsync(Artifact(), "Testing", Required, Version, default));

    [Theory]
    [InlineData("release")] [InlineData("environment")] [InlineData("decisions")]
    [InlineData("policy")] [InlineData("catalog")] [InlineData("hierarchy")] [InlineData("epoch")]
    [InlineData("future")] [InlineData("expiry")] [InlineData("interval")]
    [InlineData("payload")] [InlineData("tamper")] [InlineData("hash")] [InlineData("evidence")]
    public async Task Incomplete_stale_tampered_or_wrong_environment_release_denied_before_trust(string field)
    {
        var a = Artifact();
        a = field switch
        {
            "release" => a with { ReleaseId = " " },
            "environment" => a with { Environment = "Production" },
            "decisions" => a with { Decisions = ["D04"] },
            "policy" => a with { Authority = Version with { Policy = 2 } },
            "catalog" => a with { Authority = Version with { Catalog = 2 } },
            "hierarchy" => a with { Authority = Version with { Hierarchy = 2 } },
            "epoch" => a with { Authority = Version with { RestoreEpoch = Guid.NewGuid() } },
            "future" => a with { NotBefore = Now.AddSeconds(1) },
            "expiry" => a with { ExpiresAt = Now },
            "interval" => a with { ExpiresAt = a.NotBefore },
            "payload" => a with { Payload = [] },
            "tamper" => a with { Payload = [4, 5, 6] },
            "hash" => a with { Sha256 = new string('Z', 64) },
            "evidence" => a with { EvidenceReference = " " },
            _ => throw new InvalidOperationException()
        };
        var verifier = new FixtureEvidence();
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => Validator(verifier).DemandValidAsync(a, "Testing", Required, Version, default));
        Assert.Equal(0, verifier.Calls);
    }
    [Fact] public async Task Empty_required_set_is_not_vacuously_approved() =>
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => Validator().DemandValidAsync(Artifact(), "Testing", ImmutableHashSet<string>.Empty, Version, default));

    [Theory] [InlineData(0, 1, 1)] [InlineData(1, 0, 1)] [InlineData(1, 1, 0)]
    public async Task Unconfigured_current_versions_denied(long policy, long catalog, long hierarchy)
    {
        var v = Version with { Policy = policy, Catalog = catalog, Hierarchy = hierarchy };
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => Validator().DemandValidAsync(Artifact() with { Authority = v }, "Testing", Required, v, default));
    }
    [Fact] public async Task Cancellation_does_not_call_verifier()
    {
        var verifier = new FixtureEvidence();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Validator(verifier).DemandValidAsync(Artifact(), "Testing", Required, Version, new CancellationToken(true)));
        Assert.Equal(0, verifier.Calls);
    }
    [Fact] public async Task Fixture_evidence_validates_metadata_only_pending_runtime_still_denies()
    {
        var verifier = new FixtureEvidence();
        await Validator(verifier).DemandValidAsync(Artifact(), "Testing", Required, Version, default);
        Assert.Equal(1, verifier.Calls);
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new PendingAuthenticationPolicy().GetAsync(default));
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new PendingMfaPolicy().DemandConfiguredAsync(default));
    }
    [Fact] public async Task Caller_supplied_case_insensitive_comparer_does_not_change_decision_ids()
    {
        var artifact = Artifact() with { Decisions = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "d04", "d09") };
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => Validator(new FixtureEvidence()).DemandValidAsync(artifact, "Testing", Required, Version, default));
    }
    [Fact] public async Task Release_expiring_while_evidence_is_verified_is_denied()
    {
        var clock = new MutableClock();
        var validator = new PolicyReleaseValidator(new AdvancingEvidence(clock), clock);
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => validator.DemandValidAsync(Artifact(), "Testing", Required, Version, default));
    }
    // Test double never registered in the application, never represents a real owner/approval actor.
    private sealed class FixtureEvidence : IPolicyReleaseEvidenceVerifier
    {
        public int Calls;
        public Task<bool> VerifyAsync(PolicyReleaseArtifact artifact, CancellationToken ct) { Calls++; return Task.FromResult(true); }
    }
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class MutableClock : TimeProvider
    {
        public DateTimeOffset Current = Now;
        public override DateTimeOffset GetUtcNow() => Current;
    }
    private sealed class AdvancingEvidence(MutableClock clock) : IPolicyReleaseEvidenceVerifier
    {
        public Task<bool> VerifyAsync(PolicyReleaseArtifact artifact, CancellationToken ct)
        { clock.Current = artifact.ExpiresAt; return Task.FromResult(true); }
    }
}
