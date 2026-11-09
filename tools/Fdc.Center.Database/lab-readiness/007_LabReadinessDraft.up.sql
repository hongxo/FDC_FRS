-- Standalone, opt-in migration candidate. The deployment runner owns the transaction.
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 1 OR DB_NAME() <> N'FDC_Center_Lab'
    OR NOT EXISTS (SELECT 1 FROM sys.extended_properties
        WHERE class = 0 AND name = N'Fdc.Disposable' AND TRY_CONVERT(bit, value) = 1)
    OR NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'202611080006_OperationalEventStation')
    THROW 51440, 'Unexpected Lab database or migration boundary.', 1;

DECLARE @lock int;
EXEC @lock = sys.sp_getapplock @Resource = N'fdc.labreadiness.007', @LockMode = 'Exclusive',
    @LockOwner = 'Transaction', @LockTimeout = 5000;
IF @lock < 0 THROW 51441, 'Lab readiness migration lock unavailable.', 1;

IF SCHEMA_ID(N'labprep') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [labprep] AUTHORIZATION [dbo]');
    CREATE TABLE labprep.InventoryProposal (
        InventoryId uniqueidentifier NOT NULL CONSTRAINT PK_LabPrepInventory PRIMARY KEY,
        StationCode nvarchar(64) NOT NULL,
        SourceDigest binary(32) NOT NULL,
        SourceFile nvarchar(255) NOT NULL,
        SourceSheet nvarchar(128) NOT NULL,
        SourceRow int NOT NULL CONSTRAINT CK_LabPrepSourceRow CHECK (SourceRow > 0),
        ProposalJson nvarchar(max) NOT NULL CONSTRAINT CK_LabPrepInventoryJson CHECK (ISJSON(ProposalJson, OBJECT) = 1),
        VersionStamp rowversion NOT NULL,
        CONSTRAINT UQ_LabPrepInventorySource UNIQUE (SourceDigest, SourceSheet, SourceRow, StationCode)
    );
    CREATE TABLE labprep.PermitDraft (
        PermitId nvarchar(32) NOT NULL,
        PermitVersion int NOT NULL CONSTRAINT CK_LabPrepPermitVersion CHECK (PermitVersion > 0),
        InventoryId uniqueidentifier NOT NULL,
        ScopeDigest binary(32) NOT NULL,
        StartsUtc datetime2(3) NOT NULL,
        EndsUtc datetime2(3) NOT NULL,
        DraftStatus varchar(12) NOT NULL CONSTRAINT CK_LabPrepDraftStatus CHECK (DraftStatus IN ('DRAFT','SUBMITTED')),
        DraftJson nvarchar(max) NOT NULL CONSTRAINT CK_LabPrepDraftJson CHECK (ISJSON(DraftJson, OBJECT) = 1),
        VersionStamp rowversion NOT NULL,
        CONSTRAINT PK_LabPrepPermit PRIMARY KEY (PermitId, PermitVersion),
        CONSTRAINT FK_LabPrepPermitInventory FOREIGN KEY (InventoryId) REFERENCES labprep.InventoryProposal(InventoryId),
        CONSTRAINT CK_LabPrepPermitWindow CHECK (StartsUtc < EndsUtc)
    );
    CREATE INDEX IX_LabPrepPermitInventory ON labprep.PermitDraft(InventoryId);
    CREATE TABLE labprep.ProposalAudit (
        AuditId bigint IDENTITY CONSTRAINT PK_LabPrepAudit PRIMARY KEY,
        PermitId nvarchar(32) NOT NULL,
        PermitVersion int NOT NULL,
        ActorReference nvarchar(256) NOT NULL,
        EvidenceDigest binary(32) NOT NULL,
        ActionCode varchar(12) NOT NULL CONSTRAINT CK_LabPrepAuditAction CHECK (ActionCode IN ('DRAFT','SUBMITTED')),
        RecordedUtc datetime2(3) NOT NULL CONSTRAINT DF_LabPrepAuditUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LabPrepAuditPermit FOREIGN KEY (PermitId, PermitVersion)
            REFERENCES labprep.PermitDraft(PermitId, PermitVersion)
    );
    CREATE TABLE labprep.SchemaVersion (VersionNumber int CONSTRAINT PK_LabPrepSchemaVersion PRIMARY KEY CHECK (VersionNumber = 7));
    INSERT labprep.SchemaVersion VALUES (7);
END
ELSE
BEGIN
    IF OBJECT_ID(N'labprep.SchemaVersion', N'U') IS NULL OR OBJECT_ID(N'labprep.InventoryProposal', N'U') IS NULL
        OR OBJECT_ID(N'labprep.PermitDraft', N'U') IS NULL OR OBJECT_ID(N'labprep.ProposalAudit', N'U') IS NULL
        THROW 51442, 'Unexpected existing Lab preparation schema.', 1;
    IF NOT EXISTS (SELECT 1 FROM labprep.SchemaVersion WHERE VersionNumber = 7)
        THROW 51442, 'Unexpected Lab preparation schema version.', 1;
END;
