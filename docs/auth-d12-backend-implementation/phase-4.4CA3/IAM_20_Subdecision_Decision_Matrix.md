# Ma trận 20 quyết định

Trích trực tiếp [Decision_Records.json](../phase-4.3A/Decision_Records.json), thứ tự gốc và trạng thái gốc. Có 20 bản ghi formal D01–D12/E01–E08, không có trường parent. Mục mở rộng có dấu chấm trong security register là chi tiết tham khảo, không phải ID từ JSON. Cột chuẩn bị của phase này là RECOMMENDATION_READY / AWAITING_APPROVAL. Không có selected option, tên người ký hoặc approval evidence hợp lệ trong nguồn.

| ID | Parent | Title / description theo nguồn | Current / approval | Options | Recommendation | Owner theo nguồn |
|---|---|---|---|---|---|---|
| D01 | — | Host/listener/proxy/route ownership | PENDING / PENDING | A:listener riêng cùng process; B:Human/Machine service riêng; C:legacy mixed | B production; A chỉ lab có guard | Architecture + Security + OT + App owner |
| D02 | — | Hasher/password/throttle policy | PENDING / PENDING | A:Argon2id reviewed; B:IdentityV3 PBKDF2 calibrated; C:framework defaults | B giai đoạn đầu,owner xác nhận assurance; A hợp lệ | Security + IAM operations |
| D03 | — | MFA/freshness/enrollment | PENDING / PENDING | A:device-bound WebAuthn UV; B:governed TOTP; C:no factor | A privileged; B exception có hạn | Security + Business + PKI/support |
| D04 | — | Dynamic RBAC/action/scope/governance | PENDING / PENDING | A:25 codes+operation delegation dimension; B:explicit catalog extension; C:manage=all | B admin granularity,25 baseline giữ nguyên tới release | Business + Security + Architecture |
| D05 | — | Session registry/rotation/revocation/DP | PENDING / PENDING | A:opaque SQL registry; B:protected cookie+registry; C:long JWT TTL | A | Security + Architecture + SRE |
| D06 | — | Bootstrap/recovery/disable/emergency | PENDING / PENDING | A:independent verified recovery; B:email-only; C:default admin | A; không bypass | Security + Business + IAM custodians |
| D07 | — | Collector identity/PKI lifecycle | PENDING / PENDING | A:per-installation cert/registry; B:shared cert; C:legacy key lâu dài | A | PKI + OT + Security |
| D08 | — | OT operations/workflow boundary | PENDING / PENDING | A:deny config write,controlled acquisition; B:login enables writes; C:broader engineering workflow | A | OT + Protection engineering + Security |
| D09 | — | SQL transaction authority/database model | PENDING / PENDING | A:single DB+common gate+controlled writers; B:separate DB+distributed consistency; C:snapshot only | A; procedure-enforced DML path | DBA + Architecture + Security |
| D10 | — | Audit/archive/restore/custody | PENDING / PENDING | A:atomic outbox+independent immutable archive; B:DB-only; C:best-effort logs | A | Audit + Security + DBA + SRE |
| D11 | — | Independent review/acceptance scope | PENDING / PENDING | A:named independent QA+Security; B:author self-accept; C:UI PASS=IAM pass | A | QA owner + Security + Architecture |
| D12 | — | Implementation authorization/release scope | PENDING / PENDING | A:scoped Lab authorization+explicit gates; B:blanket activation; C:ignore prior authorization | A; user scope đã VERIFIED,formal record pending | Project authority + Architecture/Security |
| E01 | — | 380 station workload/WAN/offline | PENDING / PENDING | A:measure representative cohorts; B:assume workbook | A | OT + Business + Network/SRE |
| E02 | — | SQL provider/edition/HA | PENDING / PENDING | A:SQL Server; B:PostgreSQL; C:SQLite Center | A ưu tiên có điều kiện | Architecture + DBA + Procurement |
| E03 | — | COMTRADE file durability | PENDING / PENDING | A:private object HA; B:private NAS; C:SQL blobs | A hoặc B sau đánh giá operator; không blobs mặc định | Storage + DBA + SRE |
| E04 | — | Transfer protocol/idempotency/chunks | PENDING / PENDING | A:v1 parity rồi versioned v2; B:DB+protocol big bang | A | App + OT + QA + Security |
| E05 | — | Queue/backpressure/fairness | PENDING / PENDING | A:SQL outbox bounded workers; B:broker; C:unbounded ingest | A trước,broker khi evidence cần | SRE + Network + App |
| E06 | — | RPO/RTO/retention/hold/restore | PENDING / PENDING | A:per-layer loss budgets+coordinated recovery; B:assume RPO0 | A | Business + Audit + DBA + SRE |
| E07 | — | Cutover/rollback/waves | PENDING / PENDING | A:expand-contract/parity/waves; B:big bang | A | App + DBA + OT + Change owner |
| E08 | — | Support/procurement/operations | PENDING / PENDING | A:costed model sau measurement; B:fixed380 BOM | A | Sponsor + SRE + Procurement + OT |

