using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Fdc.Collector.Pilot;

public static class InventoryView
{
    public static void Write(TextWriter output, InventoryPreview preview)
    {
        string Html(string? s) => WebUtility.HtmlEncode(s ?? "UNKNOWN");
        var filter = ("""
            document.querySelector("#filters").addEventListener("input", () => {
              const fields = [...document.querySelectorAll("#filters input")];
              for (const row of document.querySelectorAll("tbody tr")) {
                row.hidden = fields.some(field => !row.cells[Number(field.dataset.column)].textContent
                  .toLowerCase().includes(field.value.trim().toLowerCase()));
              }
            });
            """).Replace("\r\n", "\n", StringComparison.Ordinal);
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(filter)));
        output.Write("""
            <!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width">
            <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src 'unsafe-inline'; script-src 'sha256-FILTER_HASH'">
            <title>Lab inventory preview</title><style>
            body{font:16px system-ui;margin:2rem;color:#173344} table{border-collapse:collapse;width:100%}
            th,td{padding:.55rem;text-align:left;border-bottom:1px solid #d3dde4}section{overflow:auto}
            summary{cursor:pointer;padding:.4rem}code{overflow-wrap:anywhere}
            </style><h1>Lab inventory preview</h1><p>Private local report. Imported inventory and test authorization are separate.</p>
            """.Replace("FILTER_HASH", hash, StringComparison.Ordinal));
        output.Write($"<p>Preview hash <code>{Html(preview.Id)}</code></p><p>Accepted {preview.Devices.Count}. Issues {preview.Issues.Count}.</p>");
        output.Write("<div id=\"filters\"><label>Bay <input data-column=\"1\"></label> <label>Search device <input data-column=\"2\"></label> <label>IPv4 <input data-column=\"4\"></label> <label>Vendor <input data-column=\"5\"></label> <label>Type <input data-column=\"6\"></label> <label>Status <input data-column=\"7\"></label></div>");
        output.Write("<section><h2>Device inventory</h2><table><thead><tr><th>Row</th><th>Bay</th><th>Device</th><th>Code</th><th>IPv4</th><th>Vendor</th><th>Type</th><th>Status</th></tr></thead><tbody>");
        foreach (var d in preview.Devices)
            output.Write($"<tr><td>{d.Row}</td><td>{Html(d.Bay)}</td><td>{Html(d.Device)}</td><td>{Html(d.Code)}</td><td>{Html(d.Address)}</td><td>{d.Vendor}</td><td>{d.DeviceType}</td><td>{d.CapabilityStatus}</td></tr>");
        output.Write("</tbody></table></section><h2>Validation errors</h2><ul>");
        foreach (var issue in preview.Issues) output.Write($"<li>Row {issue.Row} · {Html(issue.Code)}</li>");
        output.Write("</ul><details><summary>Device details and capabilities</summary><ul>");
        foreach (var d in preview.Devices)
            output.Write($"<li>Row {d.Row} · {Html(d.Code)} · Model {Html(d.Model)} · Firmware {Html(d.Firmware)} · {d.CapabilityStatus}</li>");
        output.Write("</ul></details><details><summary>Connectivity test</summary>BLOCKED until independent Lab approval is verified.</details><details><summary>COMTRADE files</summary>No approved discovery results.</details><details><summary>Download history</summary>No downloads in this import preview.</details><details><summary>Test results</summary>No device test was run by this import.</details>");
        output.Write("<script>" + filter + "</script></html>");
    }
}
