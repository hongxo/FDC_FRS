# Phase4.3B final assessment

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

**PARTIALLY_COMPLETED / BLOCKED. Không Lab GO/UAT READY/P0-IAM READY.**

## Delivered actual code

- Borrowed-transaction authority reader reused by standalone snapshot; null/malformed proof fail-closed.
- Typed release validator:environment/decision/version/epoch/hash/trusted evidence/expiry after await; default verifier deny, not effective policy activation.
- Candidate internal commit intent/affected-user lock coverage; not wired as D09 provider.
- Product UI safe401/403/409/412/428/429/503, no body leak/automatic retry/mock fallback.
- Isolated singleton SQL fixtures + secret-environment sequential Lab runner, preserving actual concurrent participants and expected outcomes.
- Real automated tests/negative HTTP/browser, limited measured workloads and bounded scans.

Source commits:787f306 SQL reader; a3e1df4 release/intent validators; e307a98 runner/UI/fixture hardening; e1a3f3e policy-neutral UI wording. Source_Manifest binds exact files. New cases:38 IAM,3 Center,8 Node relative baseline. Final IAM243 PASS/1 optional SKIP, Center22 PASS, Node25 PASS; build0 warnings/errors. Initial2 SQL1205 FAIL and intermediate1 fixture FAIL retained, not relabeled. Results are component/path-specific, not full acceptance.

## Definition of done

| Requirement | Status |
|---|---|
| Center actual consumer/SQL foundation |PARTIAL; negative path verified|
| Authentication/MFA |BLOCKED_BY_POLICY + missing issuers/verifier|
| Session revocation |SQL subset PASS; effective API/policy integration BLOCKED|
| SQL authority |Read foundation PARTIAL; concrete mapping/commit integration BLOCKED|
| Dynamic RBAC/StationGrant |Components/SQL subset; effective workflows BLOCKED|
| Independent workflow |Candidate only; current persistent runtime BLOCKED|
| D09 all writers |BLOCKED|
| D10 audit/restore |BLOCKED|
| UI S01-S08 complete |BLOCKED; negative/error states verified|
| Full DR |0 PASS/29 BLOCKED|
| Critical/High acceptance issues |B01-B05 remain OPEN|
| Independent reviewer |NOT_PROVIDED|
| GitHub |See actual observed delivery report|

## Next activation prerequisites

Owner-selected D01 routes/trust; D02/D03/D05/D06 hash/password/throttle/RP/origin/UV/session/recovery/custody; D04 catalog/delegation/SoD/seed; D09 canonical DB/writer/principal/gate protocol; D10 event/archive/retention/restore. Approval must bind version/hash/scope. Then finish real providers/issuers/writers/UI and full actor/two-node/restore/DR verification with independent reviewer. Authorization to code does not choose policy values.

No migration/seed/principal/connection/production config or Collector/IED/SCADA/firewall change. Both Labs post-run0 application rows; histories unchanged. Production single Center DB/multiple schema remains recommendation, not newly approved. Rollback reviewed source reverts, never down applied migrations or restore stale grants.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
