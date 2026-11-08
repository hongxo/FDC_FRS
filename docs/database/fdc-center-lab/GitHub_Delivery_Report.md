# GitHub delivery

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

Official origin https://github.com/hongxo/FDC_FRS.git. Baseline PR5 OPEN/DRAFT,head9cd3588; PR1–PR4 also OPEN/DRAFT,dependencies retained. Branch feature/fdc-center-lab-database created from exact verified Phase4.2 tip. No main merge/reset/force push.

Pushed code commits: e8127d5 database schema/repositories/tools/Center integration;3512fb0 actual integration/security/performance tests;070fc09 additive source-manifest snapshot and SQL batch remediation;a0a6096 station FK for optional-Collector events. Raw evidence/backups/DPAPI secret/runtime configs excluded from Git. Applied SQL bytes preserved explicitly by Git attributes; original source/Collector checksums unchanged. Pattern-based history scan recorded,not a comprehensive security audit.

Documentation/evidence index committed separately. Draft PR must target feature/d12-phase4-runtime-integration while PR5 remains open. PR URL recorded only after observed creation. No independent reviewer or domain approvals supplied; Draft stays pending,not readiness acceptance.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
