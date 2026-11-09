using System.Collections.Immutable;
using System.Security.Cryptography;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Tests;

public sealed class FeatureActivationGateTests
{
    private const string Scope = "isolated-fixture";
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 0, 0, 0, TimeSpan.Zero);
    private static readonly AuthorityVersion Authority = new(1, 1, 1, Guid.NewGuid());
    private static readonly Clock Time = new();

    [Fact]
    public async Task Every_feature_defaults_to_blocked_without_trusted_records()
    {
        var gate = Make(new PendingFeatureActivationReader());
        Assert.Equal(15, Enum.GetValues<IamFeature>().Length);
        foreach (var feature in Enum.GetValues<IamFeature>())
            Assert.Equal(FeatureActivationStatus.BLOCKED_BY_POLICY, await gate.EvaluateAsync(feature, "Testing", Scope, default));
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() =>
            gate.DemandActiveAsync(IamFeature.AUTH_LOGIN, "Testing", Scope, default));
    }

    [Theory]
    [InlineData("missing-decisions")]
    [InlineData("wrong-feature")]
    [InlineData("wrong-scope")]
    [InlineData("invalid-digest")]
    [InlineData("stale-authority")]
    [InlineData("missing-audit")]
    [InlineData("missing-actor")]
    [InlineData("production")]
    [InlineData("wrong-environment")]
    public async Task Invalid_activation_cannot_enable_runtime(string fault)
    {
        var valid = Snapshot();
        var candidate = fault switch
        {
            "missing-decisions" => valid with { Approval = valid.Approval with { Decisions = ["D12"] } },
            "wrong-feature" => valid with { FeatureKey = IamFeature.AUTH_RECOVERY },
            "wrong-scope" => valid with { ApprovalScope = "other" },
            "invalid-digest" => valid with { Approval = valid.Approval with { Sha256 = new string('a', 64) } },
            "stale-authority" => valid with { CurrentAuthority = Authority with { Policy = 2 } },
            "missing-audit" => valid with { ActivationAuditId = null },
            "missing-actor" => valid with { ActivatedBy = null },
            "wrong-environment" => valid with { Approval = valid.Approval with { Environment = "Lab" } },
            _ => valid
        };
        var gate = Make(new Reader(candidate), new FixtureEvidence());
        Assert.Equal(FeatureActivationStatus.BLOCKED_BY_POLICY, await gate.EvaluateAsync(IamFeature.AUTH_LOGIN,
            fault == "production" ? "Production" : "Testing", Scope, default));
    }

    [Fact]
    public async Task Self_asserted_record_cannot_replace_independent_evidence()
    {
        var gate = Make(new Reader(Snapshot()));
        Assert.Equal(FeatureActivationStatus.BLOCKED_BY_POLICY, await gate.EvaluateAsync(IamFeature.AUTH_LOGIN, "Testing", Scope, default));
    }

    [Fact]
    public async Task Validated_release_still_requires_current_committed_activation()
    {
        var valid = Snapshot();
        Assert.Equal(FeatureActivationStatus.APPROVED_NOT_ACTIVATED, await Evaluate(new Reader(valid with { Enabled = false })));
        Assert.Equal(FeatureActivationStatus.ACTIVE, await Evaluate(new Reader(valid)));
        Assert.Equal(FeatureActivationStatus.EXPIRED, await Evaluate(new Reader(valid with { ExpiresAt = Now })));
        Assert.Equal(FeatureActivationStatus.REVOKED, await Evaluate(new Reader(valid with { RevokedAt = Now })));
    }

    [Fact]
    public async Task Changes_during_verification_are_detected()
    {
        var snapshot = Snapshot();
        Assert.Equal(FeatureActivationStatus.REVOKED, await Evaluate(new Reader(snapshot, snapshot with { RevokedAt = Now })));
        Assert.Equal(FeatureActivationStatus.BLOCKED_BY_POLICY, await Evaluate(new Reader(snapshot, snapshot with { ApprovalVersion = 2 })));
    }

    private static FeatureActivationSnapshot Snapshot()
    {
        ImmutableArray<byte> payload = [1, 2, 3];
        var artifact = new PolicyReleaseArtifact("fixture", "Testing", ["D01", "D02", "D03", "D05", "D06", "D09", "D10", "D12"],
            Authority, Now.AddMinutes(-1), Now.AddMinutes(1), payload,
            Convert.ToHexString(SHA256.HashData(payload.AsSpan())), "fixture-only");
        return new(IamFeature.AUTH_LOGIN, Scope, 1, artifact, Authority, true,
            Now.AddSeconds(-30), Now.AddSeconds(30), null, "fixture", Guid.NewGuid());
    }

    private static Task<FeatureActivationStatus> Evaluate(IFeatureActivationReader reader) =>
        Make(reader, new FixtureEvidence()).EvaluateAsync(IamFeature.AUTH_LOGIN, "Testing", Scope, default);
    private static FeatureActivationGate Make(IFeatureActivationReader reader, IPolicyReleaseEvidenceVerifier? verifier = null) =>
        new(reader, new(verifier ?? new PendingPolicyReleaseEvidenceVerifier(), Time), Time);

    private sealed class Reader(FeatureActivationSnapshot value, FeatureActivationSnapshot? next = null) : IFeatureActivationReader
    {
        private int reads;
        public Task<FeatureActivationSnapshot?> ReadAsync(IamFeature feature, CancellationToken ct) =>
            Task.FromResult<FeatureActivationSnapshot?>(reads++ > 0 ? next ?? value : value);
    }

    private sealed class FixtureEvidence : IPolicyReleaseEvidenceVerifier
    {
        public Task<bool> VerifyAsync(PolicyReleaseArtifact artifact, CancellationToken ct) => Task.FromResult(true);
    }

    private sealed class Clock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
