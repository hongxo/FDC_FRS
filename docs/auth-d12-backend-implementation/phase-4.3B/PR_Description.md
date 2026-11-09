# Phase4.3B SQL authority foundation

Existing snapshot reader owns its transaction and cannot serve as same-context commit recheck. Extract borrowed-transaction authority reader; preserve standalone snapshot and reject null proofs before SQL. Add policy evidence/intent validation without activating pending decisions. Product UI distinguishes safe precondition/rate-limit errors. Isolate empty-singleton SQL fixtures and sequence assemblies; in-test concurrency remains.

Validation:build0 warnings/errors; IAM243 PASS/1 optional SKIP; Center22 PASS including real SQL/HTTPS/browser negatives/profile; Node25 PASS. Initial failures retained. NuGet and bounded secret/history scans recorded. No migrations/seeds/permission/production config/Collector change.

Draft stacked on PR7. Not complete Login/MFA/effective grants/workflow/UI/D09/D10. Full10 flows/29DR BLOCKED; B01-B05 open, independent reviewer missing, readiness unchanged.

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.


## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
