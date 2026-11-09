# Gói phê duyệt 20 quyết định

Các phương án, tác động, dependency và acceptance theo [ma trận](IAM_20_Subdecision_Decision_Matrix.md). Dữ liệu dưới đây là đề xuất để owner review.

| ID | Phương án đề nghị theo nguồn | Chủ trì phê duyệt | Bằng chứng bắt buộc | Trạng thái |
|---|---|---|---|---|
| D01 | B production; A chỉ lab có guard | Architecture + Security + OT + App owner · OWNER_UNCONFIRMED | Topology/route release,proxy bypass+cert negative tests | AWAITING_APPROVAL |
| D02 | B giai đoạn đầu,owner xác nhận assurance; A hợp lệ | Security + IAM operations · OWNER_UNCONFIRMED | Cost/concurrency measurements,blocklist/normalization/limits release | AWAITING_APPROVAL |
| D03 | A privileged; B exception có hạn | Security + Business + PKI/support · OWNER_UNCONFIRMED | RP/origin/UV/devices,library review,replay/enrollment evidence | AWAITING_APPROVAL |
| D04 | B admin granularity,25 baseline giữ nguyên tới release | Business + Security + Architecture · OWNER_UNCONFIRMED | Signed catalog diff,operation map,scope/SoD/seed/custody | AWAITING_APPROVAL |
| D05 | A | Security + Architecture + SRE · OWNER_UNCONFIRMED | Session limits/rotation/keys/HA/SQL-outage acceptance | AWAITING_APPROVAL |
| D06 | A; không bypass | Security + Business + IAM custodians · OWNER_UNCONFIRMED | Named independent custodians,proof lifecycle,loss/incident rehearsal | AWAITING_APPROVAL |
| D07 | A | PKI + OT + Security · OWNER_UNCONFIRMED | CA/EKU/revocation/station binding/renewal/reuse tests | AWAITING_APPROVAL |
| D08 | A | OT + Protection engineering + Security · OWNER_UNCONFIRMED | Collection envelope/change workflow,safety authorization riêng | AWAITING_APPROVAL |
| D09 | A; procedure-enforced DML path | DBA + Architecture + Security · OWNER_UNCONFIRMED | All-writer map,locks/procs,2-node race,unknown outcome/failover | AWAITING_APPROVAL |
| D10 | A | Audit + Security + DBA + SRE · OWNER_UNCONFIRMED | Schema/retention/failure/key policy,archive integrity,epoch restore | AWAITING_APPROVAL |
| D11 | A | QA owner + Security + Architecture · OWNER_UNCONFIRMED | Reviewer authority,unchanged DR criteria,raw evidence review | AWAITING_APPROVAL |
| D12 | A; user scope đã VERIFIED,formal record pending | Project authority + Architecture/Security · OWNER_UNCONFIRMED | Consolidated named scope/repo/ref/exclusions/approval evidence | AWAITING_APPROVAL |
| E01 | A | OT + Business + Network/SRE · OWNER_UNCONFIRMED | IED/records/files/WAN/outage/retention distribution | AWAITING_APPROVAL |
| E02 | A ưu tiên có điều kiện | Architecture + DBA + Procurement · OWNER_UNCONFIRMED | Equivalent profile,edition/license/HA support/benchmark | AWAITING_APPROVAL |
| E03 | A hoặc B sau đánh giá operator; không blobs mặc định | Storage + DBA + SRE · OWNER_UNCONFIRMED | Cost/durability/immutability/checksum/reconciliation drills | AWAITING_APPROVAL |
| E04 | A | App + OT + QA + Security · OWNER_UNCONFIRMED | Signed protocol limits/binding/resume/ACK/replay tests | AWAITING_APPROVAL |
| E05 | A trước,broker khi evidence cần | SRE + Network + App · OWNER_UNCONFIRMED | Storm/drain/poison/quota/capacity acceptance | AWAITING_APPROVAL |
| E06 | A | Business + Audit + DBA + SRE · OWNER_UNCONFIRMED | Signed loss/retention budgets,data-bearing coordinated restore | AWAITING_APPROVAL |
| E07 | A | App + DBA + OT + Change owner · OWNER_UNCONFIRMED | Inventory/parity/nonempty upgrade/wave rollback evidence | AWAITING_APPROVAL |
| E08 | A | Sponsor + SRE + Procurement + OT · OWNER_UNCONFIRMED | BOM/license/on-call/support/training measured demand | AWAITING_APPROVAL |

Mỗi owner điền lựa chọn, lý do, residual risk, environment, deployment scope, repo/ref, version, artifact hash, signer, ngày ký, expiry/revocation, reviewer và mã kho bằng chứng riêng. Kiểm tra tất cả dependencies trước khi ký. Hoãn quyết định giữ gate liên quan ở trạng thái khóa. D12 authorization cho Dev/Lab vẫn được giữ tách biệt với formal record PENDING.