## D01 · Host/listener/proxy/route ownership

- Tác động bảo mật và lý do ưu tiên theo nguồn. Giảm blast radius/TLS identity confusion; thêm process/PKI vận hành
- Tác động triển khai/vận hành theo nguồn. Giữ loopback lab hiện có; freeze A01–A21 rồi tách entrypoints
- Phụ thuộc đề xuất. D07, E04. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. Architecture + Security + OT + App owner. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Topology/route release,proxy bypass+cert negative tests.
- Tiêu chí nghiệm thu đề xuất. Listener isolation, proxy header spoofing, backend bypass và client certificate negative paths đều đạt.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[0]`, version 0.1.

## D02 · Hasher/password/throttle policy

- Tác động bảo mật và lý do ưu tiên theo nguồn. Tái dùng primitive hiện có; PBKDF2 không memory-hard; không lấy test cost làm policy
- Tác động triển khai/vận hành theo nguồn. SQL credential/version/shared limiter và proof-completion còn thiếu
- Phụ thuộc đề xuất. D05, D09, D10. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. Security + IAM operations. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Cost/concurrency measurements,blocklist/normalization/limits release.
- Tiêu chí nghiệm thu đề xuất. Đo cost theo thiết bị, thử unknown-user timing, lockout/throttle đa instance và DoS.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[1]`, version 0.1.

## D03 · MFA/freshness/enrollment

- Tác động bảo mật và lý do ưu tiên theo nguồn. Phishing resistance; thiết bị,custody và recovery tăng chi phí
- Tác động triển khai/vận hành theo nguồn. Real factor verifier+credential metadata migration chưa có
- Phụ thuộc đề xuất. D01, D05, D06, D09. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. Security + Business + PKI/support. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. RP/origin/UV/devices,library review,replay/enrollment evidence.
- Tiêu chí nghiệm thu đề xuất. Chứng minh RP/origin/UV, challenge single-use, replay và step-up theo thiết bị được duyệt.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[2]`, version 0.1.

## D04 · Dynamic RBAC/action/scope/governance

- Tác động bảo mật và lý do ưu tiên theo nguồn. Tách request/review/apply; thêm release/negative-test work
- Tác động triển khai/vận hành theo nguồn. Permission rows hỗ trợ code mới nhưng không tự seed/activate; providers chưa có
- Phụ thuộc đề xuất. D06, D08, D09. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. Business + Security + Architecture. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Signed catalog diff,operation map,scope/SoD/seed/custody.
- Tiêu chí nghiệm thu đề xuất. Mapping ký đúng hash; chạy same-assignment, cross-station, mandatory deny và SoD.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[3]`, version 0.1.

## D05 · Session registry/rotation/revocation/DP

- Tác động bảo mật và lý do ưu tiên theo nguồn. Revocation qua primary mỗi request; SQL availability dependency
- Tác động triển khai/vận hành theo nguồn. Consumer có; issuer/rotation/self APIs/shared antiforgery keys thiếu
- Phụ thuộc đề xuất. D01, D09, D10, E06. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. Security + Architecture + SRE. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Session limits/rotation/keys/HA/SQL-outage acceptance.
- Tiêu chí nghiệm thu đề xuất. Hai node kiểm thử rotation, idle/absolute expiry, revoke, key failover và SQL outage fail-closed.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[4]`, version 0.1.

## D06 · Bootstrap/recovery/disable/emergency

- Tác động bảo mật và lý do ưu tiên theo nguồn. Chống tự nâng quyền/recovery takeover; cần custody/support trực ca
- Tác động triển khai/vận hành theo nguồn. One-use proof consume/enrollment/revoke-all và audit thiếu
- Phụ thuộc đề xuất. D03, D04, D09, D10. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. Security + Business + IAM custodians. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Named independent custodians,proof lifecycle,loss/incident rehearsal.
- Tiêu chí nghiệm thu đề xuất. Recovery exercise có hai người độc lập; token một lần, hết hạn và revoke sessions.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[5]`, version 0.1.

## D07 · Collector identity/PKI lifecycle

