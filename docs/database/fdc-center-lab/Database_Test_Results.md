# Actual database and regression results

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

| Test/workload | Expected | Actual | Result / evidence |
|---|---|---|---|
| New DB/create + explicit migrations | Exact local target only, compatible IAM, no source data copy | New FDC_Center_Lab,001–006 history | PASS deploy logs + postcheck |
| First005 add-column/check same batch | Atomic migrate or rollback | SQL207 compile failure,0 history005/0 new column afterward | FAIL candidate; preserved first SQL/postcheck/tool transcript |
| Corrected005 split batches | Guard each batch, no historical edit | Applied + replay no-op | PASS deploy-005-retest/replay-006 |
| Schema/model |53 app tables, PK/rowversion/restrict/UTC/no waveform blob |3 model tests PASS;68FK/211checks/159indexes/3views/4triggers,0 invalid | PASS model/TRX/inventory |
| FK/unique/check CRUD | Cross-station/orphan-station/duplicate/invalid queue rejected; optimistic conflict | Real SQL errors/rejected stale rowversion | PASS CenterDatabaseSqlTests |
| Physical COMTRADE + ingest | Actual checksums/format; retries same record |1 record,2 files,3 attempts,1 outbox/history after concurrent duplicate sends | PASS SQL test |
| Manifest metadata snapshot | Original JSON retained independent of catalog fields | Exact verified canonical JSON read back | PASS same ingest test |
| Corrupt file | Reject before SQL effects | Tampered DAT denied,0 records | PASS SQL test |
| Required outbox failure | No partial effect | Record/files/attempt/outbox rolled back | PASS SQL test |
| Human/Machine pending authority | No fallback or unexpected data | OwnerDecisionRequired; allowed fixture scope filter before limit; other station empty | PASS negative slice, not approved D04 |
| SOE replay/conflict | Core contract preserved, replay0, conflicting value rejected |1 event only, atomic conflict deny | PASS SOE SQL test |
| Fault lineage/analysis | Same station FK; no fake NotImplemented result | Link success, wrong station and nonnull fake result rejected | PASS fault SQL test |
| Runtime SQL principal | No sysadmin/db_owner; denied DML/secrets/global count | SQL229 for protected operations | PASS real runtime connection |
| Product startup/HTTPS | Real new SQL/non-sa; nonexistent cookie denied | Startup probe+actual HTTPS401,Machine403,static UI200 | PASS runtime SQL test |
| Migration replay | No duplicate schema/history |6 history rows,0 app data | PASS replay logs |
| Backup/restore/down | New restore DB only; empty-domain down probe reversible |53 restored/constraints/CHECKDB;24 inside down then rollback53 | PASS backup-restore-006 |
| Full Center suite | Existing behavior and new cases pass |19 PASS,0 FAIL,0 SKIP | center-final-006.trx |
| IAM components | Existing security contracts remain |184 PASS,22 gated SKIP | iam-components.trx (skips not counted) |
| Original IAM SQL regression | No change to authority/roles/outbox/races |21 PASS,1 optional profile SKIP across7 classes |7 individual TRX files |
| Full DR/production/fleet/real-data restore | Approved full scenarios | Not executed; applicable gates/code/env absent | BLOCKED/NOT_RUN, no PASS |

Initial compile defects (escaped SQL backslash, fixed table allowlist EF1003) corrected before final builds. SQL JSON export chunking normalized for metadata artifacts; no DB data defect inferred. No failed result overwritten as PASS. Final cleanup new DB53/0, old IAM24/0, both constraint sets trusted. Distinguish lab SQL infrastructure role/login from no IAM User/Role seed.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
