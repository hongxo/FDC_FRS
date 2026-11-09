# B01–B05 closure matrix

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

| Blocker | Exact root cause | Decision | Code / evidence | Reviewer | Status |
|---|---|---|---|---|---|
| B01 | Login/shared throttle/factor/recovery issuers missing; policies/routes unreleased | D02/D03/D05/D06 route release | Product SQL handler, real revoke/disable/outage HTTPS;3 revoke SQL tests | NOT_PROVIDED | PARTIAL; POLICY+IMPLEMENTATION |
| B02 | Explicit mappings/delegation/custody/seed pending; concrete authorities/object/apply writers incomplete | D04/D08 | SQL authority DI, resolver8 tests, default deny | NOT_PROVIDED | PARTIAL; POLICY+IMPLEMENTATION |
| B03 | Candidate unapproved; all-writer/direct-DML/multi-host incomplete | D09 |7 SQL protocol PASS, writer coverage report | NOT_PROVIDED | BLOCKED POLICY+IMPLEMENTATION+ENVIRONMENT |
| B04 | Prior host absent; actual host now exists, topology/business scope/full UI incomplete | D01/D04 routes | Product startup/HTTPS/SQL/browser4 tests, fences | NOT_PROVIDED | FOUNDATION_IMPLEMENTED_PENDING_REVIEW; full B04 BLOCKED |
| B05 | Archive/schema/checkpoint/restore/principals absent | D10/D09 principals | Atomicity path tests + schema/privilege postcheck | NOT_PROVIDED | PARTIAL; POLICY+IMPLEMENTATION+ENVIRONMENT |

No blocker CLOSED. Needs complete code, approved applicable policy, full passing evidence and independent confirmation. Missing code is not labeled merely missing approval.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
