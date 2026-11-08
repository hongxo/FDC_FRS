# D12 Phase 2 V2 — implementation report

Status: PARTIALLY_COMPLETED / BLOCKED_BY_POLICY. Development coding is authorized by the latest user instruction; formal D01–D11/E01–E08 decisions and P0-IAM readiness are unchanged.

User confirmed using existing FdcIam_DevelopmentLab instead of FDC_Center_Lab and authorized a separate clone of hongxo/FDC_FRS. No database rename or production access.

## Actual implementation

Nine additive C# source/test files; existing source unchanged (hash comparison, excluding runtime data/bin/obj). New source:
- Security/AuthenticationPrimitives.cs: ASP.NET PasswordHasher wrapper with explicit cost/length inputs; random 256-bit canonical opaque proofs; purpose-bound SHA256 digests; session/version/epoch validation.
- Application/AuthenticationService.cs: password proof orchestration and transaction/policy contracts. Default pending policy/store refuse issuance. No production mock implementation.
- Application/MfaContracts.cs: factor, challenge and policy interfaces; pending policy blocks activation.
- Infrastructure/SqlSessionAuthorityReader.cs: primary-only current session/user/person/authority join per request; no cache.
- Infrastructure/SqlCurrentAuthorityReader.cs: coherent read-only SQL authorization snapshot; exact UserRole-bound grants/functions/delegation; reviewed permission policy and trusted object resolver required. Pending mapping grants nothing.
- Api/HumanSessionHandler.cs: opt-in HTTPS cookie handler; current SQL authority check; no role claims, proxy identity or certificate fallback; outage challenge503.
- Api/HumanRequestSecurity.cs: exact HTTPS Origin/Host guard and ASP.NET antiforgery validation; secure host cookie helper.
- Tests: Phase2SecurityTests.cs, Phase2SqlLabTests.cs.

No routes registered in Center, no Collector/UI changes. Authentication/session are NOT operational end-to-end in a host. This is an independently testable implementation subset, not completion of all Phase2 requirements.

## Database and verification

Existing migration 202611080001_InitialIamFoundation remains installed. Phase2 migrations created/applied:0. No permanent seed. SQL tests insert synthetic user/session/authority rows only inside a rolled-back transaction and assert zero remaining rows.

SDK10.0.403 build: PASS,0 warnings/errors. Initial discovery used SDK11 preview; final verification explicitly selected installed .NET10 via temporary global.json. Unit/component suite:102 PASS,2 SQL tests SKIPPED without lab environment. Separate explicitly authorized SQL run:2 PASS,0 skipped. Thus104 unique tests passed across the two runs; this does not mean all security acceptance scenarios passed. Raw TRX in evidence/.

## Remaining work and exact blockers

| Dependency | Blocked functionality | Required input |
|---|---|---|
| D02/D05 | SQL login issuance, lockout/shared throttle, idle interaction/rotation/session cap | Hasher cost/password rules; timeout/cap behavior; normalization/source-key policy |
| D03/D06 | MFA verification/enrollment/step-up and password reset/recovery | Reviewed provider/library, RP/origin/UV/device policy, TOTP exception, independent custody |
| D04 | Effective permission activation, operation mapping, approved seed and delegated target resolution | Versioned catalog/mapping/envelopes and immutable seed manifest |
| D09/D10 | Sensitive mutation/revoke/login transactions and required audit/outbox | Commit serialization and audit failure-policy disposition; implementation and SQL race/atomicity proof |
| D01/A01–A21 | Host routing/listeners and HTTP login/me/logout/reset contracts | Exact reviewed contract; proxy trust/route isolation configuration |

Read-only serializable snapshot is NOT the pending D09 commit/revoke solution. CompletePasswordProofAsync is a transactional contract, not an implemented SQL issuer. No runtime fallback is supplied for missing policy/provider.

Rollback: additive unregistered files may be reverted without database changes. Never restore stale grants or drop IAM data as code rollback. Phase3 owns full Role CRUD and request/review/apply workflow; interfaces/primitives do not authorize their activation.

## Verified delivery checkout

User approved an initial IAM-only import into the README-only official repository. Stable10.0.403 build/test repeated from that checkout:102 unit/component PASS,2 SQL-gated skips in default run; separate real SQL run2 PASS. Code commits:c8f5782,e24fd3b,09fde6b. Reports will be delivered in a documentation commit and a draft PR. Raw TRX/binlogs are local-only, deliberately excluded from GitHub generated-artifact delivery.
