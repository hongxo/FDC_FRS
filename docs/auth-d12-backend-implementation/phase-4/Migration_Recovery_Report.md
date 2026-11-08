# Migration and recovery validation

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

No migration added/applied; existing001 SQL/class/model unchanged by Phase4. Postcheck:24 application tables,25 PKs including EF history,33 FKs,93 CHECKs,69 indexes including history,4 triggers;0 application rows,0 disabled/untrusted FKs/CHECKs. Test-only outbox constraint and all fixtures rolled back. No existing DB dropped/reset or data restored.

EF offline model/guard tests ran in suite. Prior up/down dry-run evidence belongs to earlier phase, not newly run here. Destructive migration/backup restore, backward compatible upgrade chain, archive retention/restore and externally fenced RestoreEpoch procedure NOT_RUN/BLOCKED: no reviewed backup/restore scope, full migrator snapshot or independent archive/key custody. Lab disposable label is not permission to overwrite other teams' data.

Local administrative test connection does not establish application least privilege. Existing catalog-reader grants can be statically inspected but writer/execute permissions not accepted. No startup migration call added. Production recovery requires separate approved runbook and empirical evidence. Do not treat metadata validity as full restore acceptance.
