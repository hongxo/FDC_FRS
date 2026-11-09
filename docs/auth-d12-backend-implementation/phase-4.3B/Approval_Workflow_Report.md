# Approval workflow

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Actual

Existing independent-person/beneficiary/revision/digest/policy state-machine and SQL role repository regression PASS. Test-only synthetic repository authorities are not real approved runtime actors. No new real/fake approval actors or bootstrap authority created.

New fixture isolation fixes a verified test collision: RoleCrudSqlLabTests.FixtureAuthority reads AuthorityVersion.SingleAsync() across whole table while gate tests create non-Center unconfigured probe rows. Intermediate run failed with multiple elements. IAM database collection isolates setup; concurrent participants inside gate tests remain concurrent. No expected outcomes changed and no retry added.

## Outstanding blockers

Full persistent request-review-approve-apply across UserRole/StationGrant/Delegation with current actors/beneficiaries at commit absent; D04/D09/D10 pending. No public force-apply or effective permission on202/Approved. No unsigned DTO/status/route changes. Full flows2/3/10 BLOCKED; pure state-machine/fixture PASS is not real workflow acceptance.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
