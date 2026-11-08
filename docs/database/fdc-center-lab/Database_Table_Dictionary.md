# Deployed53-table dictionary

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

All53 tables below are CURRENT deployed, not proposed. IAM names preserved. Column sizes are SQL max bytes (-1=MAX), rowversion concurrency indicated. PK/FK/index/check definitions in Database_Schema_Inventory.json. No data values exported.

## audit.AuditCheckpoint

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| ArchiveReference | nvarchar | 400 | False | 0 | 0 |
| ContentSha256 | nvarchar | 128 | False | 0 | 0 |
| Status | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_AuditCheckpoint. Checks: CK_AuditCheckpoint_ArchiveReference_Required, CK_AuditCheckpoint_ContentSha256, CK_AuditCheckpoint_ContentSha256_Required, CK_AuditCheckpoint_Status, CK_AuditCheckpoint_Status_Required, CK_AuditCheckpoint_Utc.

## audit.AuditDelivery

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| EventId | uniqueidentifier | 16 | False | 0 | 0 |
| Attempt | int | 4 | False | 0 | 0 |
| State | nvarchar | 400 | False | 0 | 0 |
| ArchiveReference | nvarchar | 400 | True | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_AuditDelivery, IX_AuditDelivery_EventId_Attempt. Checks: CK_AuditDelivery_Attempt, CK_AuditDelivery_State_Required, CK_AuditDelivery_Utc.

## audit.SecurityEvent

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| ActorUserId | uniqueidentifier | 16 | True | 0 | 0 |
| EventCode | nvarchar | 512 | False | 0 | 0 |
| OutcomeCode | nvarchar | 512 | False | 0 | 0 |
| CorrelationId | nvarchar | 512 | False | 0 | 0 |
| PolicyVersion | bigint | 8 | False | 0 | 0 |
| RestoreEpoch | uniqueidentifier | 16 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_SecurityEvent, IX_SecurityEvent_ActorUserId. Checks: CK_SecurityEvent_Utc_CreatedAt, CK_SecurityEvent_UtcCreated, CK_SecurityEvent_Version.

## catalog.Bay

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_Bay, AK_Bay_Id_StationId, IX_Bay_StationId_Code. Checks: CK_Bay_Code_Required, CK_Bay_Utc.

## catalog.CommunicationProfile

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| Protocol | nvarchar | 400 | False | 0 | 0 |
| Port | int | 4 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_CommunicationProfile, IX_CommunicationProfile_Code. Checks: CK_CommunicationProfile_Code_Required, CK_CommunicationProfile_Protocol, CK_CommunicationProfile_Protocol_Required, CK_CommunicationProfile_Utc.

## catalog.Corporation

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| Code | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_Corporation. Checks: CK_Corporation_Utc_CreatedAt, CK_Corporation_UtcCreated, CK_Corporation_Version.

## catalog.DeviceFirmware

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| DeviceModelId | uniqueidentifier | 16 | False | 0 | 0 |
| Version | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_DeviceFirmware, AK_DeviceFirmware_Id_DeviceModelId, IX_DeviceFirmware_DeviceModelId_Version. Checks: CK_DeviceFirmware_Utc, CK_DeviceFirmware_Version_Required.

## catalog.DeviceModel

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| ManufacturerId | uniqueidentifier | 16 | False | 0 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_DeviceModel, IX_DeviceModel_ManufacturerId_Code. Checks: CK_DeviceModel_Code_Required, CK_DeviceModel_Utc.

## catalog.Feeder

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_Feeder, AK_Feeder_Id_StationId, IX_Feeder_StationId_Code. Checks: CK_Feeder_Code_Required, CK_Feeder_Utc.

## catalog.IedDevice

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| BayId | uniqueidentifier | 16 | True | 0 | 0 |
| FeederId | uniqueidentifier | 16 | True | 0 | 0 |
| DeviceModelId | uniqueidentifier | 16 | True | 0 | 0 |
| FirmwareId | uniqueidentifier | 16 | True | 0 | 0 |
| CommunicationProfileId | uniqueidentifier | 16 | True | 0 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| DisplayName | nvarchar | 400 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_IedDevice, AK_IedDevice_Id_StationId, IX_IedDevice_BayId_StationId, IX_IedDevice_CommunicationProfileId, IX_IedDevice_DeviceModelId, IX_IedDevice_FeederId_StationId, IX_IedDevice_FirmwareId_DeviceModelId, IX_IedDevice_StationId_Code. Checks: CK_IedDevice_Code_Required, CK_IedDevice_DisplayName_Required, CK_IedDevice_FirmwareModel, CK_IedDevice_Utc.

