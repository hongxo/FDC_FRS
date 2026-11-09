# Dynamic RBAC implementation

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Actual delta

Existing SQL role request/CRUD/history and evaluator retained. New internal AuthorityCommitIntent carries operation/target/revision/declared impact/affected users; validates actor presence, exact affected-user exclusive gate coverage, exclusive policy gate for declared policy impact and strongest duplicate normalization. Negative tests PASS. Unknown/empty intent rejected.

Candidate not HTTP-bound, not wired into pending D09 provider. It does not prove affected set completeness or classify impact instead of server policy, and is not an authorization ALLOW. Role labels never grant rights; seven templates are not a ceiling.

## Outstanding blockers

D04 operation/catalog/delegation/SoD/seed/custody release PENDING. UserRole/StationGrant/Delegation effective writers/current runtime authority not complete. No permission/template/user/grant seed or direct mutation endpoint. Mandatory ied.configuration.write deny unchanged. Cross-role/station/expiry/OT component and SQL repository regressions PASS, but real custom-role workflow DR remains BLOCKED. Next:approved mapping then concrete providers/stores under D09/D10.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
