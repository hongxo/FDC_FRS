# D04 — Seven templates, seed vàcustody

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D04; D06; D12. Approval owner:Business + Security + IAM custodians. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

## CURRENT

v1-roles.pending.json có7 tên template, roleId/roleCode=null, permissionCodes rỗng, catalogVersion0, approvalReference null. Cả haiDB không có User/Role/Permission/StationGrant data. Không thể dùng tên template hoặcSysAdmin label để tạo authority. Current SeedInstallation làjournal idempotency, không proof owner ký. Không biến scenario seed drift test thành approved manifest.

## Options/khuyến nghị

A:signed versioned7 templates, custody riêng vàcustom roles dynamic; B:first-user default admin; C:hardcode fixed7 authority. PROPOSED:A. Ưu:release cótraceability, không auto membership. Nhược:cần verify person/custody/seed diff vàbootstrap support; không demo login ngay. B/C không khuyến nghị, không được tự đánh dấu record REJECTED.

| Tên template giữ nguyên | Intended boundary đề nghị, chưa permission assignment |
|---|---|
| System Administrator | Operations/platform functions only; không identity/grant/engineering approval mặc định |
| Security Administrator | Identity/security change requests vàcustody theo exact policy; không self-approve |
| Engineering Operator | Station engineering intents theo approved grant; không IED config write |
| Reviewer | Independent business revision review; không tự làIAM reviewer |
| Approver | Independent business approval; không tự làglobal administrator |
| Auditor | Scoped business audit; security audit cần separate delegation |
| Viewer | Scoped read; download/export không mặc định |

Owner phải bind immutable role IDs/codes, names, exact approved catalog version/permissions, description, release hash/signatures vàcompatibility diff. RoleCode phục vụ metadata, idempotency/history; không authorization switch. Seed definitions không User, password, UserRole, StationGrant hoặcdelegation. Custom roles ngoài7 được quản lý bằng cùng SQL/workflow, không UI ceiling.

Bootstrap là quy trình D06 riêng:hai custodian thật độc lập, verified PersonReference vàreal factor enrollment; bounded initial capabilities cần explicit signed assignment/grant intent vàaudit. Không tạo database startup bypass hoặcfirst registration becomes admin. Nếu chưa có catalog/custody release thì vẫn0 accounts, đúng fail-closed.

Last-custody safety đánh giá khả năng khôi phục độc lập thực tế theo active users/factors/permissions/delegations/scope/validity, không đếm role tênSystem Administrator. Before removal/disable/recovery apply, recompute custody graph trong D09; đề nghị duy trì ít nhất2 independent recovery custodians trong từngpopulation đã chọn. Ngoại lệ cần incident authorization riêng, no standing superuser. Review expiry/offboarding/role permission drift; person aliases không tăng count.

Seed replay chỉ same signed hash/version; conflicting replay deny, không resurrect revoked memberships orreplace changed permissions. Catalog upgrades không auto grant new codes. Test:unapproved hash, extra code, role drift, custom8th role, zero accidental memberships, last-custodian concurrent removal, factor loss/recovery self-approval. Chưa chạy ceremony ở 4.3A.

## Phiếu quyết định cần owner hoàn tất

Option được chọn:CHƯA CÓ. Named accountable approver/delegation authority:CHƯA CÓ. Security/OT/DBA concurrence và independent reviewer:CHƯA CÓ. Approval date/evidence URI/chữ ký:NOT_PROVIDED. Owner phải ghi exact option, document+catalog+route+policy/schema version/hash, phạm vi Lab/production riêng, rationale, trade-offs, residual risk/expiry/review triggers và prerequisite dispositions. Chỉ reviewer kiểm bằng chứng ký thực tế mới cập nhật register; không dùng checkbox/điền tên thay approval.
