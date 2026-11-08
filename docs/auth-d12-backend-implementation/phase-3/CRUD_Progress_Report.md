# Role CRUD continuation — 2026-11-08

PARTIALLY_COMPLETED / RUNTIME_ACTIVATION_BLOCKED. Latest user instruction authorizes continuing implementation, not formal D04/D09 approval or production rollout.

## New executable source

- Application/RoleCrudAuthority.cs: fixed server operations, actor/policy binding and reviewed-request authority contract; default provider denies.
- Infrastructure/SqlRoleCrudService.cs: real SQL request submission/idempotent replay plus reviewed create, rename, activate, deactivate, clone and RolePermission diff apply.
- Tests/RoleCrudTests.cs and RoleCrudSqlLabTests.cs: default-deny/input tests and actual SQL integrity/rollback exercises.

Submit writes immutable proposed payload/revision plus audit/outbox; role remains absent/unchanged and outcome Pending/NotApplied. Same key+same canonical request replays current authorized result; differing payload/version conflicts. Apply requires current authority and independent review recheck, Approved/NotApplied status, immutable requester/digest, revision, request rowversion, target role version, live SQL-approved/implemented catalog intersected with supported server mapping. Tracked authority/request/role/user rows reload under the supplied transaction to avoid stale EF state.

Create and clone stay inactive; clone copies supported role permissions only, no user/grant/delegation. Rename preserves code. Deactivate is logical deletion, no physical DELETE Role. Permission changes update only exact target RolePermission. Apply increments role/request versions, affected users' GrantVersion and global PolicyVersion, appends RoleHistory and security event/outbox in one SaveChanges inside the caller's mandatory transaction. Applied request replay does not create another role/history/success event.

## Actual verification

.NET10 build PASS. Default suite164 PASS,6 SQL cases SKIPPED. Explicit NEW SQL CRUD suite3 PASS,0 skipped:
1. Create→rename→activate→permission diff→clone→deactivate, request replay/conflicting idempotency, no effective role before approval/apply, clone isolation, history/outbox counts, affected-user version and global version increments.
2. Duplicate code, stale request rowversion and changed target role version rejected without stale apply.
3. Test-only outbox CHECK constraint forces failure: SQL SaveChanges/savepoint rolls back role, success audit, history/version/effective status. Entire fixture transaction subsequently rolled back, including the test constraint.

Raw TRX: evidence/crud-unit-final.trx and crud-sql-final.trx, local-only. New SQL tests use three synthetic independent fixture persons and TEST-ONLY authority/gate implementations. They prove SQL DML/constraint/savepoint behavior, not approved real-session/MFA/delegation authority, durable review processing, cross-node serialization or production commit ordering. Existing three SQL cases were not rerun in this continuation; their earlier evidence remains scoped to the earlier build.

## Deliberate activation limits

No new HTTP endpoint, host registration or UI write flow is exposed. No default admin or approved permission seed; no migration created/applied; no persistent user/role/grant/fixture rows. Existing migration001 unchanged. Runtime authority and IAuthorizedCommitBoundary remain pending and block writes. Role mutations need a reviewed role-specific D09 global PolicyGate Exclusive plus appropriate user/object locks; a generic actor lock alone is insufficient. Conservative global version invalidation behavior still needs D04/D05/D09 disposition.

ApprovalStatus string alone never permits apply: DemandReviewedRequestAsync must recheck all current actor independence, MFA, beneficiary impact and authority inside that gate. No runtime implementation is supplied yet. Missing full audit target/request/reason schema and immutable archive remain D10 blockers; the new SQL failure test does not close audit security acceptance. SQL-side least-privilege enforcement and direct-DML bypass prevention are still required before activation.

User CRUD, durable UserRole/StationGrant/delegation CRUD workflows, review/approval HTTP APIs and UI mutation forms are NOT completed by this Role service increment. Existing read adapters and proposal validators remain reusable. No P0/readiness status changed.

Rollback: revert these additive service/tests; no schema down or authority restore. SQL fixtures rollback automatically; no production or Collector/IED/SCADA/firewall changes. Delivery continues on existing draft PR#2; it still depends on unmerged Phase2 PR#1.
