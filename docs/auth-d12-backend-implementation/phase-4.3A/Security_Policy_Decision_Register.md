# Security policy subdecision register

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D01–D10 security subdecisions. Approval owner:Security owner cùng owner từng ADR. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

Subdecisions dưới đây triển khai chi tiết20 record formal, KHÔNG cộng thành số quyết định APPROVED mới. Tất cả PENDING. Không có selected/effective value. AUTH-R2, threat model và source primitives là căn cứ, không signed release.

| Subdecision | Chọn gì trước activation | Phụ thuộc | Bằng chứng còn thiếu |
|---|---|---|---|
| D01.TLS/PROXY/ROUTES | Hop/listener/Host/forwarding/cert trust; exact A01–A21 | D07, E04 | Deploy diagram, bypass/cert tests, route/status DTO freeze |
| D02.HASH/PASSWORD | Provider/cost/normalization/blocklist/max length | D05, D10 | Current-device cost/concurrency, algorithm assurance review |
| D02.THROTTLE | Shared source/account thresholds/window/lockout | D01, D09, D10 | NAT/source cohorts, enumeration/DoS tests |
| D03.RP/ORIGIN/UV | RP ID, origin, UV/device/sync/attestation policy | D01 | Exact host/cert/device list, library review |
| D03.TOTP/ASSURANCE | Exception/drift/replay/step-up age/credential revoke | D05, D06, D09 | Real factor replay/freshness proof |
| D05.SESSION | Timeout/cap/rotation/revoke/stream semantics | D09, D10 | Two-node session/race/unavailable tests |
| D05.KEYS | DP app identity/shared storage/ACL/encryption/rotation | D01, E06 | Node failover and key compromise/recovery drill |
| D06.CUSTODY/RECOVERY | Named persons, channels, one-use expiry, bootstrap/emergency | D03, D04, D09, D10 | Independent verified recovery andrevoke-all evidence |
| D04.CATALOG/DELEGATION | Explicit action granularity, same-assignment, target envelope | D08, D09 | Signed mapping/catalog, object scope/beneficiary tests |
| D04.SEED |7 role template IDs/codes/permissions, custody safety | D06 | Signed manifest diff, no account/grant auto seed |
| D07.MACHINE | Per-install identity/renewal/revocation/station binding | D01, E04 | Actual PKI lifecycle/TLS reuse tests |
| D08.OT | Collection envelope and mandatory config-write deny | D04, E04 | OT safety authorization; not activated here |
| D09.GATES/PRINCIPALS | Namespace/order/isolation/timeouts/all writers/direct DML | DB architecture |2-node real product races, procedure/principal tests |
| D10.EVENT/ARCHIVE/RESTORE | Expanded schema, atomic failures, retention/keys/epoch | D05, D09, E03, E06 | Independent archive tamper andcó dữ liệu restore |

Các candidate numbers trong auth ADR chỉ giúp owner so sánh, không tự áp giá trị. Named owner, approval date, signature/evidence, version/hash đều NOT_PROVIDED. D07/D08 formal options ở Architecture Register; production safety/PKI acceptance riêng, không do chốt auth timeout mà được vượt qua.

## Phiếu quyết định cần owner hoàn tất

Option được chọn:CHƯA CÓ. Named accountable approver/delegation authority:CHƯA CÓ. Security/OT/DBA concurrence và independent reviewer:CHƯA CÓ. Approval date/evidence URI/chữ ký:NOT_PROVIDED. Owner phải ghi exact option, document+catalog+route+policy/schema version/hash, phạm vi Lab/production riêng, rationale, trade-offs, residual risk/expiry/review triggers và prerequisite dispositions. Chỉ reviewer kiểm bằng chứng ký thực tế mới cập nhật register; không dùng checkbox/điền tên thay approval.
