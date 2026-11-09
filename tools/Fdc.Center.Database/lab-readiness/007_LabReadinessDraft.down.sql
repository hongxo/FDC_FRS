-- Deliberately refuse rollback if any evidence exists.
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 1 OR DB_NAME() <> N'FDC_Center_Lab'
    OR NOT EXISTS (SELECT 1 FROM sys.extended_properties
        WHERE class = 0 AND name = N'Fdc.Disposable' AND TRY_CONVERT(bit, value) = 1)
    THROW 51440, 'Unexpected Lab database or migration boundary.', 1;

DECLARE @lock int;
EXEC @lock = sys.sp_getapplock @Resource = N'fdc.labreadiness.007', @LockMode = 'Exclusive',
    @LockOwner = 'Transaction', @LockTimeout = 5000;
IF @lock < 0 THROW 51441, 'Lab readiness migration lock unavailable.', 1;

IF SCHEMA_ID(N'labprep') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'labprep.SchemaVersion', N'U') IS NULL OR OBJECT_ID(N'labprep.InventoryProposal', N'U') IS NULL
        OR OBJECT_ID(N'labprep.PermitDraft', N'U') IS NULL OR OBJECT_ID(N'labprep.ProposalAudit', N'U') IS NULL
        THROW 51442, 'Unexpected Lab preparation schema.', 1;
    IF NOT EXISTS (SELECT 1 FROM labprep.SchemaVersion WHERE VersionNumber = 7)
        THROW 51442, 'Unexpected Lab preparation schema version.', 1;
    IF EXISTS (SELECT 1 FROM labprep.ProposalAudit WITH (TABLOCKX,HOLDLOCK))
        OR EXISTS (SELECT 1 FROM labprep.PermitDraft WITH (TABLOCKX,HOLDLOCK))
        OR EXISTS (SELECT 1 FROM labprep.InventoryProposal WITH (TABLOCKX,HOLDLOCK))
        THROW 51443, 'Rollback refused: preserve Lab records.', 1;

    DROP TABLE labprep.ProposalAudit, labprep.PermitDraft, labprep.InventoryProposal, labprep.SchemaVersion;
    DROP SCHEMA labprep;
END;
