# Phase3 security test execution

Final actual: SDK10.0.403 build PASS,0 warnings/errors;159 C# unit/component PASS with3 SQL SKIPPED in default run. Explicit runs:ManagementSqlLabTests1 PASS;Phase2SqlLabTests2 PASS.162 unique C# cases passed across runs (57 new component cases+1 new SQL case;102+2 prior cases). Node17 PASS; browser8 fixture checks PASS. Do not sum repeated checkpoints as new tests.

Evidence local-only:phase3-final.trx,phase3-sql-read.trx,phase3-sql-regression.trx,node-tests.tap,browser-results.json,management-mobile.png. Earlier checkpoint phase3-workflow.trx contains a failed ImmutableArray equality assertion; corrected sequence assertion and final rerun passed. No test results invented or security gates closed.

## SQL evidence and limits

Exact local host/database/disposable guard. Synthetic rollback-only fixtures include2 users,2 dynamic roles/assignments/grants,station hierarchy,permission/function,audit events. Scope-before-paging/search and cross-subject assignment query checks passed; after rollback user/grant/audit tables empty. No persistent seed or migration. This is real SQL Server testing, not in-memory substitution. Default skip is deliberate when lab environment absent;3 explicit SQL cases executed separately.

NOT_RUN:live management HTTP/session integration; approved role CRUD/workflow persistence; UserRole/StationGrant/revoke writers; idempotent duplicate apply; concurrent approvals; D09 two-node race/failover; management audit/outbox atomic failure; immutable archive integrity; full live UI integration. Browser fixture tests do not prove SQL authorization or real MFA.

## DR01–DR29 reconciliation

All29 retain NOT_RUN as COMPLETE acceptance scenarios; partial subcases below do not convert a whole row to PASS.

| DR cases | Evidence or blocker |
|---|---|
| DR01–DR02 | Role create/clone preview unit subcases PASS; durable request/apply NOT_RUN |
| DR03 | Assignment policy contract blocks; effective assign workflow NOT_RUN |
| DR04 | SQL assignment-bound read and domain grant linkage PASS subcases; effective apply NOT_RUN |
| DR05–DR06 | Unsupported/OT/wildcard diff rejection and existing evaluator deny subcases PASS; persistence/revoke NOT_RUN |
| DR07–DR09 | Existing evaluator and new proposal lifetime subcases PASS |
| DR10–DR11 | Scoped SQL read-before-paging/cross-subject grants and existing same-role evaluator subcases PASS; all HTTP resources NOT_RUN |
| DR12–DR14 | Person/beneficiary review/approval/apply preconditions PASS; durable impact-set race NOT_RUN |
| DR15 | Mandatory OT deny/cannot propose grants PASS domain subcases |
| DR16–DR17 | Domain optimistic version checks only; SQL concurrent update/revoke-commit NOT_RUN |
| DR18–DR20 | Existing real SQL session version reads PASS, pending boundaries fail closed; actual HA/outage acceptance NOT_RUN |
| DR21–DR22 | Duplicate/retired code preview and existing schema guard tests; real lifecycle/seed replay NOT_RUN |
| DR23–DR24 | Scope/hierarchy/expiry preview checks and pending separate delegation policy; effective delegation NOT_RUN |
| DR25 | Immutable role revision/candidate review preconditions PASS; HTTP202/durable idempotent apply NOT_RUN |
| DR26 | Audit scoped reads only; atomic mutation/archive/restore NOT_RUN |
| DR27 | Existing Human/CSRF tests and new GET metadata guards; deployed listener/proxy boundary NOT_RUN |
| DR28 | Last-administration custody not approved/implemented |
| DR29 | API client/fixture browser subset PASS; real-session full S06/S07 acceptance NOT_RUN |

Known-secret pattern scan and explicit Git path review performed before commits. Limited pattern scan is not independent penetration testing. Runtime has no new mock authentication provider or permission bypass. All management write paths remain unexposed pending policy. Current original Center vulnerabilities/readiness are not silently fixed by this standalone library.
