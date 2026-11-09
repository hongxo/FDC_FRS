# D10 audit and recovery

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Actual

Required audit/outbox failure regressions on Role/session and Center ingestion PASS:rollback/no false success. Existing dedicated runtime SQL principal denial tests PASS using existing DPAPI credential artifact; no new principal provisioned. Operator sa used only guarded lab fixtures/inventory, not runtime identity.

## Outstanding blockers

Expanded target/request/reason/sanitized before-after/full versions/factor fields, independent immutable archive/checkpoints/keys/retention/failure policy and restore ceremony D10 PENDING. No migration001-006 edits or migration007/seed/acceptance flags created. No credentials/session proofs/private keys copied into audit.

Backup/restore NOT_RUN_THIS_PHASE; approved nonempty restore/provider/reconciliation absent. Historical empty restore is not current PASS. Full RestoreEpoch invalidation of sessions/challenges/step-up, data/file/archive reconciliation and tamper/gap/key-loss acceptance BLOCKED. Reader transaction rollback is not disaster recovery evidence. Next:release D10 model, additive review/dry-run on separate disposable target, all-writer audit atomicity and independent archive review.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
