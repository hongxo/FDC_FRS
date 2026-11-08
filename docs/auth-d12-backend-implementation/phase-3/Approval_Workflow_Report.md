# Approval workflow

IndependentReviewWorkflow composes the existing immutable RoleChangeRequest digest/revision primitive. Candidate transitions implemented: Draft→Submitted→UnderReview→Approved, independent rejection, requester cancellation, version/expiry conflicts. Three different PersonReference values are enforced conservatively; beneficiaries cannot review/approve. Final preconditions recheck current requester/reviewer/approver authority and whether any has become a beneficiary. Policy version/digest/expiry mismatch denies.

This is an unpersisted role-domain state machine, not a signed canonical state mapping or complete management workflow. Applied/Expired/Conflicted/Failed are candidate enum values; no implementation persists these transitions. Approval does not imply effective rights. No endpoint emits202 or public force-apply. There is no duplicate-apply idempotency implementation, SQL approval concurrency proof or durable UserRole/StationGrant/delegation workflow.

ManagementApplyService delegates exclusively to IAuthorizedCommitBoundary plus IManagementApplyStore. Existing PendingD09CommitBoundary prevents callback invocation; PendingManagementApplyStore also refuses pending mapping/transaction/audit choices. No current-authority boolean from client or memory mutex replaces SQL serialization.

Domain tests PASS: self-review/approval, same-person role alias, reviewer=approver, beneficiary changes, revoked reviewer, stale digest/version/policy, duplicate approval, expired revision, cancel/reject terminal checks, pending commit callback blocked. These do not prove concurrency across SQL nodes or atomic audit/outbox. D04/D09/D10 policy/schema/stored-operation design and empirical lab proof remain mandatory.
