# Phase 4.1 — truy vết B01–B05 và dependency

2026-11-08 (Asia/Bangkok). Baseline `415cca4cb032528928518701e5606c47d37cd363`; source/test content `8848dd9ad716ba12a6445f8549ffbe5522c0a9cb`. Development/Lab only. APPROVAL/readiness không thay đổi.

## Phạm vi đối chiếu

Đã đối chiếu toàn bộ17 báo cáo Markdown Phase4, Phase2/Phase3 Implementation Report, Phase3 Security_Test_Report và source tương ứng; hashes trong evidence/input-manifest.json. Phase4 không có file tên Security_Test_Report.md; dùng các báo cáo chuyên sâu/Hardening của Phase4 và Security_Test_Report của Phase3, không tạo alias giả. Không sửa hồ sơ Phase4 hoặc quyết định D/E.

Branch bắt đầu từ Phase4 PR#3 chưa merge: feature/d12-phase4-remediation dựa trên feature/d12-security-validation. Chủ dự án xác nhận lại dùng FdcIam_DevelopmentLab cho Phase4.1, không tự chuyển/tạo FDC_Center_Lab.

## Root cause có bằng chứng — không đồng nhất policy với source gap

| Blocker gốc | Evidence source/schema chính xác | Gap còn lại | Delta Phase4.1 | Trạng thái toàn blocker |
|---|---|---|---|---|
| B01: session/login/MFA/recovery và D03/D05/D06 | AuthenticationService.cs:18,47 có PendingAuthenticationPolicy/PendingLoginTransactionStore; MfaContracts.cs:7–18 chỉ factor interfaces/pending; SqlSessionAuthorityReader chỉ đọc registry. Schema User không có shared failure/lockout counters. | SQL login issuer/shared throttle/lockout, real factor verifier/enrollment, rotation/reset/recovery writer và policies chưa có. Không phải mọi phần chỉ thiếu wiring. D02 cost/password policy cũng có trong source dependency. | Thêm SqlSessionRevocationService: revoke-current/revoke-all, SecurityVersion/challenge invalidation + audit/outbox;3 SQL cases PASS. | PARTIAL_IMPLEMENTATION, vẫn BLOCKED để nghiệm thu Login/MFA |
| B02: D04 mapping/delegation/seed/custody và authority provider | PendingManagementReadAuthority (ManagementQueries.cs:14), PendingRoleMutationAuthority (RoleCrudAuthority.cs:17), PendingPermissionPolicy (SqlCurrentAuthorityReader.cs:16). Seed manifest vẫn template thiếu approved IDs/catalog. | Operation→permission release, real read/delegation/impact authority, full User/UserRole/StationGrant/delegation request/review/apply; không được suy role.permission.manage cấp mọi thao tác. | Không thay catalog hoặc dùng role-name bypass. Existing SQL role CRUD + reads retest PASS. | BLOCKED, không đóng B02 |
| B03: D09 serialized commit/revoke/multi-node | Baseline AuthorizationService.cs:36 chỉ PendingD09CommitBoundary; RoleCrud yêu cầu transaction từ caller. | Operation-specific plan, shared namespace, all-writer coverage, actual authority/MFA callback, SQL least-privilege prevention of direct bypass, HA/failover still chưa được duyệt/chứng minh. | Có SqlAuthorityCommitBoundary và7 real SQL tests trên candidate protocol. PendingSqlCommitGatePlanProvider vẫn chặn; không host registration. | CANDIDATE_CODE_AND_LAB_EVIDENCE; D09/DR17 vẫn BLOCKED |
| B04: Human/Machine host chưa tích hợp | Official IAM checkout không chứa src/Fdc.Center/Fdc.Collector; Fdc.Identity.slnx chỉ library/tests/schema tool. Original workspace Fdc.Center.csproj chỉ reference Fdc.Core; Program.cs còn Demo/AdminApiKey/global RequireCertificate. Hash/metadata ở host-source-assessment.json. | Phải thống nhất runtime-host import/integration scope và D01 listener/proxy/scheme/cert binding; không có real login/UI host để wire. Không suy deployment thực tế từ source. | Có test-only Kestrel HTTPS→real Human handler→real SQL→canonical role adapter. Authenticated fixture vẫn503 pending; forged/revoked/disabled401, SQL failure503. | TEST_SLICE_ONLY; deployed boundary/live S01–S08 vẫn BLOCKED |
| B05: full D10 audit schema/archive/restore/least privilege | Entities.cs:114/119 có SecurityEventRow/AuditOutboxRow; actual sys.columns được lưu trong schema-columns.json; SecurityEvent không có TargetId/RequestId/Reason/Before/After summary. Append-only trigger không là independent archive. | Event schema/migration release, structured redaction, archive/checkpoint/retention/restore và runtime DB permissions chưa có. | SQL revocation+outbox failure rollback được kiểm chứng, bổ sung evidence ngoài Role CRUD cũ. Không tự tạo migration/immutable archive hoặc gán tamper-proof. | ATOMIC_STORE_SUBCASE_PASS; B05 vẫn BLOCKED |

