SET XACT_ABORT ON;
IF @@TRANCOUNT<>1 OR DB_NAME() NOT LIKE N'FDC[_]A10[_]TEST[_]%'
 OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.A10.Run'
 AND CONVERT(nvarchar(32),value)=CONVERT(nvarchar(32),SESSION_CONTEXT(N'Fdc.A10.Run')))
 THROW 51454,'Owned A.10 disposable database required.',1;
CREATE TABLE ops.CollectorBindingRequest(
 Id uniqueidentifier NOT NULL PRIMARY KEY, CollectorId uniqueidentifier NOT NULL, StationId uniqueidentifier NOT NULL,
 ExpectedConfigurationVersion bigint NOT NULL CHECK(ExpectedConfigurationVersion>0),
 TargetStatus varchar(16) NOT NULL CHECK(TargetStatus IN('Bound','Revoked')),
 State varchar(16) NOT NULL CHECK(State IN('Pending','Reviewed','Approved','Applied')),
 RequesterPerson nvarchar(64) NOT NULL, ReviewerPerson nvarchar(64) NULL, ApproverPerson nvarchar(64) NULL,
 ApplierPerson nvarchar(64) NULL, ExpiresUtc datetimeoffset(7) NOT NULL, VersionStamp rowversion NOT NULL,
 CONSTRAINT FK_BindingCollector FOREIGN KEY(CollectorId,StationId) REFERENCES ops.CollectorRegistration(Id,StationId)
);
CREATE TABLE ops.CollectorBindingHistory(
 Id uniqueidentifier NOT NULL PRIMARY KEY, RequestId uniqueidentifier NOT NULL, EventId uniqueidentifier NOT NULL,
 CONSTRAINT FK_BindingHistoryRequest FOREIGN KEY(RequestId) REFERENCES ops.CollectorBindingRequest(Id),
 CONSTRAINT FK_BindingHistoryEvent FOREIGN KEY(EventId) REFERENCES audit.SecurityEvent(Id)
);
