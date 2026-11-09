# D09 transaction integration

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Actual code / tests

Borrowed-context authority reader and candidate intent lock-coverage validator delivered. Existing SqlAuthorityCommitBoundary/plan provider not activated. No selection of effective lock namespace/timeout/isolation from recommendation. Active transaction is not D09 serialization.

New2 SQL reader tests PASS. Existing7 SQL protocol probe tests PASS, covering both gate orderings/policy locking/timeout/cancel/rollback/unknown commit ACK per methods in Test_Execution.json. These are non-authoritative probes, not real two-host Role/Grant race acceptance. Existing dedicated runtime principal IAM DML/secret-read denial tests PASS.

Initial parallel solution run had2 SQL1205 FAIL in SessionRevocation/CenterHTTPS fixture setup. Sequential assemblies then exposed1 FAIL:RoleCrud fixture SingleAsync saw a gate probe row. Shared fixture setup now isolated, no mutation retry. In-test races remain concurrent. Exact SQL deadlock graph NOT_COLLECTED:corrected system-health ring-buffer read returned no graph. Do not claim exact lock cycle proved or production race fixed. Raw failures retained.

## Outstanding blockers

D09 writer path/principal/procedure/isolation release pending. ActorId-only provider not integrated with trusted intent. Full W01-W19 current actor/target/MFA/SoD/impact checks, controlled direct-DML prevention, durable unknown-outcome lookup, actual multi-host failover NOT_IMPLEMENTED/BLOCKED. No procedures/GRANT/DDL changed. Same-context helper is a prerequisite only; callers must obtain reviewed gates and commit effect/audit/outbox together.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
