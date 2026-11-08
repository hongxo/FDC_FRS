namespace Fdc.Center.Persistence;

public abstract class CenterRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public bool IsSynthetic { get; set; }
}
public sealed class StationReference { public Guid Id { get; set; } }
public sealed class UnitReference { public Guid Id { get; set; } }
public sealed class SecurityEventReference { public Guid Id { get; set; } }
public sealed class VoltageLevel : CenterRow { public decimal Kilovolts { get; set; } }
public sealed class Manufacturer : CenterRow { public required string Code { get; set; } public required string Name { get; set; } }
public sealed class DeviceModel : CenterRow { public Guid ManufacturerId { get; set; } public required string Code { get; set; } }
public sealed class DeviceFirmware : CenterRow { public Guid DeviceModelId { get; set; } public required string Version { get; set; } }
public sealed class CommunicationProfile : CenterRow { public required string Code { get; set; } public required string Protocol { get; set; } public int Port { get; set; } }
public sealed class StationProfile : CenterRow { public Guid StationId { get; set; } public Guid VoltageLevelId { get; set; } public required string DisplayName { get; set; } public bool Active { get; set; } = true; }
public sealed class StationUnitHistory : CenterRow { public Guid StationId { get; set; } public Guid ManagementUnitId { get; set; } public DateTimeOffset ValidFromUtc { get; set; } public DateTimeOffset? ValidUntilUtc { get; set; } }
public sealed class Bay : CenterRow { public Guid StationId { get; set; } public required string Code { get; set; } public bool Active { get; set; } = true; }
public sealed class Feeder : CenterRow { public Guid StationId { get; set; } public required string Code { get; set; } public bool Active { get; set; } = true; }
public sealed class IedDevice : CenterRow
{
    public Guid StationId { get; set; } public Guid? BayId { get; set; } public Guid? FeederId { get; set; }
    public Guid? DeviceModelId { get; set; } public Guid? FirmwareId { get; set; } public Guid? CommunicationProfileId { get; set; }
    public required string Code { get; set; } public required string DisplayName { get; set; } public bool Active { get; set; } = true;
}
public sealed class CollectorRegistration : CenterRow
{
    public Guid StationId { get; set; } public required string Code { get; set; } public bool Active { get; set; }
    public long ConfigurationVersion { get; set; } = 1; public string BindingStatus { get; set; } = "Pending";
}
public sealed class CollectorIed : CenterRow { public Guid StationId { get; set; } public Guid CollectorId { get; set; } public Guid IedId { get; set; } public bool Active { get; set; } }
public sealed class CollectorHeartbeat : CenterRow { public Guid StationId { get; set; } public Guid CollectorId { get; set; } public DateTimeOffset ReceivedAtUtc { get; set; } public required string Status { get; set; } public long QueueDepth { get; set; } }
public sealed class CollectionJob : CenterRow { public Guid StationId { get; set; } public Guid CollectorId { get; set; } public Guid IedId { get; set; } public string State { get; set; } = "Pending"; public DateTimeOffset? NextRunAtUtc { get; set; } }
public sealed class CollectionAttempt : CenterRow { public Guid JobId { get; set; } public int Number { get; set; } public required string Outcome { get; set; } public required string CorrelationId { get; set; } public string? ErrorCode { get; set; } }
public sealed class DisturbanceRecord : CenterRow
{
    public Guid StationId { get; set; } public Guid IedId { get; set; } public Guid CollectorId { get; set; }
    public string? ManifestSnapshotJson { get; set; } public required string ManifestId { get; set; } public required string ManifestSha256 { get; set; } public required string RemoteIdentity { get; set; }
    public DateTimeOffset CollectedAtUtc { get; set; } public DateTimeOffset? EventAtUtc { get; set; } public string? SourceTimestampRaw { get; set; }
    public required string TimeSource { get; set; } public string State { get; set; } = "MetadataAccepted"; public required string CorrelationId { get; set; }
}
public sealed class RecordFile : CenterRow
{
    public Guid StationId { get; set; } public Guid RecordId { get; set; } public required string Name { get; set; }
    public required string Extension { get; set; } public long SizeBytes { get; set; } public required string Sha256 { get; set; }
    public required string StorageObjectKey { get; set; } public string ValidationStatus { get; set; } = "Pending";
}
public sealed class IngestionAttempt : CenterRow { public Guid StationId { get; set; } public Guid RecordId { get; set; } public int Number { get; set; } public required string Outcome { get; set; } public required string CorrelationId { get; set; } public string? ErrorCode { get; set; } }
public sealed class RecordStatusHistory : CenterRow { public Guid StationId { get; set; } public Guid RecordId { get; set; } public required string State { get; set; } public required string CorrelationId { get; set; } }
public sealed class ProcessingOutbox : CenterRow { public Guid StationId { get; set; } public Guid RecordId { get; set; } public required string Kind { get; set; } public string State { get; set; } = "Pending"; public int Attempts { get; set; } public DateTimeOffset AvailableAtUtc { get; set; } public DateTimeOffset? LeaseUntilUtc { get; set; } public string? LeaseOwner { get; set; } }
public sealed class FaultEvent : CenterRow { public Guid StationId { get; set; } public DateTimeOffset EventAtUtc { get; set; } public string AnalysisStatus { get; set; } = "NotImplemented"; public string? SourceTimestampRaw { get; set; } public required string TimeSource { get; set; } }
public sealed class FaultEventRecord : CenterRow { public Guid StationId { get; set; } public Guid FaultEventId { get; set; } public Guid RecordId { get; set; } }
public sealed class AnalysisAlgorithm : CenterRow { public required string Code { get; set; } public required string Version { get; set; } public bool Implemented { get; set; } }
public sealed class AnalysisJob : CenterRow { public Guid StationId { get; set; } public Guid FaultEventId { get; set; } public Guid AlgorithmId { get; set; } public string State { get; set; } = "NotImplemented"; public required string CorrelationId { get; set; } }
public sealed class AnalysisResult : CenterRow { public Guid JobId { get; set; } public string Status { get; set; } = "NotImplemented"; public string? ResultJson { get; set; } }
public sealed class OperationalEvent : CenterRow { public Guid StationId { get; set; } public Guid? CollectorId { get; set; } public required string Code { get; set; } public required string CorrelationId { get; set; } }
public sealed class AuditDelivery : CenterRow { public Guid EventId { get; set; } public int Attempt { get; set; } public required string State { get; set; } public string? ArchiveReference { get; set; } }
public sealed class AuditCheckpoint : CenterRow { public required string ArchiveReference { get; set; } public required string ContentSha256 { get; set; } public string Status { get; set; } = "Unverified"; }
public sealed class StationSoeEvent : CenterRow
{
 public Guid StationId { get; set; } public Guid IedId { get; set; } public required string SourceEventId { get; set; }
 public required string RcbReference { get; set; } public required string DataReference { get; set; } public required string Value { get; set; } public required string Source { get; set; }
 public DateTimeOffset ReceivedAtUtc { get; set; } public DateTimeOffset? IedTimestampUtc { get; set; } public required string TimeSource { get; set; }
 public byte? TimeQuality { get; set; } public string? Quality { get; set; } public int Reason { get; set; } public string? EntryId { get; set; }
 public int? Sequence { get; set; } public long? ConfRev { get; set; } public bool BufferOverflow { get; set; } public int? SubSequence { get; set; } public bool MoreSegments { get; set; }
}
