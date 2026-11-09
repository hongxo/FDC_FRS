using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace Fdc.Core.Lab;

public sealed record LabEvidenceReference(string Kind, string Reference, string Sha256);

public sealed record LabPermitDraft
{
    public string? PermitId { get; init; }
    public int Version { get; init; }
    public string? InventoryId { get; init; }
    public string? Station { get; init; }
    public string? Bay { get; init; }
    public string? Vendor { get; init; }
    public string? Model { get; init; }
    public string? Firmware { get; init; }
    public string? IedName { get; init; }
    public string? IpAddress { get; init; }
    public string? InterfaceName { get; init; }
    public int Port { get; init; }
    public string[]? Operations { get; init; }
    public string[]? Directories { get; init; }
    public string[]? FileNames { get; init; }
    public int MaximumFiles { get; init; }
    public long MaximumFileBytes { get; init; }
    public int MaximumSessions { get; init; }
    public int ConnectionTimeoutSeconds { get; init; }
    public int RetryLimit { get; init; }
    public int PollIntervalSeconds { get; init; }
    public int MaximumDurationSeconds { get; init; }
    public long MaximumBytesPerSecond { get; init; }
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
    public string? TimeZone { get; init; }
    public string? Operator { get; init; }
    public string? ProtectionEngineer { get; init; }
    public string? ScadaSupervisor { get; init; }
    public string? SecurityReviewer { get; init; }
    public string? StopOwner { get; init; }
    public string? StopConditions { get; init; }
    public string? EmergencyContact { get; init; }
    public string? IsolationReference { get; init; }
    public string? RollbackReference { get; init; }
    public string? CommunicationReference { get; init; }
    public string? RequestedBy { get; init; }
    public LabEvidenceReference[]? Evidence { get; init; }
}

public sealed record LabReadinessResult(string Status, string? ScopeHash, string[] Findings);

// This evaluator accepts proposals only. An evidence pointer or digest is not proof of authority.
public static class LabReadinessGate
{
    public static LabReadinessResult Evaluate(LabPermitDraft draft, DateTimeOffset now)
    {
        var findings = new List<string>();
        if (draft.Version < 1 || !Present(draft.PermitId) || !Present(draft.RequestedBy))
            findings.Add("DRAFT_IDENTITY_REQUIRED");
        if (new[] { draft.InventoryId, draft.Station, draft.Bay, draft.Vendor, draft.Model, draft.Firmware,
            draft.IedName, draft.InterfaceName }.Any(value => !Present(value)))
            findings.Add("INVENTORY_FIELDS_REQUIRED");
        if (!IPAddress.TryParse(draft.IpAddress, out var address) || address.ToString() != draft.IpAddress ||
            address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || draft.Port is < 1 or > 65535 ||
            address.Equals(IPAddress.Any) || address.Equals(IPAddress.Broadcast) || address.GetAddressBytes()[0] >= 224)
            findings.Add("EXACT_UNICAST_ENDPOINT_REQUIRED");
        if (draft.Operations is not { Length: > 0 and <= 3 } ||
            draft.Operations.Any(op => op is not ("ASSOCIATE" or "LIST" or "READ")) ||
            draft.Directories is not { Length: > 0 and <= 64 } || draft.Directories.Any(path => !DirectoryPath(path)) ||
            draft.FileNames is not { Length: > 0 and <= 64 } || draft.FileNames.Any(name => !FileName(name)))
            findings.Add("EXPLICIT_READ_ONLY_SCOPE_REQUIRED");
        if (draft.MaximumFiles < 1 || draft.MaximumFileBytes < 1 || draft.MaximumSessions < 1 ||
            draft.ConnectionTimeoutSeconds < 1 || draft.RetryLimit < 0 || draft.PollIntervalSeconds < 1 ||
            draft.MaximumDurationSeconds < 1 || draft.MaximumBytesPerSecond < 1)
            findings.Add("FINITE_LIMITS_REQUIRED");
        if (draft.Start == default || draft.End <= draft.Start ||
            (draft.End - draft.Start).TotalSeconds < draft.MaximumDurationSeconds ||
            !ValidTimeZone(draft.TimeZone))
            findings.Add("VALID_WINDOW_REQUIRED");
        if (!Present(draft.StopOwner)) findings.Add("STOP_OWNER_REQUIRED");
        if (!Present(draft.RollbackReference)) findings.Add("ROLLBACK_REQUIRED");
        if (new[] { draft.Operator, draft.ProtectionEngineer, draft.ScadaSupervisor, draft.SecurityReviewer,
            draft.StopConditions, draft.EmergencyContact, draft.IsolationReference, draft.CommunicationReference }
            .Any(value => !Present(value)))
            findings.Add("SAFETY_CONTACTS_AND_PROCEDURES_REQUIRED");

        var evidence = draft.Evidence ?? [];
        if (evidence.Length > 32 || evidence.Any(item => item is null || !Present(item.Kind) ||
            !Present(item.Reference) || item.Sha256 is not { Length: 64 } || !item.Sha256.All(char.IsAsciiHexDigit)) ||
            evidence.Where(item => item is not null).Select(item => item.Kind).Distinct(StringComparer.Ordinal).Count() != evidence.Length)
            findings.Add("INVALID_EVIDENCE_MANIFEST");
        foreach (var kind in new[] { "INVENTORY", "SCADA", "TIME", "SECURITY", "NETWORK" })
            if (!evidence.Any(item => item?.Kind == kind)) findings.Add(kind + "_REFERENCE_REQUIRED");

        if (findings.Count != 0) return new("NOT_READY", null, findings.ToArray());
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(draft)));
        if (now >= draft.End) return new("EXPIRED", hash, ["WINDOW_ENDED"]);
        return new("BLOCKED_BY_POLICY", hash, ["INDEPENDENT_VERIFICATION_AND_APPROVAL_AUTHORITY_REQUIRED"]);
    }

    private static bool Present(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 512 && !value.Any(char.IsControl);

    private static bool DirectoryPath(string? value) =>
        value is { Length: > 1 and <= 256 } && value[0] == '/' &&
        value[1..].Split('/').All(FileName);

    private static bool FileName(string? value) =>
        value is { Length: > 0 and <= 120 } && value is not ("." or "..") &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-');

    private static bool ValidTimeZone(string? value) =>
        Present(value) && TimeZoneInfo.TryFindSystemTimeZoneById(value!, out _);
}
