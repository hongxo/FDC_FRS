using System.Collections.Immutable;
using Fdc.Identity.Application;

namespace Fdc.Identity.Tests;

public sealed class IndependentWorkflowTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-11-08T00:00:00Z");
    private static readonly ImmutableHashSet<string> None = ImmutableHashSet<string>.Empty;
    private static readonly ImmutableHashSet<string> Actors = ImmutableHashSet.Create("requester", "reviewer", "approver");
    private static IndependentReviewWorkflow Draft() => new(new RoleChangeRequest("requester", new(RoleChangeKind.Deactivate, Guid.NewGuid(), null, null, 1, 1, [], [], "synthetic reason"), 1, Now.AddMinutes(10)), 1);
    private static IndependentReviewWorkflow Submitted() { var x = Draft(); x.Submit("requester", 1, Now); return x; }
    private static IndependentReviewWorkflow Reviewed() { var x = Submitted(); x.Review("reviewer", 2, None, Now); return x; }
    private static IndependentReviewWorkflow Approved() { var x = Reviewed(); x.Approve("approver", 3, x.Request.Digest, None, Now); return x; }
    [Fact] public void Request_review_approval_never_implies_applied()
    {
        var x = Approved(); Assert.Equal(ReviewStage.Approved, x.Stage);
        x.DemandApplicable(4, 1, x.Request.Digest, None, Actors, Now);
        Assert.Equal(ReviewStage.Approved, x.Stage);
    }
    [Fact] public void Requester_cannot_review() => Assert.Throws<UnauthorizedAccessException>(() => Submitted().Review("requester", 2, None, Now));
    [Fact] public void Requester_cannot_approve() { var x = Reviewed(); Assert.Throws<UnauthorizedAccessException>(() => x.Approve("requester", 3, x.Request.Digest, None, Now)); }
    [Fact] public void Reviewer_cannot_approve() { var x = Reviewed(); Assert.Throws<UnauthorizedAccessException>(() => x.Approve("reviewer", 3, x.Request.Digest, None, Now)); }
    [Fact] public void Alias_person_string_does_not_become_independent_role() => Assert.Throws<UnauthorizedAccessException>(() => Submitted().Review("requester", 2, None, Now));
    [Fact] public void Beneficiary_cannot_review() => Assert.Throws<UnauthorizedAccessException>(() => Submitted().Review("beneficiary", 2, ImmutableHashSet.Create("beneficiary"), Now));
    [Fact] public void Approve_before_review_rejected() { var x = Submitted(); Assert.Throws<UnauthorizedAccessException>(() => x.Approve("approver", 2, x.Request.Digest, None, Now)); }
    [Fact] public void Stale_workflow_version_rejected() => Assert.Throws<InvalidOperationException>(() => Submitted().Review("reviewer", 1, None, Now));
    [Fact] public void Duplicate_approval_cannot_advance_state() { var x = Approved(); Assert.Throws<InvalidOperationException>(() => x.Approve("other", 3, x.Request.Digest, None, Now)); Assert.Equal(4, x.Version); }
    [Fact] public void Wrong_digest_rejected() { var x = Reviewed(); Assert.Throws<InvalidOperationException>(() => x.Approve("approver", 3, "wrong", None, Now)); }
    [Fact] public void Policy_change_prevents_apply() { var x = Approved(); Assert.Throws<InvalidOperationException>(() => x.DemandApplicable(4, 2, x.Request.Digest, None, Actors, Now)); }
    [Fact] public void Reviewer_revoked_before_apply_denied() { var x = Approved(); Assert.Throws<UnauthorizedAccessException>(() => x.DemandApplicable(4, 1, x.Request.Digest, None, Actors.Remove("reviewer"), Now)); }
    [Fact] public void Requester_becomes_beneficiary_denied() { var x = Approved(); Assert.Throws<UnauthorizedAccessException>(() => x.DemandApplicable(4, 1, x.Request.Digest, ImmutableHashSet.Create("requester"), Actors, Now)); }
    [Fact] public void Approver_becomes_beneficiary_denied() { var x = Approved(); Assert.Throws<UnauthorizedAccessException>(() => x.DemandApplicable(4, 1, x.Request.Digest, ImmutableHashSet.Create("approver"), Actors, Now)); }
    [Fact] public void Expiry_prevents_apply() { var x = Approved(); Assert.Throws<InvalidOperationException>(() => x.DemandApplicable(4, 1, x.Request.Digest, None, Actors, x.Request.ExpiresAt)); }
    [Fact] public void Rejected_revision_cannot_be_reviewed() { var x = Submitted(); x.Reject("independent", 2, None, Now); Assert.Equal(ReviewStage.Rejected, x.Stage); Assert.Throws<InvalidOperationException>(() => x.Review("reviewer", 3, None, Now)); }
    [Fact] public void Only_requester_can_cancel() => Assert.Throws<UnauthorizedAccessException>(() => Submitted().Cancel("other", 2, Now));
    [Fact] public void Cancelled_revision_cannot_be_reviewed() { var x = Submitted(); x.Cancel("requester", 2, Now); Assert.Throws<InvalidOperationException>(() => x.Review("reviewer", 3, None, Now)); }
    [Fact] public async Task Pending_commit_gate_blocks_effective_apply()
    {
        var service = new ManagementApplyService(new PendingD09CommitBoundary(), new UnexpectedStore());
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => service.ApplyAsync(Guid.NewGuid(), Guid.NewGuid(), 1, "digest", default));
    }
    private sealed class UnexpectedStore : IManagementApplyStore
    {
        public Task<Guid> RecheckApplyAndAuditAsync(Guid actorId, Guid requestId, long revision, string digest, CancellationToken ct) => throw new InvalidOperationException("Pending boundary must not call store.");
    }
}
