# Phase4.3A architecture/security decision package

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D01–D12/E01–E08. Approval owner:Domain owners. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

Documentation-only architecture/security decision package based on verified Phase4.2 and database PR6.

Includes20 formal decision records(all PENDING), D01/API route ownership, authentication options, D04 operation/permission extension proposals andseed custody, D09 writer inventory, D10 archive/restore design, database alignment, B01–B05 dependencies andthe4.3B backlog. New permission names remain unapproved proposals, with no catalog seed or effective grant.

Read-only verification:old IAM24 tables/0rows; Center53 tables/0rows, 68FK/211CHECK andmigrations001–006. No DB merge, connection/configuration change, privilege change orruntime source edit. Historical67-versus68FK reporting andstale source comments are reconciled without rewriting earlier evidence.

All5 blockers still require both decisions andimplementation. No runtime/benchmark/DR acceptance tests rerun in this phase; documentation QA validates package consistency andunchanged source. Existing user-scoped Lab authorization is recognized separately from formal owner approvals.

Draft PR stacked on unmerged database PR6. No automatic merge, Lab GO, UAT READY orP0-IAM closure.
