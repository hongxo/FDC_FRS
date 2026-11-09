namespace Fdc.Core;

// Candidate Lab contract only; does not select a deployment topology or grant any authority.
public sealed record PilotUploadTicket(Guid Id, string ManifestId, string ManifestSha256, DateTimeOffset ExpiresAt);
public sealed record PilotIngestionAck(Guid RecordId, string ManifestId, string ManifestSha256, string FilesDigest,
    bool Duplicate, string CorrelationId);
