IF DB_NAME()<>N'FDC_Center_Lab' OR CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))<>N'XOTH-ETC-2' OR COALESCE(TRY_CONVERT(bit,SESSION_CONTEXT(N'Fdc.ConfirmedDisposableLab')),0)<>1 OR COALESCE(CONVERT(nvarchar(128),SESSION_CONTEXT(N'Fdc.ExpectedLabDatabase')),N'')<>DB_NAME() THROW 51100,'Exact confirmed Center lab required.',1;
GRANT VIEW DEFINITION ON OBJECT::ingest.DisturbanceRecord TO fdc_center_runtime;
ALTER TABLE [ops].[CollectionJob] WITH CHECK ADD CONSTRAINT [CK_CollectionJob_Utc_NextRunAtUtc] CHECK ([NextRunAtUtc] IS NULL OR DATEPART(TZOFFSET,[NextRunAtUtc])=0);
ALTER TABLE [ops].[CollectorHeartbeat] WITH CHECK ADD CONSTRAINT [CK_CollectorHeartbeat_Utc_ReceivedAtUtc] CHECK ([ReceivedAtUtc] IS NULL OR DATEPART(TZOFFSET,[ReceivedAtUtc])=0);
ALTER TABLE [ingest].[DisturbanceRecord] WITH CHECK ADD CONSTRAINT [CK_DisturbanceRecord_Utc_CollectedAtUtc] CHECK ([CollectedAtUtc] IS NULL OR DATEPART(TZOFFSET,[CollectedAtUtc])=0);
ALTER TABLE [ingest].[DisturbanceRecord] WITH CHECK ADD CONSTRAINT [CK_DisturbanceRecord_Utc_EventAtUtc] CHECK ([EventAtUtc] IS NULL OR DATEPART(TZOFFSET,[EventAtUtc])=0);
ALTER TABLE [ops].[FaultEvent] WITH CHECK ADD CONSTRAINT [CK_FaultEvent_Utc_EventAtUtc] CHECK ([EventAtUtc] IS NULL OR DATEPART(TZOFFSET,[EventAtUtc])=0);
ALTER TABLE [ops].[ProcessingOutbox] WITH CHECK ADD CONSTRAINT [CK_ProcessingOutbox_Utc_AvailableAtUtc] CHECK ([AvailableAtUtc] IS NULL OR DATEPART(TZOFFSET,[AvailableAtUtc])=0);
ALTER TABLE [ops].[ProcessingOutbox] WITH CHECK ADD CONSTRAINT [CK_ProcessingOutbox_Utc_LeaseUntilUtc] CHECK ([LeaseUntilUtc] IS NULL OR DATEPART(TZOFFSET,[LeaseUntilUtc])=0);
ALTER TABLE [catalog].[StationUnitHistory] WITH CHECK ADD CONSTRAINT [CK_StationUnitHistory_Utc_ValidFromUtc] CHECK ([ValidFromUtc] IS NULL OR DATEPART(TZOFFSET,[ValidFromUtc])=0);
ALTER TABLE [catalog].[StationUnitHistory] WITH CHECK ADD CONSTRAINT [CK_StationUnitHistory_Utc_ValidUntilUtc] CHECK ([ValidUntilUtc] IS NULL OR DATEPART(TZOFFSET,[ValidUntilUtc])=0);

