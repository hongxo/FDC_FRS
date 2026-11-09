using System.Collections.Immutable;
using System.Security.Cryptography;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Tests;

public sealed class SignedPolicyReleaseEvidenceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Clock Time = new();
    private static readonly ReviewedApprovalTarget Target = new("fixture/repo", "refs/heads/fixture",
        new string('a', 40), "fixture-isolated", "Testing",
        ImmutableDictionary<string, string>.Empty.Add("D02", "B"));

    [Theory]
    [InlineData("valid")]
    [InlineData("unsigned")]
    [InlineData("signature")]
    [InlineData("payload")]
    [InlineData("scope")]
    [InlineData("ref")]
    [InlineData("sha")]
    [InlineData("environment")]
    [InlineData("option")]
    [InlineData("signer")]
    [InlineData("self")]
    [InlineData("expiry")]
    [InlineData("revoked")]
    [InlineData("untrusted")]
    public async Task Exact_signed_release_requires_current_authorized_approver(string fault)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        ImmutableArray<byte> payload = [1, 2, 3];
        var authority = new AuthorityVersion(1, 1, 1, Guid.NewGuid());
        var artifact = new PolicyReleaseArtifact("fixture-release", Target.Environment, ["D02"], authority,
            Now.AddMinutes(-1), Now.AddMinutes(1), payload, Convert.ToHexString(SHA256.HashData(payload.AsSpan())), "fixture-evidence");
        var approval = new ApprovalAttestation("D02", "B", "fixture-owner", "fixture-key", []);
        var signed = new SignedPolicyEvidence(Target.Repository, Target.Ref, Target.CommitSha, Target.Scope,
            IamFeature.AUTH_LOGIN, 1, "fixture-operator", Guid.NewGuid(), Now, Now.AddSeconds(30), [approval]);
        approval = approval with { Signature = ImmutableArray.Create(key.SignData(
            SignedPolicyReleaseEvidenceVerifier.SigningBytes(artifact, signed, approval), HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation)) };
        signed = signed with { Attestations = [approval] };
        var trustedKey = new TrustedApproverKey(approval.ApproverId, ImmutableArray.Create(key.ExportSubjectPublicKeyInfo()),
            [("D02", "B")], Now.AddMinutes(-1), Now.AddMinutes(1), false);
        artifact = artifact with { SignedEvidence = fault switch
        {
            "unsigned" => null,
            "scope" => signed with { Scope = "another" },
            "ref" => signed with { Ref = "refs/heads/another" },
            "sha" => signed with { CommitSha = new string('b', 40) },
            "signature" => signed with { Attestations = [approval with { Signature = approval.Signature.SetItem(0, (byte)(approval.Signature[0] ^ 1)) }] },
            "option" => signed with { Attestations = [approval with { Option = "A" }] },
            "signer" => signed with { Attestations = [approval with { ApproverId = "another" }] },
            "self" => signed with { ActivatedBy = approval.ApproverId },
            _ => signed
        }};
        if (fault == "payload") artifact = artifact with { Payload = [3, 2, 1] };
        if (fault == "environment") artifact = artifact with { Environment = "Production" };
        var verifier = new SignedPolicyReleaseEvidenceVerifier(Target, new Reader(fault switch
        {
            "expiry" => trustedKey with { ValidUntil = Now },
            "revoked" => trustedKey with { Revoked = true },
            "untrusted" => null,
            _ => trustedKey
        }), Time);
        Assert.Equal(fault == "valid", await verifier.VerifyAsync(artifact, default));
    }

    private sealed class Reader(TrustedApproverKey? key) : IApprovalTrustReader
    {
        public Task<TrustedApproverKey?> ReadAsync(string keyId, CancellationToken ct) => Task.FromResult(key);
    }

    private sealed class Clock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