Các paths trong bảng tính từ src/Fdc.Identity trừ original host. Đây là trạng thái source/schema được đọc; không khẳng định đã khảo sát production. Các pending implementations còn nguyên; test fixture providers chỉ ở test assembly.

## Dependency matrix và thứ tự khắc phục

| Priority | Work item | Input/owner disposition cần có | Phụ thuộc | Deliverable/exit evidence | Actual |
|---|---|---|---|---|---|
| P0 | Chốt route/host baseline D01 + A01–A21 | Architecture/Security + project owner; named sign-off chưa có | none | Exact namespace/listeners/proxy/cert trust + approved runtime source import scope | BLOCKED |
| P0 | Chốt operation mapping/custody D04 | Business/Security; versioned catalog/envelopes/seed hashes | D01 contracts | Every read/request/review/apply maps exact approved code, no wildcard/manage expansion | BLOCKED |
| P0 | Review candidate SQL gate D09 | DBA/Architecture/Security; modes/namespace/principal/timeout/isolation, all writers | D04 scopes, D01 primary topology | Approve/alter concrete code, cover all actual writers + no direct-DML bypass, real authority+two-host race evidence | Candidate implemented/tested; decision PENDING |
| P1 | SQL self-session revocation | D05 current/all assurance and response semantics; D09/D10 | gate and real session | Real-context callback, revoke before cookie clear, atomic event;3 lab cases available | SQL service completed as opt-in; activation BLOCKED |
| P1 | SQL LoginTransactionStore/schema | D02/D05 password cost, normalization, shared limiter/lockout, idle/absolute/cap rules | D09, D10, reviewed schema migration | Real credential verification→MFA restriction→session issuance; no bypass/default account | NOT_IMPLEMENTED |
| P1 | MFA/recovery | D03 RP/origin/UV/device/TOTP exception; D06 custody/channels | login/challenge schema, keys/time | Real-factor enrollment/proof/replay/recovery tests; no fake OTP | BLOCKED_BY_POLICY + source gaps |
| P1 | Full D10 audit and runtime privileges | Security/Auditor/DBA + archive/storage owners | schema/authority boundaries | Versioned expansion, least privilege, independent archive and combined restore | NOT_IMPLEMENTED/NOT_RUN |
| P2 | User/Role/Grant/delegation workflow + UI | Frozen D04/SoD/state/status contracts, complete Human runtime | above | Request/review/approval/apply + S01–S08 real E2E by cohort | partial library only |
| P3 | Independent DR/UAT review | Named independent reviewer and criteria | all runtime + policies | Full29 cases, HA/restore and acceptance sign-off | NOT_PROVIDED |

Owners trên là vai trò cần nhận quyết định, không phải người đã ký. Không đòi production-load acceptance để tiếp tục code độc lập. Coding có thể tiếp tục trong scope cho phép; activation và chốt gate chỉ sau đúng disposition/evidence.

Route dependency cụ thể: A02/A11 trong AUTH_UI_API_Contract_Proposal.md dùng /api/identity/v1/login và /logout; chỉ đạo Phase2 có candidates /api/identity/v1/auth/login và /auth/logout. Cả hai chưa có signed final contract trong hồ sơ. Không tự chọn/alias để làm UI xanh; cần freeze route/schema release trước map auth host.