## catalog.ManagementUnit

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| CorporationId | uniqueidentifier | 16 | False | 0 | 0 |
| ParentUnitId | uniqueidentifier | 16 | True | 0 | 0 |
| Code | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_ManagementUnit, IX_ManagementUnit_CorporationId, IX_ManagementUnit_ParentUnitId. Checks: CK_ManagementUnit_Utc_CreatedAt, CK_ManagementUnit_UtcCreated, CK_ManagementUnit_Version.

## catalog.Manufacturer

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| Name | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_Manufacturer, IX_Manufacturer_Code. Checks: CK_Manufacturer_Code_Required, CK_Manufacturer_Name_Required, CK_Manufacturer_Utc.

## catalog.Scope

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| Kind | nvarchar | 512 | False | 0 | 0 |
| CorporationId | uniqueidentifier | 16 | True | 0 | 0 |
| ManagementUnitId | uniqueidentifier | 16 | True | 0 | 0 |
| StationId | uniqueidentifier | 16 | True | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_Scope, IX_Scope_CorporationId, IX_Scope_ManagementUnitId, IX_Scope_StationId. Checks: CK_Scope_Target, CK_Scope_Utc_CreatedAt, CK_Scope_UtcCreated, CK_Scope_Version.

## catalog.Station

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| ManagementUnitId | uniqueidentifier | 16 | False | 0 | 0 |
| Code | nvarchar | 512 | False | 0 | 0 |
| HierarchyVersion | bigint | 8 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_Station, IX_Station_ManagementUnitId. Checks: CK_Station_Utc_CreatedAt, CK_Station_UtcCreated, CK_Station_Version.

## catalog.StationProfile

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| VoltageLevelId | uniqueidentifier | 16 | False | 0 | 0 |
| DisplayName | nvarchar | 400 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_StationProfile, IX_StationProfile_StationId, IX_StationProfile_VoltageLevelId. Checks: CK_StationProfile_DisplayName_Required, CK_StationProfile_Utc.

## catalog.StationUnitHistory

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| ManagementUnitId | uniqueidentifier | 16 | False | 0 | 0 |
| ValidFromUtc | datetimeoffset | 10 | False | 0 | 0 |
| ValidUntilUtc | datetimeoffset | 10 | True | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_StationUnitHistory, IX_StationUnitHistory_ManagementUnitId, IX_StationUnitHistory_StationId. Checks: CK_StationUnitHistory_Utc, CK_StationUnitHistory_Validity, CK_StationUnitHistory_Utc_ValidFromUtc, CK_StationUnitHistory_Utc_ValidUntilUtc.

## catalog.VoltageLevel

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| Kilovolts | decimal | 5 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_VoltageLevel, IX_VoltageLevel_Kilovolts. Checks: CK_VoltageLevel_PositiveKv, CK_VoltageLevel_Utc.

## iam.AdministrativeDelegation

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| UserRoleId | uniqueidentifier | 16 | False | 0 | 0 |
| PermissionId | uniqueidentifier | 16 | False | 0 | 0 |
| TargetPopulation | nvarchar | 512 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| ValidFrom | datetimeoffset | 10 | False | 0 | 0 |
| ValidUntil | datetimeoffset | 10 | False | 0 | 0 |
| ApprovalReference | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_AdministrativeDelegation, IX_AdministrativeDelegation_PermissionId, IX_AdministrativeDelegation_UserRoleId. Checks: CK_AdministrativeDelegation_Utc_CreatedAt, CK_AdministrativeDelegation_Utc_ValidFrom, CK_AdministrativeDelegation_Utc_ValidUntil, CK_AdministrativeDelegation_UtcCreated, CK_AdministrativeDelegation_Validity, CK_AdministrativeDelegation_Version.

