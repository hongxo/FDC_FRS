# Versioned migrations and impact

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

Canonical001 is original IAM EF/SQL, byte/source unchanged. New Center EF migrations load embedded immutable SQL; no EnsureCreated or startup Migrate. Separate contexts share dbo.__EFMigrationsHistory and original catalog objects without duplicate creation.

| Version | Actual change | Applied |
|---|---|---|
|202611080001_InitialIamFoundation |24 canonical IAM/catalog/audit/ops tables and original constraints/triggers | Fresh target only; no old DB data copy |
|202611080002_CenterDomains |28 independent domain tables +3 restricted reporting views + role contracts | PASS, backup first |
|202611080003_UtcAndRuntimeMetadata |Additional UTC checks, VIEW DEFINITION for runtime schema probe | PASS additive;002 not rewritten |
|202611080004_StationSoeEvents |Source-compatible SOE table/FK/unique/range/UTC/indexes | PASS additive; prior versions unchanged |
|202611080005_RecordSourceSnapshot | Original bounded manifest snapshot JSON retained with content digest | PASS after corrected unapplied batch candidate; replay verified |
|202611080006_OperationalEventStation | Direct Station FK when optional CollectorId is NULL | PASS additive; orphan SQL test rejects |

Author review before deployment: target absent, machine/SQL16 match, caller explicitly authorized isolated Lab; no active target sessions/data, no destructive up SQL, no production scope. Model tests/build passed before first apply. Hash-pinned resource checks before any DB connection; exact SQL bytes retained in Git attributes. New backups COPY_ONLY/CHECKSUM+VERIFYONLY before every apply/replay. Independent architecture/schema/policy approval NOT_PROVIDED; Lab apply is not formal D09 approval.

Migration replay actually repeated with final hashes; no duplicate DDL/history,6 rows remain. Tool refuses target with app data/active sessions/unmarked lab; future nonempty upgrades require separate reviewed compatibility/backfill plan. No auto destructive migration. Generator now writes CurrentSchema.review.sql/current-model JSON into a new directory only, never overwrites historical migrations. Review snapshot is not a deployment migration.

IAM initial schema compatible, no sensitive source data/permissions/session transfer and no two active authorities. Existing EF version counters/StationGrant/constraints remain. Down artifacts only empty tables; verified transactionally in Lab and rolled back. Backup/restore separate disposable target, no WITH REPLACE, no database DROP/RESET.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
