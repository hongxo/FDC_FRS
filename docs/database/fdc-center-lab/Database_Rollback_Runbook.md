# Rollback and recovery runbook

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

Actual recovery tool backed up final53-table empty Lab WITH COPY_ONLY,CHECKSUM, verified backup, restored into unique new FDC_Center_Lab_Restore_<GUID> with new physical MDF/LDF paths, no WITH REPLACE/overwrite. Physical CHECKDB and constraints PASS. Restore databases retained/disposable and backups retained; do not automatically delete them. Their exact names/paths are in local evidence. Old FdcIam_DevelopmentLab never restored/altered.

Actual down probe in a transaction on confirmed empty Lab:004 SOE down then002 domain down,24 canonical tables remain; ROLLBACK restores53, all data zero. Later005 source snapshot column/constraint is removed only as its empty parent table is transactionally dropped; no source data discarded. No committed destructive down or DB deletion occurred. Recovery is schema/empty-data proof, not recovery of real users/files/fleet traffic.

Before any rollback: stop writers, verify exact server/target/disposable labels, app rows, sessions and verified backup; capture hashes/history/object/file state. Down artifacts refuse nonempty new tables or nonnull snapshot loss. Never execute down against data-bearing shared Lab to satisfy a test. Prefer roll-forward compatible fix; migrate to new disposable target to validate alternatives.

Code: revert isolated feature commits after review, do not switch Human authority to legacy keys. Schema: do not edit applied001–006; create next additive version. No automatic drop of shared schemas, canonical IAM tables or SQL login. Preserve audit/approval/record histories, outbox parity and external files. Future data-bearing restore requires RestoreEpoch/session invalidation, schema/policy/catalog/hierarchy reconciliation and independent D10 checks; these are still blocked. SQL-only backup cannot prove private COMTRADE store consistency. Retention/RTO/RPO and cleanup window pending owners.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
