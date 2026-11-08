namespace Fdc.Identity.Application;

// Factor providers and custody policies are not selected by these contracts. No fake OTP/provider.
public sealed record MfaBinding(Guid UserId, Guid SessionId, string Purpose, string Action,
    string ObjectRevision, DateTimeOffset ExpiresAt);
public sealed record MfaVerification(bool Verified, DateTimeOffset? VerifiedAt, string Assurance);
public interface IMfaPolicyEvaluator { Task DemandConfiguredAsync(CancellationToken ct); }
public interface IMfaService { Task<MfaVerification> VerifyAsync(MfaBinding binding, string proof, CancellationToken ct); }
public interface IWebAuthnService { Task<MfaVerification> VerifyAssertionAsync(MfaBinding binding, ReadOnlyMemory<byte> assertion, CancellationToken ct); }
public interface ITotpService { Task<MfaVerification> VerifyAsync(MfaBinding binding, string code, CancellationToken ct); }
public interface IMfaChallengeStore
{
    // Implementation must atomically consume a single-use proof, bind all fields, and append audit/outbox.
    Task<bool> ConsumeAsync(MfaBinding expected, string proofDigest, CancellationToken ct);
}
public sealed class PendingMfaPolicy : IMfaPolicyEvaluator
{
    public Task DemandConfiguredAsync(CancellationToken ct) => throw new OwnerDecisionRequiredException("D03/D06: MFA and independent recovery custody");
}