## iam.AuthorityVersion

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| AuthorityKey | nvarchar | 512 | False | 0 | 0 |
| PolicyVersion | bigint | 8 | False | 0 | 0 |
| CatalogVersion | bigint | 8 | False | 0 | 0 |
| HierarchyVersion | bigint | 8 | False | 0 | 0 |
| RestoreEpoch | uniqueidentifier | 16 | False | 0 | 0 |
| PolicyConfigured | bit | 1 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_AuthorityVersion, IX_AuthorityVersion_AuthorityKey. Checks: CK_AuthorityVersion_Utc_CreatedAt, CK_AuthorityVersion_UtcCreated, CK_AuthorityVersion_Version.

## iam.Challenge

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| UserId | uniqueidentifier | 16 | False | 0 | 0 |
| ProofDigest | nchar | 128 | False | 0 | 0 |
| Purpose | nvarchar | 512 | False | 0 | 0 |
| ExpiresAt | datetimeoffset | 10 | False | 0 | 0 |
| ConsumedAt | datetimeoffset | 10 | True | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_Challenge, IX_Challenge_ProofDigest, IX_Challenge_UserId. Checks: CK_Challenge_Utc_ConsumedAt, CK_Challenge_Utc_CreatedAt, CK_Challenge_Utc_ExpiresAt, CK_Challenge_UtcCreated, CK_Challenge_Version.

## iam.GrantFunction

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationGrantId | uniqueidentifier | 16 | False | 0 | 0 |
| PermissionId | uniqueidentifier | 16 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_GrantFunction, IX_GrantFunction_PermissionId, IX_GrantFunction_StationGrantId_PermissionId. Checks: CK_GrantFunction_Utc_CreatedAt, CK_GrantFunction_UtcCreated, CK_GrantFunction_Version.

## iam.MfaCredential

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| UserId | uniqueidentifier | 16 | False | 0 | 0 |
| Kind | nvarchar | 512 | False | 0 | 0 |
| CredentialReference | nvarchar | 512 | False | 0 | 0 |
| PublicKey | varbinary | -1 | True | 0 | 0 |
| EncryptedSecret | varbinary | -1 | True | 0 | 0 |
| KeyVersion | nvarchar | 512 | True | 0 | 0 |
| LastAcceptedStep | bigint | 8 | True | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_MfaCredential, IX_MfaCredential_CredentialReference, IX_MfaCredential_UserId. Checks: CK_MfaCredential_Material, CK_MfaCredential_Utc_CreatedAt, CK_MfaCredential_UtcCreated, CK_MfaCredential_Version.

## iam.PasswordCredential

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| UserId | uniqueidentifier | 16 | False | 0 | 0 |
| EncodedHash | nvarchar | 4096 | False | 0 | 0 |
| AlgorithmVersion | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_PasswordCredential, IX_PasswordCredential_UserId. Checks: CK_PasswordCredential_Utc_CreatedAt, CK_PasswordCredential_UtcCreated, CK_PasswordCredential_Version.

## iam.Permission

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| PermissionCode | nvarchar | 240 | False | 0 | 0 |
| Module | nvarchar | 512 | False | 0 | 0 |
| Action | nvarchar | 512 | False | 0 | 0 |
| ScopeRequirement | nvarchar | 512 | False | 0 | 0 |
| SoDPolicy | nvarchar | 512 | False | 0 | 0 |
| RiskLevel | nvarchar | 512 | False | 0 | 0 |
| IsApproved | bit | 1 | False | 0 | 0 |
| IsImplemented | bit | 1 | False | 0 | 0 |
| CatalogVersion | bigint | 8 | False | 0 | 0 |
| ApprovalReference | nvarchar | 512 | True | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_Permission, IX_Permission_PermissionCode. Checks: CK_Permission_Utc_CreatedAt, CK_Permission_UtcCreated, CK_Permission_Version.

## iam.PersonReference

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| PersonKey | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_PersonReference, IX_PersonReference_PersonKey. Checks: CK_PersonReference_Utc_CreatedAt, CK_PersonReference_UtcCreated, CK_PersonReference_Version.

