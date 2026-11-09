# Security regression

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Actual results / limits

| Check | Actual | Limit |
|---|---|---|
| Build |PASS,0 warnings/errors|Not security acceptance|
| IAM |243 PASS/1 SKIP/0 final FAIL|222 component+21 SQL; optional old query profile not opted in|
| Center |22 PASS/0 FAIL/SKIP|Includes actual SQL/HTTPS/browser negative paths/profile; not login success|
| Node |25 PASS|Product client contract/error states|
| NuGet |8 projects no vulnerable packages reported|Current advisory sources/time; not independent audit|
| Bounded secret/history patterns |0 findings|Known Lab password/GitHub token/private-key markers only|
| Runtime SQL principal |Direct DML/secret-read denials PASS|Not all-writer D09 acceptance|
| Mandatory audit rollback |Path-specific PASS|Not independent archive/restore proof|
| Full security acceptance |BLOCKED|B01-B05/reviewer/full runtime missing|

Existing regression covers cross-role/station/OT deny, versions, session proof primitives, CSRF/proxy/certificate guards and SQL outage. Real MFA replay/recovery/certificate revocation/proxy deployment/two-host failover unavailable. No complete OWASP acceptance or proof of SQL-injection absence; no new dynamic SQL writer introduced.

## Initial FAIL retained

Initial solution parallel run:IAM240 PASS/1 FAIL/1 SKIP; Center20 PASS/1 FAIL, SQL1205 fixture setup. Sequential-assembly attempt:IAM240 PASS/1 FAIL/1 SKIP, unrelated gate probe visible to FixtureAuthority.SingleAsync(). Isolated collection+sequential runner fixed fixture interference, in-test races unchanged; no retry/expected change. Exact deadlock graph NOT_COLLECTED, so production cause/closure not asserted.

Harness initially refused valid target because PowerShell IDictionary adapter treated ConnectionString assignment as a key; explicit setters/getters corrected before test DML. SQL diagnostic initially shell-quote/QUOTED_IDENTIFIER errors, then ring buffer returned no graph. These diagnostics are not tests marked PASS. Raw failure records retained.

B01-B05 High acceptance blockers still OPEN; no independent security review or P0-IAM/readiness promotion. Bounded scan is not exhaustive secrets assurance. Dependency logs/hashes portable; raw evidence local only.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
