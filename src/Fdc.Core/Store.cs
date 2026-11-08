using Microsoft.Data.Sqlite;

namespace Fdc.Core;

// Each operation opens its own connection; SQLite coordinates the collector and API.
public sealed partial class Store
{
    private readonly string connectionString;
    public Store(string directory)
    {
        System.IO.Directory.CreateDirectory(directory);
        connectionString = new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "fdc.db"), DefaultTimeout = 15 }.ToString();
        using var db = Open();
        using var sql = db.CreateCommand();
        sql.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS records (
              id TEXT PRIMARY KEY, ied TEXT NOT NULL, remote TEXT NOT NULL,
              state TEXT NOT NULL, path TEXT NOT NULL, manifest TEXT NOT NULL,
              attempts INTEGER NOT NULL DEFAULT 0, error TEXT, updated TEXT NOT NULL,
              next_attempt TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS commands (
              id TEXT PRIMARY KEY, station TEXT NOT NULL, ied TEXT NOT NULL,
              kind TEXT NOT NULL, record_path TEXT, state TEXT NOT NULL,
              expires TEXT NOT NULL, result TEXT);
            CREATE TABLE IF NOT EXISTS health (
              ied TEXT PRIMARY KEY, healthy INTEGER NOT NULL, detail TEXT NOT NULL, updated TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS observations (source TEXT PRIMARY KEY, fingerprint TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS telemetry (station TEXT PRIMARY KEY, payload TEXT NOT NULL, updated TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS events (id TEXT PRIMARY KEY, station TEXT NOT NULL, ied TEXT NOT NULL,
              received TEXT NOT NULL, payload TEXT NOT NULL, delivered INTEGER NOT NULL DEFAULT 0);
            CREATE INDEX IF NOT EXISTS events_pending ON events(delivered,received);
            """;
        sql.ExecuteNonQuery();
    }
    private SqliteConnection Open() { var db = new SqliteConnection(connectionString); db.Open(); return db; }
    private int Execute(string text, params (string, object?)[] args)
    {
        using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = text;
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return cmd.ExecuteNonQuery();
    }
    private static string Now => DateTimeOffset.UtcNow.ToString("O");
    public void Add(Manifest manifest, string path, string json) => Execute("""
        INSERT OR IGNORE INTO records(id,ied,remote,state,path,manifest,updated,next_attempt)
        VALUES($id,$ied,$remote,'Queued',$path,$json,$now,$now)
        """, ("$id", manifest.Id), ("$ied", manifest.IedId), ("$remote", manifest.RemoteIdentity),
        ("$path", path), ("$json", json), ("$now", Now));
    public bool Has(string id) => Records().Any(x => x.Id == id);
    public RecordRow? Find(string id) => Records().SingleOrDefault(x => x.Id == id);
    public bool Unchanged(string source, string fingerprint)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT fingerprint FROM observations WHERE source=$source";
        cmd.Parameters.AddWithValue("$source", source);
        return cmd.ExecuteScalar() as string == fingerprint;
    }
    public void Observe(string source, string fingerprint) => Execute("INSERT INTO observations VALUES($source,$fp) ON CONFLICT(source) DO UPDATE SET fingerprint=$fp", ("$source", source), ("$fp", fingerprint));
    public void Telemetry(string station, string payload) => Execute("INSERT INTO telemetry VALUES($station,$payload,$now) ON CONFLICT(station) DO UPDATE SET payload=$payload,updated=$now", ("$station", station), ("$payload", payload), ("$now", Now));
    public object[] Telemetry()
    {
        using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "SELECT station,payload,updated FROM telemetry";
        using var r = cmd.ExecuteReader(); var rows = new List<object>();
        while (r.Read()) rows.Add(new { stationId = r.GetString(0), payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(r.GetString(1)), updatedAt = r.GetString(2) });
        return rows.ToArray();
    }
    public List<RecordRow> Records(bool pendingOnly = false)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT id,ied,remote,state,path,manifest,attempts,error,updated FROM records " +
            (pendingOnly ? "WHERE state='Queued' AND next_attempt <= $now " : "") + "ORDER BY updated DESC";
        if (pendingOnly) cmd.Parameters.AddWithValue("$now", Now);
        using var reader = cmd.ExecuteReader(); var rows = new List<RecordRow>();
        while (reader.Read()) rows.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6),
            reader.IsDBNull(7) ? null : reader.GetString(7), reader.GetString(8)));
        return rows;
    }
    public void Delivered(string id) => Execute("UPDATE records SET state='Delivered',error=NULL,updated=$now WHERE id=$id", ("$id", id), ("$now", Now));
    public void Requeue(string id) => Execute("UPDATE records SET state='Queued',attempts=0,error=NULL,next_attempt=$now,updated=$now WHERE id=$id", ("$id", id), ("$now", Now));
    public void Retry(RecordRow row, string error) => Execute("""
        UPDATE records SET attempts=attempts+1,error=$error,updated=$now,next_attempt=$next WHERE id=$id
        """, ("$id", row.Id), ("$error", error), ("$now", Now),
        ("$next", DateTimeOffset.UtcNow.AddSeconds(Math.Min(900, 5 * Math.Pow(2, Math.Min(row.Attempts, 8)))).ToString("O")));
    public void Health(string ied, bool healthy, string detail) => Execute("""
        INSERT INTO health VALUES($ied,$ok,$detail,$now)
        ON CONFLICT(ied) DO UPDATE SET healthy=$ok,detail=$detail,updated=$now
        """, ("$ied", ied), ("$ok", healthy ? 1 : 0), ("$detail", detail), ("$now", Now));
    public object[] Health()
    {
        using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "SELECT * FROM health ORDER BY ied";
        using var r = cmd.ExecuteReader(); var rows = new List<object>();
        while (r.Read()) rows.Add(new { iedId = r.GetString(0), healthy = r.GetInt32(1) == 1, detail = r.GetString(2), updatedAt = r.GetString(3) });
        return rows.ToArray();
    }
    public CommandRow Enqueue(CommandRequest request)
    {
        if (!Identity.SafeId(request.StationId) || !Identity.SafeId(request.IedId) || request.Kind is not ("List" or "Collect" or "Reload"))
            throw new ArgumentException("Lệnh hoặc định danh không hợp lệ.");
        if (request.Kind == "Reload" && string.IsNullOrWhiteSpace(request.RecordPath)) throw new ArgumentException("Reload cần đường dẫn bản ghi.");
        var row = new CommandRow(Guid.NewGuid().ToString("N"), request.StationId, request.IedId, request.Kind,
            request.RecordPath, "Pending", DateTimeOffset.UtcNow.AddMinutes(15).ToString("O"), null);
        Execute("INSERT INTO commands VALUES($id,$station,$ied,$kind,$path,$state,$expires,NULL)",
            ("$id", row.Id), ("$station", row.StationId), ("$ied", row.IedId), ("$kind", row.Kind),
            ("$path", row.RecordPath), ("$state", row.State), ("$expires", row.ExpiresAt));
        return row;
    }
    // Delivery is at least once. Pending commands remain visible until acknowledged.
    public List<CommandRow> Commands(string? station = null)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT * FROM commands" + (station == null ? "" : " WHERE station=$station AND state='Pending' AND expires > $now") + " ORDER BY rowid DESC";
        if (station != null) { cmd.Parameters.AddWithValue("$station", station); cmd.Parameters.AddWithValue("$now", Now); }
        using var r = cmd.ExecuteReader(); var rows = new List<CommandRow>();
        while (r.Read()) rows.Add(new(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
            r.IsDBNull(4) ? null : r.GetString(4), r.GetString(5), r.GetString(6), r.IsDBNull(7) ? null : r.GetString(7)));
        return rows;
    }
    public void CompleteCommand(string id, string station, CommandResult result) => Execute("""
        UPDATE commands SET state=$state,result=$result WHERE id=$id AND station=$station AND state='Pending'
        """, ("$id", id), ("$station", station), ("$state", result.Success ? "Completed" : "Failed"), ("$result", result.Detail));
}
