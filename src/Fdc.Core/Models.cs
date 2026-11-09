using System.Security.Cryptography;
using System.Text;

namespace Fdc.Core;

public sealed class CollectorOptions
{
    public string StationId { get; set; } = "TBA_DEMO";
    public string Mode { get; set; } = "Demo";
    public string DataRoot { get; set; } = "data/collector";
    public int PollSeconds { get; set; } = 60;
    public int MaxConcurrentIeds { get; set; } = 2;
    public long MaxFileBytes { get; set; } = 100 * 1024 * 1024;
    public int RetentionDays { get; set; } = 90;
    public string CenterUrl { get; set; } = "http://127.0.0.1:5081";
    public string ApiKey { get; set; } = "";
    public string TransferProtocol { get; set; } = "Sftp";
    public List<Ied> Ieds { get; set; } = [];
    public SftpOptions Sftp { get; set; } = new();
    public MtlsClientOptions Mtls { get; set; } = new();
}

public sealed class SftpOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; } = 22;
    public string Username { get; set; } = "";
    public string PrivateKeyPath { get; set; } = "";
    public string HostKeySha256 { get; set; } = "";
    public string RemoteRoot { get; set; } = "/incoming";
}

public sealed record Ied(string Id, string Name, string Host, int Port = 102,
    string Directory = "/COMTRADE/", string Bay = "", string Manufacturer = "",
    string Model = "", string Firmware = "", bool Enabled = true, ReportOptions[]? Reports = null);

public sealed class MtlsClientOptions
{
    public bool Enabled { get; set; }
    public string CertificatePath { get; set; } = "";
    public string CertificatePassword { get; set; } = "";
    public string TrustedCaPath { get; set; } = "";
    public bool CheckRevocation { get; set; } = true;
}
public sealed record ReportOptions(string RcbReference, string[]? TriggerReferences = null, int SettleSeconds = 5, string? ResumeEntryId = null);
public sealed record SoeEvent(string Id, string StationId, string IedId, string RcbReference, string DataReference,
    string Value, string Source, DateTimeOffset ReceivedAt, DateTimeOffset? IedTimestamp,
    string TimeSource, byte? TimeQuality, string? Quality, int Reason, string? EntryId,
    ushort? Sequence, uint? ConfRev, bool BufferOverflow, ushort? SubSequence = null, bool MoreSegments = false);
public sealed record RemoteFile(string Path, long Size, DateTimeOffset? Modified);
public sealed record FileDigest(string Name, long Size, string Sha256);
public sealed record Manifest(string Id, string StationId, string IedId, string Bay,
    string Manufacturer, string Model, string Firmware, string RemoteIdentity,
    DateTimeOffset CollectedAt, string? TriggerTimeRaw, string TimeSource,
    IReadOnlyList<FileDigest> Files);
public sealed record RecordRow(string Id, string IedId, string RemoteIdentity, string State,
    string BundlePath, string ManifestJson, int Attempts, string? Error, string UpdatedAt);
public sealed record CommandRow(string Id, string StationId, string IedId, string Kind,
    string? RecordPath, string State, string ExpiresAt, string? Result);
public sealed record CommandRequest(string StationId, string IedId, string Kind, string? RecordPath);
public sealed record CommandResult(bool Success, string Detail);
public sealed record Receipt(string Id, string ManifestSha256, string State);

public static class Identity
{
    public static string Hash(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    public static string FileHash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }
    public static bool SafeId(string value) => value.Length is > 0 and <= 80 &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-');
    public static string FileName(string remotePath)
    {
        var name = remotePath.Replace('\\', '/').Split('/').Last();
        if (name.Length == 0 || name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains(':'))
            throw new InvalidDataException("Tên file từ IED không hợp lệ.");
        return name;
    }
}
