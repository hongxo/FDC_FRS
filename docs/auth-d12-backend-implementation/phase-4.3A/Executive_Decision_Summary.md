# Executive decision summary

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D01–D12/E01–E08. Approval owner:Project authority + domain owners. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

Bộ hồ sơ version0.1 đã được chuẩn bị để owner review. Đây chưa phải baseline được phê duyệt để kích hoạt chức năng nhạy cảm.

**Quyết định formal:0 APPROVED, 20 PENDING, 0 REJECTED, 0 SUPERSEDED.** Mẫu số20 gồm19 quyết định AUTH-R2 và một record D12 riêng. Authorization Development/Lab mà user đã cấp vẫn được ghi nhận VERIFIED; thiếu chữ ký formal không phủ nhận phạm vi công việc đã được cho phép.

**Cả5 blocker B01–B05 cần quyết định và cả5 vẫn thiếu code/integration.** Phê duyệt policy không thay thế implementation hoặc acceptance. Source đã có host thật, SQL reader và database foundation; không tiếp tục mô tả chúng là hoàn toàn chưa tồn tại. Readiness giữ nguyên, không Lab GO/UAT READY.

## Những lựa chọn cần trình owner

| Nhóm | Khuyến nghị PROPOSED | Trade-off chính |
|---|---|---|
| D01 | Tách Human/Machine thành service riêng cho production; giữ Lab loopback hiện có; freeze A01–A21 | Tách trust tốt hơn nhưng thêm service/PKI/release operations |
| D02/D03/D05/D06 | IdentityV3 PBKDF2 sau calibration; WebAuthn UV cho privileged users; opaque SQL sessions; recovery độc lập | Tái dùng source hiện có, đổi ưu tiên so với Argon2id trong hồ sơ cũ; cần owner chọn assurance/device/custody |
| D04 | Catalog extension rõ từng thao tác/stage, UserRole-bound StationGrant vàdelegation độc lập | Thêm code/release review; không auto mở rộng25 baseline codes hay gán quyền cho7 template |
| D09 | Một Center DB, common commit gate vàcontrolled SQL writer paths | Refactor EF direct writes vàkiểm contention; candidate7 tests chưa là all-writer acceptance |
| D10 | Audit/outbox atomic vàarchive độc lập cócheckpoint/key custody | Thêm storage/delivery/restore operations; empty restore không chứng minh recovery với dữ liệu thật |

Các candidate timeout/lockout/hash targets trong auth ADR chỉ là phương án để so sánh, không config có hiệu lực. Các PermissionCode mới trong matrix chưa được catalogued, seed hoặc phê duyệt.

## Những việc Phase4.3B có thể chuẩn bị ngay

I01–I04:typed policy/release validators mặc định deny; route/DTO/state compatibility tests; deterministic SQL/process evidence harness; additive schema/principal impact designs. Chỉ làm trong scope Development/Lab khi được yêu cầu; không tự wire chức năng nhạy cảm hoặc cấp quyền.

Login/MFA/session issuance, concrete authority providers, UserRole/StationGrant apply, workflow, all-writer D09, audit archive vàfull UI/E2E cần owner release tương ứng và implementation/test thật. Backlog nêu code location, dependencies, acceptance vàcomplexity; không bịa ngày công.

## Database recommendation và approval

Ưu tiên single central database, nhiều schema. FDC_Center_Lab đã có cho integrated Lab; production canonical architecture vẫn PENDING. FdcIam_DevelopmentLab giữ vai trò test target riêng; không tự copy credential/session/effective grants, hợp nhất DB hoặc đổi connection.

Owner cần chọn option, bind version/hash/scope vàcung cấp named approver, concurrence, approval date/evidence, residual risk. D07/D08 PKI/OT, D11 independent review vàE01–E08 production evidence có owner-role trong register. Không tự điền chữ ký hoặc thay đổi readiness.

Thứ tự đọc:Baseline→Architecture/Security Registers→D01/Auth/D04/D09/D10→Database Compatibility→B01–B05→4.3B Backlog→Final Assessment.