- Tác động bảo mật và lý do ưu tiên theo nguồn. Cô lập compromise từng Collector; cần lifecycle380+ cert
- Tác động triển khai/vận hành theo nguồn. BindingStatus không phải proof; default machine authority pending
- Phụ thuộc đề xuất. D01, E04. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. PKI + OT + Security. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. CA/EKU/revocation/station binding/renewal/reuse tests.
- Tiêu chí nghiệm thu đề xuất. Đăng ký mỗi máy, bind trạm, rollover/revoke và deny sai certificate trên hai tuyến.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[6]`, version 0.1.

## D08 · OT operations/workflow boundary

- Tác động bảo mật và lý do ưu tiên theo nguồn. Giữ mandatory OT deny; không chặn thu thập hợp lệ sau approve
- Tác động triển khai/vận hành theo nguồn. Không thay IED/SCADA/firewall; command dispatch vẫn cần D04
- Phụ thuộc đề xuất. D04, D07, E04. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. OT + Protection engineering + Security. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Collection envelope/change workflow,safety authorization riêng.
- Tiêu chí nghiệm thu đề xuất. Đường OT allowlist và kiểm thử mandatory deny; phiếu thử riêng cho thao tác read-only.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[7]`, version 0.1.

## D09 · SQL transaction authority/database model

- Tác động bảo mật và lý do ưu tiên theo nguồn. Có serialization; global gate contention/direct-DML bypass phải giải quyết
- Tác động triển khai/vận hành theo nguồn. Candidate+tests không đủ; writer/provider/principal refactor còn thiếu
- Phụ thuộc đề xuất. E02, D04, D10. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. DBA + Architecture + Security. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. All-writer map,locks/procs,2-node race,unknown outcome/failover.
- Tiêu chí nghiệm thu đề xuất. Chứng minh mọi writer tham gia lock order, race apply/revoke, unknown commit và failover.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[8]`, version 0.1.

## D10 · Audit/archive/restore/custody

- Tác động bảo mật và lý do ưu tiên theo nguồn. Chống mất/sửa audit; archive/key separation có chi phí
- Tác động triển khai/vận hành theo nguồn. Schema event thiếu target/reason/versions; dispatcher/restore acceptance chưa có
- Phụ thuộc đề xuất. D05, D09, E03, E06. Rủi ro nếu kích hoạt thiếu bằng chứng. Critical.
- Required approver. Audit + Security + DBA + SRE. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Schema/retention/failure/key policy,archive integrity,epoch restore.
- Tiêu chí nghiệm thu đề xuất. Audit/outbox atomic, kiểm tra tamper và restore với epoch; kiểm quyền truy cập archive.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[9]`, version 0.1.

## D11 · Independent review/acceptance scope

- Tác động bảo mật và lý do ưu tiên theo nguồn. Tách implementation và acceptance; cần reviewer thật
- Tác động triển khai/vận hành theo nguồn. Chưa có independent sign-off/full DR run
- Phụ thuộc đề xuất. D01–D10, D12. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. QA owner + Security + Architecture. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Reviewer authority,unchanged DR criteria,raw evidence review.
- Tiêu chí nghiệm thu đề xuất. Reviewer độc lập tái lập kết quả theo tiêu chí DR và xác nhận bằng chứng gốc.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[10]`, version 0.1.

## D12 · Implementation authorization/release scope

- Tác động bảo mật và lý do ưu tiên theo nguồn. Cho phép work độc lập; không auto-approve D01–D11
- Tác động triển khai/vận hành theo nguồn. 4.3A chỉ docs/read-only;4.3B coding theo scope đã có khi được yêu cầu
- Phụ thuộc đề xuất. D11; scope theo từng release. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. Project authority + Architecture/Security. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Consolidated named scope/repo/ref/exclusions/approval evidence.
- Tiêu chí nghiệm thu đề xuất. Biên bản ký đúng branch, SHA, Lab, phạm vi và exclusions. Tách Dev/Lab authorization hiện có.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[11]`, version 0.1.

## E01 · 380 station workload/WAN/offline

- Tác động bảo mật và lý do ưu tiên theo nguồn. Tránh under/over sizing; cần field samples
- Tác động triển khai/vận hành theo nguồn. Local5000-row query không chứng minh fleet
- Phụ thuộc đề xuất. E03, E04, E05, E08. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. OT + Business + Network/SRE. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. IED/records/files/WAN/outage/retention distribution.
- Tiêu chí nghiệm thu đề xuất. Phân phối tải và outage được owner xác nhận; đủ dữ liệu hiệu chỉnh sizing.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[12]`, version 0.1.

## E02 · SQL provider/edition/HA

- Tác động bảo mật và lý do ưu tiên theo nguồn. Phù hợp code/DBA; license/HA/TCO cần chứng cứ
- Tác động triển khai/vận hành theo nguồn. SQL2022 Lab có; production edition/HA chưa chọn
- Phụ thuộc đề xuất. E01, E06, D09. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. Architecture + DBA + Procurement. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Equivalent profile,edition/license/HA support/benchmark.
- Tiêu chí nghiệm thu đề xuất. Edition/HA benchmark, failover, cost và restore theo mục tiêu đã duyệt.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[13]`, version 0.1.

