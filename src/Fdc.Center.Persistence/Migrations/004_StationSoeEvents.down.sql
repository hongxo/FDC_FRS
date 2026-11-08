IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2' OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME() THROW 51100,'Exact confirmed Center lab required.',1;
IF EXISTS(SELECT 1 FROM ops.StationSoeEvent) THROW 51101,'Nonempty SOE table cannot be destructively rolled back.',1;
DROP TABLE ops.StationSoeEvent;
