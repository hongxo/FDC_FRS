# Database foundation PR description

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

Creates the explicitly authorized new FDC_Center_Lab with canonical IAM001 and additive Center002–006, preserving existing IAM data/authority and Collector code. Adds29 domain tables (53 total),station-bound FKs/constraints/indexes/views and source manifest snapshots. No human user/password/admin/grant seed or waveform binary storage.

Adds real EF SQL repositories for verified physical COMTRADE metadata/idempotent concurrent ingestion,Core SOE replay/conflict handling and authority-before-scope read queries. Adds guarded migration/backup/recovery tooling and an opt-in actual Center startup using a dedicated least-privilege SQL runtime principal; unapproved Human/Machine business paths remain fail-closed.

Validation: final Center19 PASS; IAM184 components PASS (22 SQL/profile gated SKIP) and21 sequential SQL PASS (1 optional profile SKIP). New DB53/0,68FK/211checks/159indexes,6 migrations; old IAM24/0 unchanged. Backup/restore to unique disposable target and empty-domain transactional rollback PASS; actual5000-row scoped query profile and SQL plan captured without SLA/fleet claims. First005 compile batch failure fully rolled back,corrected and retested; no applied migration edited.

Pending: D01–D10 applicable policies,PKI,real scoped authority/mutation providers,all-writer D09,D10 immutable archive/data-bearing restore,analysis/operational business workflows,representative380-station benchmark and independent review. No full-system completion,Lab GO,UAT or P0 closure. Draft stacked on unmerged PR5; no automatic merge.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
