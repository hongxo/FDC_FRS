# Database architecture alignment ADR

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D09; E02/E07; D05/D10. Approval owner:Architecture + DBA + Security + App owner. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

## CURRENT đối chiếu hai DB

Read-only inventory2026-11-09:old IAM24 tables/0 app rows/33FK/93checks/69indexes/migration001; new Center53 tables/0 app rows/68FK/211checks/159indexes/3views/migrations001–006. Cả haiAuthorityVersion0/User0; chưa configured human authority. IAM/catalog/ops/audit schemas owner thực tếdbo; ingest chỉ new DB. Owner logic IAM làSecurity+App, database release DBA; schema ownerdbo không làquyềnruntime db_owner. New runtime SQL role có1 member, old DB0 cho role đó. Không đổiconnection hoặcquyền trong 4.3A.

## Options và recommendation

| Option | Consistency/security | Operations/compatibility | Recommendation |
|---|---|---|---|
| A:single Center DB, nhiều schema | Có thểsame-transaction authority/effect/outbox vàcommon DB-scoped gates | Backup/restore/capacity chung; least privilege/controlled writers bắtbuộc | PROPOSED canonical deployment; new FDC_Center_Lab là targetLab đã có, không production approval |
| B:separate IAM DB | Tách admin/capacity nhưngrevoke/effect boundary phức tạp | Cùnginstance/cùngconnection có thểcross-DB tx, không tựđòiDTC; candidategates/DbContexts/backup/HA chưa hỗ trợ mặc định | Alternative nếu owner cólýdo isolation vàfund protocol redesign |
| C:hybrid external IAM authority/cache | Service separation; cache/lease dễintroduce staleallow | Cầnproved commit fencing/distributed recovery, không current code | Không dùng allow-cache làmnguồn authority; archive độc lập vẫn phùhợpA |

Chọn A cho 4.3B làrecommendation chờowner, sau user-authorized Lab implementation. Không tự hợp nhất/chuyển old data. Storage/immutable archive có thểkhácfaultdomain, trongkhieffective IAM/data vẫnsingleSQL. Collector SQLite/file queue không cóhuman IAM mặc định.

## Canonical source of truth vàconnection

Source migration001 ởFdc.Identity/Infrastructure/Migrations, 002–006 ởFdc.Center.Persistence/Migrations, shared dbo.__EFMigrationsHistory. IamDbContext có24 mappings; CenterDbContext29new +thinreferences catalog/audit exclude migrations, khôngduplicate tables. Mộtconfigured runtime connection trongopt-inprofile cho cả2contexts; current host guardhardcodes local target/non-sa/non-owner, production endpoint/key/edition chưa đượcchọn.

D09 same transaction phảiexplicit cùngSqlConnection/DbTransaction khi2contexts thamgia; migrationhistorychung hoặcsame connectionstring không tựtạo unit-of-work. Current snapshot reader own transaction cầnadaptercommit-aware, khôngnested newfactory read. AuthorityVersion/Session/User/SecurityVersion/GrantVersion, Role/Grant/Delegate trongiam/catalog; security events audit.SecurityEvent; outbox ops.AuditOutbox, operational queue ops.ProcessingOutbox làkhác purpose. Không renameto parallel security schema.

## Cutover/data/rollback decision

1. Chốt single canonical authority target và signed policy/catalog/route/seed releases. Labbootstrap mới cần ceremonyD06; khôngcopy user/hash/MFAseed/session/effective grants từ old DB đểtạo quyền.
2. Snapshot manifests/schema/source hashes, stable string→GUID catalog bindings, nonempty migration/parity plan, quiesce writers. Khôngđổi operatingLegacy Center/Collector provider trongdocumentation phase.
3. New runtime node cùng authority/key domain; disable oldHuman writer routes trướcswitch; khôngtwo active authorities hoặcclient fallbacktoold DB. OldDB vẫnisolated test target, no effectiveauthoritydata hiện tại.
4. Restore/cutover epoch/reconciliation D05/D09/D10 trước mở listener. Roll-forward ưu tiên; rollback app khôngrestore oldeffective permissions hoặcpointback to stale authority. Phảireview RPO/file/audit consistency và SQLHA/edition.

Production SQLTLS/secretvault/principals vàHA listener cần owner lựa chọn; localDPAPI credential helper khôngproduction secret/key infrastructure. No source/config/migration/DDL/DML hoặcoperatorpermissions changed in4.3A.

## Phiếu quyết định cần owner hoàn tất

Option được chọn:CHƯA CÓ. Named accountable approver/delegation authority:CHƯA CÓ. Security/OT/DBA concurrence và independent reviewer:CHƯA CÓ. Approval date/evidence URI/chữ ký:NOT_PROVIDED. Owner phải ghi exact option, document+catalog+route+policy/schema version/hash, phạm vi Lab/production riêng, rationale, trade-offs, residual risk/expiry/review triggers và prerequisite dispositions. Chỉ reviewer kiểm bằng chứng ký thực tế mới cập nhật register; không dùng checkbox/điền tên thay approval.
