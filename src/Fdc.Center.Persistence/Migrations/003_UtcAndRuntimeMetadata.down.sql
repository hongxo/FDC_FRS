IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2' OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME() THROW 51100,'Exact confirmed Center lab required.',1;
REVOKE VIEW DEFINITION ON OBJECT::ingest.DisturbanceRecord FROM fdc_center_runtime;
ALTER TABLE [ops].[CollectionJob] DROP CONSTRAINT [CK_CollectionJob_Utc_NextRunAtUtc];
ALTER TABLE [ops].[CollectorHeartbeat] DROP CONSTRAINT [CK_CollectorHeartbeat_Utc_ReceivedAtUtc];
ALTER TABLE [ingest].[DisturbanceRecord] DROP CONSTRAINT [CK_DisturbanceRecord_Utc_CollectedAtUtc];
ALTER TABLE [ingest].[DisturbanceRecord] DROP CONSTRAINT [CK_DisturbanceRecord_Utc_EventAtUtc];
ALTER TABLE [ops].[FaultEvent] DROP CONSTRAINT [CK_FaultEvent_Utc_EventAtUtc];
ALTER TABLE [ops].[ProcessingOutbox] DROP CONSTRAINT [CK_ProcessingOutbox_Utc_AvailableAtUtc];
ALTER TABLE [ops].[ProcessingOutbox] DROP CONSTRAINT [CK_ProcessingOutbox_Utc_LeaseUntilUtc];
ALTER TABLE [catalog].[StationUnitHistory] DROP CONSTRAINT [CK_StationUnitHistory_Utc_ValidFromUtc];
ALTER TABLE [catalog].[StationUnitHistory] DROP CONSTRAINT [CK_StationUnitHistory_Utc_ValidUntilUtc];

