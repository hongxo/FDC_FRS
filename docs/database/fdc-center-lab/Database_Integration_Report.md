# Application integration and compatibility

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

New project Fdc.Center.Persistence references existing Core and Identity; CenterDbContext maps29 new tables plus existing catalog/security references excluded from migrations. Original IamDbContext/001 unchanged. Both contexts share the exact configured new DB connection in CenterDatabase opt-in profile, no secondary IAM connection. EF migration registration/embedded SQL/source generator/guarded operator tool are actual compiled code.

Real Center startup registers context, pending machine/station authorities, SqlRecordIngestion/SqlSoeIngestion/SqlStationRecords and startup schema/principal probe. Requires isolated IdentityRuntime,local Lab DB and non-sa/non-owner runtime. No startup migration, seed or legacy-key fallback. Runtime private SQL account created and DPAPI-protected outside Git. No permanent app service, existing config/production endpoint or Collector cutover performed; new profile is implemented and real HTTPS smoke-tested.

Record writer accepts only locally verified physical package: source identity formula,CFG/DAT+optionalHDR/INF, file size/SHA256/COMTRADE validation; serialized source snapshot+digest retained. Reads bound active Collector/IED/station inside SERIALIZABLE transaction,transaction-owned idempotency applock, creates metadata/files/history/outbox/attempt atomically. Concurrent replay same manifest returns same record, conflicting digest rejected. Different remote identities/content aliases are not globally coalesced; duplicate semantics beyond source identity require owner E04 decision. No broker/lease worker or upload API silently enabled.

SOE writer adapts actual Core.SoeEvent, bound batch<=500, real SQL idempotency lock/unique tuple, immutable replay comparison; field lengths bounded. No changes to edge SQLite/native reporting workers. Human read repository demands current authority before filtering/paging, pending by default. Fixture-only authorities are in tests; runtime SQL role additionally denies unscoped ingest/summary data. No frontend grants or RoleCode hardcoding.

Fault linkage and source snapshots tested; analysis tables are NOT_IMPLEMENTED execution metadata. No synthetic metrics reported as algorithm output. Canonical existing IAM workflow remains distinct; no record/analysis approval engine duplicated. Full Human Login/MFA/session issuance/admin mutation UI and all-writer D09 remain Phase4.2 gaps. New DB foundation does not close B01–B05/DR/security readiness.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
