IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'IF SCHEMA_ID(N''ops'') IS NULL EXEC(N''CREATE SCHEMA [ops];'');
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
IF SCHEMA_ID(N''audit'') IS NULL EXEC(N''CREATE SCHEMA [audit];'');
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
IF SCHEMA_ID(N''catalog'') IS NULL EXEC(N''CREATE SCHEMA [catalog];'');
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
IF SCHEMA_ID(N''ingest'') IS NULL EXEC(N''CREATE SCHEMA [ingest];'');
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[AnalysisAlgorithm] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(200) NOT NULL,
    [Version] nvarchar(200) NOT NULL,
    [Implemented] bit NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_AnalysisAlgorithm] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AnalysisAlgorithm_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_AnalysisAlgorithm_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [CK_AnalysisAlgorithm_Version_Required] CHECK (LEN([Version])>0)
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [audit].[AuditCheckpoint] (
    [Id] uniqueidentifier NOT NULL,
    [ArchiveReference] nvarchar(200) NOT NULL,
    [ContentSha256] nvarchar(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [Status] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_AuditCheckpoint] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AuditCheckpoint_ArchiveReference_Required] CHECK (LEN([ArchiveReference])>0),
    CONSTRAINT [CK_AuditCheckpoint_ContentSha256] CHECK (LEN([ContentSha256])=64 AND [ContentSha256] NOT LIKE ''%[^0-9a-f]%''),
    CONSTRAINT [CK_AuditCheckpoint_ContentSha256_Required] CHECK (LEN([ContentSha256])>0),
    CONSTRAINT [CK_AuditCheckpoint_Status] CHECK ([Status] IN (''Unverified'',''Verified'',''Rejected'')),
    CONSTRAINT [CK_AuditCheckpoint_Status_Required] CHECK (LEN([Status])>0),
    CONSTRAINT [CK_AuditCheckpoint_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0))
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [audit].[AuditDelivery] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [Attempt] int NOT NULL,
    [State] nvarchar(200) NOT NULL,
    [ArchiveReference] nvarchar(200) NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_AuditDelivery] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AuditDelivery_Attempt] CHECK ([Attempt]>0 AND [State] IN (''Pending'',''Delivered'',''Failed'')),
    CONSTRAINT [CK_AuditDelivery_State_Required] CHECK (LEN([State])>0),
    CONSTRAINT [CK_AuditDelivery_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_AuditDelivery_SecurityEvent_EventId] FOREIGN KEY ([EventId]) REFERENCES [audit].[SecurityEvent] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[Bay] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [Code] nvarchar(200) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_Bay] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Bay_Id_StationId] UNIQUE ([Id], [StationId]),
    CONSTRAINT [CK_Bay_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_Bay_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_Bay_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[CollectorRegistration] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [Code] nvarchar(200) NOT NULL,
    [Active] bit NOT NULL,
    [ConfigurationVersion] bigint NOT NULL,
    [BindingStatus] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_CollectorRegistration] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_CollectorRegistration_Id_StationId] UNIQUE ([Id], [StationId]),
    CONSTRAINT [CK_CollectorRegistration_BindingStatus_Required] CHECK (LEN([BindingStatus])>0),
    CONSTRAINT [CK_CollectorRegistration_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_CollectorRegistration_Config] CHECK ([ConfigurationVersion]>0 AND [BindingStatus] IN (''Pending'',''Bound'',''Revoked'')),
    CONSTRAINT [CK_CollectorRegistration_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_CollectorRegistration_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[CommunicationProfile] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(200) NOT NULL,
    [Protocol] nvarchar(200) NOT NULL,
    [Port] int NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_CommunicationProfile] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CommunicationProfile_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_CommunicationProfile_Protocol] CHECK ([Protocol] IN (''IEC61850'',''SFTP'',''HTTPS'') AND [Port] BETWEEN 1 AND 65535),
    CONSTRAINT [CK_CommunicationProfile_Protocol_Required] CHECK (LEN([Protocol])>0),
    CONSTRAINT [CK_CommunicationProfile_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0))
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[FaultEvent] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [EventAtUtc] datetimeoffset NOT NULL,
    [AnalysisStatus] nvarchar(200) NOT NULL,
    [SourceTimestampRaw] nvarchar(200) NULL,
    [TimeSource] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_FaultEvent] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_FaultEvent_Id_StationId] UNIQUE ([Id], [StationId]),
    CONSTRAINT [CK_FaultEvent_AnalysisState] CHECK ([AnalysisStatus] IN (''NotImplemented'',''Pending'',''Completed'',''Failed'')),
    CONSTRAINT [CK_FaultEvent_AnalysisStatus_Required] CHECK (LEN([AnalysisStatus])>0),
    CONSTRAINT [CK_FaultEvent_TimeSource_Required] CHECK (LEN([TimeSource])>0),
    CONSTRAINT [CK_FaultEvent_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_FaultEvent_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[Feeder] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [Code] nvarchar(200) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_Feeder] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Feeder_Id_StationId] UNIQUE ([Id], [StationId]),
    CONSTRAINT [CK_Feeder_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_Feeder_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_Feeder_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[Manufacturer] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(200) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_Manufacturer] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Manufacturer_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_Manufacturer_Name_Required] CHECK (LEN([Name])>0),
    CONSTRAINT [CK_Manufacturer_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0))
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[StationUnitHistory] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [ManagementUnitId] uniqueidentifier NOT NULL,
    [ValidFromUtc] datetimeoffset NOT NULL,
    [ValidUntilUtc] datetimeoffset NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_StationUnitHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StationUnitHistory_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [CK_StationUnitHistory_Validity] CHECK ([ValidUntilUtc] IS NULL OR [ValidUntilUtc]>[ValidFromUtc]),
    CONSTRAINT [FK_StationUnitHistory_ManagementUnit_ManagementUnitId] FOREIGN KEY ([ManagementUnitId]) REFERENCES [catalog].[ManagementUnit] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StationUnitHistory_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[VoltageLevel] (
    [Id] uniqueidentifier NOT NULL,
    [Kilovolts] decimal(9,3) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_VoltageLevel] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_VoltageLevel_PositiveKv] CHECK ([Kilovolts]>0),
    CONSTRAINT [CK_VoltageLevel_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0))
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[CollectorHeartbeat] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [CollectorId] uniqueidentifier NOT NULL,
    [ReceivedAtUtc] datetimeoffset NOT NULL,
    [Status] nvarchar(200) NOT NULL,
    [QueueDepth] bigint NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_CollectorHeartbeat] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CollectorHeartbeat_Queue] CHECK ([QueueDepth]>=0 AND [Status] IN (''Healthy'',''Degraded'',''Offline'',''Unknown'')),
    CONSTRAINT [CK_CollectorHeartbeat_Status_Required] CHECK (LEN([Status])>0),
    CONSTRAINT [CK_CollectorHeartbeat_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_CollectorHeartbeat_CollectorRegistration_CollectorId_StationId] FOREIGN KEY ([CollectorId], [StationId]) REFERENCES [ops].[CollectorRegistration] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[OperationalEvent] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [CollectorId] uniqueidentifier NULL,
    [Code] nvarchar(200) NOT NULL,
    [CorrelationId] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_OperationalEvent] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OperationalEvent_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_OperationalEvent_CorrelationId_Required] CHECK (LEN([CorrelationId])>0),
    CONSTRAINT [CK_OperationalEvent_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_OperationalEvent_CollectorRegistration_CollectorId_StationId] FOREIGN KEY ([CollectorId], [StationId]) REFERENCES [ops].[CollectorRegistration] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[AnalysisJob] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [FaultEventId] uniqueidentifier NOT NULL,
    [AlgorithmId] uniqueidentifier NOT NULL,
    [State] nvarchar(200) NOT NULL,
    [CorrelationId] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_AnalysisJob] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AnalysisJob_CorrelationId_Required] CHECK (LEN([CorrelationId])>0),
    CONSTRAINT [CK_AnalysisJob_State] CHECK ([State] IN (''NotImplemented'',''Pending'',''Running'',''Completed'',''Failed'')),
    CONSTRAINT [CK_AnalysisJob_State_Required] CHECK (LEN([State])>0),
    CONSTRAINT [CK_AnalysisJob_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_AnalysisJob_AnalysisAlgorithm_AlgorithmId] FOREIGN KEY ([AlgorithmId]) REFERENCES [ops].[AnalysisAlgorithm] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AnalysisJob_FaultEvent_FaultEventId_StationId] FOREIGN KEY ([FaultEventId], [StationId]) REFERENCES [ops].[FaultEvent] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[DeviceModel] (
    [Id] uniqueidentifier NOT NULL,
    [ManufacturerId] uniqueidentifier NOT NULL,
    [Code] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_DeviceModel] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DeviceModel_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_DeviceModel_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_DeviceModel_Manufacturer_ManufacturerId] FOREIGN KEY ([ManufacturerId]) REFERENCES [catalog].[Manufacturer] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[StationProfile] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [VoltageLevelId] uniqueidentifier NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_StationProfile] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StationProfile_DisplayName_Required] CHECK (LEN([DisplayName])>0),
    CONSTRAINT [CK_StationProfile_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_StationProfile_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StationProfile_VoltageLevel_VoltageLevelId] FOREIGN KEY ([VoltageLevelId]) REFERENCES [catalog].[VoltageLevel] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[AnalysisResult] (
    [Id] uniqueidentifier NOT NULL,
    [JobId] uniqueidentifier NOT NULL,
    [Status] nvarchar(200) NOT NULL,
    [ResultJson] nvarchar(4000) NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_AnalysisResult] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AnalysisResult_Json] CHECK (([ResultJson] IS NULL OR ISJSON([ResultJson])=1) AND ([Status]<>''NotImplemented'' OR [ResultJson] IS NULL)),
    CONSTRAINT [CK_AnalysisResult_State] CHECK ([Status] IN (''NotImplemented'',''Completed'',''Failed'')),
    CONSTRAINT [CK_AnalysisResult_Status_Required] CHECK (LEN([Status])>0),
    CONSTRAINT [CK_AnalysisResult_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_AnalysisResult_AnalysisJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [ops].[AnalysisJob] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[DeviceFirmware] (
    [Id] uniqueidentifier NOT NULL,
    [DeviceModelId] uniqueidentifier NOT NULL,
    [Version] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_DeviceFirmware] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_DeviceFirmware_Id_DeviceModelId] UNIQUE ([Id], [DeviceModelId]),
    CONSTRAINT [CK_DeviceFirmware_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [CK_DeviceFirmware_Version_Required] CHECK (LEN([Version])>0),
    CONSTRAINT [FK_DeviceFirmware_DeviceModel_DeviceModelId] FOREIGN KEY ([DeviceModelId]) REFERENCES [catalog].[DeviceModel] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [catalog].[IedDevice] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [BayId] uniqueidentifier NULL,
    [FeederId] uniqueidentifier NULL,
    [DeviceModelId] uniqueidentifier NULL,
    [FirmwareId] uniqueidentifier NULL,
    [CommunicationProfileId] uniqueidentifier NULL,
    [Code] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_IedDevice] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_IedDevice_Id_StationId] UNIQUE ([Id], [StationId]),
    CONSTRAINT [CK_IedDevice_Code_Required] CHECK (LEN([Code])>0),
    CONSTRAINT [CK_IedDevice_DisplayName_Required] CHECK (LEN([DisplayName])>0),
    CONSTRAINT [CK_IedDevice_FirmwareModel] CHECK ([FirmwareId] IS NULL OR [DeviceModelId] IS NOT NULL),
    CONSTRAINT [CK_IedDevice_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_IedDevice_Bay_BayId_StationId] FOREIGN KEY ([BayId], [StationId]) REFERENCES [catalog].[Bay] ([Id], [StationId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IedDevice_CommunicationProfile_CommunicationProfileId] FOREIGN KEY ([CommunicationProfileId]) REFERENCES [catalog].[CommunicationProfile] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IedDevice_DeviceFirmware_FirmwareId_DeviceModelId] FOREIGN KEY ([FirmwareId], [DeviceModelId]) REFERENCES [catalog].[DeviceFirmware] ([Id], [DeviceModelId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IedDevice_DeviceModel_DeviceModelId] FOREIGN KEY ([DeviceModelId]) REFERENCES [catalog].[DeviceModel] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IedDevice_Feeder_FeederId_StationId] FOREIGN KEY ([FeederId], [StationId]) REFERENCES [catalog].[Feeder] ([Id], [StationId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_IedDevice_Station_StationId] FOREIGN KEY ([StationId]) REFERENCES [catalog].[Station] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[CollectorIed] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [CollectorId] uniqueidentifier NOT NULL,
    [IedId] uniqueidentifier NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_CollectorIed] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_CollectorIed_CollectorId_IedId_StationId] UNIQUE ([CollectorId], [IedId], [StationId]),
    CONSTRAINT [CK_CollectorIed_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_CollectorIed_CollectorRegistration_CollectorId_StationId] FOREIGN KEY ([CollectorId], [StationId]) REFERENCES [ops].[CollectorRegistration] ([Id], [StationId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CollectorIed_IedDevice_IedId_StationId] FOREIGN KEY ([IedId], [StationId]) REFERENCES [catalog].[IedDevice] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[CollectionJob] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [CollectorId] uniqueidentifier NOT NULL,
    [IedId] uniqueidentifier NOT NULL,
    [State] nvarchar(200) NOT NULL,
    [NextRunAtUtc] datetimeoffset NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_CollectionJob] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CollectionJob_State] CHECK ([State] IN (''Pending'',''Running'',''Completed'',''Failed'',''Cancelled'')),
    CONSTRAINT [CK_CollectionJob_State_Required] CHECK (LEN([State])>0),
    CONSTRAINT [CK_CollectionJob_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_CollectionJob_CollectorIed_CollectorId_IedId_StationId] FOREIGN KEY ([CollectorId], [IedId], [StationId]) REFERENCES [ops].[CollectorIed] ([CollectorId], [IedId], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ingest].[DisturbanceRecord] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [IedId] uniqueidentifier NOT NULL,
    [CollectorId] uniqueidentifier NOT NULL,
    [ManifestId] nvarchar(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [ManifestSha256] nvarchar(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [RemoteIdentity] nvarchar(512) NOT NULL,
    [CollectedAtUtc] datetimeoffset NOT NULL,
    [EventAtUtc] datetimeoffset NULL,
    [SourceTimestampRaw] nvarchar(200) NULL,
    [TimeSource] nvarchar(200) NOT NULL,
    [State] nvarchar(200) NOT NULL,
    [CorrelationId] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_DisturbanceRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_DisturbanceRecord_Id_StationId] UNIQUE ([Id], [StationId]),
    CONSTRAINT [CK_DisturbanceRecord_CorrelationId_Required] CHECK (LEN([CorrelationId])>0),
    CONSTRAINT [CK_DisturbanceRecord_ManifestId] CHECK (LEN([ManifestId])=64 AND [ManifestId] NOT LIKE ''%[^0-9a-f]%''),
    CONSTRAINT [CK_DisturbanceRecord_ManifestId_Required] CHECK (LEN([ManifestId])>0),
    CONSTRAINT [CK_DisturbanceRecord_ManifestSha256] CHECK (LEN([ManifestSha256])=64 AND [ManifestSha256] NOT LIKE ''%[^0-9a-f]%''),
    CONSTRAINT [CK_DisturbanceRecord_ManifestSha256_Required] CHECK (LEN([ManifestSha256])>0),
    CONSTRAINT [CK_DisturbanceRecord_RemoteIdentity_Required] CHECK (LEN([RemoteIdentity])>0),
    CONSTRAINT [CK_DisturbanceRecord_State] CHECK ([State] IN (''MetadataAccepted'',''Validated'',''Quarantined'',''Processing'',''Processed'',''Failed'',''Retained'')),
    CONSTRAINT [CK_DisturbanceRecord_State_Required] CHECK (LEN([State])>0),
    CONSTRAINT [CK_DisturbanceRecord_TimeSource_Required] CHECK (LEN([TimeSource])>0),
    CONSTRAINT [CK_DisturbanceRecord_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_DisturbanceRecord_CollectorIed_CollectorId_IedId_StationId] FOREIGN KEY ([CollectorId], [IedId], [StationId]) REFERENCES [ops].[CollectorIed] ([CollectorId], [IedId], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[CollectionAttempt] (
    [Id] uniqueidentifier NOT NULL,
    [JobId] uniqueidentifier NOT NULL,
    [Number] int NOT NULL,
    [Outcome] nvarchar(200) NOT NULL,
    [CorrelationId] nvarchar(200) NOT NULL,
    [ErrorCode] nvarchar(200) NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_CollectionAttempt] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CollectionAttempt_Attempt] CHECK ([Number]>0 AND [Outcome] IN (''Succeeded'',''Failed'',''Cancelled'')),
    CONSTRAINT [CK_CollectionAttempt_CorrelationId_Required] CHECK (LEN([CorrelationId])>0),
    CONSTRAINT [CK_CollectionAttempt_Outcome_Required] CHECK (LEN([Outcome])>0),
    CONSTRAINT [CK_CollectionAttempt_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_CollectionAttempt_CollectionJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [ops].[CollectionJob] ([Id]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[FaultEventRecord] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [FaultEventId] uniqueidentifier NOT NULL,
    [RecordId] uniqueidentifier NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_FaultEventRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_FaultEventRecord_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_FaultEventRecord_DisturbanceRecord_RecordId_StationId] FOREIGN KEY ([RecordId], [StationId]) REFERENCES [ingest].[DisturbanceRecord] ([Id], [StationId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FaultEventRecord_FaultEvent_FaultEventId_StationId] FOREIGN KEY ([FaultEventId], [StationId]) REFERENCES [ops].[FaultEvent] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ingest].[IngestionAttempt] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [RecordId] uniqueidentifier NOT NULL,
    [Number] int NOT NULL,
    [Outcome] nvarchar(200) NOT NULL,
    [CorrelationId] nvarchar(200) NOT NULL,
    [ErrorCode] nvarchar(200) NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_IngestionAttempt] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_IngestionAttempt_Attempt] CHECK ([Number]>0 AND [Outcome] IN (''Accepted'',''Duplicate'',''Rejected'',''Failed'')),
    CONSTRAINT [CK_IngestionAttempt_CorrelationId_Required] CHECK (LEN([CorrelationId])>0),
    CONSTRAINT [CK_IngestionAttempt_Outcome_Required] CHECK (LEN([Outcome])>0),
    CONSTRAINT [CK_IngestionAttempt_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_IngestionAttempt_DisturbanceRecord_RecordId_StationId] FOREIGN KEY ([RecordId], [StationId]) REFERENCES [ingest].[DisturbanceRecord] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ops].[ProcessingOutbox] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [RecordId] uniqueidentifier NOT NULL,
    [Kind] nvarchar(200) NOT NULL,
    [State] nvarchar(200) NOT NULL,
    [Attempts] int NOT NULL,
    [AvailableAtUtc] datetimeoffset NOT NULL,
    [LeaseUntilUtc] datetimeoffset NULL,
    [LeaseOwner] nvarchar(200) NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_ProcessingOutbox] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ProcessingOutbox_Kind_Required] CHECK (LEN([Kind])>0),
    CONSTRAINT [CK_ProcessingOutbox_Queue] CHECK ([Attempts]>=0 AND [State] IN (''Pending'',''Leased'',''Completed'',''Failed'') AND (([LeaseUntilUtc] IS NULL AND [LeaseOwner] IS NULL) OR ([LeaseUntilUtc] IS NOT NULL AND [LeaseOwner] IS NOT NULL))),
    CONSTRAINT [CK_ProcessingOutbox_State_Required] CHECK (LEN([State])>0),
    CONSTRAINT [CK_ProcessingOutbox_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_ProcessingOutbox_DisturbanceRecord_RecordId_StationId] FOREIGN KEY ([RecordId], [StationId]) REFERENCES [ingest].[DisturbanceRecord] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ingest].[RecordFile] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [RecordId] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) COLLATE Latin1_General_100_CI_AS NOT NULL,
    [Extension] nvarchar(200) NOT NULL,
    [SizeBytes] bigint NOT NULL,
    [Sha256] nvarchar(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [StorageObjectKey] nvarchar(512) NOT NULL,
    [ValidationStatus] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_RecordFile] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RecordFile_Extension_Required] CHECK (LEN([Extension])>0),
    CONSTRAINT [CK_RecordFile_File] CHECK ([SizeBytes]>=0 AND [Extension] IN (''.cfg'',''.dat'',''.hdr'',''.inf'') AND [ValidationStatus] IN (''Pending'',''Verified'',''Rejected'') AND [Name] NOT LIKE ''%/%'' AND [Name] NOT LIKE ''%\%'' AND [Name] NOT LIKE ''%:%''),
    CONSTRAINT [CK_RecordFile_Name_Required] CHECK (LEN([Name])>0),
    CONSTRAINT [CK_RecordFile_ObjectKey] CHECK ([StorageObjectKey] NOT LIKE ''%..%'' AND [StorageObjectKey] NOT LIKE ''%:%'' AND LEFT([StorageObjectKey],1)<>''/'' AND [StorageObjectKey] NOT LIKE ''%\%''),
    CONSTRAINT [CK_RecordFile_Sha256] CHECK (LEN([Sha256])=64 AND [Sha256] NOT LIKE ''%[^0-9a-f]%''),
    CONSTRAINT [CK_RecordFile_Sha256_Required] CHECK (LEN([Sha256])>0),
    CONSTRAINT [CK_RecordFile_StorageObjectKey_Required] CHECK (LEN([StorageObjectKey])>0),
    CONSTRAINT [CK_RecordFile_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [CK_RecordFile_ValidationStatus_Required] CHECK (LEN([ValidationStatus])>0),
    CONSTRAINT [FK_RecordFile_DisturbanceRecord_RecordId_StationId] FOREIGN KEY ([RecordId], [StationId]) REFERENCES [ingest].[DisturbanceRecord] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE TABLE [ingest].[RecordStatusHistory] (
    [Id] uniqueidentifier NOT NULL,
    [StationId] uniqueidentifier NOT NULL,
    [RecordId] uniqueidentifier NOT NULL,
    [State] nvarchar(200) NOT NULL,
    [CorrelationId] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NULL,
    [RowVersion] rowversion NOT NULL,
    [IsSynthetic] bit NOT NULL,
    CONSTRAINT [PK_RecordStatusHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RecordStatusHistory_CorrelationId_Required] CHECK (LEN([CorrelationId])>0),
    CONSTRAINT [CK_RecordStatusHistory_State_Required] CHECK (LEN([State])>0),
    CONSTRAINT [CK_RecordStatusHistory_Utc] CHECK (DATEPART(TZOFFSET,[CreatedAtUtc])=0 AND ([UpdatedAtUtc] IS NULL OR DATEPART(TZOFFSET,[UpdatedAtUtc])=0)),
    CONSTRAINT [FK_RecordStatusHistory_DisturbanceRecord_RecordId_StationId] FOREIGN KEY ([RecordId], [StationId]) REFERENCES [ingest].[DisturbanceRecord] ([Id], [StationId]) ON DELETE NO ACTION
);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_AnalysisAlgorithm_Code_Version] ON [ops].[AnalysisAlgorithm] ([Code], [Version]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_AnalysisJob_AlgorithmId] ON [ops].[AnalysisJob] ([AlgorithmId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_AnalysisJob_FaultEventId_StationId] ON [ops].[AnalysisJob] ([FaultEventId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_AnalysisResult_JobId] ON [ops].[AnalysisResult] ([JobId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_AuditDelivery_EventId_Attempt] ON [audit].[AuditDelivery] ([EventId], [Attempt]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_Bay_StationId_Code] ON [catalog].[Bay] ([StationId], [Code]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_CollectionAttempt_JobId_Number] ON [ops].[CollectionAttempt] ([JobId], [Number]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_CollectionJob_CollectorId_IedId_StationId] ON [ops].[CollectionJob] ([CollectorId], [IedId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_CollectionJob_State_NextRunAtUtc] ON [ops].[CollectionJob] ([State], [NextRunAtUtc]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_CollectorHeartbeat_CollectorId_ReceivedAtUtc] ON [ops].[CollectorHeartbeat] ([CollectorId], [ReceivedAtUtc]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_CollectorHeartbeat_CollectorId_StationId] ON [ops].[CollectorHeartbeat] ([CollectorId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_CollectorIed_CollectorId_StationId] ON [ops].[CollectorIed] ([CollectorId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_CollectorIed_IedId_StationId] ON [ops].[CollectorIed] ([IedId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_CollectorRegistration_Code] ON [ops].[CollectorRegistration] ([Code]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_CollectorRegistration_StationId] ON [ops].[CollectorRegistration] ([StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_CommunicationProfile_Code] ON [catalog].[CommunicationProfile] ([Code]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_DeviceFirmware_DeviceModelId_Version] ON [catalog].[DeviceFirmware] ([DeviceModelId], [Version]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_DeviceModel_ManufacturerId_Code] ON [catalog].[DeviceModel] ([ManufacturerId], [Code]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_DisturbanceRecord_CollectorId_IedId_StationId] ON [ingest].[DisturbanceRecord] ([CollectorId], [IedId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_DisturbanceRecord_StationId_EventAtUtc_Id] ON [ingest].[DisturbanceRecord] ([StationId], [EventAtUtc], [Id]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_DisturbanceRecord_StationId_IedId_ManifestId] ON [ingest].[DisturbanceRecord] ([StationId], [IedId], [ManifestId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_DisturbanceRecord_StationId_State_CreatedAtUtc] ON [ingest].[DisturbanceRecord] ([StationId], [State], [CreatedAtUtc]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_FaultEvent_StationId_EventAtUtc_Id] ON [ops].[FaultEvent] ([StationId], [EventAtUtc], [Id]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_FaultEventRecord_FaultEventId_RecordId] ON [ops].[FaultEventRecord] ([FaultEventId], [RecordId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_FaultEventRecord_FaultEventId_StationId] ON [ops].[FaultEventRecord] ([FaultEventId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_FaultEventRecord_RecordId_StationId] ON [ops].[FaultEventRecord] ([RecordId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_Feeder_StationId_Code] ON [catalog].[Feeder] ([StationId], [Code]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_IedDevice_BayId_StationId] ON [catalog].[IedDevice] ([BayId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_IedDevice_CommunicationProfileId] ON [catalog].[IedDevice] ([CommunicationProfileId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_IedDevice_DeviceModelId] ON [catalog].[IedDevice] ([DeviceModelId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_IedDevice_FeederId_StationId] ON [catalog].[IedDevice] ([FeederId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_IedDevice_FirmwareId_DeviceModelId] ON [catalog].[IedDevice] ([FirmwareId], [DeviceModelId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_IedDevice_StationId_Code] ON [catalog].[IedDevice] ([StationId], [Code]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_IngestionAttempt_RecordId_Number] ON [ingest].[IngestionAttempt] ([RecordId], [Number]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_IngestionAttempt_RecordId_StationId] ON [ingest].[IngestionAttempt] ([RecordId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_Manufacturer_Code] ON [catalog].[Manufacturer] ([Code]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_OperationalEvent_CollectorId_StationId] ON [ops].[OperationalEvent] ([CollectorId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_ProcessingOutbox_RecordId_Kind] ON [ops].[ProcessingOutbox] ([RecordId], [Kind]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_ProcessingOutbox_RecordId_StationId] ON [ops].[ProcessingOutbox] ([RecordId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_ProcessingOutbox_State_AvailableAtUtc] ON [ops].[ProcessingOutbox] ([State], [AvailableAtUtc]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_RecordFile_RecordId_Name] ON [ingest].[RecordFile] ([RecordId], [Name]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_RecordFile_RecordId_StationId] ON [ingest].[RecordFile] ([RecordId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_RecordFile_Sha256] ON [ingest].[RecordFile] ([Sha256]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_RecordStatusHistory_RecordId_CreatedAtUtc] ON [ingest].[RecordStatusHistory] ([RecordId], [CreatedAtUtc]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_RecordStatusHistory_RecordId_StationId] ON [ingest].[RecordStatusHistory] ([RecordId], [StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_StationProfile_StationId] ON [catalog].[StationProfile] ([StationId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_StationProfile_VoltageLevelId] ON [catalog].[StationProfile] ([VoltageLevelId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE INDEX [IX_StationUnitHistory_ManagementUnitId] ON [catalog].[StationUnitHistory] ([ManagementUnitId]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_StationUnitHistory_StationId] ON [catalog].[StationUnitHistory] ([StationId]) WHERE [ValidUntilUtc] IS NULL;
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE UNIQUE INDEX [IX_VoltageLevel_Kilovolts] ON [catalog].[VoltageLevel] ([Kilovolts]);
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE OR ALTER VIEW [catalog].[vStationInventory] AS
 SELECT s.Id AS StationId,(SELECT COUNT_BIG(*) FROM catalog.IedDevice d WHERE d.StationId=s.Id) AS IedCount,
 (SELECT COUNT_BIG(*) FROM ops.CollectorRegistration c WHERE c.StationId=s.Id) AS CollectorCount FROM catalog.Station s;
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE OR ALTER VIEW [ingest].[vRecordSummary] AS
 SELECT StationId,State,COUNT_BIG(*) AS RecordCount FROM ingest.DisturbanceRecord GROUP BY StationId,State;
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
CREATE OR ALTER VIEW [ops].[vFaultSummary] AS
 SELECT StationId,AnalysisStatus,COUNT_BIG(*) AS EventCount FROM ops.FaultEvent GROUP BY StationId,AnalysisStatus;
');

GO
IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2'
 OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1
 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME()
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1)
 THROW 51100,'Exact explicitly confirmed Center lab required.',1;
EXEC(N'
-- No members/logins/credentials. These roles are lab permission contracts, not IAM grants.
IF DATABASE_PRINCIPAL_ID(N''fdc_center_runtime'') IS NULL CREATE ROLE fdc_center_runtime;
IF DATABASE_PRINCIPAL_ID(N''fdc_center_reporting'') IS NULL CREATE ROLE fdc_center_reporting;
IF DATABASE_PRINCIPAL_ID(N''fdc_center_audit_writer'') IS NULL CREATE ROLE fdc_center_audit_writer;
IF DATABASE_PRINCIPAL_ID(N''fdc_center_migration'') IS NULL CREATE ROLE fdc_center_migration;
IF DATABASE_PRINCIPAL_ID(N''fdc_center_maintenance'') IS NULL CREATE ROLE fdc_center_maintenance;
GRANT SELECT ON OBJECT::iam.[User] TO fdc_center_runtime;
GRANT SELECT ON OBJECT::iam.PersonReference TO fdc_center_runtime;
GRANT SELECT ON OBJECT::iam.Session TO fdc_center_runtime;
GRANT SELECT ON OBJECT::iam.AuthorityVersion TO fdc_center_runtime;
DENY INSERT,UPDATE,DELETE ON SCHEMA::iam TO fdc_center_runtime;
DENY SELECT ON OBJECT::iam.PasswordCredential TO fdc_center_runtime;
DENY SELECT ON OBJECT::iam.MfaCredential TO fdc_center_runtime;
DENY INSERT,UPDATE,DELETE ON SCHEMA::audit TO fdc_center_runtime;
DENY SELECT,INSERT,UPDATE,DELETE ON SCHEMA::ingest TO fdc_center_runtime;
DENY SELECT ON OBJECT::catalog.vStationInventory TO fdc_center_reporting;
DENY SELECT ON OBJECT::ingest.vRecordSummary TO fdc_center_reporting;
DENY SELECT ON OBJECT::ops.vFaultSummary TO fdc_center_reporting;
DENY INSERT,UPDATE,DELETE ON SCHEMA::iam TO fdc_center_reporting;
DENY INSERT,UPDATE,DELETE ON SCHEMA::audit TO fdc_center_reporting;
DENY INSERT,UPDATE,DELETE ON SCHEMA::ingest TO fdc_center_reporting;
-- Pending D09/D10: no authority DML or archive signing powers are granted to runtime/audit roles.
GRANT VIEW DEFINITION TO fdc_center_migration;
GRANT VIEW DATABASE STATE TO fdc_center_maintenance;
');
