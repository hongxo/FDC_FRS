# UI S01-S08 integration

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Actual source

Product management.mjs distinguishes401/403/409/412/428/429/503 with stable local codes/safe text. 401 suspends/clears private state;409/412 reload;428 precondition;429 manual wait/retry. No server exception body rendered, automatic retry or fixture fallback. Existing503 message preserved for browser compatibility. No endpoint/status contract changed.

8 new Node tests PASS; total25 PASS includes17 baseline. Each status test verifies1 fetch, no parsing private error body/retry. Unknown sensitive exception text is not shown. POC source unchanged; its tests run against product module.

| Screen | Actual integration result | Outstanding |
|---|---|---|
| S01 Login |Policy-unavailable rendering PASS|Real login issuer BLOCKED|
| S02 MFA |Policy-unavailable rendering PASS|Real WebAuthn BLOCKED|
| S03 Reset |Policy-unavailable rendering PASS|Recovery BLOCKED|
| S04 Profile |Policy-unavailable rendering PASS|Successful session/profile BLOCKED|
| S05 Users |Real SQL-session/API503 denial PASS|CRUD workflow BLOCKED|
| S06 Roles |Real SQL-session/API503 denial PASS|Effective mutations BLOCKED|
| S07 Station |Real SQL-session/API503 denial PASS|Grant workflow BLOCKED|
| S08 Audit |Real SQL-session/API503 denial PASS|Authorized archive/audit views BLOCKED|

Actual Playwright/Edge8 screens, anonymous401, desktop/tablet/mobile overflow and no JS page errors PASS. SQL session fixture rollback-only, not Login issuance/approved authority. Full A01-A21 freeze/CSRF/ETag/step-up/mutation integration and independent accessibility acceptance outstanding. Responsive subset is not WCAG compliance or successful business E2E.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
