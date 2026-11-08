# Owner decisions

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

| Package | Owner required | Decision | State |
|---|---|---|---|
| D01_Runtime_Host_Decision.md | Architecture/Security/OT/application owner | Runtime topology, canonical routes, TLS/proxy/direct bypass | PENDING_OWNER_DECISION |
| D04_Operation_Permission_Mapping_Decision.md | Business/Security/Architecture | Exact operations/scope/delegation/SoD/custody/seed release | PENDING_OWNER_DECISION |
| D02_D03_D05_D06_Authentication_Policy_Decision.md | Security/IAM operations/Business | Password/throttle, RP/origin/UV, recovery, session/CSRF | PENDING_OWNER_DECISION |
| D09_Transaction_Protocol_Decision.md | DBA/Architecture/Security | Candidate disposition, all writers/locks/DML/HA/unknown outcome | PENDING_OWNER_DECISION |
| D10_Audit_Restore_Policy_Decision.md | Security/Audit/DBA/SRE | Schema/archive/custody/checkpoint/retention/restore/principals | PENDING_OWNER_DECISION |

Version0.1 PROPOSED for all. Signature/name/signed-at/evidence NOT_PROVIDED. D12 coding/testing/push scope from explicit user instructions, not approval of domain choices. No D01–D12/E01–E08 auto-approved; no P0-IAM gate closed. Independent reviewer to compare source commits, SQL traces and exact original DR expectations; approval references immutable versions/scope. Production readiness is separate.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
