SET XACT_ABORT ON;
IF @@TRANCOUNT<>1 OR DB_NAME() NOT LIKE N'FDC[_]A10[_]TEST[_]%'
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
    AND CONVERT(nvarchar(32),value)=CONVERT(nvarchar(32),SESSION_CONTEXT(N'Fdc.A10.Run')))
 THROW 51452,'Owned A.10 database required.',1;
IF EXISTS(SELECT 1 FROM labprep.ExportJob) THROW 51453,'Dispose owned synthetic exports before rollback.',1;
DROP TABLE labprep.ExportJob;