## iam.Role

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| CreatedBy | uniqueidentifier | 16 | True | 0 | 0 |
| UpdatedBy | uniqueidentifier | 16 | True | 0 | 0 |
| UpdatedAt | datetimeoffset | 10 | True | 0 | 0 |
| RoleCode | nvarchar | 160 | False | 0 | 0 |
| RoleName | nvarchar | 320 | False | 0 | 0 |
| Description | nvarchar | 4000 | False | 0 | 0 |
| IsSystemSeed | bit | 1 | False | 0 | 0 |
| IsActive | bit | 1 | False | 0 | 0 |
| ValidUntil | datetimeoffset | 10 | True | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_Role, IX_Role_CreatedBy, IX_Role_RoleCode, IX_Role_UpdatedBy. Checks: CK_Role_Utc_CreatedAt, CK_Role_Utc_UpdatedAt, CK_Role_Utc_ValidUntil, CK_Role_UtcCreated, CK_Role_Version.

## iam.RoleChangeApproval

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| RequestId | uniqueidentifier | 16 | False | 0 | 0 |
| Revision | bigint | 8 | False | 0 | 0 |
| ApproverUserId | uniqueidentifier | 16 | False | 0 | 0 |
| ApproverPersonReferenceId | uniqueidentifier | 16 | False | 0 | 0 |
| RequestDigest | nvarchar | 512 | False | 0 | 0 |
| Decision | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_RoleChangeApproval, IX_RoleChangeApproval_ApproverPersonReferenceId, IX_RoleChangeApproval_ApproverUserId_ApproverPersonReferenceId, IX_RoleChangeApproval_RequestId_Revision_ApproverPersonReferenceId. Checks: CK_RoleChangeApproval_Utc_CreatedAt, CK_RoleChangeApproval_UtcCreated, CK_RoleChangeApproval_Version.

## iam.RoleChangeRequest

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| RequesterUserId | uniqueidentifier | 16 | False | 0 | 0 |
| RoleId | uniqueidentifier | 16 | True | 0 | 0 |
| Revision | bigint | 8 | False | 0 | 0 |
| PayloadJson | nvarchar | -1 | False | 0 | 0 |
| Digest | nchar | 128 | False | 0 | 0 |
| IdempotencyKey | nvarchar | 512 | False | 0 | 0 |
| BaseRoleVersion | bigint | 8 | False | 0 | 0 |
| PolicyVersion | bigint | 8 | False | 0 | 0 |
| CatalogVersion | bigint | 8 | False | 0 | 0 |
| ExpiresAt | datetimeoffset | 10 | False | 0 | 0 |
| ApprovalStatus | nvarchar | 512 | False | 0 | 0 |
| EffectiveStatus | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_RoleChangeRequest, AK_RoleChangeRequest_Id_Revision, IX_RoleChangeRequest_RequesterUserId_IdempotencyKey, IX_RoleChangeRequest_RoleId. Checks: CK_RoleChangeRequest_Json, CK_RoleChangeRequest_Utc_CreatedAt, CK_RoleChangeRequest_Utc_ExpiresAt, CK_RoleChangeRequest_UtcCreated, CK_RoleChangeRequest_Version.

## iam.RolePermission

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| RoleId | uniqueidentifier | 16 | False | 0 | 0 |
| PermissionId | uniqueidentifier | 16 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_RolePermission, IX_RolePermission_PermissionId, IX_RolePermission_RoleId_PermissionId. Checks: CK_RolePermission_Utc_CreatedAt, CK_RolePermission_UtcCreated, CK_RolePermission_Version.

## iam.Session

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| UserId | uniqueidentifier | 16 | False | 0 | 0 |
| HandleDigest | nchar | 128 | False | 0 | 0 |
| Revoked | bit | 1 | False | 0 | 0 |
| IdleUntil | datetimeoffset | 10 | False | 0 | 0 |
| AbsoluteUntil | datetimeoffset | 10 | False | 0 | 0 |
| MfaAt | datetimeoffset | 10 | True | 0 | 0 |
| SecurityVersion | bigint | 8 | False | 0 | 0 |
| GrantVersion | bigint | 8 | False | 0 | 0 |
| PolicyVersion | bigint | 8 | False | 0 | 0 |
| CatalogVersion | bigint | 8 | False | 0 | 0 |
| HierarchyVersion | bigint | 8 | False | 0 | 0 |
| RestoreEpoch | uniqueidentifier | 16 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_Session, IX_Session_HandleDigest, IX_Session_UserId_Revoked_AbsoluteUntil. Checks: CK_Session_Utc_AbsoluteUntil, CK_Session_Utc_CreatedAt, CK_Session_Utc_IdleUntil, CK_Session_Utc_MfaAt, CK_Session_UtcCreated, CK_Session_Version.

