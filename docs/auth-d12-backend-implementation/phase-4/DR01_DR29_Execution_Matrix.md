# DR01–DR29 traceability and exact expectations

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

Original plan SHA256: `19471472789b0b970daa35ab584deea74f8b27fe41866abd54a8d8917212fd21`. Original file unchanged. Exact names/setup/expected copied below; no expected outcome rewritten.

**Full scenario results:0 PASS,0 FAIL,29 BLOCKED,0 NOT_RUN,0 NOT_APPLICABLE.** Constituent tests may pass; this is not full DR acceptance.

| Test ID | Requirement | Implementation | Test method | Result | Evidence | Defect ID |
|---|---|---|---|---|---|---|
| DR01 | Create custom Role | SQL role request/create fixture; no effective role before apply | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR02 | Clone Role | SQL clone copies supported role config only; no assignments copied | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR03 | Role assignment | Assignment policy remains pending; no effective assignment workflow | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR04 | UserRole-bound StationGrant | SQL scoped grant queries + evaluator/domain same-assignment tests | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR05 | Add/remove Permission | SQL permission diff + version/outbox rollback subcases | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR06 | Deactivate/reactivate Role | SQL activate/deactivate + affected-user version checks | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR07 | Role definition expiry | Pure optional role-expiry evaluator only; signed scope missing | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR08 | UserRole expiry | Evaluator and proposal assignment-expiry tests | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR09 | Grant expiry | Evaluator and proposal grant-expiry tests | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR10 | Cross-station isolation | Scoped SQL user/grant reads and new target-binding regression; waveform/download not integrated | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | P4-D001 fixed subcase |
| DR11 | Role A permission + Role B scope | Existing evaluator rejects cross-role permission/scope composition | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR12 | Self-grant including indirect role edit | Person/beneficiary domain preconditions; no current SQL approval-impact authority | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR13 | Self-approval / Role creator approval | Independent workflow domain tests; test-only SQL review fixtures are not real authority | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR14 | Custom Role SoD bypass | Role-independent person/beneficiary domain checks | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR15 | OT deny with custom Role | Mandatory OT deny evaluator/proposal tests | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR16 | Concurrent role update | Sequential stale rowversion/base-version SQL tests, not concurrent nodes | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR17 | Revoke/commit race | PendingD09CommitBoundary still blocks; no race implementation | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR18 | Multi-instance session consistency | Real SQL current-session version reads; no login/two-node session issuer | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR19 | Catalog/version mismatch | Catalog/epoch/version default-deny component tests | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR20 | SQL authority unavailable | Real SQL missing-DB and timeout fail-closed tests; no full server outage/failover | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | P4-D003 fixed subcase |
| DR21 | Immutable RoleCode/history | SQL role rename/history subcases; full attempted-code/delete lifecycle not executed | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR22 | Seed replay/drift/rollback | Pending seed manifest; no approved release/drift fixture | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR23 | Delegation/hierarchy/validity | Domain hierarchy/validity envelope checks, no live move/delegation workflow | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR24 | Global admin delegation | Existing evaluator + explicit pending delegation policy, no effective writer | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR25 | Request/review/apply and idempotency | SQL request replay/apply/outbox/stale metadata tests; HTTP202 and real authority not deployed | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | P4-D001/P4-D002 fixed subcases |
| DR26 | Audit/outbox failure and restore | Real SQL outbox failure rolls back role/status/audit; archive/restore absent | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR27 | Human/Machine confusion + CSRF | Real antiforgery/handler/GET adapter components, not deployed proxy/mTLS boundary | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR28 | Last administration custody safety | Last-independent-administration custody policy missing | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |
| DR29 | UI S06/S07 contract compatibility | 17 client and8 browser fixture checks; no live authenticated UI end-to-end | component/SQL subcases only; full preconditions blocked | BLOCKED | evidence index + reports | none linked |

## DR01 — Create custom Role

Setup/workload (verbatim): Authorized requester submit metadata/new code, no UserRole/StationGrant

Expected result (verbatim): Pending role zero effective rights; sensitive activation requires independent approval

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: SQL role request/create fixture; no effective role before apply. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR02 — Clone Role

Setup/workload (verbatim): Clone readable role permission snapshot at fixed version; include hidden membership/grants in malicious body

Expected result (verbatim): New ID/code, IsSystemSeed=false; do not copy users/grants/approvals; unauthorized source copy denied

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: SQL clone copies supported role config only; no assignments copied. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR03 — Role assignment

Setup/workload (verbatim): Assign reviewed custom role to synthetic user via approved request

Expected result (verbatim): UserRole persisted with validity/version/audit; no station/global access without matching grant/delegation

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Assignment policy remains pending; no effective assignment workflow. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR04 — UserRole-bound StationGrant

