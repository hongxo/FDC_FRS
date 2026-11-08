> Update 2026-11-08: Role SQL CRUD service and actual rollback tests now implemented; runtime activation remains blocked. See [CRUD progress](CRUD_Progress_Report.md). Statements below describing missing CRUD are the earlier checkpoint.

# D12 Phase 3 — actual implementation

2026-11-08 Asia/Bangkok. PARTIALLY_COMPLETED / BLOCKED_BY_POLICY. No P0-IAM, owner decision or production-readiness status changed.

## Baseline and delivery

Official repo hongxo/FDC_FRS; clean branch started at verified Phase2 commit6d72ce1. Phase2 PR#1 is still OPEN/DRAFT and main remains initial README commit0a39c92. Phase3 branch feature/d12-management-workflow depends on feature/d12-authentication-authorization; do not merge in the wrong order. No second IAM or authentication provider introduced.

Prior explicit user confirmation selected FdcIam_DevelopmentLab, despite generic attachment name FDC_Center_Lab. This phase uses that existing disposable local DB for rollback-only tests; no schema migration or persistent grant/account installation. No FDC_Center_Lab database is created or changed.

## Source completed

- ManagementQueries.cs: explicit server read scopes, bounded pagination, safe DTOs and service operation enum; PendingManagementReadAuthority blocks unreviewed mappings before SQL.
- SqlManagementQueries.cs: SQL users/search/status/detail, assignments, assignment-bound grants/functions, catalog and scoped audit/request status. Visibility predicates precede paging. No null/all scope fallback.
- ManagementEndpoints.cs:8 opt-in canonical GET adapters, Human scheme, HTTPS/certificate boundary checks, no-store, safe error responses. No host registration or force-apply route.
- ManagementProposals.cs: inactive custom-role/clone previews, occupied immutable-code detection, exact supported catalog checks, no wildcard/OT write; StationGrant subset/scope/person/validity/version/hierarchy preview validation and overlap detection.
- IndependentReviewWorkflow.cs: candidate role-request lifecycle composed over existing RoleChangeRequest; independent three-person review, revision/version/expiry/beneficiary/current-actor preconditions; approval never mutates effective permissions. Apply service is blocked by existing PendingD09CommitBoundary.
- AssignmentDelegationContracts.cs: UserRole/delegation proposal contracts and explicitly pending conflict/delegation policy providers.
- New tests: ManagementReadTests, ManagementSqlLabTests, ManagementProposalTests, IndependentWorkflowTests, AssignmentPolicyTests.
- UI management.html/management.mjs and scoped style: read-only S05–S08 entry using original V1 style assets and same-origin API calls. No fixture runtime. Original V1 files/S01–S04 remain unchanged.

## What is NOT complete

No live Login/Session/MFA host from Phase2; no activated management API; no create/disable user, session-revoke writer, full dynamic Role CRUD, request persistence/review/apply SQL, effective UserRole/StationGrant/delegation mutation, transactional management audit/outbox or immutable archive. Domain validators are not authority or a durable workflow engine. The new state machine handles role revisions only; assignment/station-grant workflow persistence/transition mapping remains unimplemented. No claim of HTTP202 request acceptance, idempotent apply or SQL revoke/commit race protection.

## Verification

Stable SDK10.0.403 build PASS,0 warnings/errors. Final .NET suite159 PASS,3 SQL cases SKIPPED by default. Explicit local SQL runs:1 new scoped-management test PASS plus2 existing session-authority regressions PASS;162 distinct C# cases passed across these runs. Node client17 PASS. Browser8 fixture component checks PASS, including mobile screenshot inspection. No benchmark or full security acceptance inferred.

An initial test used ImmutableArray object equality and failed despite equal elements; assertion corrected to sequence equality, subsequent final run passed. Browser sandbox could not read externally supplied Playwright; permitted elevated headless run succeeded. Raw evidence retained locally in phase-3/evidence; not committed as generated artifacts.

## Remaining blockers / transition conditions

D04 exact operation→permission mapping, read/admin delegation envelopes, approval/conflict rules and catalog/seed release; D09 approved serialized commit/revoke mechanism; D03/D05/D06 real current session/MFA/recovery policies; D10 complete audit schema/failure/retention/archive controls; D01/A01–A21 route/proxy/DTO/status baseline. Approve these specific choices before activation, implement durable writers/migrations, then run actual HTTP+SQL workflow/race/audit tests and real-session UI integration. Phase4 cannot treat this phase as completed IAM.

Rollback: revert additive feature commits without touching DB or restoring stale authority. No schema down required; fixture transactions already rolled back. Existing original source is protected by before/after hashes. Phase2 dependencies remain unmerged and unapproved.
