using System.Text.Json;

namespace Fdc.Collector.Pilot;

public sealed record OfflineDashboardData(string EvidenceKind, DateTimeOffset GeneratedAt,
    IReadOnlyList<PilotCollectedRecord> Records, IReadOnlyList<IedFileMetadata> Discovery,
    IReadOnlyList<object> DownloadHistory, IReadOnlyList<object> UploadStatus, long FreeBytes,
    string Limitation = "Offline artifact only; no authenticated Center dashboard, hardware or SCADA acceptance.");

public static class OfflineDashboard
{
    public static void Write(string outputDirectory, OfflineDashboardData data)
    {
        if (data.EvidenceKind != "OFFLINE_FIXTURE" || data.Records.Any(r => r.EvidenceKind != EvidenceKind.OFFLINE_FIXTURE))
            throw new InvalidOperationException("This unauthenticated offline viewer only accepts explicit offline fixture data.");
        Directory.CreateDirectory(outputDirectory);
        var json = JsonSerializer.Serialize(data); // Default HTML-sensitive escaping, never UnsafeRelaxedJsonEscaping.
        if (System.Text.Encoding.UTF8.GetByteCount(json) > 16 * 1024 * 1024) throw new InvalidDataException("Offline view data bound.");
        File.WriteAllText(Path.Combine(outputDirectory, "index.html"), """
            <!doctype html><html lang="vi"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
            <meta http-equiv="Content-Security-Policy" content="default-src 'none'; script-src 'self'; style-src 'self'; img-src 'self'; connect-src 'none'; object-src 'none'; base-uri 'none'; form-action 'none'">
            <title>FDC offline Lab evidence viewer</title><link rel="stylesheet" href="pilot.css"><script defer src="pilot.js"></script></head>
            <body><header><h1>FDC/FRS - Lab pilot evidence</h1><p id="scope">OFFLINE_FIXTURE - Không xác nhận đã kết nối IED thật hoặc SCADA.</p></header>
            <main><nav aria-label="Pilot screens" id="screens"></nav><h2 id="title">COMTRADE Records</h2>
            <section class="filters" aria-label="Lọc bản ghi"><label>Vendor<select id="vendor"></select></label><label>Station<select id="station"></select></label><label>IED<select id="ied"></select></label><label>Ngày thu thập UTC<input id="date" type="date"></label></section>
            <p id="status" role="status" aria-live="polite"></p><div class="scroll" id="results" tabindex="0"></div>
            <section id="wave" hidden><label>Kênh analog<select id="channel"></select></label><svg id="plot" viewBox="0 0 900 320" role="img" aria-label="Waveform preview"></svg><p id="scaling"></p></section></main>
            <script id="pilot-data" type="application/json">
            """ + json + "\n</script></body></html>");
        var assembly = typeof(OfflineDashboard).Assembly;
        foreach (var (resource, file) in new[] { ("pilot.js", "pilot.js"), ("pilot.css", "pilot.css") })
        {
            using var input = assembly.GetManifestResourceStream("Fdc.Collector.Pilot.Dashboard." + resource) ?? throw new InvalidOperationException("Missing viewer asset.");
            using var output = File.Create(Path.Combine(outputDirectory, file)); input.CopyTo(output);
        }
    }
}