Setup/workload (verbatim): User with two assignments, grant function subset to exact UserRole

Expected result (verbatim): Effective tuple respects FK/assignment/function/scope; wrong subject/assignment relationship denied

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: SQL scoped grant queries + evaluator/domain same-assignment tests. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR05 — Add/remove Permission

Setup/workload (verbatim): Known allowed exact code and forbidden/unknown/inactive/deny-only codes

Expected result (verbatim): Approved diff atomic; remove denies next request/commit, no unknown code/alias/OT write allow

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: SQL permission diff + version/outbox rollback subcases. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR06 — Deactivate/reactivate Role

Setup/workload (verbatim): Role with active assignments/grants, expired/revoked grants also present

Expected result (verbatim): Deactivate cuts rights without deleting history; reactivation reviewed and does not restore revoked/expired rights

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: SQL activate/deactivate + affected-user version checks. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR07 — Role definition expiry

Setup/workload (verbatim): Optional Role.ValidUntil policy if signed, boundary before/after expiry

Expected result (verbatim): If feature in scope, expired role deny regardless assignment; otherwise NOT_APPLICABLE only by signed scope, not false PASS

Preconditions missing: Optional expiry scope not signed; cannot claim NOT_APPLICABLE.

Actual: Pure optional role-expiry evaluator only; signed scope missing. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR08 — UserRole expiry

Setup/workload (verbatim): Assignment expires while underlying Role/grant valid

Expected result (verbatim): Deny after authority-time expiry; no role active/cache override

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Evaluator and proposal assignment-expiry tests. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR09 — Grant expiry

Setup/workload (verbatim): Grant ValidUntil boundary and null/permanent not allowed by policy

Expected result (verbatim): Deny expired/invalid lifetime; permanent only explicit approved policy

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Evaluator and proposal grant-expiry tests. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR10 — Cross-station isolation

Setup/workload (verbatim): Path/body A1 but detail/list/count/waveform/download/export object B1

Expected result (verbatim): Zero forbidden disclosures; filter SQL before paging/counts; consistent deny/object404 policy

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Scoped SQL user/grant reads and new target-binding regression; waveform/download not integrated. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR11 — Role A permission + Role B scope

Setup/workload (verbatim): Approver assignment A→station A1, Viewer assignment B→station B1

Expected result (verbatim): candidate.approve B1 denied even same user has permission at A1

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Existing evaluator rejects cross-role permission/scope composition. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR12 — Self-grant including indirect role edit

Setup/workload (verbatim): Requester or approver benefits via role membership/alias PersonReference or new assignment during review

Expected result (verbatim): Direct/indirect own escalation cannot approve/apply; impact set recomputed under D09 gate

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Person/beneficiary domain preconditions; no current SQL approval-impact authority. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR13 — Self-approval / Role creator approval

Setup/workload (verbatim): Role creator requests privileged permission addition and votes own change; same PersonReference alias

Expected result (verbatim): Deny own sensitive approval; new account alias/role label does not create independent reviewer

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Independent workflow domain tests; test-only SQL review fixtures are not real authority. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR14 — Custom Role SoD bypass

Setup/workload (verbatim): Custom role has create/review/approve; same person performs multiple stages on same revision

Expected result (verbatim): Mandatory creator/reviewer/approver independence enforced per workflow, not RoleCode

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Role-independent person/beneficiary domain checks. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR15 — OT deny with custom Role

Setup/workload (verbatim): Try role.permission/manage add ied.configuration.write or direct config-write endpoint

Expected result (verbatim): Mandatory deny for all roles; no endpoint effect/SCADA config change

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Mandatory OT deny evaluator/proposal tests. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR16 — Concurrent role update

Setup/workload (verbatim): Two nodes same Role ETag/version, differing permission diffs

Expected result (verbatim): One authorized version outcome, stale409/412; no lost update or audit mismatch

Preconditions missing: D09 commit/revoke serialization and multi-instance runtime not implemented/approved.

Actual: Sequential stale rowversion/base-version SQL tests, not concurrent nodes. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR17 — Revoke/commit race

Setup/workload (verbatim): Two nodes barrier race rolepermission remove/deactivate/userrole/grant revoke vs sensitive commit; both orderings

Expected result (verbatim): If revoke committed first deny; if mutation committed first show ordering; transaction/audit/outbox consistent

Preconditions missing: D09 commit/revoke serialization and multi-instance runtime not implemented/approved.

Actual: PendingD09CommitBoundary still blocks; no race implementation. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR18 — Multi-instance session consistency

Setup/workload (verbatim): Login/use A then revoke role/grant/user on B, use/commit A again; stale local cache

Expected result (verbatim): Every protected request/current primary denies stale rights; invalidation lag not authorization TTL

