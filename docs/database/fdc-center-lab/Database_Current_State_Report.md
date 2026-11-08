# Current state and source assessment

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

## Verified before and after

Before: FDC_Center_Lab absent (master database-list inventory), FdcIam_DevelopmentLab ONLINE with24 app tables/0 rows/69 indexes/33FK/93checks, migration001 and4 triggers, no application sessions on new target. Other database names were inventoried only from master metadata; none connected/modified. PR1–PR5 all OPEN/DRAFT; chose exact PR5 tip, created feature/fdc-center-lab-database. No reset/merge/force push. Original root has no .git; separate authorized checkout retains dependency history.

After: FDC_Center_Lab53 app tables + EF history,0 app rows,68FK/211checks/159indexes/3views/4 inherited triggers, no untrusted/disabled constraints. History001/002/003/004/005/006 product10.0.14. Old IAM still24/0 with001 only,33FK/93checks. No user-defined stored procedures/functions deployed;4 existing IAM/security triggers retained. No accounts/sessions/effective permissions transferred. Runtime SQL login is infrastructure identity, not IAM User seed.

Source inventory reads original Core/Center/Collector C#/projects and hashes; original SQLite Store shared by edge/legacy center has records/commands/health/observations/telemetry/events, WAL and per-operation connections. Core Manifest/BundleReceiver use content-aware SHA256 identity and local COMTRADE files; EventStore/Core.SoeEvent carry SOE metadata. Native libiec61850 boundary untouched, no native/IED operation performed. Independent SQL IAM has24 entities and immutable001 SQL/EF migration; source and bytes preserved.

## Missing/duplicates/compatibility

No second User/Role/Station/ManagementUnit/SecurityEvent authority table. Reuse canonical names; thin Center EF references excluded from migrations. IedDevice models source IED/relay; no duplicate Relay table without taxonomy decision. StationUnitHistory stores intervals/open uniqueness, not an automatically approved hierarchy editor. Legacy SQLite data not converted/copied; opt-in SQL profile does not silently replace legacy Store/InboxWorker. Record metadata/waveform/file durability integration and migration of operational data require separate E03/E04 cutover. Old IAM schema read-only inventory is not migration source data.

New independent domain tables implemented; complete analysis algorithms, operational approval workflows, queue workers, PKI enrollment, D10 archive and full business API/UI remain missing or pending. Detailed gaps in Database_Gap_Register.md.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
