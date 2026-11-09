# Database compatibility matrix

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:D09; E02/E07. Approval owner:DBA + Architecture + Security. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

CURRENT comparisons fromread-only2026-11-09 andsource/history. VERIFIED历史tests chỉphạm vi ghi ởreports, không rerun/testPASS mới trong 4.3A. Status ofsinglecentral productiondecision remainsPENDING.

| Item | FdcIam_DevelopmentLab | FDC_Center_Lab | Compatibility/action required |
|---|---|---|---|
| Role/User/Permission/Session | Canonical001 hiện có, 0data | Same001, 0data | Reuse exactnames; no copy proof/credential/effectivegrant |
| Station/ManagementUnit/Scope | catalog canonical from001 | Samecatalog+independent profiles/devicehistory | StableGUID/keymapping+hierarchyversionreview; no duplicateStation |
| Authority storage | iam.AuthorityVersion0rows | iam.AuthorityVersion0rows | Neither hasconfiguredAllowauthority; bootstrap/release required |
| RestoreEpoch | Field exists; no active epoch row | Samefield; no active epoch row | Trustednew epoch afterrestore; neverinferfromdatabasecreation |
| Migrations | 001 only | 001+002–006; sharedhistory | ImmutableSQLresourcebytes; nextadditiveversiononlyafterreview |
| DbContext | IamDbContext | IamDbContext+CenterDbContext | SameDBstring notautomaticsharedtransaction; explicitD09adapter |
| IAM schema owner | dbo | dbo | LogicalSecurity/DBA releaseowner; runtime notschemaowner |
| Session/authority versions | SQLconsumer/componenttests | Sameconsumer inactualCenterprofile | Realissuance/factor/rotationstillmissing |
| Audit/security | audit.SecurityEvent+ops.AuditOutbox | Same+AuditDelivery/Checkpoint metadata | No duplicateaudit; fullD10 schema/archive pending |
| Operational data | No Centerdomain tables | catalog/ingest/ops domain metadata+SOE | SQLmetadataadaptersnotoperatingSQLitecutover |
| File storage | No coordinated objectstore | Relativekeys/hash/verifiedpackage source snapshot | No waveform blobs; immutablepublication/fileDR pending |
| Currentruntimeconnection | LegacyIAMtests explicitly oldtarget | Opt-inCenterprofile same newtargetbothcontexts | No4.3Achange; productionconnectionnotselected |
| SQLprincipals | Historicaltestoperator; catalogreadercontract | Dedicatedminimalruntime, otherrolecontracts scaffolds | Nosa/db_owner runtime verifiedhistorically; privilegedmigratornotaccepted |
| Backup/restore | Oldfixture rollbacktests | Actualempty53-table disposable restore+DDLprobe | No có dữ liệuepoch/archive/fileproof; no sharedDBrestore |
| Performance/HA | Localnegativequery/protocol evidence | Synthetic5000metadata profile, oneSQLinstance | No380fleetSLA/AGfailover; notlicense/hardwareapproval |
| Collector | SQLite/source unchanged | SQLite/source unchanged | No edgeDB/humanIAM migrationauthorized here |

Stale67FK inoldDatabase_Final_Assessment isdocumentation discrepancy F43A-F01; actualmetadata/currentdictionary68FK isauthoritative forinventory. Không sửa lịch sửhoặcđổi readiness từchênh lệchdocument này.

## Phiếu quyết định cần owner hoàn tất

Option được chọn:CHƯA CÓ. Named accountable approver/delegation authority:CHƯA CÓ. Security/OT/DBA concurrence và independent reviewer:CHƯA CÓ. Approval date/evidence URI/chữ ký:NOT_PROVIDED. Owner phải ghi exact option, document+catalog+route+policy/schema version/hash, phạm vi Lab/production riêng, rationale, trade-offs, residual risk/expiry/review triggers và prerequisite dispositions. Chỉ reviewer kiểm bằng chứng ký thực tế mới cập nhật register; không dùng checkbox/điền tên thay approval.

Compatibility bổ sung cần freeze:legacy Core.RecordRow.Id dùng manifest identity chuỗi SHA256;DisturbanceRecord.Id ởSQL làGUID nội bộ vàcó ManifestId riêng. Trusted object resolver/API adapter phải chọn định danh hợp lệ vàstation binding rõ ràng,không đổi định dạng hoặc tự cast gâylookup ngoài phạm vi. Role/detail DTO hiện trả Version,nhưng mutation service cần8-byte RowVersion;ETag/If-Match wire format vàstatus casing cần nằm trong signed API release,tránh coi một số Version làrowversion proof.