## E03 · COMTRADE file durability

- Tác động bảo mật và lý do ưu tiên theo nguồn. Metadata/file consistency và recovery boundary
- Tác động triển khai/vận hành theo nguồn. Local verifier không là durable immutable publication
- Phụ thuộc đề xuất. E01, E04, E06. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. Storage + DBA + SRE. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Cost/durability/immutability/checksum/reconciliation drills.
- Tiêu chí nghiệm thu đề xuất. Checksum, reconciliation sau gián đoạn và kiểm thử durability/immutability.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[14]`, version 0.1.

## E04 · Transfer protocol/idempotency/chunks

- Tác động bảo mật và lý do ưu tiên theo nguồn. Giảm compatibility risk; hai rollout cần kiểm soát
- Tác động triển khai/vận hành theo nguồn. Candidate metadata/SOE adapters không là machine API
- Phụ thuộc đề xuất. D01, D07, E01, E03. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. App + OT + QA + Security. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Signed protocol limits/binding/resume/ACK/replay tests.
- Tiêu chí nghiệm thu đề xuất. Manifest/ACK idempotent, chunk resume, replay denial và station scope.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[15]`, version 0.1.

## E05 · Queue/backpressure/fairness

- Tác động bảo mật và lý do ưu tiên theo nguồn. Ít components nhưng SQL contention; quotas không được đoán
- Tác động triển khai/vận hành theo nguồn. Outbox schema có;leased workers/quota/fairness chưa triển khai
- Phụ thuộc đề xuất. E01, E04. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. SRE + Network + App. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Storm/drain/poison/quota/capacity acceptance.
- Tiêu chí nghiệm thu đề xuất. Đo reconnect storm, fairness, poison và drain với 380 nguồn mô phỏng.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[16]`, version 0.1.

## E06 · RPO/RTO/retention/hold/restore

- Tác động bảo mật và lý do ưu tiên theo nguồn. Đòi hỏi storage/IAM/audit consistency; thêm drill cost
- Tác động triển khai/vận hành theo nguồn. Empty-schema restore đã test; data+file+epoch chưa test
- Phụ thuộc đề xuất. E02, E03, D10. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. Business + Audit + DBA + SRE. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Signed loss/retention budgets,data-bearing coordinated restore.
- Tiêu chí nghiệm thu đề xuất. Restore phối hợp SQL/file/IAM trên dữ liệu mẫu và đo RPO/RTO.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[17]`, version 0.1.

## E07 · Cutover/rollback/waves

- Tác động bảo mật và lý do ưu tiên theo nguồn. Hạn chế rollback stale authority; nhiều release gates
- Tác động triển khai/vận hành theo nguồn. Không cutover operating Center/Collector trong4.3A
- Phụ thuộc đề xuất. D11, E02, E06. Rủi ro nếu kích hoạt thiếu bằng chứng. High.
- Required approver. App + DBA + OT + Change owner. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. Inventory/parity/nonempty upgrade/wave rollback evidence.
- Tiêu chí nghiệm thu đề xuất. Diễn tập cutover và rollback mỗi wave, kiểm tra parity dữ liệu.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[18]`, version 0.1.

## E08 · Support/procurement/operations

- Tác động bảo mật và lý do ưu tiên theo nguồn. Tránh SLA/cost giả; cần owner/support coverage
- Tác động triển khai/vận hành theo nguồn. Chưa chốt hardware/SLA từ local query
- Phụ thuộc đề xuất. E01, E02, E06, E07. Rủi ro nếu kích hoạt thiếu bằng chứng. Medium.
- Required approver. Sponsor + SRE + Procurement + OT. Named approver **OWNER_UNCONFIRMED**.
- Bằng chứng cần nộp. BOM/license/on-call/support/training measured demand.
- Tiêu chí nghiệm thu đề xuất. BOM và on-call được phê duyệt theo tải đo, kiểm chứng training/support.
- Runtime activation impact. Giữ gate liên quan khóa cho đến khi owner xác nhận release và phạm vi. Approval D12 Dev/Lab không thay được các gate khác.
- Source. Decision_Records.json, `records[19]`, version 0.1.
