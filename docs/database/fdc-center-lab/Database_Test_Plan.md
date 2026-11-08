# Database test plan and execution contract

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

Expected and actual cases are separated in Database_Test_Results.md. Dataset strictly SYNTHETIC: GUID-keyed corp/unit/two stations/IED/Collector assignment, tiny generated valid ASCII CFG/DAT, metadata records, linked fault, NOT_IMPLEMENTED analysis with NULL result, Core SOE event. No human password/admin/grant seed. Fixture allow providers only in tests; runtime defaults pending. Exact-ID cleanup reverses FK order and verifies0 rows. SQL guard checks local machine/target/labels/no real users.

Automated sources: DatabaseModelTests; CenterDatabaseSqlTests; CenterRuntimeDatabaseTests; existing CenterStartupTests/CenterHttpsSqlLabTests; existing IAM tests. Profile5000 synthetic metadata rows,5 warmups,50 scoped queries; measures actual latency and captures SQL STATISTICS XML. This is not COMTRADE ingestion throughput or380-Collector benchmark.

Commands executed:

```text
dotnet test tests/Fdc.Center.Tests/Fdc.Center.Tests.csproj --no-restore --logger trx --results-directory <local-evidence>
dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <local-evidence>
dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --filter FullyQualifiedName~<SQL-class> --logger trx --results-directory <local-evidence>
```

Center run explicitly sets both confirmed lab targets/connections, DPAPI runtime secret-file path, FDC_CENTER_EVIDENCE, FDC_RUN_CENTER_BROWSER=1, NODE_PATH to existing Playwright and browser evidence path. Never log secrets. Without opt-in, gated tests SKIP; SKIP is not PASS. Original IAM SQL classes run sequentially to avoid fixture contention. No service-kill/outage of operating systems; failure injection isolated to callbacks and unavailable SQL targets. No native IED calls.

Future blocked: fully approved Login/MFA/recovery, all-writer D09/revoke/commit product multi-node, real PKI collector identity, data-bearing restore/RestoreEpoch across sessions, coordinated object-store recovery, recurring scheduling/analysis approval/workers, representative380 station load. Existing DR01–DR29 acceptance unchanged; component/schema completion cannot close them.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
