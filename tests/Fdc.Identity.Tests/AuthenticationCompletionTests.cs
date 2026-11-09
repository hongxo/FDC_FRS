using Fdc.Identity.Application;
using Fdc.Identity.Security;

namespace Fdc.Identity.Tests;

public sealed class AuthenticationCompletionTests
{
    [Fact]
    public async Task Invalid_credentials_cannot_reach_completion()
    {
        var proofs = new PasswordProofs(1, 256);
        var store = new IsolatedStore(proofs.Hash("fixture-password"), new(LoginOutcome.SessionCreated));
        var result = await Service(store, proofs).LoginAsync("FIXTURE", "wrong", "fixture", default);
        Assert.Equal(LoginOutcome.Rejected, result.Outcome);
        Assert.Null(result.Proof);
        Assert.Equal(1, store.Failures);
        Assert.Null(store.Proposed);
    }

    [Fact]
    public async Task Committed_fixture_session_returns_only_its_matching_opaque_handle()
    {
        var proofs = new PasswordProofs(1, 256);
        var store = new IsolatedStore(proofs.Hash("fixture-password"), new(LoginOutcome.SessionCreated));
        var result = await Service(store, proofs).LoginAsync("FIXTURE", "fixture-password", "fixture", default);
        Assert.True(store.Committed);
        Assert.Equal(LoginOutcome.SessionCreated, result.Outcome);
        Assert.Equal(store.Proposed!.Digest, OpaqueProofs.Digest(result.Proof!, "session"));
    }

    [Theory]
    [InlineData(LoginOutcome.Rejected, LoginOutcome.Rejected)]
    [InlineData(LoginOutcome.MfaRequired, LoginOutcome.Unavailable)]
    [InlineData(LoginOutcome.Unavailable, LoginOutcome.Unavailable)]
    public async Task Invalid_completion_never_releases_proof(LoginOutcome completion, LoginOutcome expected)
    {
        var proofs = new PasswordProofs(1, 256);
        var store = new IsolatedStore(proofs.Hash("fixture-password"), new(completion, "invalid-proof-fixture"));
        var result = await Service(store, proofs).LoginAsync("FIXTURE", "fixture-password", "fixture", default);
        Assert.Equal(expected, result.Outcome);
        Assert.Null(result.Proof);
    }

    [Fact]
    public async Task Challenge_completion_does_not_return_proposed_session_handle()
    {
        var proofs = new PasswordProofs(1, 256);
        var challenge = OpaqueProofs.Create();
        var store = new IsolatedStore(proofs.Hash("fixture-password"), new(LoginOutcome.MfaRequired, challenge));
        var result = await Service(store, proofs).LoginAsync("FIXTURE", "fixture-password", "fixture", default);
        Assert.Equal(LoginOutcome.MfaRequired, result.Outcome);
        Assert.Equal(challenge, result.Proof);
        Assert.NotEqual(store.Proposed!.Digest, OpaqueProofs.Digest(challenge, "session"));
    }

    [Fact]
    public async Task Failed_commit_never_releases_session_handle()
    {
        var proofs = new PasswordProofs(1, 256);
        var store = new IsolatedStore(proofs.Hash("fixture-password"), new(LoginOutcome.SessionCreated)) { FailCommit = true };
        var result = await Service(store, proofs).LoginAsync("FIXTURE", "fixture-password", "fixture", default);
        Assert.Equal(LoginOutcome.Unavailable, result.Outcome);
        Assert.Null(result.Proof);
        Assert.False(store.Committed);
    }

    private static AuthenticationService Service(IsolatedStore store, PasswordProofs proofs) =>
        new(new FixturePolicy(), store, proofs, TimeProvider.System);

    private sealed class FixturePolicy : IAuthenticationPolicyProvider
    {
        public Task<AuthenticationPolicy> GetAsync(CancellationToken ct) =>
            Task.FromResult(new AuthenticationPolicy(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), 1, 1,
                TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), "fixture"));
    }

    private sealed class IsolatedStore(string hash, LoginResult completion) : ILoginTransactionStore, ILoginTransaction
    {
        public int Failures { get; private set; }
        public SessionIssue? Proposed { get; private set; }
        public bool Committed { get; private set; }
        public bool FailCommit { get; init; }

        public async Task<LoginResult> ExecuteAsync(Func<ILoginTransaction, CancellationToken, Task<LoginResult>> action, CancellationToken ct)
        {
            var result = await action(this, ct);
            if (FailCommit) throw new AuthorityUnavailableException();
            Committed = true;
            return result;
        }
        public Task<CredentialCandidate?> ReadForUpdateAsync(string normalizedUsername, CancellationToken ct) =>
            Task.FromResult<CredentialCandidate?>(new(Guid.NewGuid(), true, hash, false));
        public Task<bool> IsSourceLimitedAsync(string trustedSourceKey, AuthenticationPolicy policy, CancellationToken ct) =>
            Task.FromResult(false);
        public Task RecordFailureAsync(Guid? userId, string trustedSourceKey, AuthenticationPolicy policy, CancellationToken ct)
        {
            Failures++;
            return Task.CompletedTask;
        }
        public Task<LoginResult> CompletePasswordProofAsync(SessionIssue proposed, AuthenticationPolicy policy, string? upgradedHash, CancellationToken ct)
        {
            Proposed = proposed;
            return Task.FromResult(completion);
        }
    }
}
