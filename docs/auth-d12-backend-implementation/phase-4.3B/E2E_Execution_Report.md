# End-to-end execution

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Full flow outcome:10 BLOCKED

No fake approval actors created. Test doubles/rollback-only sessions are component fixtures, not approved business actors. No API mock or substituted verifier used to claim successful E2E.

## FLOW 1: Login-MFA-Session-Profile

- Preconditions:Approved auth/RP/origin/session/recovery + enrolled person.
- Test actors:Real enrollee unavailable.
- Expected:Factor-bound session and own profile.
- Actual:Unavailable screen + SQL consumer denial only; issuer/verifier absent.
- Result:**BLOCKED** full flow; dependency:D02/D03/D05/D06.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 2: Role request-review-approve-apply

- Preconditions:Signed catalog/SoD + independent current persons.
- Test actors:Real requester/reviewer/approver NOT_PROVIDED.
- Expected:Applied revision after independent approval, atomic audit.
- Actual:Pure workflow/SQL repository subset only.
- Result:**BLOCKED** full flow; dependency:D04/D09/D10.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 3: Assign role-grant station-access

- Preconditions:Current grant writer and trusted scope.
- Test actors:Real admin/subject NOT_PROVIDED.
- Expected:Same UserRole-bound grant permits only target station.
- Actual:Evaluator/FK subset; effective store absent.
- Result:**BLOCKED** full flow; dependency:D04/D09/D10.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 4: Revoke grant-next deny

- Preconditions:Real grant endpoint and gate.
- Test actors:Rollback-only reader user, not approval actor.
- Expected:Post-revoke request/commit denied.
- Actual:Saved GrantVersion change invalidates reader session; not grant revoke endpoint.
- Result:**BLOCKED** full flow; dependency:D04/D05/D09.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 5: Unauthorized station denied

- Preconditions:Runtime scope mapping and object resolver.
- Test actors:Component synthetic tuples.
- Expected:Zero cross-station disclosure.
- Actual:Evaluator/scoped-repository negative subset only.
- Result:**BLOCKED** full flow; dependency:D04/D01.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 6: SQL unavailable-fail closed

- Preconditions:All runtime authority paths.
- Test actors:Rollback-only HTTP session fixture.
- Expected:No protected success/cache fallback.
- Actual:Actual connection failure/HTTP503 subset PASS, full business flow absent.
- Result:**BLOCKED** full flow; dependency:D01/D04/D09.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 7: Audit/outbox failure-rollback

- Preconditions:All sensitive runtime writers.
- Test actors:SQL repository fixtures, not approved actors.
- Expected:No effect/success on mandatory audit failure.
- Actual:Role/session/ingestion rollback subset PASS.
- Result:**BLOCKED** full flow; dependency:D09/D10.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 8: Machine mTLS cannot Human admin

- Preconditions:Approved PKI/two-host topology.
- Test actors:Real Collector cert actor unavailable.
- Expected:Certificate never Human admin principal.
- Actual:Component certificate guard only; real topology absent.
- Result:**BLOCKED** full flow; dependency:D01/D07.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 9: Human cookie cannot machine ingress

- Preconditions:Approved machine host/listeners.
- Test actors:Rollback-only Human session fixture.
- Expected:Cookie never machine authority.
- Actual:Human lane /api/stations403 PASS; machine host acceptance absent.
- Result:**BLOCKED** full flow; dependency:D01/D07.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## FLOW 10: Pending request never effective

- Preconditions:Integrated current request/apply workflow.
- Test actors:Real independent approvers unavailable.
- Expected:202/approval alone never grants rights.
- Actual:Domain/SQL/client state checks subset; runtime mutation absent.
- Result:**BLOCKED** full flow; dependency:D04/D09/D10.
- Evidence:Test_Execution.json + center-live-browser.json, subset only.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
