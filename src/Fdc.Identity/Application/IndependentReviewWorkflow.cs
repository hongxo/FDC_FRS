using System.Collections.Immutable;

namespace Fdc.Identity.Application;

// Candidate domain state machine; not a declaration that canonical state/status mappings have been approved.
public enum ReviewStage { Draft, Submitted, UnderReview, Approved, Applied, Rejected, Cancelled, Expired, Conflicted, Failed }
public sealed class IndependentReviewWorkflow(RoleChangeRequest request, long policyVersion)
{
    public RoleChangeRequest Request { get; } = request;
    public long PolicyVersion { get; } = policyVersion > 0 ? policyVersion : throw new ArgumentException("Policy version required.");
    public ReviewStage Stage { get; private set; } = ReviewStage.Draft;
    public long Version { get; private set; } = 1;
    public string? ReviewerPerson { get; private set; }
    public string? ApproverPerson { get; private set; }
    public void Submit(string requesterPerson, long expectedVersion, DateTimeOffset now)
    {
        Check(expectedVersion, now);
        if (Stage != ReviewStage.Draft || requesterPerson != Request.RequesterPerson) throw new InvalidOperationException("Invalid submission.");
        Stage = ReviewStage.Submitted; Version++;
    }
    public void Review(string person, long expectedVersion, ImmutableHashSet<string> currentBeneficiaries, DateTimeOffset now)
    {
        Check(expectedVersion, now); Independent(person, currentBeneficiaries);
        if (Stage != ReviewStage.Submitted) throw new InvalidOperationException("Review requires submitted revision.");
        ReviewerPerson = person; Stage = ReviewStage.UnderReview; Version++;
    }
    public void Approve(string person, long expectedVersion, string digest, ImmutableHashSet<string> currentBeneficiaries, DateTimeOffset now)
    {
        Check(expectedVersion, now); Independent(person, currentBeneficiaries);
        if (Stage != ReviewStage.UnderReview || person == ReviewerPerson || ReviewerPerson is null || currentBeneficiaries.Contains(ReviewerPerson))
            throw new UnauthorizedAccessException("Three independent persons required by this candidate domain model.");
        if (digest != Request.Digest) throw new InvalidOperationException("Stale revision digest.");
        ApproverPerson = person; Stage = ReviewStage.Approved; Version++;
    }
    public void DemandApplicable(long expectedVersion, long currentPolicyVersion, string digest,
        ImmutableHashSet<string> currentBeneficiaries, ImmutableHashSet<string> currentlyAuthorizedPersons, DateTimeOffset now)
    {
        Check(expectedVersion, now);
        if (Stage != ReviewStage.Approved || digest != Request.Digest || currentPolicyVersion != PolicyVersion)
            throw new InvalidOperationException("Approved revision/policy conflict.");
        if (ReviewerPerson is null || ApproverPerson is null || new[] { Request.RequesterPerson, ReviewerPerson, ApproverPerson }.Distinct(StringComparer.Ordinal).Count() != 3 ||
            currentBeneficiaries.Contains(Request.RequesterPerson) || currentBeneficiaries.Contains(ReviewerPerson) || currentBeneficiaries.Contains(ApproverPerson) ||
            !currentlyAuthorizedPersons.Contains(Request.RequesterPerson) || !currentlyAuthorizedPersons.Contains(ReviewerPerson) || !currentlyAuthorizedPersons.Contains(ApproverPerson))
            throw new UnauthorizedAccessException("Current requester/reviewer/approver authority or impact changed.");
        // This is a pure precondition. It does not mutate a role, mark Applied or authorize a SQL transaction.
    }
    public void Reject(string person, long expectedVersion, ImmutableHashSet<string> currentBeneficiaries, DateTimeOffset now)
    {
        Check(expectedVersion, now); Independent(person, currentBeneficiaries);
        if (Stage is not ReviewStage.Submitted and not ReviewStage.UnderReview) throw new InvalidOperationException("Cannot reject this stage.");
        Stage = ReviewStage.Rejected; Version++;
    }
    public void Cancel(string person, long expectedVersion, DateTimeOffset now)
    {
        Check(expectedVersion, now);
        if (person != Request.RequesterPerson || Stage is not ReviewStage.Draft and not ReviewStage.Submitted) throw new UnauthorizedAccessException("Cannot cancel this stage.");
        Stage = ReviewStage.Cancelled; Version++;
    }
    private void Check(long expectedVersion, DateTimeOffset now)
    {
        if (now >= Request.ExpiresAt) throw new InvalidOperationException("Revision expired; no effective change.");
        if (expectedVersion != Version) throw new InvalidOperationException("Workflow version conflict.");
    }
    private void Independent(string person, ImmutableHashSet<string> beneficiaries)
    {
        if (string.IsNullOrWhiteSpace(person) || person == Request.RequesterPerson || beneficiaries.Contains(person))
            throw new UnauthorizedAccessException("Independent non-beneficiary person required.");
    }
}
public interface IManagementApplyStore
{
    // Re-read revision, current actors/delegation/MFA/SoD/target versions; persist effective change,
    // version increments, idempotent outcome and sanitized audit/outbox atomically under D09.
    Task<Guid> RecheckApplyAndAuditAsync(Guid actorId, Guid requestId, long revision, string digest, CancellationToken ct);
}
public sealed class PendingManagementApplyStore : IManagementApplyStore
{
    public Task<Guid> RecheckApplyAndAuditAsync(Guid actorId, Guid requestId, long revision, string digest, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D04/D09/D10: operation mapping, canonical workflow, atomic apply and audit");
}
public sealed class ManagementApplyService(IAuthorizedCommitBoundary boundary, IManagementApplyStore store)
{
    // Internal service only, no public force-apply endpoint. Pending boundary prevents callback execution.
    public Task<Guid> ApplyAsync(Guid actorId, Guid requestId, long revision, string digest, CancellationToken ct) =>
        boundary.CommitAsync(actorId, token => store.RecheckApplyAndAuditAsync(actorId, requestId, revision, digest, token), ct);
}