## iam.StationGrant

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| UserRoleId | uniqueidentifier | 16 | False | 0 | 0 |
| ScopeId | uniqueidentifier | 16 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| ValidFrom | datetimeoffset | 10 | False | 0 | 0 |
| ValidUntil | datetimeoffset | 10 | False | 0 | 0 |
| GrantVersion | bigint | 8 | False | 0 | 0 |
| HierarchyVersion | bigint | 8 | False | 0 | 0 |
| ApprovalReference | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_StationGrant, IX_StationGrant_ScopeId, IX_StationGrant_UserRoleId_Active_ValidUntil. Checks: CK_StationGrant_Utc_CreatedAt, CK_StationGrant_Utc_ValidFrom, CK_StationGrant_Utc_ValidUntil, CK_StationGrant_UtcCreated, CK_StationGrant_Validity, CK_StationGrant_Version.

## iam.User

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| PersonReferenceId | uniqueidentifier | 16 | False | 0 | 0 |
| NormalizedUsername | nvarchar | 512 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| SecurityVersion | bigint | 8 | False | 0 | 0 |
| GrantVersion | bigint | 8 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_User, AK_User_Id_PersonReferenceId, IX_User_NormalizedUsername, IX_User_PersonReferenceId. Checks: CK_User_Utc_CreatedAt, CK_User_UtcCreated, CK_User_Version.

## iam.UserRole

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| UserId | uniqueidentifier | 16 | False | 0 | 0 |
| RoleId | uniqueidentifier | 16 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| ValidFrom | datetimeoffset | 10 | False | 0 | 0 |
| ValidUntil | datetimeoffset | 10 | False | 0 | 0 |
| ApprovalReference | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_UserRole, IX_UserRole_RoleId, IX_UserRole_UserId_Active_ValidUntil. Checks: CK_UserRole_Utc_CreatedAt, CK_UserRole_Utc_ValidFrom, CK_UserRole_Utc_ValidUntil, CK_UserRole_UtcCreated, CK_UserRole_Validity, CK_UserRole_Version.

## ingest.DisturbanceRecord

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| IedId | uniqueidentifier | 16 | False | 0 | 0 |
| CollectorId | uniqueidentifier | 16 | False | 0 | 0 |
| ManifestId | nvarchar | 128 | False | 0 | 0 |
| ManifestSha256 | nvarchar | 128 | False | 0 | 0 |
| RemoteIdentity | nvarchar | 1024 | False | 0 | 0 |
| CollectedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| EventAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| SourceTimestampRaw | nvarchar | 400 | True | 0 | 0 |
| TimeSource | nvarchar | 400 | False | 0 | 0 |
| State | nvarchar | 400 | False | 0 | 0 |
| CorrelationId | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |
| ManifestSnapshotJson | nvarchar | -1 | True | 0 | 0 |

Indexes: PK_DisturbanceRecord, AK_DisturbanceRecord_Id_StationId, IX_DisturbanceRecord_CollectorId_IedId_StationId, IX_DisturbanceRecord_StationId_EventAtUtc_Id, IX_DisturbanceRecord_StationId_IedId_ManifestId, IX_DisturbanceRecord_StationId_State_CreatedAtUtc. Checks: CK_DisturbanceRecord_Utc_CollectedAtUtc, CK_DisturbanceRecord_Utc_EventAtUtc, CK_DisturbanceRecord_ManifestSnapshot, CK_DisturbanceRecord_CorrelationId_Required, CK_DisturbanceRecord_ManifestId, CK_DisturbanceRecord_ManifestId_Required, CK_DisturbanceRecord_ManifestSha256, CK_DisturbanceRecord_ManifestSha256_Required, CK_DisturbanceRecord_RemoteIdentity_Required, CK_DisturbanceRecord_State, CK_DisturbanceRecord_State_Required, CK_DisturbanceRecord_TimeSource_Required, CK_DisturbanceRecord_Utc.

