SET XACT_ABORT ON;
IF @@TRANCOUNT<>1 OR DB_NAME() NOT LIKE N'FDC[_]A10[_]TEST[_]%'
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
  AND CONVERT(nvarchar(32),value)=CONVERT(nvarchar(32),SESSION_CONTEXT(N'Fdc.A10.Run')))
 THROW 51450,'Owned A.10 database required.',1;
IF EXISTS(SELECT 1 FROM labprep.PermitDraft WHERE IsSynthetic=1)
 THROW 51451,'Archive and remove synthetic workflow rows before rollback.',1;
DROP TABLE labprep.WorkflowHistory;
EXEC sys.sp_dropextendedproperty @name=N'Fdc.LabWorkflowVersion';
DROP INDEX IX_LabPrepStation ON labprep.PermitDraft;
ALTER TABLE labprep.PermitDraft DROP CONSTRAINT FK_LabPrepIed,CK_LabPrepSynthetic,CK_LabPrepDraftStatus;
ALTER TABLE labprep.PermitDraft DROP COLUMN StationId,IedId,TestType,IsSynthetic,RequesterPerson,
 ReviewerPerson,ApproverPerson,ApplierPerson,PermitDigest,ResultDigest;
ALTER TABLE labprep.PermitDraft ALTER COLUMN DraftStatus varchar(12) NOT NULL;
ALTER TABLE labprep.PermitDraft ADD CONSTRAINT CK_LabPrepDraftStatus CHECK(DraftStatus IN('DRAFT','SUBMITTED'));
