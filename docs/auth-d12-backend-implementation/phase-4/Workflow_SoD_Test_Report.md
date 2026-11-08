# Workflow and SoD security tests

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

Existing independent-role domain state tests PASS: requester cannot review/approve, reviewer cannot also approve, beneficiaries cannot vote, current authority/digest/policy/expiry/version mismatch blocks applicability. Approved domain state does not set effective rights. All are domain preconditions, not a persisted reviewed authority.

Real SQL service tests use explicitly test-only three-person review/gate fixtures and an outer rollback transaction. They prove request remains Pending/NotApplied before apply, stale versions/duplicate code reject, clone does not copy grants, successful apply records history/version/outbox, and an outbox CHECK failure rolls back effect+success audit/status. They do NOT prove actual MFA, no-self impact resolution, concurrent approval serialization or independently authorized HTTP workflow.

P4-D002 fixed replay returning the role's later version: replay now resolves the original matching request/revision/role/digest history version and denies if missing. SQL failing-before and passing-after traces retained. Idempotent result correctness is tested; exactly-once distributed execution is not claimed. Rejected/cancelled/expired persisted flow and HTTP202 acceptance are BLOCKED by absent full runtime workflow.
