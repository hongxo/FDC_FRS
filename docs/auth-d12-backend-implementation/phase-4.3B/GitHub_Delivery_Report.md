# GitHub delivery

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

Official origin:https://github.com/hongxo/FDC_FRS.git. Branch feature/d12-phase4-3b-iam-runtime stacked on feature/d12-phase4-3a-decisions PR7. PR3-PR7 OPEN/DRAFT at baseline; no merge/force/reset. Source commits787f306/a3e1df4/e307a98/e1a3f3e; full latest source in header.

Build/tests/diff/secret-history bounded scan completed. Immutable migrations001-006/Program/host activation/Core/Collector/POC source unchanged. No secret config/DPAPI/private keys/raw proof shipped.

Observed delivery:branch pushed; Draft PR https://github.com/hongxo/FDC_FRS/pull/8 verified OPEN/isDraft=true, base feature/d12-phase4-3a-decisions, head feature/d12-phase4-3b-iam-runtime. Initial package head93b3041d5552a9e51e622c4483c179f9190c1417, source e1a3f3e; this documentation follow-up records actual result. No merge/force-push. Delivery is not acceptance. Full system PARTIAL/BLOCKED, readiness unchanged. Rollback reviewed feature reverts; no auto-merge.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