## ingest.IngestionAttempt

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| RecordId | uniqueidentifier | 16 | False | 0 | 0 |
| Number | int | 4 | False | 0 | 0 |
| Outcome | nvarchar | 400 | False | 0 | 0 |
| CorrelationId | nvarchar | 400 | False | 0 | 0 |
| ErrorCode | nvarchar | 400 | True | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_IngestionAttempt, IX_IngestionAttempt_RecordId_Number, IX_IngestionAttempt_RecordId_StationId. Checks: CK_IngestionAttempt_Attempt, CK_IngestionAttempt_CorrelationId_Required, CK_IngestionAttempt_Outcome_Required, CK_IngestionAttempt_Utc.

## ingest.RecordFile

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| RecordId | uniqueidentifier | 16 | False | 0 | 0 |
| Name | nvarchar | 400 | False | 0 | 0 |
| Extension | nvarchar | 400 | False | 0 | 0 |
| SizeBytes | bigint | 8 | False | 0 | 0 |
| Sha256 | nvarchar | 128 | False | 0 | 0 |
| StorageObjectKey | nvarchar | 1024 | False | 0 | 0 |
| ValidationStatus | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_RecordFile, IX_RecordFile_RecordId_Name, IX_RecordFile_RecordId_StationId, IX_RecordFile_Sha256. Checks: CK_RecordFile_Extension_Required, CK_RecordFile_File, CK_RecordFile_Name_Required, CK_RecordFile_ObjectKey, CK_RecordFile_Sha256, CK_RecordFile_Sha256_Required, CK_RecordFile_StorageObjectKey_Required, CK_RecordFile_Utc, CK_RecordFile_ValidationStatus_Required.

## ingest.RecordStatusHistory

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| RecordId | uniqueidentifier | 16 | False | 0 | 0 |
| State | nvarchar | 400 | False | 0 | 0 |
| CorrelationId | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_RecordStatusHistory, IX_RecordStatusHistory_RecordId_CreatedAtUtc, IX_RecordStatusHistory_RecordId_StationId. Checks: CK_RecordStatusHistory_CorrelationId_Required, CK_RecordStatusHistory_State_Required, CK_RecordStatusHistory_Utc.

## ops.AnalysisAlgorithm

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| Version | nvarchar | 400 | False | 0 | 0 |
| Implemented | bit | 1 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_AnalysisAlgorithm, IX_AnalysisAlgorithm_Code_Version. Checks: CK_AnalysisAlgorithm_Code_Required, CK_AnalysisAlgorithm_Utc, CK_AnalysisAlgorithm_Version_Required.

## ops.AnalysisJob

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| FaultEventId | uniqueidentifier | 16 | False | 0 | 0 |
| AlgorithmId | uniqueidentifier | 16 | False | 0 | 0 |
| State | nvarchar | 400 | False | 0 | 0 |
| CorrelationId | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_AnalysisJob, IX_AnalysisJob_AlgorithmId, IX_AnalysisJob_FaultEventId_StationId. Checks: CK_AnalysisJob_CorrelationId_Required, CK_AnalysisJob_State, CK_AnalysisJob_State_Required, CK_AnalysisJob_Utc.

## ops.AnalysisResult

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| JobId | uniqueidentifier | 16 | False | 0 | 0 |
| Status | nvarchar | 400 | False | 0 | 0 |
| ResultJson | nvarchar | 8000 | True | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_AnalysisResult, IX_AnalysisResult_JobId. Checks: CK_AnalysisResult_Json, CK_AnalysisResult_State, CK_AnalysisResult_Status_Required, CK_AnalysisResult_Utc.

## ops.AuditOutbox

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| EventId | uniqueidentifier | 16 | False | 0 | 0 |
| ArchivedAt | datetimeoffset | 10 | True | 0 | 0 |
| Fence | bigint | 8 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_AuditOutbox, IX_AuditOutbox_ArchivedAt_CreatedAt, IX_AuditOutbox_EventId. Checks: CK_AuditOutbox_Utc_ArchivedAt, CK_AuditOutbox_Utc_CreatedAt, CK_AuditOutbox_UtcCreated, CK_AuditOutbox_Version.

## ops.CollectionAttempt

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| JobId | uniqueidentifier | 16 | False | 0 | 0 |
| Number | int | 4 | False | 0 | 0 |
| Outcome | nvarchar | 400 | False | 0 | 0 |
| CorrelationId | nvarchar | 400 | False | 0 | 0 |
| ErrorCode | nvarchar | 400 | True | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_CollectionAttempt, IX_CollectionAttempt_JobId_Number. Checks: CK_CollectionAttempt_Attempt, CK_CollectionAttempt_CorrelationId_Required, CK_CollectionAttempt_Outcome_Required, CK_CollectionAttempt_Utc.

