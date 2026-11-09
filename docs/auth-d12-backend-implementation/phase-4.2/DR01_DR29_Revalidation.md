# DR01–DR29 exact revalidation

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Original plan SHA256 19471472789b0b970daa35ab584deea74f8b27fe41866abd54a8d8917212fd21. Exact original setup/expected below, unchanged.

| ID | Name | Setup/workload | Expected | Original actual | Original evidence |
|---|---|---|---|---|---|
| DR01 | Create custom Role | Authorized requester submit metadata/new code, no UserRole/StationGrant | Pending role zero effective rights; sensitive activation requires independent approval | NOT_RUN | NOT_COLLECTED |
| DR02 | Clone Role | Clone readable role permission snapshot at fixed version; include hidden membership/grants in malicious body | New ID/code, IsSystemSeed=false; do not copy users/grants/approvals; unauthorized source copy denied | NOT_RUN | NOT_COLLECTED |
| DR03 | Role assignment | Assign reviewed custom role to synthetic user via approved request | UserRole persisted with validity/version/audit; no station/global access without matching grant/delegation | NOT_RUN | NOT_COLLECTED |
| DR04 | UserRole-bound StationGrant | User with two assignments, grant function subset to exact UserRole | Effective tuple respects FK/assignment/function/scope; wrong subject/assignment relationship denied | NOT_RUN | NOT_COLLECTED |
| DR05 | Add/remove Permission | Known allowed exact code and forbidden/unknown/inactive/deny-only codes | Approved diff atomic; remove denies next request/commit, no unknown code/alias/OT write allow | NOT_RUN | NOT_COLLECTED |
| DR06 | Deactivate/reactivate Role | Role with active assignments/grants, expired/revoked grants also present | Deactivate cuts rights without deleting history; reactivation reviewed and does not restore revoked/expired rights | NOT_RUN | NOT_COLLECTED |
| DR07 | Role definition expiry | Optional Role.ValidUntil policy if signed, boundary before/after expiry | If feature in scope, expired role deny regardless assignment; otherwise NOT_APPLICABLE only by signed scope, not false PASS | NOT_RUN | NOT_COLLECTED |
| DR08 | UserRole expiry | Assignment expires while underlying Role/grant valid | Deny after authority-time expiry; no role active/cache override | NOT_RUN | NOT_COLLECTED |
| DR09 | Grant expiry | Grant ValidUntil boundary and null/permanent not allowed by policy | Deny expired/invalid lifetime; permanent only explicit approved policy | NOT_RUN | NOT_COLLECTED |
| DR10 | Cross-station isolation | Path/body A1 but detail/list/count/waveform/download/export object B1 | Zero forbidden disclosures; filter SQL before paging/counts; consistent deny/object404 policy | NOT_RUN | NOT_COLLECTED |
| DR11 | Role A permission + Role B scope | Approver assignment A→station A1, Viewer assignment B→station B1 | candidate.approve B1 denied even same user has permission at A1 | NOT_RUN | NOT_COLLECTED |
| DR12 | Self-grant including indirect role edit | Requester or approver benefits via role membership/alias PersonReference or new assignment during review | Direct/indirect own escalation cannot approve/apply; impact set recomputed under D09 gate | NOT_RUN | NOT_COLLECTED |
| DR13 | Self-approval / Role creator approval | Role creator requests privileged permission addition and votes own change; same PersonReference alias | Deny own sensitive approval; new account alias/role label does not create independent reviewer | NOT_RUN | NOT_COLLECTED |
| DR14 | Custom Role SoD bypass | Custom role has create/review/approve; same person performs multiple stages on same revision | Mandatory creator/reviewer/approver independence enforced per workflow, not RoleCode | NOT_RUN | NOT_COLLECTED |
| DR15 | OT deny with custom Role | Try role.permission/manage add ied.configuration.write or direct config-write endpoint | Mandatory deny for all roles; no endpoint effect/SCADA config change | NOT_RUN | NOT_COLLECTED |
| DR16 | Concurrent role update | Two nodes same Role ETag/version, differing permission diffs | One authorized version outcome, stale409/412; no lost update or audit mismatch | NOT_RUN | NOT_COLLECTED |
| DR17 | Revoke/commit race | Two nodes barrier race rolepermission remove/deactivate/userrole/grant revoke vs sensitive commit; both orderings | If revoke committed first deny; if mutation committed first show ordering; transaction/audit/outbox consistent | NOT_RUN | NOT_COLLECTED |
| DR18 | Multi-instance session consistency | Login/use A then revoke role/grant/user on B, use/commit A again; stale local cache | Every protected request/current primary denies stale rights; invalidation lag not authorization TTL | NOT_RUN | NOT_COLLECTED |
| DR19 | Catalog/version mismatch | Node/client old catalog or missing enforcement mapping; unsupported permission action | Fail-closed explicit version/conflict; no fallback to broader code/fixture permission | NOT_RUN | NOT_COLLECTED |
| DR20 | SQL authority unavailable | Disconnect authorized disposable authority during read/role apply/session checks | Protected operations fail-closed, no cached allow/admin-key fallback; required mutation+audit no false success | NOT_RUN | NOT_COLLECTED |
| DR21 | Immutable RoleCode/history | Rename RoleName then attempt rename/reuse retired RoleCode/delete used role | Code stable/unique, audit preserved, deactivate instead delete; RoleName no authority | NOT_RUN | NOT_COLLECTED |
| DR22 | Seed replay/drift/rollback | Approved manifest replay, modified seed role, deactivated role, release adding permission | Idempotent no grants/accounts; no overwrite approved drift/auto-reactivate/new permission grant; rollback no stale privilege restore | NOT_RUN | NOT_COLLECTED |
| DR23 | Delegation/hierarchy/validity | Corp/unit scope move/new station, function outside envelope, excessive expiry, transitive delegation | Deny outside approved scope/validity/current hierarchy; sensitive inherited access requires signed review policy | NOT_RUN | NOT_COLLECTED |
| DR24 | Global admin delegation | Station-only grant or role lacking administrative delegation tries user/role administration | Deny even station read scope exists; separate same-assignment administrative delegation mandatory | NOT_RUN | NOT_COLLECTED |
| DR25 | Request/review/apply and idempotency | 202 create/edit/permissions/clone request, stale revision/catalog, conflicting key, approvals revoked before apply | Never effective on202/approval alone; stale/mismatch deny, exact retry reauthorized, no force PUT apply | NOT_RUN | NOT_COLLECTED |
| DR26 | Audit/outbox failure and restore | Force required audit insert failure, archive unavailable per policy; restore old DB snapshot in isolated authorized test | Mutation rollback/no false success, gaps visible; independent epoch/session revocation/policy reconciliation before reopen | NOT_RUN | NOT_COLLECTED |
| DR27 | Human/Machine confusion + CSRF | Missing CSRF, forged actor/station/MFA/proxy header, cookie to machine, cert to role admin | Zero forbidden mutation/session/scope success; no fake principal/credential fallback | NOT_RUN | NOT_COLLECTED |
| DR28 | Last administration custody safety | Deactivate/remove capabilities from last independent recovery/admin custodians according signed policy | Capability-based safety/control recovery honored without hardcoding SysAdmin RoleCode or auto admin seed | NOT_RUN | NOT_COLLECTED |
| DR29 | UI S06/S07 contract compatibility | Dynamic custom role beyond seven, pending/effective split, authorized userrole options, error/403/503 | No seven-role hardcoded ceiling, no mock fallback/optimistic grant; frontend prototype not modified for this task | NOT_RUN | NOT_COLLECTED |

