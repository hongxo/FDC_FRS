# Phase4.3A — Baseline inventory

Ngày rà soát:2026-11-09 (Asia/Bangkok). Package version0.1 PROPOSED. Baseline kết hợp PR6:d5ebc2acc18265786bae5af96ef3ba58c223e522; Phase4.2:9cd35889ddbbcc8d88e831419284877f4add5c6a. Source runtime/database: a0a6096; không sửa code trong 4.3A.
CURRENT là trạng thái source/DB; VERIFIED chỉ phạm vi quan sát/evidence được nêu; PROPOSED không thực thi. Approval date/evidence/người ký:NOT_PROVIDED. Independent reviewer:NOT_PROVIDED. Không tự đổi P0-IAM/readiness.
Nguồn đối chiếu và hash: [Baseline](Phase4_3A_Baseline_Inventory.md), [Evidence](Evidence_Index.md), Input_Manifest.json. Khuyến nghị cần được owner chọn, bind version/hash và phê duyệt; không phải configuration hoặc quyền đã có hiệu lực.

Decision ID:BASELINE/D01–D12. Approval owner:Architecture + App + DBA + Security. Current approval status:PENDING. Recommended/alternative options và tác động bên dưới.

## Verified baseline

Officialorigin https://github.com/hongxo/FDC_FRS.git; start workingtree clean. PR3/4/5/6 OPEN, DRAFT verified bygh; headrefs respectively415cca4, 648c031, 9cd3588, d5ebc2a. SelectedPR6tipd5ebc2a toincludePhase4.2+database source. Newbranch feature/d12-phase4-3a-decisions, no merge/reset/forcepush. OriginalrootnotGitcheckout; authorized separateofficialclone used. Source/docs underoriginalworkspace areexternal inputs, not assumed availableonGitHub.

Mandatoryreports read:Phase4_Final_Report/Defect_Register/DR01_DR29_Execution_Matrix, Phase4_1_Final_Assessment, Phase4_2_Final_Assessment, Database_Final_Assessment/Table_Dictionary plus AUTH-R2workbook/approvalpackage, D01–D12 authorizationhistory, canonicalA01–A21, Dynamic_RBAC_Architecture/Schema/API/Catalog/StationGrant/SoD/implementation andpendingseed. Input_Manifest hashesreadsource/artifacts; workbook19records allPENDING_OWNER_DECISION. D12 formalrecordseparate; prioruser-scopedLab authorization isrecognized, notrevoked byoldNOT_AUTHORIZED snapshots.

## Current lab state(read-only, thisphase)

| Target | App tables/rows | FK/CHECK/index | Migrationhistory | Authority/Userrows | Schema owner |
|---|---|---|---|---|---|
| FdcIam_DevelopmentLab |24/0 |33/93/69 |001 |0/0 |dbo foriam/catalog/ops/audit |
| FDC_Center_Lab |53/0 |68/211/159 |001–006 |0/0 |dbo plusingest |

SQL2022 16.0.1210.3; MachineName XOTH-ETC-2, Lab labelsconfirmed. Currentread-only connectionusedexistinglocallabcredentialtransiently, no passwordwritten/printed. NoDDL/DML/permission/connection/configchange; no merge/copy ofDBs. LabSQLruntime hasmemberinnew DBrole, notnewhumanaccount.

Historicalexecution evidence:latestCenter19PASS, IAM184componentPASS and21SQLPASS; gatedSKIPnotPASS; realempty restore/schema/replay/currentprofiletestsbounded.4.3A doesNOT rerunthoseworkloads orclaimfull DR/MFA/fleet/HApass. Full29DRbaseline remainsBLOCKED, no independentacceptance. R2/enterprise designapprovalfields stillunfilled; recommendations are notsignatures.

## Reconciliation findings

| Finding | Conflict/gap | Evidence | Dispositionin4.3A |
|---|---|---|---|
| F43A-F01 | Database_Final_Assessment says67FK; current metadata/export says68 | Read-only2026-11-09 count68; 006 addsStationFK | Use68forCURRENT; historicaldocnotedited, no new runtime defect inferred |
| F43A-F02 | Sourcecomments RoleEndpoints/SessionRevocation saynohostregistration | CenterIdentityRuntime explicitlyregisters/mapsnow | Startup/DI wins over stale comments; doc/sourcecomment cleanup future, no4.3Asourceedit |
| F43A-F03 | API /login versus/auth/login; status EFFECTIVE versusApplied | A01–A21 proposal andcurrent UI/source | Propose root/login +explicitPascalCase DTO release; ownerfreeze, no aliases |
| F43A-F04 | No explicitforwardedmiddleware not sufficient deploymentproof | Microsoftguidance +sourcehost/environmentnotliveaudited | Reviewimplicithostingflags/trustedproxy/bypass inD01; notcurrentexploittested |
| F43A-F05 | ProxyhopmTLS versusHumanhandlerrejectinganycertificate | HumanSessionHandler andHumanRequestSecurity | Distinguishverifiedproxytransportidentity beforeadoptingtopology; no bluntcheckremoval |
| F43A-F06 | Singlemanagecode lacksoperation-stage perdelegationdimension | AdministrativeDelegationRow.PermissionId +OperationRule | Proposeexplicitadmincatalogextension; allnewcodesnotcatalogued/no seed |
| F43A-F07 | SQL snapshotreaderownstransaction; gateplanactor-only | SqlCurrentAuthorityReader/ISqlCommitGatePlanProvider | Transaction-awareauthority+trusted CommitIntent required, notregistrationalone |
| F43A-F08 | Emptyrestore/minimalruntimeprivileges versusfullD10/D09 readiness | Databaseevidence andsource/principalroles | Keepcó dữ liệurestore/archives/controlledeffectivewriters pending |

F43A-F09:Role apply hiện bump global PolicyVersion vàSessionValidity so khớp toàn bộ version. Tác động buộc reauthentication cần policy/UX/performance decision,không âm thầm thêm refresh bypass. Collector source local API key/maintenance plane vẫn làlegacy riêng;không triển khai human IAM offline hoặc thay Collector trong4.3A.