## ops.CollectionJob

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| CollectorId | uniqueidentifier | 16 | False | 0 | 0 |
| IedId | uniqueidentifier | 16 | False | 0 | 0 |
| State | nvarchar | 400 | False | 0 | 0 |
| NextRunAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_CollectionJob, IX_CollectionJob_CollectorId_IedId_StationId, IX_CollectionJob_State_NextRunAtUtc. Checks: CK_CollectionJob_Utc_NextRunAtUtc, CK_CollectionJob_State, CK_CollectionJob_State_Required, CK_CollectionJob_Utc.

## ops.CollectorHeartbeat

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| CollectorId | uniqueidentifier | 16 | False | 0 | 0 |
| ReceivedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| Status | nvarchar | 400 | False | 0 | 0 |
| QueueDepth | bigint | 8 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_CollectorHeartbeat, IX_CollectorHeartbeat_CollectorId_ReceivedAtUtc, IX_CollectorHeartbeat_CollectorId_StationId. Checks: CK_CollectorHeartbeat_Utc_ReceivedAtUtc, CK_CollectorHeartbeat_Queue, CK_CollectorHeartbeat_Status_Required, CK_CollectorHeartbeat_Utc.

## ops.CollectorIed

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| CollectorId | uniqueidentifier | 16 | False | 0 | 0 |
| IedId | uniqueidentifier | 16 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_CollectorIed, AK_CollectorIed_CollectorId_IedId_StationId, IX_CollectorIed_CollectorId_StationId, IX_CollectorIed_IedId_StationId. Checks: CK_CollectorIed_Utc.

## ops.CollectorRegistration

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| Active | bit | 1 | False | 0 | 0 |
| ConfigurationVersion | bigint | 8 | False | 0 | 0 |
| BindingStatus | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_CollectorRegistration, AK_CollectorRegistration_Id_StationId, IX_CollectorRegistration_Code, IX_CollectorRegistration_StationId. Checks: CK_CollectorRegistration_Utc, CK_CollectorRegistration_BindingStatus_Required, CK_CollectorRegistration_Code_Required, CK_CollectorRegistration_Config.

## ops.FaultEvent

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| EventAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| AnalysisStatus | nvarchar | 400 | False | 0 | 0 |
| SourceTimestampRaw | nvarchar | 400 | True | 0 | 0 |
| TimeSource | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_FaultEvent, AK_FaultEvent_Id_StationId, IX_FaultEvent_StationId_EventAtUtc_Id. Checks: CK_FaultEvent_AnalysisState, CK_FaultEvent_AnalysisStatus_Required, CK_FaultEvent_TimeSource_Required, CK_FaultEvent_Utc, CK_FaultEvent_Utc_EventAtUtc.

## ops.FaultEventRecord

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| FaultEventId | uniqueidentifier | 16 | False | 0 | 0 |
| RecordId | uniqueidentifier | 16 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_FaultEventRecord, IX_FaultEventRecord_FaultEventId_RecordId, IX_FaultEventRecord_FaultEventId_StationId, IX_FaultEventRecord_RecordId_StationId. Checks: CK_FaultEventRecord_Utc.

## ops.OperationalEvent

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| CollectorId | uniqueidentifier | 16 | True | 0 | 0 |
| Code | nvarchar | 400 | False | 0 | 0 |
| CorrelationId | nvarchar | 400 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_OperationalEvent, IX_OperationalEvent_CollectorId_StationId. Checks: CK_OperationalEvent_Code_Required, CK_OperationalEvent_CorrelationId_Required, CK_OperationalEvent_Utc.