## Actual per-case revalidation

Common preconditions: signed policies/catalog/route release, complete runtime, synthetic organization/stations/users and independent review. Not met. Environment/commands and actual subcases in Security_Regression_Report.md/Evidence_Index.md. Full workload is exact original setup above; not executed when prerequisites blocked. No component PASS substituted.

| ID | Actual | Executed subcase only | Missing precondition/defect | Retest |
|---|---|---|---|---|
| DR01 | BLOCKED_BY_POLICY | SQL role-create fixture | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR02 | BLOCKED_BY_POLICY | SQL clone fixture | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR03 | BLOCKED_BY_POLICY | assignment policy component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR04 | BLOCKED_BY_POLICY | same-UserRole component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR05 | BLOCKED_BY_POLICY | SQL role-permission fixture | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR06 | BLOCKED_BY_POLICY | SQL deactivate fixture | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR07 | BLOCKED_BY_POLICY | Component contract only | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR08 | BLOCKED_BY_POLICY | Component contract only | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR09 | BLOCKED_BY_POLICY | Component contract only | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR10 | BLOCKED_BY_POLICY | SQL scoped management query/component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR11 | BLOCKED_BY_POLICY | cross-role component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR12 | BLOCKED_BY_POLICY | SoD component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR13 | BLOCKED_BY_POLICY | SoD component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR14 | BLOCKED_BY_POLICY | workflow component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR15 | BLOCKED_BY_POLICY | mandatory deny component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR16 | BLOCKED_BY_POLICY | candidate concurrency protocol | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR17 | BLOCKED_BY_POLICY | candidate two orderings | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR18 | BLOCKED_BY_POLICY | candidate only; no actual multi-host | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR19 | BLOCKED_BY_POLICY | Component contract only | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR20 | BLOCKED_BY_POLICY | actual SQL timeout/missing DB + Center HTTPS | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR21 | BLOCKED_BY_POLICY | Component contract only | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR22 | BLOCKED_BY_POLICY | seed component | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR23 | BLOCKED_BY_POLICY | Component contract only | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR24 | BLOCKED_BY_POLICY | Component contract only | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR25 | BLOCKED_BY_POLICY | SQL idempotency/history | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR26 | BLOCKED_BY_POLICY | outbox rollback only; no restore | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR27 | BLOCKED_BY_POLICY | Center deny paths only; not full mutation CSRF | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR28 | BLOCKED_BY_POLICY | pending custody negative | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |
| DR29 | BLOCKED_BY_POLICY | live UI policy denial slice | Applicable B01–B05 approval/current-provider/issuer/writer gaps; environment gaps detailed in closure matrix | Full NOT_RUN; rerun after prerequisites |

Totals:0 PASS/0 FAIL/29 BLOCKED_BY_POLICY, with secondary IMPLEMENTATION/ENVIRONMENT gaps. Classification does not imply full workload ran. Phase4 original execution matrix also read/unchanged.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
