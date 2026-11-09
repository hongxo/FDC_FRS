# D04 — Dynamic RBAC governance

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D04; D08; D09. Approval owner:Business + Security + Architecture. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

## D04 quyết định cần ký

CURRENT25 baseline codes chỉ là đề xuất trong Dynamic_RBAC_Permission_Catalog/workbook; new DB Permission0 rows. Typed operation resolver có nhưng verifier luôn false. AdministrativeDelegationRow chỉ UserRoleId+PermissionId+TargetPopulation/validity, không operation dimension. Do đó one manage permission không thể tự phân loại requester/reviewer/approver khác nhau mà không thay model hoặc catalog. Các code role.read trong tests cũng không trở thành approved baseline.

| Option | Rationale/ưu | Trade-off/nhược | Khuyến nghị |
|---|---|---|---|
| A:giữ25 codes, thêm per-UserRole operation-bound delegation | Không tăng catalog codes; granularity ở envelope | Phải thêm schema/evaluator/gate/release dimensions; không thể chỉ khai báo global mapping | Hợp lệ nếu owner muốn governance theo operation envelope |
| B:catalog extension admin thao tác/stage riêng | Permission/RolePermission/GrantFunction/delegation hiện có dùng exact PermissionId; ít thay schema hơn | Nhiều codes/template review và tests; không được alias/migrate manage rộng quyền | PROPOSED preferred4.3B |
| C:manage cho mọi CRUD/review/apply | Ít sửa | Overgrant/SoD ambiguity | Không khuyến nghị; không là disposition REJECTED đã ký |

Giữ25 baseline nguyên trạng tới signed diff; new code rows chỉ là names đề xuất trong matrix, KHÔNG deploy, seed hoặc IsApproved=true. Catalog extension/deprecation phải có semantics, risk, scope, MFA, SoD, implemented-handler version, tombstone và rollout skew policy. Không tự cấp new code cho role cũ; manage không auto-map/alias.

## Invariants cần giữ

Business ALLOW phải tồn tại một active/current UserRole có active Role, exact current RolePermission, cùng UserRole-bound StationGrant và GrantFunction, validity/hierarchy versions và trusted object ownership đúng. Không sum permission Role A với scope Role B. Org→ManagementUnit→Station expansion do server current hierarchy; empty/unknown không là all. RecordId/file/object từ client không tự chứng minh Station; aggregate/count/filter/paging/download cùng scope vàobject checks.

Admin ALLOW dùng exact PermissionId+AdministrativeDelegation cùng actor UserRole, reviewed target population/function/expiry; StationGrant không thay thế. Grant/delegation mutations cần thêm containment của target scope/functions/expiry trong envelope của actor. Source AccessRequest phân nhánh admin-target và station resource, không accept cả hai; 4.3B cần trusted dual check trong cùng D09 transaction, không nhét scope từ browser hoặc bỏ station containment.

Request→Review→Approve→Apply giữ approval/effective status riêng. Candidate source yêu cầu3 PersonReference độc lập, không requester/reviewer/approver nào là current beneficiary; hai account không làhai người. Expand indirect beneficiaries qua role assignments hiện tại, kể cảcustom roles. Revision/digest/rowversion thay đổi làm approval cũ không applicable; apply re-read all actors/permissions/delegation/MFA/current impact. Approved không Applied. Apply internal, không public force endpoint hoặcCollector credential.

Role mới/custom role/clone chưa có quyền hiệu lực trước approval vàassignment/scope hợp lệ. Clone không copy users, grants, delegations, approvals. Seven defaults làtemplates, không trần số role; RoleCode/RoleName/IsSystemSeed không tạo authority. Mandatory ied.configuration.write deny không override. Emergency chỉ quy trình D06 signed, bounded, no OT write.

Current domain state machine không tự lưu approved database workflow. SQL apply role có tests nhưng authority provider pending; membership/station/delegation effective stores thiếu. Current SQL snapshot reader own Serializable transaction không thay commit recheck dùng same DbContext/transaction. D04 release phải bind operation catalog/policy/hierarchy versions vàdeployment support; unsupported/missing/stale deny.

## Acceptance sau approval

Custom role thứ8; exact code/unknown/retired/wildcard denied; cross-role andcross-station read/count/download denied; active/expiry/role deactivate; PersonReference alias/self-grant/indirect beneficiary; no role-name admin; non-admin grant envelope; concurrent policy/hierarchy change; pending/approved/applied UI; real SQL outage. Full DR vẫn cần entire workflow/runtime, không chuyển component PASS thành acceptance.

## Phiếu quyết định cần owner hoàn tất

Option được chọn:CHƯA CÓ. Named accountable approver/delegation authority:CHƯA CÓ. Security/OT/DBA concurrence và independent reviewer:CHƯA CÓ. Approval date/evidence URI/chữ ký:NOT_PROVIDED. Owner phải ghi exact option, document+catalog+route+policy/schema version/hash, phạm vi Lab/production riêng, rationale, trade-offs, residual risk/expiry/review triggers và prerequisite dispositions. Chỉ reviewer kiểm bằng chứng ký thực tế mới cập nhật register; không dùng checkbox/điền tên thay approval.