Preconditions missing: D09 commit/revoke serialization and multi-instance runtime not implemented/approved.

Actual: Real SQL current-session version reads; no login/two-node session issuer. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR19 — Catalog/version mismatch

Setup/workload (verbatim): Node/client old catalog or missing enforcement mapping; unsupported permission action

Expected result (verbatim): Fail-closed explicit version/conflict; no fallback to broader code/fixture permission

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Catalog/epoch/version default-deny component tests. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR20 — SQL authority unavailable

Setup/workload (verbatim): Disconnect authorized disposable authority during read/role apply/session checks

Expected result (verbatim): Protected operations fail-closed, no cached allow/admin-key fallback; required mutation+audit no false success

Preconditions missing: Full authority outage/HA/recovery acceptance requires integrated host and approved fault scope.

Actual: Real SQL missing-DB and timeout fail-closed tests; no full server outage/failover. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR21 — Immutable RoleCode/history

Setup/workload (verbatim): Rename RoleName then attempt rename/reuse retired RoleCode/delete used role

Expected result (verbatim): Code stable/unique, audit preserved, deactivate instead delete; RoleName no authority

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: SQL role rename/history subcases; full attempted-code/delete lifecycle not executed. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR22 — Seed replay/drift/rollback

Setup/workload (verbatim): Approved manifest replay, modified seed role, deactivated role, release adding permission

Expected result (verbatim): Idempotent no grants/accounts; no overwrite approved drift/auto-reactivate/new permission grant; rollback no stale privilege restore

Preconditions missing: Approved seed/custody policy and fixtures unavailable.

Actual: Pending seed manifest; no approved release/drift fixture. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR23 — Delegation/hierarchy/validity

Setup/workload (verbatim): Corp/unit scope move/new station, function outside envelope, excessive expiry, transitive delegation

Expected result (verbatim): Deny outside approved scope/validity/current hierarchy; sensitive inherited access requires signed review policy

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Domain hierarchy/validity envelope checks, no live move/delegation workflow. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR24 — Global admin delegation

Setup/workload (verbatim): Station-only grant or role lacking administrative delegation tries user/role administration

Expected result (verbatim): Deny even station read scope exists; separate same-assignment administrative delegation mandatory

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: Existing evaluator + explicit pending delegation policy, no effective writer. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR25 — Request/review/apply and idempotency

Setup/workload (verbatim): 202 create/edit/permissions/clone request, stale revision/catalog, conflicting key, approvals revoked before apply

Expected result (verbatim): Never effective on202/approval alone; stale/mismatch deny, exact retry reauthorized, no force PUT apply

Preconditions missing: Real current Human session/MFA, approved D04 mapping and complete request/review/apply integration missing.

Actual: SQL request replay/apply/outbox/stale metadata tests; HTTP202 and real authority not deployed. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR26 — Audit/outbox failure and restore

Setup/workload (verbatim): Force required audit insert failure, archive unavailable per policy; restore old DB snapshot in isolated authorized test

Expected result (verbatim): Mutation rollback/no false success, gaps visible; independent epoch/session revocation/policy reconciliation before reopen

Preconditions missing: Independent archive/restore epoch/reconciliation and D10 acceptance absent.

Actual: Real SQL outbox failure rolls back role/status/audit; archive/restore absent. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR27 — Human/Machine confusion + CSRF

Setup/workload (verbatim): Missing CSRF, forged actor/station/MFA/proxy header, cookie to machine, cert to role admin

Expected result (verbatim): Zero forbidden mutation/session/scope success; no fake principal/credential fallback

Preconditions missing: Real Human/Machine topology and authenticated S01-S08 integration absent.

Actual: Real antiforgery/handler/GET adapter components, not deployed proxy/mTLS boundary. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR28 — Last administration custody safety

Setup/workload (verbatim): Deactivate/remove capabilities from last independent recovery/admin custodians according signed policy

Expected result (verbatim): Capability-based safety/control recovery honored without hardcoding SysAdmin RoleCode or auto admin seed

Preconditions missing: Approved seed/custody policy and fixtures unavailable.

Actual: Last-independent-administration custody policy missing. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.

## DR29 — UI S06/S07 contract compatibility

Setup/workload (verbatim): Dynamic custom role beyond seven, pending/effective split, authorized userrole options, error/403/503

Expected result (verbatim): No seven-role hardcoded ceiling, no mock fallback/optimistic grant; frontend prototype not modified for this task

Preconditions missing: Real Human/Machine topology and authenticated S01-S08 integration absent.

Actual: 17 client and8 browser fixture checks; no live authenticated UI end-to-end. Full scenario BLOCKED; no whole-case PASS. Source test methods and raw actual outcomes are indexed in local evidence/execution-index.json. No destructive or production execution.
