-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'IF SCHEMA_ID(N''iam'') IS NULL EXEC(N''CREATE SCHEMA [iam];'');
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
IF SCHEMA_ID(N''ops'') IS NULL EXEC(N''CREATE SCHEMA [ops];'');
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
IF SCHEMA_ID(N''catalog'') IS NULL EXEC(N''CREATE SCHEMA [catalog];'');
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
IF SCHEMA_ID(N''audit'') IS NULL EXEC(N''CREATE SCHEMA [audit];'');
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[AuthorityVersion] (
    [Id] uniqueidentifier NOT NULL,
    [AuthorityKey] nvarchar(256) NOT NULL,
    [PolicyVersion] bigint NOT NULL,
    [CatalogVersion] bigint NOT NULL,
    [HierarchyVersion] bigint NOT NULL,
    [RestoreEpoch] uniqueidentifier NOT NULL,
    [PolicyConfigured] bit NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_AuthorityVersion] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AuthorityVersion_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_AuthorityVersion_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_AuthorityVersion_Version] CHECK ([Version] > 0)
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [catalog].[Corporation] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Corporation] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Corporation_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Corporation_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Corporation_Version] CHECK ([Version] > 0)
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[Permission] (
    [Id] uniqueidentifier NOT NULL,
    [PermissionCode] nvarchar(120) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [Module] nvarchar(256) NOT NULL,
    [Action] nvarchar(256) NOT NULL,
    [ScopeRequirement] nvarchar(256) NOT NULL,
    [SoDPolicy] nvarchar(256) NOT NULL,
    [RiskLevel] nvarchar(256) NOT NULL,
    [IsApproved] bit NOT NULL,
    [IsImplemented] bit NOT NULL,
    [CatalogVersion] bigint NOT NULL,
    [ApprovalReference] nvarchar(256) NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Permission] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Permission_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Permission_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Permission_Version] CHECK ([Version] > 0)
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[PersonReference] (
    [Id] uniqueidentifier NOT NULL,
    [PersonKey] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PersonReference] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PersonReference_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_PersonReference_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_PersonReference_Version] CHECK ([Version] > 0)
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [ops].[SeedInstallation] (
    [Id] uniqueidentifier NOT NULL,
    [ManifestId] nvarchar(256) NOT NULL,
    [ManifestDigest] nvarchar(256) NOT NULL,
    [ApprovalReference] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SeedInstallation] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SeedInstallation_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_SeedInstallation_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_SeedInstallation_Version] CHECK ([Version] > 0)
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [catalog].[ManagementUnit] (
    [Id] uniqueidentifier NOT NULL,
    [CorporationId] uniqueidentifier NOT NULL,
    [ParentUnitId] uniqueidentifier NULL,
    [Code] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ManagementUnit] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ManagementUnit_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_ManagementUnit_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_ManagementUnit_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_ManagementUnit_Corporation_CorporationId] FOREIGN KEY ([CorporationId]) REFERENCES [catalog].[Corporation] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ManagementUnit_ManagementUnit_ParentUnitId] FOREIGN KEY ([ParentUnitId]) REFERENCES [catalog].[ManagementUnit] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[User] (
    [Id] uniqueidentifier NOT NULL,
    [PersonReferenceId] uniqueidentifier NOT NULL,
    [NormalizedUsername] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL,
    [SecurityVersion] bigint NOT NULL,
    [GrantVersion] bigint NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_User_Id_PersonReferenceId] UNIQUE ([Id], [PersonReferenceId]),
    CONSTRAINT [CK_User_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_User_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_User_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_User_PersonReference_PersonReferenceId] FOREIGN KEY ([PersonReferenceId]) REFERENCES [iam].[PersonReference] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [catalog].[Station] (
    [Id] uniqueidentifier NOT NULL,
    [ManagementUnitId] uniqueidentifier NOT NULL,
    [Code] nvarchar(256) NOT NULL,
    [HierarchyVersion] bigint NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Station] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Station_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Station_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Station_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_Station_ManagementUnit_ManagementUnitId] FOREIGN KEY ([ManagementUnitId]) REFERENCES [catalog].[ManagementUnit] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[Challenge] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ProofDigest] nchar(64) NOT NULL,
    [Purpose] nvarchar(256) NOT NULL,
    [ExpiresAt] datetimeoffset NOT NULL,
    [ConsumedAt] datetimeoffset NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Challenge] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Challenge_Utc_ConsumedAt] CHECK ([ConsumedAt] IS NULL OR DATEPART(TZOFFSET,[ConsumedAt])=0),
    CONSTRAINT [CK_Challenge_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Challenge_Utc_ExpiresAt] CHECK ([ExpiresAt] IS NULL OR DATEPART(TZOFFSET,[ExpiresAt])=0),
    CONSTRAINT [CK_Challenge_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Challenge_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_Challenge_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[MfaCredential] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Kind] nvarchar(256) NOT NULL,
    [CredentialReference] nvarchar(256) NOT NULL,
    [PublicKey] varbinary(max) NULL,
    [EncryptedSecret] varbinary(max) NULL,
    [KeyVersion] nvarchar(256) NULL,
    [LastAcceptedStep] bigint NULL,
    [Active] bit NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_MfaCredential] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_MfaCredential_Material] CHECK (([Kind] = N''WebAuthn'' AND [PublicKey] IS NOT NULL AND [EncryptedSecret] IS NULL) OR ([Kind] = N''Totp'' AND [EncryptedSecret] IS NOT NULL AND [KeyVersion] IS NOT NULL AND [PublicKey] IS NULL)),
    CONSTRAINT [CK_MfaCredential_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_MfaCredential_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_MfaCredential_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_MfaCredential_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[PasswordCredential] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [EncodedHash] nvarchar(2048) NOT NULL,
    [AlgorithmVersion] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PasswordCredential] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PasswordCredential_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_PasswordCredential_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_PasswordCredential_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_PasswordCredential_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[Role] (
    [Id] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [RoleCode] nvarchar(80) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [RoleName] nvarchar(160) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [IsSystemSeed] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [ValidUntil] datetimeoffset NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Role_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Role_Utc_UpdatedAt] CHECK ([UpdatedAt] IS NULL OR DATEPART(TZOFFSET,[UpdatedAt])=0),
    CONSTRAINT [CK_Role_Utc_ValidUntil] CHECK ([ValidUntil] IS NULL OR DATEPART(TZOFFSET,[ValidUntil])=0),
    CONSTRAINT [CK_Role_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Role_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_Role_User_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Role_User_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [audit].[SecurityEvent] (
    [Id] uniqueidentifier NOT NULL,
    [ActorUserId] uniqueidentifier NULL,
    [EventCode] nvarchar(256) NOT NULL,
    [OutcomeCode] nvarchar(256) NOT NULL,
    [CorrelationId] nvarchar(256) NOT NULL,
    [PolicyVersion] bigint NOT NULL,
    [RestoreEpoch] uniqueidentifier NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SecurityEvent] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SecurityEvent_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_SecurityEvent_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_SecurityEvent_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_SecurityEvent_User_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[Session] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [HandleDigest] nchar(64) NOT NULL,
    [Revoked] bit NOT NULL,
    [IdleUntil] datetimeoffset NOT NULL,
    [AbsoluteUntil] datetimeoffset NOT NULL,
    [MfaAt] datetimeoffset NULL,
    [SecurityVersion] bigint NOT NULL,
    [GrantVersion] bigint NOT NULL,
    [PolicyVersion] bigint NOT NULL,
    [CatalogVersion] bigint NOT NULL,
    [HierarchyVersion] bigint NOT NULL,
    [RestoreEpoch] uniqueidentifier NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Session] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Session_Utc_AbsoluteUntil] CHECK ([AbsoluteUntil] IS NULL OR DATEPART(TZOFFSET,[AbsoluteUntil])=0),
    CONSTRAINT [CK_Session_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Session_Utc_IdleUntil] CHECK ([IdleUntil] IS NULL OR DATEPART(TZOFFSET,[IdleUntil])=0),
    CONSTRAINT [CK_Session_Utc_MfaAt] CHECK ([MfaAt] IS NULL OR DATEPART(TZOFFSET,[MfaAt])=0),
    CONSTRAINT [CK_Session_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Session_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_Session_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [catalog].[Scope] (
    [Id] uniqueidentifier NOT NULL,
    [Kind] nvarchar(256) NOT NULL,
    [CorporationId] uniqueidentifier NULL,
    [ManagementUnitId] uniqueidentifier NULL,
    [StationId] uniqueidentifier NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Scope] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Scope_Target] CHECK (([Kind]=N''Corporation'' AND [CorporationId] IS NOT NULL AND [ManagementUnitId] IS NULL AND [StationId] IS NULL) OR ([Kind]=N''ManagementUnit'' AND [CorporationId] IS NULL AND [ManagementUnitId] IS NOT NULL AND [StationId] IS NULL) OR ([Kind]=N''Station'' AND [CorporationId] IS NULL AND [ManagementUnitId] IS NULL AND [StationId] IS NOT NULL)),
    CONSTRAINT [CK_Scope_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Scope_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_Scope_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_Scope_Corporation_CorporationId] FOREIGN KEY ([CorporationId]) REFERENCES [catalog].[Corporation] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Scope_ManagementUnit_ManagementUnitId] FOREIGN KEY ([ManagementUnitId]) REFERENCES [catalog].[ManagementUnit] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Scope_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[RoleChangeRequest] (
    [Id] uniqueidentifier NOT NULL,
    [RequesterUserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NULL,
    [Revision] bigint NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [Digest] nchar(64) NOT NULL,
    [IdempotencyKey] nvarchar(256) NOT NULL,
    [BaseRoleVersion] bigint NOT NULL,
    [PolicyVersion] bigint NOT NULL,
    [CatalogVersion] bigint NOT NULL,
    [ExpiresAt] datetimeoffset NOT NULL,
    [ApprovalStatus] nvarchar(256) NOT NULL,
    [EffectiveStatus] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_RoleChangeRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_RoleChangeRequest_Id_Revision] UNIQUE ([Id], [Revision]),
    CONSTRAINT [CK_RoleChangeRequest_Json] CHECK (ISJSON([PayloadJson])=1),
    CONSTRAINT [CK_RoleChangeRequest_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RoleChangeRequest_Utc_ExpiresAt] CHECK ([ExpiresAt] IS NULL OR DATEPART(TZOFFSET,[ExpiresAt])=0),
    CONSTRAINT [CK_RoleChangeRequest_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RoleChangeRequest_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_RoleChangeRequest_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [iam].[Role] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RoleChangeRequest_User_RequesterUserId] FOREIGN KEY ([RequesterUserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[RolePermission] (
    [Id] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_RolePermission] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RolePermission_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RolePermission_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RolePermission_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_RolePermission_Permission_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [iam].[Permission] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RolePermission_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [iam].[Role] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[UserRole] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [Active] bit NOT NULL,
    [ValidFrom] datetimeoffset NOT NULL,
    [ValidUntil] datetimeoffset NOT NULL,
    [ApprovalReference] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_UserRole] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_UserRole_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_UserRole_Utc_ValidFrom] CHECK ([ValidFrom] IS NULL OR DATEPART(TZOFFSET,[ValidFrom])=0),
    CONSTRAINT [CK_UserRole_Utc_ValidUntil] CHECK ([ValidUntil] IS NULL OR DATEPART(TZOFFSET,[ValidUntil])=0),
    CONSTRAINT [CK_UserRole_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_UserRole_Validity] CHECK ([ValidUntil] > [ValidFrom]),
    CONSTRAINT [CK_UserRole_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_UserRole_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [iam].[Role] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserRole_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [ops].[AuditOutbox] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [ArchivedAt] datetimeoffset NULL,
    [Fence] bigint NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_AuditOutbox] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AuditOutbox_Utc_ArchivedAt] CHECK ([ArchivedAt] IS NULL OR DATEPART(TZOFFSET,[ArchivedAt])=0),
    CONSTRAINT [CK_AuditOutbox_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_AuditOutbox_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_AuditOutbox_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_AuditOutbox_SecurityEvent_EventId] FOREIGN KEY ([EventId]) REFERENCES [audit].[SecurityEvent] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[RoleChangeApproval] (
    [Id] uniqueidentifier NOT NULL,
    [RequestId] uniqueidentifier NOT NULL,
    [Revision] bigint NOT NULL,
    [ApproverUserId] uniqueidentifier NOT NULL,
    [ApproverPersonReferenceId] uniqueidentifier NOT NULL,
    [RequestDigest] nvarchar(256) NOT NULL,
    [Decision] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_RoleChangeApproval] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RoleChangeApproval_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RoleChangeApproval_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RoleChangeApproval_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_RoleChangeApproval_PersonReference_ApproverPersonReferenceId] FOREIGN KEY ([ApproverPersonReferenceId]) REFERENCES [iam].[PersonReference] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RoleChangeApproval_RoleChangeRequest_RequestId_Revision] FOREIGN KEY ([RequestId], [Revision]) REFERENCES [iam].[RoleChangeRequest] ([Id], [Revision]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RoleChangeApproval_User_ApproverUserId] FOREIGN KEY ([ApproverUserId]) REFERENCES [iam].[User] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RoleChangeApproval_User_ApproverUserId_ApproverPersonReferenceId] FOREIGN KEY ([ApproverUserId], [ApproverPersonReferenceId]) REFERENCES [iam].[User] ([Id], [PersonReferenceId]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [ops].[RoleHistory] (
    [Id] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [RequestId] uniqueidentifier NOT NULL,
    [Revision] bigint NOT NULL,
    [Digest] nvarchar(256) NOT NULL,
    [StateJson] nvarchar(max) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_RoleHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RoleHistory_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RoleHistory_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_RoleHistory_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_RoleHistory_RoleChangeRequest_RequestId_Revision] FOREIGN KEY ([RequestId], [Revision]) REFERENCES [iam].[RoleChangeRequest] ([Id], [Revision]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RoleHistory_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [iam].[Role] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[AdministrativeDelegation] (
    [Id] uniqueidentifier NOT NULL,
    [UserRoleId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
    [TargetPopulation] nvarchar(256) NOT NULL,
    [Active] bit NOT NULL,
    [ValidFrom] datetimeoffset NOT NULL,
    [ValidUntil] datetimeoffset NOT NULL,
    [ApprovalReference] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_AdministrativeDelegation] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AdministrativeDelegation_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_AdministrativeDelegation_Utc_ValidFrom] CHECK ([ValidFrom] IS NULL OR DATEPART(TZOFFSET,[ValidFrom])=0),
    CONSTRAINT [CK_AdministrativeDelegation_Utc_ValidUntil] CHECK ([ValidUntil] IS NULL OR DATEPART(TZOFFSET,[ValidUntil])=0),
    CONSTRAINT [CK_AdministrativeDelegation_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_AdministrativeDelegation_Validity] CHECK ([ValidUntil] > [ValidFrom]),
    CONSTRAINT [CK_AdministrativeDelegation_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_AdministrativeDelegation_Permission_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [iam].[Permission] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AdministrativeDelegation_UserRole_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [iam].[UserRole] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[StationGrant] (
    [Id] uniqueidentifier NOT NULL,
    [UserRoleId] uniqueidentifier NOT NULL,
    [ScopeId] uniqueidentifier NOT NULL,
    [Active] bit NOT NULL,
    [ValidFrom] datetimeoffset NOT NULL,
    [ValidUntil] datetimeoffset NOT NULL,
    [GrantVersion] bigint NOT NULL,
    [HierarchyVersion] bigint NOT NULL,
    [ApprovalReference] nvarchar(256) NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StationGrant] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StationGrant_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_StationGrant_Utc_ValidFrom] CHECK ([ValidFrom] IS NULL OR DATEPART(TZOFFSET,[ValidFrom])=0),
    CONSTRAINT [CK_StationGrant_Utc_ValidUntil] CHECK ([ValidUntil] IS NULL OR DATEPART(TZOFFSET,[ValidUntil])=0),
    CONSTRAINT [CK_StationGrant_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_StationGrant_Validity] CHECK ([ValidUntil] > [ValidFrom]),
    CONSTRAINT [CK_StationGrant_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_StationGrant_Scope_ScopeId] FOREIGN KEY ([ScopeId]) REFERENCES [catalog].[Scope] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StationGrant_UserRole_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [iam].[UserRole] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE TABLE [iam].[GrantFunction] (
    [Id] uniqueidentifier NOT NULL,
    [StationGrantId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
    [Version] bigint NOT NULL,
    [CreatedAt] datetimeoffset(7) NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_GrantFunction] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_GrantFunction_Utc_CreatedAt] CHECK ([CreatedAt] IS NULL OR DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_GrantFunction_UtcCreated] CHECK (DATEPART(TZOFFSET,[CreatedAt])=0),
    CONSTRAINT [CK_GrantFunction_Version] CHECK ([Version] > 0),
    CONSTRAINT [FK_GrantFunction_Permission_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [iam].[Permission] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GrantFunction_StationGrant_StationGrantId] FOREIGN KEY ([StationGrantId]) REFERENCES [iam].[StationGrant] ([Id]) ON DELETE NO ACTION
);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_AdministrativeDelegation_PermissionId] ON [iam].[AdministrativeDelegation] ([PermissionId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_AdministrativeDelegation_UserRoleId] ON [iam].[AdministrativeDelegation] ([UserRoleId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_AuditOutbox_ArchivedAt_CreatedAt] ON [ops].[AuditOutbox] ([ArchivedAt], [CreatedAt]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_AuditOutbox_EventId] ON [ops].[AuditOutbox] ([EventId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_AuthorityVersion_AuthorityKey] ON [iam].[AuthorityVersion] ([AuthorityKey]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_Challenge_ProofDigest] ON [iam].[Challenge] ([ProofDigest]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Challenge_UserId] ON [iam].[Challenge] ([UserId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_GrantFunction_PermissionId] ON [iam].[GrantFunction] ([PermissionId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_GrantFunction_StationGrantId_PermissionId] ON [iam].[GrantFunction] ([StationGrantId], [PermissionId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_ManagementUnit_CorporationId] ON [catalog].[ManagementUnit] ([CorporationId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_ManagementUnit_ParentUnitId] ON [catalog].[ManagementUnit] ([ParentUnitId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_MfaCredential_CredentialReference] ON [iam].[MfaCredential] ([CredentialReference]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_MfaCredential_UserId] ON [iam].[MfaCredential] ([UserId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_PasswordCredential_UserId] ON [iam].[PasswordCredential] ([UserId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_Permission_PermissionCode] ON [iam].[Permission] ([PermissionCode]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_PersonReference_PersonKey] ON [iam].[PersonReference] ([PersonKey]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Role_CreatedBy] ON [iam].[Role] ([CreatedBy]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_Role_RoleCode] ON [iam].[Role] ([RoleCode]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Role_UpdatedBy] ON [iam].[Role] ([UpdatedBy]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_RoleChangeApproval_ApproverPersonReferenceId] ON [iam].[RoleChangeApproval] ([ApproverPersonReferenceId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_RoleChangeApproval_ApproverUserId_ApproverPersonReferenceId] ON [iam].[RoleChangeApproval] ([ApproverUserId], [ApproverPersonReferenceId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_RoleChangeApproval_RequestId_Revision_ApproverPersonReferenceId] ON [iam].[RoleChangeApproval] ([RequestId], [Revision], [ApproverPersonReferenceId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_RoleChangeRequest_RequesterUserId_IdempotencyKey] ON [iam].[RoleChangeRequest] ([RequesterUserId], [IdempotencyKey]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_RoleChangeRequest_RoleId] ON [iam].[RoleChangeRequest] ([RoleId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_RoleHistory_RequestId_Revision] ON [ops].[RoleHistory] ([RequestId], [Revision]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_RoleHistory_RoleId_Version] ON [ops].[RoleHistory] ([RoleId], [Version]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_RolePermission_PermissionId] ON [iam].[RolePermission] ([PermissionId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_RolePermission_RoleId_PermissionId] ON [iam].[RolePermission] ([RoleId], [PermissionId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Scope_CorporationId] ON [catalog].[Scope] ([CorporationId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Scope_ManagementUnitId] ON [catalog].[Scope] ([ManagementUnitId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Scope_StationId] ON [catalog].[Scope] ([StationId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_SecurityEvent_ActorUserId] ON [audit].[SecurityEvent] ([ActorUserId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_SeedInstallation_ManifestId] ON [ops].[SeedInstallation] ([ManifestId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_Session_HandleDigest] ON [iam].[Session] ([HandleDigest]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Session_UserId_Revoked_AbsoluteUntil] ON [iam].[Session] ([UserId], [Revoked], [AbsoluteUntil]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_Station_ManagementUnitId] ON [catalog].[Station] ([ManagementUnitId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_StationGrant_ScopeId] ON [iam].[StationGrant] ([ScopeId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_StationGrant_UserRoleId_Active_ValidUntil] ON [iam].[StationGrant] ([UserRoleId], [Active], [ValidUntil]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_User_NormalizedUsername] ON [iam].[User] ([NormalizedUsername]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE UNIQUE INDEX [IX_User_PersonReferenceId] ON [iam].[User] ([PersonReferenceId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_UserRole_RoleId] ON [iam].[UserRole] ([RoleId]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE INDEX [IX_UserRole_UserId_Active_ValidUntil] ON [iam].[UserRole] ([UserId], [Active], [ValidUntil]);
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE OR ALTER TRIGGER [iam].[TR_Role_ImmutableCode] ON [iam].[Role] AFTER UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.Id=d.Id WHERE i.RoleCode COLLATE Latin1_General_100_BIN2 <> d.RoleCode COLLATE Latin1_General_100_BIN2)
  THROW 51001, ''RoleCode is immutable.'', 1;
END;
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE OR ALTER TRIGGER [iam].[TR_Role_NoDelete] ON [iam].[Role] INSTEAD OF DELETE AS
BEGIN
 THROW 51002, ''Deactivate roles; preserve history.'', 1;
END;
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE OR ALTER TRIGGER [audit].[TR_SecurityEvent_AppendOnly] ON [audit].[SecurityEvent] INSTEAD OF UPDATE, DELETE AS
BEGIN
 THROW 51003, ''Security events are append-only.'', 1;
END;
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
CREATE OR ALTER TRIGGER [iam].[TR_RoleChangeRequest_ImmutableRevision] ON [iam].[RoleChangeRequest] AFTER UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.Id=d.Id
   WHERE i.Revision<>d.Revision OR i.RequesterUserId<>d.RequesterUserId OR i.Digest<>d.Digest
      OR i.PayloadJson COLLATE Latin1_General_100_BIN2 <> d.PayloadJson COLLATE Latin1_General_100_BIN2
      OR i.BaseRoleVersion<>d.BaseRoleVersion OR i.PolicyVersion<>d.PolicyVersion OR i.CatalogVersion<>d.CatalogVersion
      OR i.ExpiresAt<>d.ExpiresAt OR i.IdempotencyKey<>d.IdempotencyKey
      OR ISNULL(CONVERT(nvarchar(36),i.RoleId),N'''')<>ISNULL(CONVERT(nvarchar(36),d.RoleId),N''''))
  THROW 51004, ''Create a new immutable request revision.'', 1;
END;
');

GO
-- REVIEW CANDIDATE. Requires owner D09/schema disposition and confirmed disposable lab authorization.
-- Session context must be set explicitly by the authorized migration operator, never by application startup.
IF COALESCE(TRY_CONVERT(bit, SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0) <> 1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'') <> DB_NAME()
 THROW 51000, 'Confirmed disposable lab and exact expected database required.', 1;

EXEC(N'
-- No logins, users, default roles/permissions, sessions or station grants are installed.
IF DATABASE_PRINCIPAL_ID(N''fdc_iam_catalog_reader'') IS NULL CREATE ROLE [fdc_iam_catalog_reader];
GRANT SELECT ON OBJECT::[iam].[Role] TO [fdc_iam_catalog_reader];
GRANT SELECT ON OBJECT::[iam].[Permission] TO [fdc_iam_catalog_reader];
DENY INSERT,UPDATE,DELETE ON SCHEMA::[iam] TO [fdc_iam_catalog_reader];
DENY SELECT ON OBJECT::[iam].[PasswordCredential] TO [fdc_iam_catalog_reader];
DENY SELECT ON OBJECT::[iam].[MfaCredential] TO [fdc_iam_catalog_reader];
DENY SELECT ON OBJECT::[iam].[Session] TO [fdc_iam_catalog_reader];
');
