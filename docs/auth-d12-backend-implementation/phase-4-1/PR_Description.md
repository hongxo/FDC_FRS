# Phase 4.1 — draft PR

2026-11-08 (Asia/Bangkok). Baseline `415cca4cb032528928518701e5606c47d37cd363`; source/test content `8848dd9ad716ba12a6445f8549ffbe5522c0a9cb`. Development/Lab only. APPROVAL/readiness không thay đổi.

## Dependency

Stacked on feature/d12-security-validation; depends on unmerged Phase4 draft#3 and preceding#2/#1. Do not merge out of order or treat this as an acceptance approval.

## Implementation

Adds opt-in SQL transaction-owned Policy/User application locks with normalization/timeout/rollback/unknown-outcome handling. Adds own-session current/all revocation with SecurityVersion/challenge invalidation and atomic audit/outbox. Supplied pending policy/plan implementations still deny; no application host registration or permission expansion.

Adds deterministic real SQL gate tests (actual DMV WAIT barriers and durable probe reads), rollback session tests, and actual HTTPS Kestrel→real HumanSessionHandler→real SQL→canonical role adapter negative integration. No mock authentication. SQL policy/session fixtures are test-only; no runtime test bypass or default user.

## Verification

.NET10 build PASS;176 component/unit PASS;22 explicit SQL/HTTPS cases PASS including bounded query observation;17 Node and8 intercepted-fixture browser checks PASS.24 tables/0 application rows after cleanup, constraints valid. No migration or persistent seed. Raw execution evidence stays local, not committed.

## Limits

B01–B05 remain open/partial; no real login/MFA/recovery issuer, approved D04 authority provider, all-writer D09 integration or full D10 archive/restore. Full DR01–DR29 and live UI S01–S08 remain BLOCKED. Test host is not deployed Center, candidate probe races do not certify real IAM race freedom. No Lab GO/UAT READY/P0 closure, no independent sign-off or automatic merge.
