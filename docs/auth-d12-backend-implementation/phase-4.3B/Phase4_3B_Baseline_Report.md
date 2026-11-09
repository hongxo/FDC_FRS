# Phase4.3B baseline

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Verified baseline / actual results

Origin:https://github.com/hongxo/FDC_FRS.git. PR3/4/5/6/7 OPEN/DRAFT at baseline. Branch feature/d12-phase4-3b-iam-runtime from exact PR7 tip1961beb; no merge/reset/force-push. PR7 docs-only tip already contains PR6 implementation ancestry; original workspace source untouched. Required reports/ADRs/DR plan/A01-A21 compared; input hashes attached.

| Lab | Tables | FK | Rows before/after | Migrations | Users/AuthorityVersion rows after |
|---|---:|---:|---|---|---|
| FDC_Center_Lab |53|68|0/0|001-006 unchanged|0/0|
| FdcIam_DevelopmentLab |24|33|0/0|001 unchanged|0/0|

Disposable/DevelopmentLab markers verified. CHECKCONSTRAINTS post-run no violations. Center67FK in historical DB final report is stale; actual68, already noted in4.3A. No migration/seed/copy of users,passwords,sessions,grants or runtime connection change. DML limited to guarded lab fixtures, rolled back/cleaned; post-run zero rows.

Actual build PASS, final IAM243 PASS/1 optional profile SKIP, Center22 PASS, Node25 PASS. Initial failures retained. I01/I02 implemented in bounded scope; P02 reader foundation PARTIAL. Full I03 route/DTO freeze and I04 additive schema remain unresolved, not falsely marked complete. Sensitive runtime P01-P17 awaits exact decisions and implementation. Scope PARTIALLY_COMPLETED/BLOCKED.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