## ops.ProcessingOutbox

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| RecordId | uniqueidentifier | 16 | False | 0 | 0 |
| Kind | nvarchar | 400 | False | 0 | 0 |
| State | nvarchar | 400 | False | 0 | 0 |
| Attempts | int | 4 | False | 0 | 0 |
| AvailableAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| LeaseUntilUtc | datetimeoffset | 10 | True | 0 | 0 |
| LeaseOwner | nvarchar | 400 | True | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_ProcessingOutbox, IX_ProcessingOutbox_RecordId_Kind, IX_ProcessingOutbox_RecordId_StationId, IX_ProcessingOutbox_State_AvailableAtUtc. Checks: CK_ProcessingOutbox_Kind_Required, CK_ProcessingOutbox_Queue, CK_ProcessingOutbox_State_Required, CK_ProcessingOutbox_Utc, CK_ProcessingOutbox_Utc_AvailableAtUtc, CK_ProcessingOutbox_Utc_LeaseUntilUtc.

## ops.RoleHistory

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| RoleId | uniqueidentifier | 16 | False | 0 | 0 |
| RequestId | uniqueidentifier | 16 | False | 0 | 0 |
| Revision | bigint | 8 | False | 0 | 0 |
| Digest | nvarchar | 512 | False | 0 | 0 |
| StateJson | nvarchar | -1 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_RoleHistory, IX_RoleHistory_RequestId_Revision, IX_RoleHistory_RoleId_Version. Checks: CK_RoleHistory_Utc_CreatedAt, CK_RoleHistory_UtcCreated, CK_RoleHistory_Version.

## ops.SeedInstallation

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| ManifestId | nvarchar | 512 | False | 0 | 0 |
| ManifestDigest | nvarchar | 512 | False | 0 | 0 |
| ApprovalReference | nvarchar | 512 | False | 0 | 0 |
| Version | bigint | 8 | False | 0 | 0 |
| CreatedAt | datetimeoffset | 10 | False | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |

Indexes: PK_SeedInstallation, IX_SeedInstallation_ManifestId. Checks: CK_SeedInstallation_Utc_CreatedAt, CK_SeedInstallation_UtcCreated, CK_SeedInstallation_Version.

## ops.StationSoeEvent

| Column | SQL type | Max bytes | Nullable | PK | Concurrency |
|---|---|---|---|---|---|
| Id | uniqueidentifier | 16 | False | 1 | 0 |
| StationId | uniqueidentifier | 16 | False | 0 | 0 |
| IedId | uniqueidentifier | 16 | False | 0 | 0 |
| SourceEventId | nvarchar | 400 | False | 0 | 0 |
| RcbReference | nvarchar | 400 | False | 0 | 0 |
| DataReference | nvarchar | 400 | False | 0 | 0 |
| Value | nvarchar | 400 | False | 0 | 0 |
| Source | nvarchar | 400 | False | 0 | 0 |
| ReceivedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| IedTimestampUtc | datetimeoffset | 10 | True | 0 | 0 |
| TimeSource | nvarchar | 400 | False | 0 | 0 |
| TimeQuality | tinyint | 1 | True | 0 | 0 |
| Quality | nvarchar | 400 | True | 0 | 0 |
| Reason | int | 4 | False | 0 | 0 |
| EntryId | nvarchar | 400 | True | 0 | 0 |
| Sequence | int | 4 | True | 0 | 0 |
| ConfRev | bigint | 8 | True | 0 | 0 |
| BufferOverflow | bit | 1 | False | 0 | 0 |
| SubSequence | int | 4 | True | 0 | 0 |
| MoreSegments | bit | 1 | False | 0 | 0 |
| CreatedAtUtc | datetimeoffset | 10 | False | 0 | 0 |
| UpdatedAtUtc | datetimeoffset | 10 | True | 0 | 0 |
| RowVersion | timestamp | 8 | False | 0 | 1 |
| IsSynthetic | bit | 1 | False | 0 | 0 |

Indexes: PK_StationSoeEvent, IX_StationSoeEvent_IedId_StationId, IX_StationSoeEvent_StationId_IedId_SourceEventId, IX_StationSoeEvent_StationId_ReceivedAtUtc_Id. Checks: CK_StationSoeEvent_DataReference_Required, CK_StationSoeEvent_Range, CK_StationSoeEvent_RcbReference_Required, CK_StationSoeEvent_Source_Required, CK_StationSoeEvent_SourceEventId_Required, CK_StationSoeEvent_TimeSource_Required, CK_StationSoeEvent_Utc, CK_StationSoeEvent_Utc_IedTimestampUtc, CK_StationSoeEvent_Utc_ReceivedAtUtc, CK_StationSoeEvent_Value_Required.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
