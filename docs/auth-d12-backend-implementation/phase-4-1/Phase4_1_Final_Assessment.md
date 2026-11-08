# Phase 4.1 — technical assessment

2026-11-08 (Asia/Bangkok). Baseline `415cca4cb032528928518701e5606c47d37cd363`; source/test content `8848dd9ad716ba12a6445f8549ffbe5522c0a9cb`. Development/Lab only. APPROVAL/readiness không thay đổi.

**PARTIALLY_COMPLETED / BLOCKED. Không đề xuất Lab GO hoặc UAT READY.** Đây là nhận xét kỹ thuật của tác giả implementation; không thay reviewer độc lập hoặc chữ ký owner.

Đã trực tiếp bổ sung4 source files và5 test files, không chỉ viết lại tài liệu: SQL authority transaction gate; own-session revoke-current/all; deterministic/cooperating-writer SQL protocol tests; actual HTTPS→SQL negative integration và unknown-commit handling. Mọi thay đổi code được build/test/review/commit/push theo nhóm. Không thêm runtime mock/fallback hoặc quyền theo RoleCode/RoleName.

Actual build PASS;176 unit/component PASS,22 SQL/HTTPS cases PASS trong runs riêng;17 Node/8 fixture browser PASS. DB postcheck24 bảng/0 dòng ứng dụng, constraints hợp lệ; no migration. Toàn bộ29 DR vẫn BLOCKED ở mức acceptance; protocol probe/pass không tự đóng DR17, actual session fixture không là Login/MFA issuance.

## Blocker closure

- B01: phần SQL revocation đã viết/test; login issuer/shared throttling/lockout/MFA/recovery/rotation vẫn thiếu code hoặc choices. BLOCKED/PARTIAL.
- B02: D04 mapping/delegation/seed/authority vẫn pending; no broad administrative grant inferred. BLOCKED.
- B03: candidate gate code và7 protocol lab tests đã có; chưa owner-approved/wired/all-writer covered/least-privilege enforced, chưa real-role/real-grant multi-host race/HA. BLOCKED/PARTIAL.
- B04: actual negative HTTP/SQL slice có; official IAM checkout chưa có application host, original Center chưa integration, no deployed Human/Machine topology/full S01–S08. BLOCKED.
- B05: thêm atomic revocation/outbox evidence; thiếu full audit schema/archive/restore/DB permissions. BLOCKED/PARTIAL.

## Quyết định và đầu vào cần có để tiếp tục activation

1. Architecture/owner chốt runtime source/host integration và exact A01–A21 route release, reconcile /login versus /auth/login; D01 trust/listener/mTLS/proxy isolation.
2. Business/Security chốt versioned D04 operation mappings/delegated scopes/custody/seed manifest, không coi manage là mọi quyền.
3. Security chốt D02/D03/D05/D06 password/hash/throttle/session/revocation assurance, RP/origin/UV/TOTP exceptions/recovery custody/channel; không hardcoded defaults.
4. DBA/Architecture/Security disposition D09 candidate implementation: namespace/modes/isolation/timeout, actor+target lock plan recheck, every writer and direct-DML prevention, idempotent unknown-outcome resolution/failover semantics.
5. D10 schema/event fields + archive/key/retention/checkpoint/restore và real runtime least privilege; then implement missing writers/issuers and revalidate end-to-end.
6. Named independent QA/security reviewer executes/reviews full DR, source/transaction traces and live S01–S08 cohorts; formal acceptance separately.

No signing fields auto-filled or pending decision promoted. Existing P0/readiness unchanged. Coding độc lập được tiếp tục theo user authorization; không gán tất cả source gap thành chỉ thiếu approval, và không coi stub thành runtime complete.

Rollback: revert isolated opt-in library/tests without schema down or restoring stale authority. No host registration to undo. Test probes/fixtures cleaned/rolled back; imported test certificates were disposable, no trusted-store install/production PKI modification. Raw evidence remains local; official delivery remains stacked/draft, no auto merge.
