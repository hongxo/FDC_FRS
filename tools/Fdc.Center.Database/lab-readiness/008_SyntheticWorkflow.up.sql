-- Candidate, explicit disposable-only extension of the existing PermitDraft workflow.
SET XACT_ABORT ON;
IF @@TRANCOUNT<>1 OR DB_NAME() NOT LIKE N'FDC[_]A10[_]TEST[_]%'
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
    AND CONVERT(nvarchar(32),value)=CONVERT(nvarchar(32),SESSION_CONTEXT(N'Fdc.A10.Run')))
 OR NOT EXISTS(SELECT 1 FROM labprep.SchemaVersion WHERE VersionNumber=7)
 THROW 51450,'Owned A.10 disposable Lab preparation database required.',1;
ALTER TABLE labprep.PermitDraft DROP CONSTRAINT CK_LabPrepDraftStatus;
ALTER TABLE labprep.PermitDraft ALTER COLUMN DraftStatus varchar(32) NOT NULL;
ALTER TABLE labprep.PermitDraft ADD
 StationId uniqueidentifier NULL, IedId uniqueidentifier NULL,
 TestType varchar(32) NULL, IsSynthetic bit NULL,
 RequesterPerson nvarchar(64) NULL, ReviewerPerson nvarchar(64) NULL,
 ApproverPerson nvarchar(64) NULL, ApplierPerson nvarchar(64) NULL,
 PermitDigest varchar(64) NULL, ResultDigest varchar(64) NULL;
GO
IF @@TRANCOUNT<>1 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
    AND CONVERT(nvarchar(32),value)=CONVERT(nvarchar(32),SESSION_CONTEXT(N'Fdc.A10.Run')))
 THROW 51450,'Owned A.10 migration transaction required.',1;
ALTER TABLE labprep.PermitDraft WITH CHECK ADD
 CONSTRAINT CK_LabPrepDraftStatus CHECK(DraftStatus IN
 ('DRAFT','SUBMITTED','REVIEWED','APPROVED','READY','SYNTHETIC_EXECUTION','COMPLETED','FAILED','ABORTED')),
 CONSTRAINT FK_LabPrepIed FOREIGN KEY(IedId,StationId) REFERENCES catalog.IedDevice(Id,StationId),
 CONSTRAINT CK_LabPrepSynthetic CHECK(IsSynthetic IS NULL OR
 (IsSynthetic=1 AND StationId IS NOT NULL AND IedId IS NOT NULL AND TestType='SYNTHETIC_HASH'
 AND RequesterPerson IS NOT NULL));
CREATE INDEX IX_LabPrepStation ON labprep.PermitDraft(StationId,PermitId,PermitVersion);
CREATE TABLE labprep.WorkflowHistory(
 Id uniqueidentifier NOT NULL PRIMARY KEY,
 PermitId nvarchar(32) NOT NULL, PermitVersion int NOT NULL,
 ActorUserId uniqueidentifier NOT NULL, ActorPerson nvarchar(64) NOT NULL,
 State varchar(32) NOT NULL, Action varchar(32) NOT NULL,
 CorrelationId nvarchar(128) NOT NULL, RecordedUtc datetimeoffset(7) NOT NULL,
 RequestId uniqueidentifier NOT NULL, RequestDigest varchar(64) NOT NULL, ReplyVersion varbinary(8) NOT NULL,
 CONSTRAINT UQ_LabWorkflowRequest UNIQUE(ActorUserId,RequestId),
 CONSTRAINT FK_LabWorkflowPermit FOREIGN KEY(PermitId,PermitVersion)
 REFERENCES labprep.PermitDraft(PermitId,PermitVersion),
 CONSTRAINT FK_LabWorkflowActor FOREIGN KEY(ActorUserId) REFERENCES iam.[User](Id)
);
EXEC sys.sp_addextendedproperty @name=N'Fdc.LabWorkflowVersion',@value=8;
