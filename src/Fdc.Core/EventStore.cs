using System.Text.Json;

namespace Fdc.Core;
public sealed partial class Store
{
    public void AddEvents(IReadOnlyList<SoeEvent> events, bool delivered = false)
    {
        using var db = Open(); using var transaction = db.BeginTransaction();
        foreach (var item in events)
        {
            using var cmd = db.CreateCommand(); cmd.Transaction = transaction;
            cmd.CommandText = "INSERT OR IGNORE INTO events VALUES($id,$station,$ied,$received,$payload,$delivered)";
            cmd.Parameters.AddWithValue("$id", item.Id); cmd.Parameters.AddWithValue("$station", item.StationId);
            cmd.Parameters.AddWithValue("$ied", item.IedId); cmd.Parameters.AddWithValue("$received", item.ReceivedAt.ToString("O"));
            cmd.Parameters.AddWithValue("$payload", JsonSerializer.Serialize(item)); cmd.Parameters.AddWithValue("$delivered", delivered ? 1 : 0);
            cmd.ExecuteNonQuery();
        }
        transaction.Commit();
    }
    public List<SoeEvent> Events(bool pendingOnly = false, int limit = 500, string? ied = null)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT payload FROM events WHERE ($ied IS NULL OR ied=$ied) "
            + (pendingOnly ? "AND delivered=0 " : "") + "ORDER BY received " + (pendingOnly ? "ASC" : "DESC") + " LIMIT $limit";
        cmd.Parameters.AddWithValue("$ied", (object?)ied ?? DBNull.Value); cmd.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 2000));
        using var r = cmd.ExecuteReader(); var result = new List<SoeEvent>();
        while (r.Read()) result.Add(JsonSerializer.Deserialize<SoeEvent>(r.GetString(0))!);
        return result;
    }
    public void EventsDelivered(IEnumerable<string> ids)
    {
        using var db = Open(); using var tx = db.BeginTransaction();
        foreach (var id in ids)
        {
            using var cmd = db.CreateCommand(); cmd.Transaction = tx;
            cmd.CommandText = "UPDATE events SET delivered=1 WHERE id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }
    public string? ReportCursor(string ied, string rcb)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = """
            SELECT json_extract(payload,'$.EntryId') FROM events
            WHERE ied=$ied AND json_extract(payload,'$.RcbReference')=$rcb
              AND json_extract(payload,'$.EntryId') IS NOT NULL
              AND COALESCE(json_extract(payload,'$.MoreSegments'),0)=0
              AND json_extract(payload,'$.BufferOverflow')=0
            ORDER BY rowid DESC LIMIT 1
            """;
        cmd.Parameters.AddWithValue("$ied", ied); cmd.Parameters.AddWithValue("$rcb", rcb);
        return cmd.ExecuteScalar() as string;
    }
}
