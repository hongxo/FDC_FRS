IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2' OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME() THROW 51100,'Exact confirmed Center lab required.',1;
IF EXISTS(SELECT 1 FROM ingest.DisturbanceRecord WHERE ManifestSnapshotJson IS NOT NULL) THROW 51101,'Never discard persisted source snapshots.',1;
ALTER TABLE ingest.DisturbanceRecord DROP CONSTRAINT CK_DisturbanceRecord_ManifestSnapshot; ALTER TABLE ingest.DisturbanceRecord DROP COLUMN ManifestSnapshotJson;

