# D09 writer coverage

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Candidate SqlAuthorityCommitBoundary owns primary transaction, rejects dirty/nested context, locks Policy then sorted strongest User locks with bounded timeout, callback rechecks/mutates/audits. Rollback clears tracker; lost commit ACK unknown outcome, no safe retry. Advisory locks do not protect direct DML. Runtime PendingD09 retained.

7 candidate SQL protocol cases PASS: deterministic barriers/DMV wait, both cooperating orderings, exclusive policy gate, timeout, rollback, nested refusal, real commit/lost ACK. These are not full real-role/grant multi-host product races or AG failover. No sleep-only claim.

| Sensitive writer | Transaction/recheck | Audit/version | Gap |
|---|---|---|---|
| Role CRUD/permission apply/deactivate | SqlRoleCrudService boundary callback; request/SoD/version recheck | History+outbox+versions;5 SQL fixtures | Runtime boundary pending; no HTTP mutations; affected-subject lock plan pending |
| Own session revoke current/all | Real current subject inside boundary | SecurityVersion/challenge/audit/outbox;3 SQL cases | Policy pending; no logout route |
| UserRole assign/revoke | Proposal/workflow primitives | Atomic version/audit contract only | SQL effective writer incomplete; actor+subject locks required |
| StationGrant assign/revoke | Tuple/SoD proposal | Required grant/security/audit updates | Apply writer incomplete; hierarchy/actor/subject gates pending |
| Delegation change | Separate pending policy | Required affected user versions/audit | Runtime incomplete |
| Disable/password/factor/recovery | Domain/session primitives | Required revoke/version/challenge/audit | Issuers/apply stores incomplete |
| Sensitive business commands | Product503 fence | No mutation | D04 scope/object resolver and commit protocol pending |
| Migration/direct DBA DML | Outside advisory lock | Owner controlled | Lab sysadmin bypass possible; least privilege NOT_MET |

D09 owner must select namespace/modes/isolation/timeouts/target enumeration and controlled principal path. Unknown outcome needs durable idempotent resolution. Actual2-host product race/stale cache/SQL failover NOT_RUN; no all-writer closure.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
