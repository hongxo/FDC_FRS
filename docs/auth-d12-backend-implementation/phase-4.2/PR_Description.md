# Phase4.2 PR description

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Integrates actual Fdc.Center startup with an opt-in loopback Development/Lab IAM profile, real SQL session validation and canonical admin GET adapters. No operational SQLite/inbox workers start in this profile. Business operations503 pending reviewed mapping, machine path403, legacy dashboard not served. Original Core/Center source imported because stacked history previously contained only IAM.

Adds runtime /identity unavailable authentication screens and same-origin real admin reads, versioned operation resolver default-denied pending owner review, and product-factory HTTPS/SQL/browser tests. Original workspace source/Collector unchanged.

Validation: build PASS;184 IAM component PASS;21 SQL regression PASS (optional profile SKIP);4 Center PASS including live SQL/HTTPS/browser8 negative screen checks+3 viewports;17 Node PASS.24 app tables/0 rows/valid constraints; no migration.6-project NuGet no vulnerabilities reported. Initial browser failures preserved and separately retested.

B01–B05/29 full DR remain partial/BLOCKED. Login/MFA/recovery issuance and sensitive admin mutations not enabled. No Lab GO/UAT/production acceptance; independent reviewer missing.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
