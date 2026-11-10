SET XACT_ABORT ON;
IF @@TRANCOUNT<>1 OR DB_NAME() NOT LIKE N'FDC[_]A10[_]TEST[_]%'
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
    AND CONVERT(nvarchar(32),value)=CONVERT(nvarchar(32),SESSION_CONTEXT(N'Fdc.A10.Run')))
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.LabWorkflowVersion' AND TRY_CONVERT(int,value)=8)
 THROW 51452,'Owned A.10 workflow schema required.',1;
CREATE TABLE labprep.ExportJob(
 Id uniqueidentifier NOT NULL PRIMARY KEY, PermitId nvarchar(32) NOT NULL, PermitVersion int NOT NULL,
 StationId uniqueidentifier NOT NULL, ActorUserId uniqueidentifier NOT NULL, SessionDigest varchar(64) NOT NULL,
 State varchar(16) NOT NULL CHECK(State IN('Pending','Preparing','Completed','Quarantined')),
 ExpiresUtc datetimeoffset(7) NOT NULL, LeaseOwner uniqueidentifier NULL, LeaseUntilUtc datetimeoffset(7) NULL,
 Payload varbinary(max) NULL CHECK(Payload IS NULL OR DATALENGTH(Payload)<=65536),
 PayloadDigest varchar(64) NULL, VersionStamp rowversion NOT NULL,
 CONSTRAINT FK_LabExportPermit FOREIGN KEY(PermitId,PermitVersion) REFERENCES labprep.PermitDraft(PermitId,PermitVersion),
 CONSTRAINT FK_LabExportActor FOREIGN KEY(ActorUserId) REFERENCES iam.[User](Id),
 CONSTRAINT CK_LabExportLease CHECK((LeaseOwner IS NULL AND LeaseUntilUtc IS NULL) OR (LeaseOwner IS NOT NULL AND LeaseUntilUtc IS NOT NULL)),
 CONSTRAINT CK_LabExportRelease CHECK(State<>'Completed' OR (Payload IS NOT NULL AND PayloadDigest IS NOT NULL))
);
