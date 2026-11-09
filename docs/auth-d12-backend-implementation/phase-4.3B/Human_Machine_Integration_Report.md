# Human/Machine integration

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Actual host

Program/CenterIdentityRuntime/DI unchanged. Guarded opt-in Development/Testing/Lab Human profile remains HTTPS loopback with opaque-cookie SQL consumer, /api/stations403, missing/revoked cookie401, pending policy503, no operational workers/legacy-key principal. Current Center runtime regression uses existing non-sa/non-db_owner credential against Center Lab; old Lab rollback-only fixture also tested. No new endpoints/listeners, production flags or connection configuration changed.

Actual HTTPS forged headers/identity/revoked proof/SQL failure and browser SQL-backed denial tests PASS. These do not prove complete reverse-proxy/PKI/two-service topology. Component certificate rejection PASS is not real Collector issuance/revocation/station-binding acceptance.

## Outstanding blockers

D01 listener/Host/proxy/TLS termination/direct bypass/exact routes and D07 CA/EKU/PKI/station binding PENDING. Legacy flag-off host still a deployment isolation concern. No trust-all forwarding, certificate guard removal, network/firewall/Collector change. Full flows8/9 BLOCKED; Human-lane machine denial is a subset. Pending release/intent helpers cannot activate authentication schemes.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
