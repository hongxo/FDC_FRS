# Database architecture and lab ADR

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

One new Lab database, existing schemas catalog/iam/ops/ingest/audit. No ornamental analysis/reporting/integration/config schemas: analysis metadata under ops, read views in owning domains. Same configured SQL connection for both IamDbContext and CenterDbContext in the opt-in Center profile; no second configured IAM authority. Old DB remains inert/empty and no live service cutover performed. This is user-authorized Lab implementation of proposed single-DB architecture, not production E02/D09 approval.

PKs use application-assigned GUIDs consistent with IAM, not a new SQL IDENTITY convention. Existing IAM Version+RowVersion unchanged; all29 new tables use RowVersion, CreatedAtUtc, optional UpdatedAtUtc, IsSynthetic flag. UTC checks, bounded strings, unique identities and restrictive FKs. Lifecycle Active/status on mutable catalog/processing data; no blanket soft-delete for immutable histories. No cascade delete, NOLOCK or waveform blobs.

Independent file/object storage contains CFG/DAT and optional HDR/INF. SQL stores relative object keys, lengths/hash/validation and operational journal/outbox. VerifiedRecordPackage reads local files/checks real SHA256/COMTRADE structure before candidate SQL ingest. SQL transaction cannot prove NAS durability or prevent external postverification changes; production storage immutability/retention/reconciliation and DB+file backup coordination pending. Test storage root is process-owned synthetic temporary data.

IAM D04/D09/D10 preserved; machine authority and Human station reads default pending. SQL app role denies authority DML/secret reads/unscoped record aggregates. No direct sensitive CRUD endpoint exposed by new repositories. Alternative separate IAM DB rejected for this Lab scope because no approved separate-authority baseline exists and user requests unified schema; production alternative remains owner decision. No credentials or certificates moved from old DB.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
