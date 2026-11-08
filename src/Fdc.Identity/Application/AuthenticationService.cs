using Fdc.Identity.Security;
using Microsoft.AspNetCore.Identity;

namespace Fdc.Identity.Application;

// Values are supplied by the host's reviewed policy provider; there are deliberately no default policies.
public sealed record AuthenticationPolicy(TimeSpan Idle, TimeSpan Absolute, int MaximumSessions,
    int FailureLimit, TimeSpan FailureWindow, TimeSpan Lockout, string Version)
{
    public void Validate()
    {
        if (Idle <= TimeSpan.Zero || Absolute < Idle || MaximumSessions < 1 || FailureLimit < 1 ||
            FailureWindow <= TimeSpan.Zero || Lockout <= TimeSpan.Zero || string.IsNullOrWhiteSpace(Version))
            throw new ArgumentException("Incomplete authentication policy.");
    }
}
public interface IAuthenticationPolicyProvider { Task<AuthenticationPolicy> GetAsync(CancellationToken ct); }
public sealed class PendingAuthenticationPolicy : IAuthenticationPolicyProvider
{
    public Task<AuthenticationPolicy> GetAsync(CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D02/D03/D05/D06: credential, MFA, session and recovery policies");
}
public sealed record CredentialCandidate(Guid UserId, bool Active, string EncodedHash, bool LockedOut);
public enum LoginOutcome { Rejected, MfaRequired, SessionCreated, Unavailable }
// Never serialize this record directly to logs: proof is a response secret.
public sealed class LoginResult(LoginOutcome outcome, string? proof = null)
{
    public LoginOutcome Outcome { get; } = outcome;
    public string? Proof { get; } = proof;
    public override string ToString() => $"LoginResult({Outcome})";
}
public sealed record SessionIssue(Guid UserId, string Digest, DateTimeOffset IdleUntil, DateTimeOffset AbsoluteUntil);
public interface ILoginTransaction
{
    Task<CredentialCandidate?> ReadForUpdateAsync(string normalizedUsername, CancellationToken ct);
    Task<bool> IsSourceLimitedAsync(string trustedSourceKey, AuthenticationPolicy policy, CancellationToken ct);
    Task RecordFailureAsync(Guid? userId, string trustedSourceKey, AuthenticationPolicy policy, CancellationToken ct);
    // Must check current account/versions/session cap, reset failures, and persist audit/outbox atomically.
    // Never issue a usable session before approved MFA policy has been satisfied. May return an MFA challenge proof instead.
    Task<LoginResult> CompletePasswordProofAsync(SessionIssue proposed, AuthenticationPolicy policy, string? upgradedHash, CancellationToken ct);
}
public interface ILoginTransactionStore
{
    // Must use D09 serialization with same-transaction audit/outbox for success and rejected attempts.
    Task<LoginResult> ExecuteAsync(Func<ILoginTransaction, CancellationToken, Task<LoginResult>> action, CancellationToken ct);
}
public sealed class PendingLoginTransactionStore : ILoginTransactionStore
{
    public Task<LoginResult> ExecuteAsync(Func<ILoginTransaction, CancellationToken, Task<LoginResult>> action, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D09/D10: login locking, shared throttling and transactional audit");
}

public sealed class AuthenticationService(IAuthenticationPolicyProvider policies, ILoginTransactionStore store,
    PasswordProofs passwords, TimeProvider clock)
{
    // Prepared once per service lifetime to equalize unknown-account password work; not a real account or password.
    private readonly string dummyHash = passwords.Hash(OpaqueProofs.Create());
    public async Task<LoginResult> LoginAsync(string normalizedUsername, string password, string trustedSourceKey, CancellationToken ct)
    {
        var policy = await policies.GetAsync(ct);
        policy.Validate();
        if (string.IsNullOrWhiteSpace(normalizedUsername) || normalizedUsername.Length > 256 ||
            string.IsNullOrWhiteSpace(trustedSourceKey) || trustedSourceKey.Length > 256)
            return new(LoginOutcome.Rejected);
        try
        {
            return await store.ExecuteAsync(async (transaction, token) =>
            {
                if (await transaction.IsSourceLimitedAsync(trustedSourceKey, policy, token)) return new(LoginOutcome.Rejected);
                var candidate = await transaction.ReadForUpdateAsync(normalizedUsername, token);
                var verified = passwords.Verify(candidate?.EncodedHash ?? dummyHash, password);
                if (candidate is null || !candidate.Active || candidate.LockedOut || verified == PasswordVerificationResult.Failed)
                {
                    await transaction.RecordFailureAsync(candidate?.UserId, trustedSourceKey, policy, token);
                    return new(LoginOutcome.Rejected);
                }
                var raw = OpaqueProofs.Create();
                var now = clock.GetUtcNow();
                var proposal = new SessionIssue(candidate.UserId, OpaqueProofs.Digest(raw, "session"), now + policy.Idle, now + policy.Absolute);
                var completed = await transaction.CompletePasswordProofAsync(proposal, policy,
                    verified == PasswordVerificationResult.SuccessRehashNeeded ? passwords.Hash(password) : null, token);
                // Raw session handle leaves this service only after the store committed a final authenticated session.
                return completed.Outcome == LoginOutcome.SessionCreated ? new(LoginOutcome.SessionCreated, raw) : completed;
            }, ct);
        }
        catch (AuthorityUnavailableException) { return new(LoginOutcome.Unavailable); }
    }
}
