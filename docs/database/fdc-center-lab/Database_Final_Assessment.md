# Final Lab database assessment

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

**FOUNDATION_IMPLEMENTED_AND_LAB_VERIFIED; COMPLETE_SYSTEM/BUSINESS_SECURITY_ACCEPTANCE: PARTIALLY_COMPLETED / BLOCKED.**

Actual new FDC_Center_Lab,53 application tables/0 data rows,67 restrictive FKs/211 checks/159 indexes/3 views/4 original triggers,001–006 applied. Old IAM unchanged24/0/migration001. No database dropped/overwritten or sensitive authority copied. SQL/EF migrations,repositories,real file verifier,source snapshot/SOE adapters,Center DI/startup guards and automated tests delivered as code,not only documents.

Actual build0 warnings/errors. Final Center19 PASS,0 FAIL/SKIP (15 new +4 existing), including physical file integrity/concurrent idempotency/outbox rollback/fault/SOE/source snapshot/scoped read/runtime SQL denial and HTTPS/browser regression. IAM184 component PASS/22 gated SKIP; original SQL21 PASS/1 optional profile SKIP. Actual query P50/P95/P99 recorded on5000 synthetic metadata only. Empty-schema backup/restore/checks and transactional down rollback PASS; migration replay no-op. First005 SQL batch failed,complete rollback verified,corrected unapplied candidate and retested successfully; no failed run relabeled.

No actual380-station catalog imported,default user/password/admin/grant seeded,analysis result fabricated or production/native device touched. Dedicated SQL runtime not sa/db_owner; operator migration is still privileged Lab execution,not production least-privilege assurance. Archive/data-bearing RestoreEpoch/PKI/all-writer/session/MFA/business workflows pending; original P0/Lab/UAT gates unchanged.

## Before activation / acceptance

Approve exact D01 trust/routes and D04 mappings/custody; approve D02/D03/D05/D06 auth/factor/recovery/session; resolve D09 writer/principal/serialization; D07 machine cert binding; D10 audit/archive/restore. Complete dependent providers/writers/algorithms/worker/UI functionality as scoped,not inferred from tables. Rehearse nonempty migration and coordinated file recovery on separate disposable targets. Independent QA/Security full DR01–DR29 and live business scenarios,then owner acceptance separately. No Lab GO/UAT READY claim here.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.

Observed GitHub delivery: Draft PR https://github.com/hongxo/FDC_FRS/pull/6; feature/fdc-center-lab-database stacked on unmerged PR5. Source a0a6096,tested final Center19 PASS; report package78135b7 plus delivery follow-up. No acceptance/readiness status changed.
