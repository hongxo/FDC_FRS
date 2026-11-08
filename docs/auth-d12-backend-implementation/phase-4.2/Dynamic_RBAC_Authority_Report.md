# Dynamic RBAC current authority

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

SqlCurrentAuthorityReader registered in product; primary SQL current user/session/roles/assignments/permissions/grants/functions/delegations/versions. PendingPermissionPolicy emits no supported descriptor; pending resource resolver rejects trusted object binding. DI registration is not full provider activation. Admin services demand pending D04 before data query.

New OperationPermissionResolver uses server-selected typed operation, immutable mapping release, exact policy/catalog and implemented code set. Default release verifier denies all; config cannot approve. Reject missing operation, wildcard, unknown/OT-denied code, stale version and delegation without function. Metadata result is not authorization allow.8 new tests PASS; accepting fixture verifier only in test source. No manage-to-all inference or role-label authorization.

Same-UserRole permission+StationGrant+GrantFunction, separate delegation, PersonReference/revision SoD and mandatory ied.configuration.write deny preserved. Existing component/SQL scoped-query tests pass, not full approved business endpoints. Concrete management read/mutation/delegation/seed policies and effective UserRole/Grant apply writers incomplete. Role CRUD source exists as isolated services/tests; mutation HTTP APIs not mapped. No7-role ceiling, seed users/passwords/grants, permission matrix edits or wildcard introduced.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
