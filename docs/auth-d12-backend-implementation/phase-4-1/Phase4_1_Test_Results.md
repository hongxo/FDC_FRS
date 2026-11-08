# Phase 4.1 — actual tests và revalidation

2026-11-08 (Asia/Bangkok). Baseline `415cca4cb032528928518701e5606c47d37cd363`; source/test content `8848dd9ad716ba12a6445f8549ffbe5522c0a9cb`. Development/Lab only. APPROVAL/readiness không thay đổi.

## Actual final runs

SDK10 build PASS,0 warnings/errors. Validated source/test content8848dd9. Raw TRX/execution-index/run-manifests local-only ghi exact timestamps, SHA và commands; generated artifacts không commit.

| Run | PASS | FAIL | SKIPPED | Scope |
|---|---:|---:|---:|---|
| final-unit.trx |176|0|22|Component/unit; SQL profile/lab cases chưa có env nên skip trong run này|
| final-gate.trx |7|0|0|Actual SQL candidate gate:7 cases,2 connections where applicable|
| final-revocation.trx |3|0|0|Actual SQL session-revoke DML/rollback with test-only policy/boundary|
| final-http-sql.trx |1|0|0|Actual HTTPS→actual SQL identity negative integration; no mock auth|
| final-role-crud.trx |5|0|0|Inherited role CRUD/target binding/replay/stale/outbox regressions|
| final-session-read.trx |2|0|0|Current primary session/version reads|
| final-scope.trx |1|0|0|Scoped SQL query-before-paging, subject/assignment isolation|
| final-sql-failures.trx |2|0|0|Real SQL command timeout/missing-DB connection fail-closed|
| final-profile.trx |1|0|0|Observation-only small scoped-query profile, no threshold acceptance|
| Node TAP |17|0|0|Client component mocks only, not live authority|
| Browser JSON |8|0|0|Intercepted HTTPS fixtures, not live UI E2E|

Default22 skips không được ghi PASS từ run mặc định; chúng đã chạy riêng đủ22 cases như các dòng SQL.198 C# distinct cases passed across separate runs; không cộng các checkpoint chạy lặp thành case mới. Profile assertion PASS chỉ nói query trả expected rows, không là performance GO.

## Gate/revocation expected versus actual

| Case | Expected | Actual |
|---|---|---|
| Mutation holds user gate | Revoke chờ; chỉ đọc effect sau mutation commit | SQL APPLICATION WAIT thấy trên đúng waiter SPID; callback revoke đọc counter2; PASS |
| Revoke commits first | Subsequent callback đọc changed version và không ghi effect | Version2 observed, mutation counter remained1; PASS |
| Policy X versus another actor | Shared-policy operation chờ dù UserId khác | SQL WAIT observed, new policy counter2 after release; PASS |
| Gate timeout | No callback/no success/transaction released | AuthorityUnavailable, callback false; PASS |
| Callback failure | Rollback saved change + clear tracker | Fresh SQL counter1, tracker empty; PASS |
| Nested/dirty context | Refuse before mutation or connection where possible | No callback; pending entity preserved; PASS |
| Lost ACK | Never infer successful rollback or auto-retry | Test hook after actual commit, fresh DB counter2, UnknownOutcome; PASS |
| Current/everywhere revoke | Own-user isolation + SecurityVersion/challenge invalidation | Other user unaffected, old session deny; PASS |
| Required outbox failure | No partial session/security/challenge/audit mutation | Fresh SQL state unchanged; PASS |
| HTTPS identity flow | Forged/invalid/disabled/revoked401, missing authority503, no SQL detail leak | Actual handler/SQL/role adapter matches; PASS |

Initial corrections retained as evidence: gate harness used int mapping for SQL smallint @@SPID and failed one case; fixed explicit CONVERT and final7 PASS. One test compilation lacked EF Storage using, corrected before execution. First HTTPS harness run failed handshake; test-certificate loading/Windows user-key context/EKU corrected, actual retest PASS. Handshake failure không được gán root cause production hoặc làm product vulnerability khi chưa có evidence. Earlier FAIL logs retained; no test removed/expectation weakened.

## SQL/schema/security postcheck

Actual postcheck24 application tables,0 application rows,33 FKs/93 checks,0 disabled/untrusted. Unique gate probes were committed for cross-connection ordering tests then deleted with exact ID/key/PolicyConfigured=false guards. Account/session/audit fixtures use outer rollback transactions; test-only CHECK constraint rolled back. No migration, schema reset, DB drop or persistent seed. No real accounts/keys/grants.

Known-secret pattern scan and Git/diff/path checks performed. No dependency/project/lockfile change; Phase4 NuGet scan is historical evidence, not a fresh Phase4.1 vulnerability/license certification. Package support/license/independent security audit still NOT_RUN for acceptance. No source sent to external scanner.

