using System.Text.Json;
using Fdc.Core;
using Microsoft.Data.Sqlite;

namespace Fdc.Collector.Pilot;

public sealed record QueueLease(string Kind, string Id, string Token, int Attempt, DateTimeOffset Until);
public sealed record PilotPackage(string Id, string ManifestJson, string ManifestSha256, string Directory, string FilesDigest);
public sealed record PilotAck(string ManifestId, Guid CenterRecordId, string ManifestSha256, string FilesDigest, string CorrelationId);
public interface ICenterReceiptTrust { void DemandVerified(PilotAck ack); }
public sealed class PendingCenterReceiptTrust : ICenterReceiptTrust
{
    public void DemandVerified(PilotAck ack) => throw new InvalidOperationException("Approved mTLS receipt validation required; HTTP success alone is not ACK.");
}

// Opt-in library: no constructor in the operating Collector is changed. Same fdc.db path,
// additive versioned pilot_* tables, no edits to existing records/commands/events schema.
public sealed class PilotTransferQueue
{
    private readonly string connection;
    private readonly TimeProvider clock;
    public PilotTransferQueue(string collectorDirectory, TimeProvider clock)
    {
        Directory.CreateDirectory(collectorDirectory); this.clock = clock;
        connection = new SqliteConnectionStringBuilder { DataSource = Path.Combine(collectorDirectory, "fdc.db"), DefaultTimeout = 15 }.ToString();
        using var db = Open(); using var tx = db.BeginTransaction();
        using var command = db.CreateCommand(); command.Transaction = tx;
        using (var script = typeof(PilotTransferQueue).Assembly.GetManifestResourceStream("Fdc.Collector.Pilot.Migrations.001_PilotTransferQueue.up.sql") ?? throw new InvalidOperationException("Missing versioned SQLite migration."))
        using (var reader = new StreamReader(script)) command.CommandText = reader.ReadToEnd();
        command.ExecuteNonQuery();
        command.CommandText = "SELECT COUNT(*) FROM pilot_schema WHERE version<>1 OR migration<>'001_PilotTransferQueue'";
        if (Convert.ToInt32(command.ExecuteScalar()) != 0) throw new InvalidOperationException("Unsupported pilot schema version; no downgrade.");
        tx.Commit();
    }
    private SqliteConnection Open()
    {
        var db = new SqliteConnection(connection); db.Open();
        using var command = db.CreateCommand(); command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=FULL; PRAGMA foreign_keys=ON;"; command.ExecuteNonQuery(); return db;
    }
    private string Now => clock.GetUtcNow().ToUniversalTime().ToString("O");
    public void RollbackEmptyPilotSchema()
    {
        // Explicit operator/test action only; never automatically drops retained queue/history/capabilities.
        using var db = Open(); using var tx = db.BeginTransaction(); using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = "SELECT COUNT(*) FROM pilot_schema WHERE version=1 AND migration='001_PilotTransferQueue'";
        if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new InvalidOperationException("Unknown schema; rollback denied.");
        command.CommandText = "SELECT (SELECT COUNT(*) FROM pilot_schema)+(SELECT COUNT(*) FROM pilot_downloads)+(SELECT COUNT(*) FROM pilot_records)+(SELECT COUNT(*) FROM pilot_attempts)+(SELECT COUNT(*) FROM pilot_capabilities)";
        if (Convert.ToInt64(command.ExecuteScalar()) != 1) throw new InvalidOperationException("Pilot data/history exists; retain schema and roll forward.");
        command.CommandText = "DROP TABLE pilot_downloads; DROP TABLE pilot_records; DROP TABLE pilot_attempts; DROP TABLE pilot_capabilities; DROP TABLE pilot_schema;";
        command.ExecuteNonQuery(); tx.Commit();
    }
    public void SaveCapability(IedTarget target, CapabilityObservation observation)
    {
        if (!Identity.SafeId(target.Station) || !Identity.SafeId(target.IedId) || !Enum.IsDefined(observation.EvidenceKind) || observation.Diagnostic.Length > 1000)
            throw new ArgumentException("Bounded capability observation required.");
        var json = JsonSerializer.Serialize(new { target.Station, target.IedId, VendorClaim = target.Vendor,
            ModelClaim = target.Model, FirmwareClaim = target.Firmware, Observation = observation,
            IdentityVerification = "Inventory claim only; edition/model/firmware not inferred from MMS association" });
        using var db = Open(); using var command = db.CreateCommand();
        command.CommandText = "INSERT INTO pilot_capabilities VALUES($id,$station,$ied,$observation,$kind,$now)";
        Parameters(command, ("$id", Guid.NewGuid().ToString("N")), ("$station", target.Station), ("$ied", target.IedId),
            ("$observation", json), ("$kind", observation.EvidenceKind.ToString()), ("$now", observation.ObservedAt.ToUniversalTime().ToString("O")));
        command.ExecuteNonQuery();
    }
    public string Discover(string station, string ied, string remoteCfg, Guid observationId, int maximumAttempts)
    {
        if (!Identity.SafeId(station) || !Identity.SafeId(ied) || observationId == Guid.Empty || maximumAttempts is < 1 or > 10) throw new ArgumentException("Bounded discovery identity required.");
        AcquisitionSafety.ValidateRemotePath(remoteCfg);
        var id = Identity.Hash(station + "\n" + ied + "\n" + remoteCfg + "\n" + observationId.ToString("N"));
        using var db = Open(); using var command = db.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO pilot_downloads(id,station,ied,remote_cfg,state,max_attempts,next_attempt) VALUES($id,$station,$ied,$path,'Discovered',$max,$now)";
        Parameters(command, ("$id", id), ("$station", station), ("$ied", ied), ("$path", remoteCfg), ("$max", maximumAttempts), ("$now", Now)); command.ExecuteNonQuery(); return id;
    }
    public QueueLease? Claim(string kind, TimeSpan leaseDuration, string? expectedId = null)
    {
        if (leaseDuration <= TimeSpan.Zero || leaseDuration > TimeSpan.FromHours(1)) throw new ArgumentException("Bounded lease required.");
        var (table, ready, active) = Table(kind); using var db = Open(); using var tx = db.BeginTransaction();
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = $"UPDATE {table} SET state='Failed',error_code='ATTEMPTS_EXHAUSTED',lease=NULL,lease_until=NULL WHERE attempts>=max_attempts AND state=$active AND lease_until<=$now";
        Parameters(command, ("$active", active), ("$now", Now)); command.ExecuteNonQuery(); command.Parameters.Clear();
        command.CommandText = $"SELECT id,attempts FROM {table} WHERE ($expected IS NULL OR id=$expected) AND attempts<max_attempts AND ((state=$ready AND next_attempt<=$now) OR (state=$active AND lease_until<=$now)) ORDER BY next_attempt,id LIMIT 1";
        Parameters(command, ("$ready", ready), ("$active", active), ("$now", Now), ("$expected", (object?)expectedId ?? DBNull.Value));
        string id; int attempt;
        using (var row = command.ExecuteReader()) { if (!row.Read()) { tx.Commit(); return null; } id = row.GetString(0); attempt = row.GetInt32(1) + 1; }
        var token = Guid.NewGuid().ToString("N"); var until = clock.GetUtcNow().ToUniversalTime() + leaseDuration;
        command.Parameters.Clear(); command.CommandText = $"UPDATE {table} SET state=$active,attempts=$attempt,lease=$token,lease_until=$until,error_code=NULL WHERE id=$id";
        Parameters(command, ("$active", active), ("$attempt", attempt), ("$token", token), ("$until", until.ToString("O")), ("$id", id)); command.ExecuteNonQuery();
        command.Parameters.Clear(); command.CommandText = "INSERT INTO pilot_attempts(kind,job_id,attempt,lease,started,result) VALUES($kind,$id,$attempt,$token,$now,'Started')";
        Parameters(command, ("$kind", kind), ("$id", id), ("$attempt", attempt), ("$token", token), ("$now", Now)); command.ExecuteNonQuery(); tx.Commit();
        return new(kind, id, token, attempt, until);
    }
    public void CompleteDownload(QueueLease lease, PilotPackage package, int uploadAttempts)
    {
        if (lease.Kind != "Download" || uploadAttempts is < 1 or > 10) throw new ArgumentException("Download lease required.");
        ValidatePackage(package);
        using var db = Open(); using var tx = db.BeginTransaction(); using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = "UPDATE pilot_downloads SET state='Complete',record_id=$record,lease=NULL,lease_until=NULL WHERE id=$id AND lease=$token AND lease_until>$now AND state='Downloading'";
        Parameters(command, ("$record", package.Id), ("$id", lease.Id), ("$token", lease.Token), ("$now", Now)); RequireOne(command);
        command.Parameters.Clear(); command.CommandText = "INSERT OR IGNORE INTO pilot_records(id,manifest,manifest_sha,directory,files_digest,state,max_attempts,next_attempt) VALUES($id,$manifest,$sha,$directory,$files,'Ready',$max,$now)";
        Parameters(command, ("$id", package.Id), ("$manifest", package.ManifestJson), ("$sha", package.ManifestSha256), ("$directory", package.Directory), ("$files", package.FilesDigest), ("$max", uploadAttempts), ("$now", Now)); command.ExecuteNonQuery();
        command.Parameters.Clear(); command.CommandText = "SELECT manifest_sha,files_digest FROM pilot_records WHERE id=$id"; Parameters(command, ("$id", package.Id));
        using (var row = command.ExecuteReader()) { if (!row.Read() || row.GetString(0) != package.ManifestSha256 || row.GetString(1) != package.FilesDigest) throw new InvalidDataException("Conflicting content replay; original metadata must be retained."); }
        FinishAttempt(command, lease, "Complete"); tx.Commit();
    }
    public void Retry(QueueLease lease, TimeSpan initialBackoff, TimeSpan maximumBackoff, string errorCode)
    {
        if (initialBackoff <= TimeSpan.Zero || maximumBackoff < initialBackoff || maximumBackoff > TimeSpan.FromDays(1) ||
            string.IsNullOrEmpty(errorCode) || errorCode.Length > 80 || errorCode.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_')) throw new ArgumentException("Bounded sanitized retry required.");
        var (table, ready, active) = Table(lease.Kind);
        var seconds = Math.Min(maximumBackoff.TotalSeconds, initialBackoff.TotalSeconds * Math.Pow(2, lease.Attempt - 1));
        using var db = Open(); using var tx = db.BeginTransaction(); using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = $"UPDATE {table} SET state=CASE WHEN attempts>=max_attempts THEN 'Failed' ELSE $ready END,next_attempt=$next,error_code=$error,lease=NULL,lease_until=NULL WHERE id=$id AND lease=$token AND lease_until>$now AND state=$active";
        Parameters(command, ("$ready", ready), ("$next", (clock.GetUtcNow() + TimeSpan.FromSeconds(seconds)).ToString("O")), ("$error", errorCode), ("$id", lease.Id), ("$token", lease.Token), ("$now", Now), ("$active", active)); RequireOne(command);
        FinishAttempt(command, lease, errorCode); tx.Commit();
    }
    public PilotPackage Package(string id)
    {
        using var db = Open(); using var command = db.CreateCommand(); command.CommandText = "SELECT id,manifest,manifest_sha,directory,files_digest FROM pilot_records WHERE id=$id"; Parameters(command, ("$id", id));
        using var row = command.ExecuteReader(); if (!row.Read()) throw new KeyNotFoundException();
        return new(row.GetString(0), row.GetString(1), row.GetString(2), row.GetString(3), row.GetString(4));
    }
    public void Acknowledge(QueueLease lease, PilotAck ack, ICenterReceiptTrust trust)
    {
        trust.DemandVerified(ack); // Transport/station/status/integrity proof must be independently verified.
        if (lease.Kind != "Upload" || lease.Id != ack.ManifestId || ack.CenterRecordId == Guid.Empty || string.IsNullOrWhiteSpace(ack.CorrelationId) || ack.CorrelationId.Length > 200) throw new InvalidDataException("ACK identity mismatch.");
        var package = Package(lease.Id);
        if (package.ManifestSha256 != ack.ManifestSha256 || package.FilesDigest != ack.FilesDigest) throw new InvalidDataException("ACK integrity mismatch.");
        using var db = Open(); using var tx = db.BeginTransaction(); using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = "UPDATE pilot_records SET state='Acknowledged',ack=$ack,lease=NULL,lease_until=NULL WHERE id=$id AND lease=$token AND lease_until>$now AND state='Uploading'";
        Parameters(command, ("$ack", JsonSerializer.Serialize(ack)), ("$id", lease.Id), ("$token", lease.Token), ("$now", Now)); RequireOne(command);
        FinishAttempt(command, lease, "Acknowledged"); tx.Commit(); // No automatic local deletion or retention policy selection.
    }
    public IReadOnlyList<(string Id, string State, int Attempts)> Status(string kind, int limit)
    {
        if (limit is < 1 or > 100) throw new ArgumentException("Bounded page required."); var table = Table(kind).Table;
        using var db = Open(); using var command = db.CreateCommand(); command.CommandText = $"SELECT id,state,attempts FROM {table} ORDER BY id LIMIT $limit"; Parameters(command, ("$limit", limit));
        using var rows = command.ExecuteReader(); var result = new List<(string, string, int)>(); while (rows.Read()) result.Add((rows.GetString(0), rows.GetString(1), rows.GetInt32(2))); return result;
    }
    private static void ValidatePackage(PilotPackage p)
    {
        if (p.Id.Length != 64 || p.Id.Any(c => !char.IsAsciiHexDigit(c) || char.IsUpper(c)) ||
            Identity.Hash(p.ManifestJson) != p.ManifestSha256 || !Path.IsPathFullyQualified(p.Directory) ||
            p.FilesDigest.Length != 64 || p.FilesDigest.Any(c => !char.IsAsciiHexDigit(c) || char.IsUpper(c))) throw new InvalidDataException("Invalid immutable package metadata.");
    }
    private static (string Table, string Ready, string Active) Table(string kind) => kind switch { "Download" => ("pilot_downloads", "Discovered", "Downloading"), "Upload" => ("pilot_records", "Ready", "Uploading"), _ => throw new ArgumentException("Unknown queue kind.") };
    private static void RequireOne(SqliteCommand command) { if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Stale/expired queue lease; no state changed."); }
    private static void Parameters(SqliteCommand command, params (string Name, object Value)[] values) { foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value); }
    private static void FinishAttempt(SqliteCommand command, QueueLease lease, string result)
    {
        command.Parameters.Clear(); command.CommandText = "UPDATE pilot_attempts SET result=$result WHERE kind=$kind AND job_id=$id AND lease=$token";
        Parameters(command, ("$result", result), ("$kind", lease.Kind), ("$id", lease.Id), ("$token", lease.Token)); RequireOne(command);
    }
}
