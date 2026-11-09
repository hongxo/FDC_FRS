# SQL authority integration

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Implemented

SqlTransactionalAuthorityReader borrows caller IamDbContext/SQL transaction, reads current session/user/authority/UserRole/Role/RolePermission/Permission/StationGrant/Scope/GrantFunction/AdministrativeDelegation, and passes same context to resource resolver. Never begins/saves/commits/disposes caller transaction. Rejects no active transaction or dirty unflushed changes before recheck; no cached allow/role-name authority.

Standalone SqlCurrentAuthorityReader reuses helper under its own Serializable snapshot; it is still not a commit gate. Same connection string alone is not atomicity.

## Actual SQL tests

Two new Center tests PASS: rollback-only current session is visible inside caller transaction; resolver confirms same context/connection/transaction; dirty GrantVersion change rejects; flushed version change makes session stale and skips resolver. Borrowed transaction stays open, rollback removes all User/Session/AuthorityVersion fixture rows. Missing-session read never consumes transaction. Pending policy yields no permission descriptors.

## Outstanding blockers

No D09 locks/current actor-target/MFA/SoD guarantee inferred. Concrete reviewed resource/permission/admin providers absent; helper not wired as effective mutation authority. Target FDC_Center_Lab follows current user instruction for Lab; production canonical architecture still pending. Old Lab remains isolated regression only; no copied authority/dual authority. Migrations created/applied0/0.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