Bounded query observation rerun:100 sequential calls after10 warmup,3 synthetic users; P50 0.4479 ms/P95 0.6959 ms/P99 0.7620 ms, 2077.61 calls/s, error rate0. No login/HTTP/MFA/authority-resolution cost; no380-station sizing, SLA or approved performance threshold. Server CPU/network/disk/pool/deadlock metrics NOT_MEASURED. Raw samples retained, threshold PENDING.

## DR01–DR29 revalidation

Original plan unchanged, SHA256 `19471472789b0b970daa35ab584deea74f8b27fe41866abd54a8d8917212fd21`. Expected column copied verbatim. Full results:0 PASS,0 FAIL,29 BLOCKED; no NOT_APPLICABLE selected without signed scope. Partial PASS below never closes a whole scenario.

| ID | Expected result (verbatim) | Full result | New/retested subcase evidence |
|---|---|---|---|
| DR01 | Pending role zero effective rights; sensitive activation requires independent approval | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR02 | New ID/code, IsSystemSeed=false; do not copy users/grants/approvals; unauthorized source copy denied | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR03 | UserRole persisted with validity/version/audit; no station/global access without matching grant/delegation | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR04 | Effective tuple respects FK/assignment/function/scope; wrong subject/assignment relationship denied | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR05 | Approved diff atomic; remove denies next request/commit, no unknown code/alias/OT write allow | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR06 | Deactivate cuts rights without deleting history; reactivation reviewed and does not restore revoked/expired rights | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR07 | If feature in scope, expired role deny regardless assignment; otherwise NOT_APPLICABLE only by signed scope, not false PASS | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR08 | Deny after authority-time expiry; no role active/cache override | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR09 | Deny expired/invalid lifetime; permanent only explicit approved policy | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR10 | Zero forbidden disclosures; filter SQL before paging/counts; consistent deny/object404 policy | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR11 | candidate.approve B1 denied even same user has permission at A1 | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR12 | Direct/indirect own escalation cannot approve/apply; impact set recomputed under D09 gate | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR13 | Deny own sensitive approval; new account alias/role label does not create independent reviewer | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR14 | Mandatory creator/reviewer/approver independence enforced per workflow, not RoleCode | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR15 | Mandatory deny for all roles; no endpoint effect/SCADA config change | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR16 | One authorized version outcome, stale409/412; no lost update or audit mismatch | BLOCKED | Global Policy X/cooperating actors gate probe PASS; actual role concurrent approvals still missing |
| DR17 | If revoke committed first deny; if mutation committed first show ordering; transaction/audit/outbox consistent | BLOCKED | Both probe orderings + actual SQL WAIT + timeout/rollback PASS; real IAM sensitive commits not integrated |
| DR18 | Every protected request/current primary denies stale rights; invalidation lag not authorization TTL | BLOCKED | SQL revoke-current/all and actual HTTP old-cookie denial PASS; not two full Center hosts |
| DR19 | Fail-closed explicit version/conflict; no fallback to broader code/fixture permission | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR20 | Protected operations fail-closed, no cached allow/admin-key fallback; required mutation+audit no false success | BLOCKED | Actual SQL timeout/connection failure and HTTP503 PASS; no service outage/HA |
| DR21 | Code stable/unique, audit preserved, deactivate instead delete; RoleName no authority | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR22 | Idempotent no grants/accounts; no overwrite approved drift/auto-reactivate/new permission grant; rollback no stale privilege restore | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR23 | Deny outside approved scope/validity/current hierarchy; sensitive inherited access requires signed review policy | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR24 | Deny even station read scope exists; separate same-assignment administrative delegation mandatory | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR25 | Never effective on202/approval alone; stale/mismatch deny, exact retry reauthorized, no force PUT apply | BLOCKED | Existing5 role SQL CRUD/replay regressions PASS; durable review/MFA authority missing |
| DR26 | Mutation rollback/no false success, gaps visible; independent epoch/session revocation/policy reconciliation before reopen | BLOCKED | Session and role outbox failure atomic rollback PASS; archive/restore missing |
| DR27 | Zero forbidden mutation/session/scope success; no fake principal/credential fallback | BLOCKED | Actual HTTPS→SQL negative slice PASS; actual Machine listener/mTLS/proxy and mutations missing |
| DR28 | Capability-based safety/control recovery honored without hardcoding SysAdmin RoleCode or auto admin seed | BLOCKED | Inherited component/SQL assertions retested where implemented; full policy/runtime prerequisite still absent |
| DR29 | No seven-role hardcoded ceiling, no mock fallback/optimistic grant; frontend prototype not modified for this task | BLOCKED | 17 client/8 fixture browser regressions PASS; S01–S08 real-session E2E missing |

## E2E and acceptance gaps

Actual HTTP integration is one real negative slice; UI browser run still fixtures. Login/logout endpoint issuance, factor/recovery crypto, User/Role/Grant actual approved request/review/apply, two deployed Center instances, full mTLS/proxy/direct bypass and combined DB/archive/epoch restore remain BLOCKED. Fault test uses deliberate local missing DB/connection-only delay; no production service fault or IED/SCADA test. Independent acceptance review NOT_PROVIDED.
