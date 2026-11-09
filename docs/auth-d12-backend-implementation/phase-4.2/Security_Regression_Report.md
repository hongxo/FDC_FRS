# Security regression

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

## Commands actually executed

From official checkout, secrets only transient environment, cleared afterward:

- dotnet restore Fdc.Identity.slnx --source C:/Users/lavie/.nuget/packages --packages ../d12-nuget -p:NuGetAudit=false
- dotnet build Fdc.Identity.slnx --no-restore
- dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-restore --logger trx --results-directory local-evidence
- Same test project --no-build --no-restore --filter FullyQualifiedName~SQL-class, sequential seven classes
- dotnet test tests/Fdc.Center.Tests/Fdc.Center.Tests.csproj --no-build --no-restore --logger trx --results-directory local-evidence (confirmed lab and FDC_RUN_CENTER_BROWSER=1)
- node --test poc/authentication-ui-v1/tests/management-client.test.mjs
- dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive --format json --source https://api.nuget.org/v3/index.json
- git diff --cached --check; source/commit-history secret pattern review
- sqlcmd authorized local target using environment credential, read-only postcheck.sql

| Run | Actual | Limit |
|---|---|---|
| Solution build | PASS0 warnings/errors | No SLA claim |
| IAM suite |184 PASS/22 SQL-profile SKIP | Includes8 new mapping tests |
| Sequential7 SQL classes |21 PASS/1 profiling SKIP | Protocol/rollback fixtures, not full DR |
| Committed-source Center suite |4 PASS:3 startup+1 HTTPS/SQL/browser | No credential-login issuance |
| Browser inside Center test |8 screens+401+3 viewports PASS | Negative policy slice only |
| Node client |17 PASS | Supplemental component |
| Official NuGet audit |6 projects, no vulnerabilities reported | Point-in-time feed, not comprehensive review |
| SQL postcheck |24/0/33FK/93checks/0 invalid | Sysadmin runner; least privilege not proven |
| Full DR |0 PASS/29 BLOCKED | Exact original expectations retained |
| Restore/AG failover/real2-host/380 fleet |NOT_RUN/BLOCKED | No fabricated throughput/percentiles |

Evidence covers same-assignment deny, OT deny, version/MFA/CSRF primitives, real disabled/revoked/missing cookie, SQL missing DB/timeout, forged headers ignored, machine path403, legacy dashboard404 in Human profile, no-store/CSP. CSRF registration/component tests are not live mutation acceptance. Parameterized source review is not a penetration test.

Failed browser trials retained; final retests separate. No unauthorized success observed in tested slice; cannot infer no Critical/High defects throughout system. Secret scan pattern-based, not full independent audit. Raw TRX/screenshots ignored; sanitized evidence hashes/counters published.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
