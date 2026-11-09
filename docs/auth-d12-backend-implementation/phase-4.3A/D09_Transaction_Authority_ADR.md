# D09 — SQL transaction authority

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D09; D05; D10; E02. Approval owner:DBA + Architecture + Security. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

## CURRENT candidate và các option

SqlAuthorityCommitBoundary:primary check, transaction-owned applocks, Policy trước User GUID ordinal, strongest mode từ đầu, no nesting/dirty context; callback phải recheck/mutate/audit. Normalize cho prefix<=80, timeout1..30000ms, ReadCommitted hoặcSerializable; runtime Center vẫn PendingD09CommitBoundary. Tests candidate dùngReadCommitted và prefix test, không là deployment policy. SqlCurrentAuthorityReader own Serializable read transaction là read snapshot, không commit gate.

| Option | Ưu | Nhược/risk | Khuyến nghị |
|---|---|---|---|
| A:common SQL gate + controlled procedure writers, single DB | Có enforced writer path/atomic authority+effect+audit | Refactor EF direct writers/permissions và review SQL policy/procedure | PROPOSED preferred |
| B:application applocks + EF table DML | Ít sửa candidate | Principal bị compromise/direct DML bỏ qua gate; không đạt prevention chỉ bằng convention | Chỉ test candidate, không acceptance mặc định |
| C:row/procedure serialization alternative | Có thể tối ưu contention | Cần chứng minh cùng order/fresh authority và all writers | DBA option hợp lệ sau review |

sp_getapplock resource là database-scoped, Transaction owner cần active transaction; không tự ngăn mộtwriter không dùngprotocol.[SQL Server documentation](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver16)

## Proposed protocol release cần owner chốt

Candidate prefix Fdc.Iam.Authority.v1 (PROPOSED), same database/principal namespace trên mọi node. Policy gate Shared cho ordinary bounded mutation/revoke; Exclusive cho role/catalog/hierarchy/delegation-impact change khó giới hạn. SauPolicy, lock user set gồmactor + targets/beneficiaries cần thiết theo GUID.ToString(N) ordinal, mode mạnh nhất ngay đầu. Không upgrade khóa giữa transaction. Shared-user cho action chỉ đọc quyền; Exclusive-user cho security/grant/session changes. Role change ban đầu dùngExclusivePolicy để bảo thủ vàổn định impact set; đo contention trước tối ưufiner gates.

Ưu tiên Serializable cho baseline mutation mới khi chưa chứng minh read sets đầy đủ; ReadCommitted làalternative nếu controlled writers+locks+fresh rechecks chứng minh tương đương. Không coi candidate7 tests(ReadCommitted) là evidence choisolation mới. Timeout candidate2000ms Lab để owner so sánh, không production default; negative code/deadlock/cancel phảiabort, không callback success. SQL transaction/key-range locks vẫn cần cùng order vàshort duration; đưa password hashing/network/archive IO ra ngoài gate.

ISqlCommitGatePlanProvider.ResolveAsync(actorId) hiện không mang operation/target/revision nên chưa đủ trusted intent.4.3B cần CommitIntent server-derived vàimpact enumeration/recheck trong gates; client không chọn prefix/mode/users. Actor session/MFA/permission/target object/delegation/reviewer+approver/current beneficiaries/policy/catalog/hierarchy/security+grant versions đọc lại trênCÙNG connection/DbContext transaction. Không gọi reader tự mở transaction riêng rồi coi atomic. Required audit/outbox/version bump cùngcommit; source Role apply đã bump impacted User.GrantVersion vàAuthority.PolicyVersion, tránh phủ nhận code đã có.

Direct DML prevention:runtime khôngINSERT/UPDATE/DELETE authority tables; restricted execute modules/procedures phải tự kiểmrole/policy/version/gate/audit, không chỉ nhậnclient allow=true. Current RoleCrudService EF writes vàrevocation phảiadapt sangpath đã chọn hoặcSQL-enforced equivalent trướcactivation. Separate migrator/audit/maintenance identities; owner có thểbypass SQL vẫn cầnprivileged change process và independent evidence. Lab operator sa không là runtime deployment model. No SQL GRANT hoặcprocedure được sửa ở 4.3A.

## Revoke/commit race và failure modes

| Ordering/failure | Acceptance cần chứng minh |
|---|---|
| Revoke acquires exclusive target gate, commits trước | Nextmutation obtains gate, re-reads current versions/active grant vàDENY; 0 effect |
| Mutation acquires gates trước | Revoke waits; effect+audit/outbox commit trước, revoke commits sau; nextrequest denies |
| Two affected-role membership changes | ExclusivePolicy hoặccomplete target set prevents impact-set phantom/SoD drift |
| SQL timeout/deadlock/cancel before commit | Rollback/discard tracked context; no effect/no misleadingsuccess |
| Lost commit ACK/connection during commit | OUTCOME_UNKNOWN; query durable actor/action/digest outcome trước retry; không blind retry |
| Failover/node restart | No stale allow-cache; primary-only re-read; idempotency+epoch reconciliation; keyring not an authority cache |
| Direct DML or privileged backfill | Runtime principal bịdeny; operator change cannot skipcontrolled release/integrity review |

Idempotency bind actor+action+normalized payload/scope/revision/digest+catalog/policy version; conflicting key reuse reject. Role replay fixP4-D002 giữ original applied history version, không trả current version. Generic outcome store và authorized statuslookup cònthiếu. Unknown outcome không tự đổi thànhfailed rồi gửi lạiPOST hoặc clear approvals.

Multi-node acceptance phải2 actual app hosts, same primary SQL, deterministic barriers+SQL wait/transaction traces, cả2 legal orderings, sessions/grants/role changes/restore/failover/outbox failure, principal DENY tests; không sleep-only evidence.7 candidate protocol tests chỉsubset lịch sử, không full real-role/grant application race. D09 closure cầnfull writer inventory phía sau và independent review.

## Phiếu quyết định cần owner hoàn tất

Option được chọn:CHƯA CÓ. Named accountable approver/delegation authority:CHƯA CÓ. Security/OT/DBA concurrence và independent reviewer:CHƯA CÓ. Approval date/evidence URI/chữ ký:NOT_PROVIDED. Owner phải ghi exact option, document+catalog+route+policy/schema version/hash, phạm vi Lab/production riêng, rationale, trade-offs, residual risk/expiry/review triggers và prerequisite dispositions. Chỉ reviewer kiểm bằng chứng ký thực tế mới cập nhật register; không dùng checkbox/điền tên thay approval.
