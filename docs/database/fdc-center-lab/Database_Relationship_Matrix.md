# Deployed relationship matrix

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

Actual68 FKs; all delete actions NO_ACTION. Nullable participation and detailed properties are in dictionary/inventory. No cascade delete of incidents/security/approval history.

| Child | FK | Columns | Principal | Delete | Trusted |
|---|---|---|---|---|---|
| audit.AuditDelivery | FK_AuditDelivery_SecurityEvent_EventId | EventId→Id | audit.SecurityEvent | NO_ACTION | True |
| audit.SecurityEvent | FK_SecurityEvent_User_ActorUserId | ActorUserId→Id | iam.User | NO_ACTION | True |
| catalog.Bay | FK_Bay_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| catalog.DeviceFirmware | FK_DeviceFirmware_DeviceModel_DeviceModelId | DeviceModelId→Id | catalog.DeviceModel | NO_ACTION | True |
| catalog.DeviceModel | FK_DeviceModel_Manufacturer_ManufacturerId | ManufacturerId→Id | catalog.Manufacturer | NO_ACTION | True |
| catalog.Feeder | FK_Feeder_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| catalog.IedDevice | FK_IedDevice_Bay_BayId_StationId | BayId→Id, StationId→StationId | catalog.Bay | NO_ACTION | True |
| catalog.IedDevice | FK_IedDevice_CommunicationProfile_CommunicationProfileId | CommunicationProfileId→Id | catalog.CommunicationProfile | NO_ACTION | True |
| catalog.IedDevice | FK_IedDevice_DeviceFirmware_FirmwareId_DeviceModelId | FirmwareId→Id, DeviceModelId→DeviceModelId | catalog.DeviceFirmware | NO_ACTION | True |
| catalog.IedDevice | FK_IedDevice_DeviceModel_DeviceModelId | DeviceModelId→Id | catalog.DeviceModel | NO_ACTION | True |
| catalog.IedDevice | FK_IedDevice_Feeder_FeederId_StationId | FeederId→Id, StationId→StationId | catalog.Feeder | NO_ACTION | True |
| catalog.IedDevice | FK_IedDevice_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| catalog.ManagementUnit | FK_ManagementUnit_Corporation_CorporationId | CorporationId→Id | catalog.Corporation | NO_ACTION | True |
| catalog.ManagementUnit | FK_ManagementUnit_ManagementUnit_ParentUnitId | ParentUnitId→Id | catalog.ManagementUnit | NO_ACTION | True |
| catalog.Scope | FK_Scope_Corporation_CorporationId | CorporationId→Id | catalog.Corporation | NO_ACTION | True |
| catalog.Scope | FK_Scope_ManagementUnit_ManagementUnitId | ManagementUnitId→Id | catalog.ManagementUnit | NO_ACTION | True |
| catalog.Scope | FK_Scope_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| catalog.Station | FK_Station_ManagementUnit_ManagementUnitId | ManagementUnitId→Id | catalog.ManagementUnit | NO_ACTION | True |
| catalog.StationProfile | FK_StationProfile_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| catalog.StationProfile | FK_StationProfile_VoltageLevel_VoltageLevelId | VoltageLevelId→Id | catalog.VoltageLevel | NO_ACTION | True |
| catalog.StationUnitHistory | FK_StationUnitHistory_ManagementUnit_ManagementUnitId | ManagementUnitId→Id | catalog.ManagementUnit | NO_ACTION | True |
| catalog.StationUnitHistory | FK_StationUnitHistory_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| iam.AdministrativeDelegation | FK_AdministrativeDelegation_Permission_PermissionId | PermissionId→Id | iam.Permission | NO_ACTION | True |
| iam.AdministrativeDelegation | FK_AdministrativeDelegation_UserRole_UserRoleId | UserRoleId→Id | iam.UserRole | NO_ACTION | True |
| iam.Challenge | FK_Challenge_User_UserId | UserId→Id | iam.User | NO_ACTION | True |
| iam.GrantFunction | FK_GrantFunction_Permission_PermissionId | PermissionId→Id | iam.Permission | NO_ACTION | True |
| iam.GrantFunction | FK_GrantFunction_StationGrant_StationGrantId | StationGrantId→Id | iam.StationGrant | NO_ACTION | True |
| iam.MfaCredential | FK_MfaCredential_User_UserId | UserId→Id | iam.User | NO_ACTION | True |
| iam.PasswordCredential | FK_PasswordCredential_User_UserId | UserId→Id | iam.User | NO_ACTION | True |
| iam.Role | FK_Role_User_CreatedBy | CreatedBy→Id | iam.User | NO_ACTION | True |
| iam.Role | FK_Role_User_UpdatedBy | UpdatedBy→Id | iam.User | NO_ACTION | True |
| iam.RoleChangeApproval | FK_RoleChangeApproval_PersonReference_ApproverPersonReferenceId | ApproverPersonReferenceId→Id | iam.PersonReference | NO_ACTION | True |
| iam.RoleChangeApproval | FK_RoleChangeApproval_RoleChangeRequest_RequestId_Revision | RequestId→Id, Revision→Revision | iam.RoleChangeRequest | NO_ACTION | True |
| iam.RoleChangeApproval | FK_RoleChangeApproval_User_ApproverUserId | ApproverUserId→Id | iam.User | NO_ACTION | True |
| iam.RoleChangeApproval | FK_RoleChangeApproval_User_ApproverUserId_ApproverPersonReferenceId | ApproverUserId→Id, ApproverPersonReferenceId→PersonReferenceId | iam.User | NO_ACTION | True |
| iam.RoleChangeRequest | FK_RoleChangeRequest_Role_RoleId | RoleId→Id | iam.Role | NO_ACTION | True |
| iam.RoleChangeRequest | FK_RoleChangeRequest_User_RequesterUserId | RequesterUserId→Id | iam.User | NO_ACTION | True |
| iam.RolePermission | FK_RolePermission_Permission_PermissionId | PermissionId→Id | iam.Permission | NO_ACTION | True |
| iam.RolePermission | FK_RolePermission_Role_RoleId | RoleId→Id | iam.Role | NO_ACTION | True |
| iam.Session | FK_Session_User_UserId | UserId→Id | iam.User | NO_ACTION | True |
| iam.StationGrant | FK_StationGrant_Scope_ScopeId | ScopeId→Id | catalog.Scope | NO_ACTION | True |
| iam.StationGrant | FK_StationGrant_UserRole_UserRoleId | UserRoleId→Id | iam.UserRole | NO_ACTION | True |
| iam.User | FK_User_PersonReference_PersonReferenceId | PersonReferenceId→Id | iam.PersonReference | NO_ACTION | True |
| iam.UserRole | FK_UserRole_Role_RoleId | RoleId→Id | iam.Role | NO_ACTION | True |
| iam.UserRole | FK_UserRole_User_UserId | UserId→Id | iam.User | NO_ACTION | True |
| ingest.DisturbanceRecord | FK_DisturbanceRecord_CollectorIed_CollectorId_IedId_StationId | CollectorId→CollectorId, IedId→IedId, StationId→StationId | ops.CollectorIed | NO_ACTION | True |
| ingest.IngestionAttempt | FK_IngestionAttempt_DisturbanceRecord_RecordId_StationId | RecordId→Id, StationId→StationId | ingest.DisturbanceRecord | NO_ACTION | True |
| ingest.RecordFile | FK_RecordFile_DisturbanceRecord_RecordId_StationId | RecordId→Id, StationId→StationId | ingest.DisturbanceRecord | NO_ACTION | True |
| ingest.RecordStatusHistory | FK_RecordStatusHistory_DisturbanceRecord_RecordId_StationId | RecordId→Id, StationId→StationId | ingest.DisturbanceRecord | NO_ACTION | True |
| ops.AnalysisJob | FK_AnalysisJob_AnalysisAlgorithm_AlgorithmId | AlgorithmId→Id | ops.AnalysisAlgorithm | NO_ACTION | True |
| ops.AnalysisJob | FK_AnalysisJob_FaultEvent_FaultEventId_StationId | FaultEventId→Id, StationId→StationId | ops.FaultEvent | NO_ACTION | True |
| ops.AnalysisResult | FK_AnalysisResult_AnalysisJob_JobId | JobId→Id | ops.AnalysisJob | NO_ACTION | True |
| ops.AuditOutbox | FK_AuditOutbox_SecurityEvent_EventId | EventId→Id | audit.SecurityEvent | NO_ACTION | True |
| ops.CollectionAttempt | FK_CollectionAttempt_CollectionJob_JobId | JobId→Id | ops.CollectionJob | NO_ACTION | True |
| ops.CollectionJob | FK_CollectionJob_CollectorIed_CollectorId_IedId_StationId | CollectorId→CollectorId, IedId→IedId, StationId→StationId | ops.CollectorIed | NO_ACTION | True |
| ops.CollectorHeartbeat | FK_CollectorHeartbeat_CollectorRegistration_CollectorId_StationId | CollectorId→Id, StationId→StationId | ops.CollectorRegistration | NO_ACTION | True |
| ops.CollectorIed | FK_CollectorIed_CollectorRegistration_CollectorId_StationId | CollectorId→Id, StationId→StationId | ops.CollectorRegistration | NO_ACTION | True |
| ops.CollectorIed | FK_CollectorIed_IedDevice_IedId_StationId | IedId→Id, StationId→StationId | catalog.IedDevice | NO_ACTION | True |
| ops.CollectorRegistration | FK_CollectorRegistration_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| ops.FaultEvent | FK_FaultEvent_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| ops.FaultEventRecord | FK_FaultEventRecord_DisturbanceRecord_RecordId_StationId | RecordId→Id, StationId→StationId | ingest.DisturbanceRecord | NO_ACTION | True |
| ops.FaultEventRecord | FK_FaultEventRecord_FaultEvent_FaultEventId_StationId | FaultEventId→Id, StationId→StationId | ops.FaultEvent | NO_ACTION | True |
| ops.OperationalEvent | FK_OperationalEvent_CollectorRegistration_CollectorId_StationId | CollectorId→Id, StationId→StationId | ops.CollectorRegistration | NO_ACTION | True |
| ops.OperationalEvent | FK_OperationalEvent_Station_StationId | StationId→Id | catalog.Station | NO_ACTION | True |
| ops.ProcessingOutbox | FK_ProcessingOutbox_DisturbanceRecord_RecordId_StationId | RecordId→Id, StationId→StationId | ingest.DisturbanceRecord | NO_ACTION | True |
| ops.RoleHistory | FK_RoleHistory_RoleChangeRequest_RequestId_Revision | RequestId→Id, Revision→Revision | iam.RoleChangeRequest | NO_ACTION | True |
| ops.RoleHistory | FK_RoleHistory_Role_RoleId | RoleId→Id | iam.Role | NO_ACTION | True |
| ops.StationSoeEvent | FK_StationSoeEvent_IedDevice_IedId_StationId | IedId→Id, StationId→StationId | catalog.IedDevice | NO_ACTION | True |

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
