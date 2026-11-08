IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2' OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME() THROW 51100,'Exact confirmed Center lab required.',1;
ALTER TABLE ops.OperationalEvent DROP CONSTRAINT FK_OperationalEvent_Station_StationId;

