# Lab deployment runbook

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

## Confirmed scope and safety

Run only from official checkout, selected stacked branch. Verify git status/remote/branch/log and PR5 current tip before changes. Exact server MachineName XOTH-ETC-2/SQL major16, local lpc:. and target FDC_Center_Lab; expected source IAM read-only. No Production config/IED/Collector changes. Never auto merge. User request authorizes new Lab database; domain-policy approvals remain separate.

Tool: tools/Fdc.Center.Database, not application startup. Supply FDC_CENTER_CONFIRMED_LAB=FDC_Center_Lab and FDC_CENTER_LAB_CONNECTION through transient secret environment; do not echo/save connection text. Clear environment in finally. First inventory target/state/sessions/data/permissions/backup paths; stop on existing data, unmarked DB or other application sessions. No DROP/TRUNCATE/RESET/WITH REPLACE.

Commands actually used (hash placeholders are content SHA256 of reviewed UTF8 SQL resources, all six mandatory):

```text
dotnet restore Fdc.Identity.slnx --source C:/Users/lavie/.nuget/packages --packages ../d12-nuget -p:NuGetAudit=false
dotnet build Fdc.Identity.slnx --no-restore
dotnet run --project tools/Fdc.Center.Database --no-build --no-restore -- generate <new-review-directory>
dotnet run --project tools/Fdc.Center.Database --no-build --no-restore -- deploy <001-hash> <002-hash> <003-hash> <004-hash> <005-hash> <006-hash>
dotnet run --project tools/Fdc.Center.Database --no-build --no-restore -- provision-runtime <outside-checkout-DPAPI-file>
dotnet run --project tools/Fdc.Center.Database --no-build --no-restore -- recover
```

Deployment verifies hash before opening DB, server identity/SQL version/operator privilege, Lab labels/empty target/no other users, then COPY_ONLY/CHECKSUM backup+VERIFYONLY. Migrations applied explicitly via EF and guarded SQL. Original IAM001 retained; new002–006 additive. Replay observed no-op with6 history rows. Generator produces review snapshots only and refuses existing output, never regenerates applied SQL in place.

Runtime provisioning performed once: random SQL login + user in fdc_center_runtime only, encrypted connection DPAPI CurrentUser outside checkout. Re-running against existing encrypted file stops, no silent rotation. Operator credential is not runtime identity. Lab SQL role contracts are not IAM Role grants. Manual cleanup/rotation of infrastructure principal requires named owner and exact identity; no default human account.

## Center profile configuration

Use approved secret channel to supply the decrypted runtime connection as ConnectionStrings__FdcIdentity; same connection powers both contexts. IdentityRuntime__Enabled=true, CenterDatabase__Enabled=true, ASPNETCORE_ENVIRONMENT=Lab and an explicitly approved loopback HTTPS listener/certificate. Do not persist plaintext config or install test CA into trust store. Tests use ephemeral cert with exact HttpClient pin; no permanent running service/certificate/config changed.

Startup verifies new DB and non-sa/non-sysadmin/non-db_owner; fails if legacy-key mode selected. App exposes existing protected/blocked routes; Machine ingress remains denied on Human lane and sensitive policy paths pending. No automatic migration/seed on startup, no live old-service cutover. Stop old Human authority instances before any future approved cutover; never point different contexts to separate active DBs. Actual lab tests prove startup/access, not full business readiness.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
