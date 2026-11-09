using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace Fdc.Collector.Pilot;

public sealed record InventoryReview(string Batch, int Row, string Reviewer, string Reference, LabVendor Vendor,
    LabDeviceType DeviceType, string Model, string IedName, string? Firmware, bool Iec61850, bool FileService);

public sealed class InventoryStore(string database)
{
    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = database, ForeignKeys = true, DefaultTimeout = 10 }.ToString());
        connection.Open();
        return connection;
    }

    public bool Commit(InventoryPreview preview, string reviewedId, bool acceptValidRowsOnly = false)
    {
        if (preview.Id != reviewedId || preview.Devices.Count == 0 || (preview.Issues.Count != 0 && !acceptValidRowsOnly))
            throw new InvalidOperationException("Preview hash, valid rows and explicit handling of row errors are required.");
        using var db = Open();
        using var tx = db.BeginTransaction();
        Execute(db, tx, """
            CREATE TABLE IF NOT EXISTS pilot_inventory_schema(version INTEGER PRIMARY KEY CHECK(version=1));
            INSERT OR IGNORE INTO pilot_inventory_schema VALUES(1);
            CREATE TABLE IF NOT EXISTS pilot_inventory_batches(
                id TEXT PRIMARY KEY, sheet TEXT NOT NULL, header INTEGER NOT NULL, imported_utc TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS pilot_inventory_devices(
                station TEXT NOT NULL, code TEXT NOT NULL, ip TEXT NOT NULL, data TEXT NOT NULL,
                PRIMARY KEY(station,code), UNIQUE(station,ip));
            CREATE TABLE IF NOT EXISTS pilot_inventory_rows(
                batch TEXT NOT NULL REFERENCES pilot_inventory_batches(id), excel_row INTEGER NOT NULL,
                station TEXT, code TEXT, issue TEXT, PRIMARY KEY(batch,excel_row),
                FOREIGN KEY(station,code) REFERENCES pilot_inventory_devices(station,code));
            CREATE TABLE IF NOT EXISTS pilot_inventory_reviews(
                id INTEGER PRIMARY KEY, batch TEXT NOT NULL, excel_row INTEGER NOT NULL,
                reviewed_utc TEXT NOT NULL, data TEXT NOT NULL,
                FOREIGN KEY(batch,excel_row) REFERENCES pilot_inventory_rows(batch,excel_row));
            CREATE INDEX IF NOT EXISTS pilot_inventory_reviews_row ON pilot_inventory_reviews(batch,excel_row,id);
            """);
        if (Execute(db, tx, "INSERT OR IGNORE INTO pilot_inventory_batches VALUES($id,$sheet,$header,$utc)",
            ("$id", preview.Id), ("$sheet", preview.Sheet), ("$header", preview.Header), ("$utc", DateTimeOffset.UtcNow.ToString("O"))) == 0)
            return false;
        foreach (var d in preview.Devices)
        {
            var payload = JsonSerializer.Serialize(d with { Row = 0 });
            Execute(db, tx, "INSERT OR IGNORE INTO pilot_inventory_devices VALUES($station,$code,$ip,$data)",
                ("$station", d.Station), ("$code", d.Code), ("$ip", d.Address), ("$data", payload));
            using var check = Command(db, tx, "SELECT data FROM pilot_inventory_devices WHERE station=$station AND code=$code",
                ("$station", d.Station), ("$code", d.Code));
            if (!Equals(check.ExecuteScalar(), payload)) throw new InvalidOperationException("Existing inventory conflict. Batch rolled back.");
            Execute(db, tx, "INSERT INTO pilot_inventory_rows VALUES($id,$row,$station,$code,NULL)",
                ("$id", preview.Id), ("$row", d.Row), ("$station", d.Station), ("$code", d.Code));
        }
        foreach (var errors in preview.Issues.GroupBy(i => i.Row))
            Execute(db, tx, "INSERT INTO pilot_inventory_rows VALUES($id,$row,NULL,NULL,$issue)",
                ("$id", preview.Id), ("$row", errors.Key), ("$issue", string.Join(",", errors.Select(i => i.Code))));
        tx.Commit();
        return true;
    }

    // Caller must enforce Lab administrator identity. An evidence reference records a review, not authorization.
    public void RecordReview(InventoryReview review)
    {
        if (new[] { review.Reviewer, review.Reference, review.Model, review.IedName }.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 200 || x.Any(char.IsControl)) ||
            review.DeviceType == LabDeviceType.UNKNOWN || !Enum.IsDefined(review.DeviceType) || !Enum.IsDefined(review.Vendor) ||
            (review.FileService && !review.Iec61850) || review.Firmware is { Length: > 120 })
            throw new ArgumentException("Complete manual device review required.");
        using var db = Open();
        using var tx = db.BeginTransaction();
        if (Execute(db, tx, """
            INSERT INTO pilot_inventory_reviews(batch,excel_row,reviewed_utc,data)
            SELECT batch,excel_row,$utc,$data FROM pilot_inventory_rows
            WHERE batch=$batch AND excel_row=$row AND issue IS NULL
            """, ("$utc", DateTimeOffset.UtcNow.ToString("O")), ("$data", JsonSerializer.Serialize(review)),
            ("$batch", review.Batch), ("$row", review.Row)) != 1)
            throw new InvalidOperationException("Review requires an accepted inventory row.");
        tx.Commit();
    }

    public InventoryReview? LatestReview(string batch, int row)
    {
        using var db = Open();
        using var command = Command(db, null, "SELECT data FROM pilot_inventory_reviews WHERE batch=$batch AND excel_row=$row ORDER BY id DESC LIMIT 1",
            ("$batch", batch), ("$row", row));
        return command.ExecuteScalar() is string json ? JsonSerializer.Deserialize<InventoryReview>(json) : null;
    }

    private static SqliteCommand Command(SqliteConnection db, SqliteTransaction? tx, string sql, params (string, object)[] values)
    {
        var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        foreach (var (name, value) in values) cmd.Parameters.AddWithValue(name, value);
        return cmd;
    }
    private static int Execute(SqliteConnection db, SqliteTransaction tx, string sql, params (string, object)[] values)
    {
        using var cmd = Command(db, tx, sql, values);
        return cmd.ExecuteNonQuery();
    }
}
