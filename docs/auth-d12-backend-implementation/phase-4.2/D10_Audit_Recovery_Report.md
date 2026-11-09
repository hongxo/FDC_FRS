# D10 audit recovery

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Existing schema001 SecurityEvent/AuditOutbox and role/session transactional writers retained. Actual role/session outbox-failure tests PASS rollback effective changes and required versions. This is path-specific atomicity, not archive integrity or coverage of missing writers.

D10 complete event fields/outcome vocabulary/schema evolution, archive dispatcher/immutable store/checkpoints/signing custody/retention/redaction/failure disposition remain pending and partly unimplemented. No new migration; never edit applied001. Future additive002 requires review/dry-run/explicit apply scope.

Actual postcheck24/0/33FK/93checks/0 invalid, migration001 only. Runner sysadmin=1 is not least privilege. Need independent app/migrator/archive/emergency principals and direct authority DML denial tests per selected D09 model; no production grants changed.

Archive tamper/gap tests and backup restore into separate disposable target BLOCKED_BY_POLICY/ENVIRONMENT, NOT_RUN. Never restore shared FdcIam_DevelopmentLab to fabricate evidence. Required future checks: old RestoreEpoch sessions denied, policy/catalog parity, outbox/checkpoint continuity, replay resolution, external archive integrity and reviewer. No RTO/RPO/retention values inferred.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
