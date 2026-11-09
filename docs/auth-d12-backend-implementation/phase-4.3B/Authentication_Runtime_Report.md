# Authentication runtime

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Implemented / actual

Existing opaque secure-cookie SQL-primary consumer retained. SqlSessionAuthorityReader now rejects null/malformed digest before factory call; null/empty/malformed tests PASS.

New PolicyReleaseValidator checks environment, required decision IDs ordinal, exact policy/catalog/hierarchy/RestoreEpoch, validity, payload SHA256 and independent evidence. Expiry/cancellation rechecked after verifier await. PendingPolicyReleaseEvidenceVerifier always denies; helper not registered as effective policy provider. Test evidence double is not a real owner/approval actor.

## Outstanding blockers

Login issuer, shared SQL limiter, credential completion, rotation/password change/reset NOT_IMPLEMENTED/BLOCKED_BY_POLICY. D02/D05/D06/D09/D10 values and custody absent. No guessed hash cost/timeouts/lockout, default password/admin, login alias or fake session endpoint. PendingAuthenticationPolicy/PendingLoginTransactionStore remain. Existing password-in-store callback must be refactored to avoid long-held SQL locks; not solved by read refactor. Full Login-MFA-Session-Profile BLOCKED, not component PASS.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
