using System.Collections.Immutable;
using System.IO.Compression;
using System.Net;
using System.Security;
using Fdc.Collector.Pilot;
using Xunit;

namespace Fdc.Collector.Pilot.Tests;

public sealed class ExcelInventoryTests
{
    [Fact]
    public void Preview_keeps_source_rows_and_distinguishes_plc()
    {
        using var fixture = new Workbook(["1"," e01 ","s7-1200","plc-a","192.0.2.1"], [],
            ["2","E02","REF620","relay-b","192.0.2.2"]);
        var p = fixture.Preview();
        Assert.Empty(p.Issues);
        Assert.Equal(new[] { 2, 4 }, p.Devices.Select(d => d.Row));
        Assert.Equal(LabDeviceType.PLC, p.Devices[0].DeviceType);
        Assert.Equal(LabVendor.SIEMENS, p.Devices[0].Vendor);
        Assert.Equal("E01", p.Devices[0].Bay);
        Assert.Equal(LabDeviceType.PROTECTION_RELAY, p.Devices[1].DeviceType);
        Assert.All(p.Devices, d => { Assert.False(d.TestEnabled); Assert.False(d.ApprovedForTesting); Assert.Equal("NOT_TESTED", d.CapabilityStatus); });
        Assert.False(File.Exists(fixture.Database));
    }

    [Fact]
    public void Reports_invalid_ip_and_both_sides_of_duplicate_ip_and_code()
    {
        using var fixture = new Workbook(["1","A","Unknown","I1","192.0.2.1"],
            ["2","A","Unknown","I2","192.0.2.1"], ["3","A","Unknown","I2","192.0.2.2"],
            ["4","A","Unknown","I3","192.000.2.1"]);
        var p = fixture.Preview();
        Assert.Empty(p.Devices);
        Assert.Equal(2, p.Issues.Count(i => i.Code == "DUPLICATE_IP"));
        Assert.Equal(2, p.Issues.Count(i => i.Code == "DUPLICATE_CODE"));
        Assert.Contains(new InventoryIssue(5, "INVALID_IPV4"), p.Issues);
    }

    [Fact]
    public void Commit_requires_preview_hash_and_is_idempotent()
    {
        using var fixture = new Workbook(["1","A","P643","R1","192.0.2.1"]);
        var p = fixture.Preview();
        var store = new InventoryStore(fixture.Database);
        Assert.Throws<InvalidOperationException>(() => store.Commit(p, "wrong"));
        Assert.False(File.Exists(fixture.Database));
        Assert.True(store.Commit(p, p.Id));
        Assert.False(store.Commit(fixture.Preview(), p.Id));
    }

    [Fact]
    public void Imported_and_manually_reviewed_device_cannot_open_adapter_without_independent_authority()
    {
        using var fixture = new Workbook(["1","A","P643","R1","192.0.2.1"]);
        var p = fixture.Preview();
        var store = new InventoryStore(fixture.Database);
        store.Commit(p, p.Id);
        store.RecordReview(new(p.Id, 2, "fixture-reviewer", "fixture-reference", LabVendor.SCHNEIDER,
            LabDeviceType.PROTECTION_RELAY, "P643", "R1", null, true, true));
        var target = new IedTarget("LAB", "R1", IPAddress.Parse("192.0.2.1"), 102, "/records", null, null, null);
        var now = DateTimeOffset.UtcNow;
        var profile = new LabReadProfile("fixture", "fixture", now.AddMinutes(-1), now.AddMinutes(1),
            ImmutableHashSet.Create(AcquisitionSafety.Endpoint(target)), 1000, 1000, 10, 10000, 10000, TimeSpan.FromSeconds(1));
        var scope = new InventoryTestScope(p.Id, 2, "fixture-supervisor", "stop", "read", profile,
            ImmutableHashSet.Create("/records/event.cfg", "/records/event.dat"), 1);
        var gate = new InventoryTestGate(store, new PendingInventoryScopeAuthority(), TimeProvider.System);
        var opened = false;
        Assert.Throws<UnauthorizedAccessException>(() => gate.Probe(p, target, scope, () =>
        {
            opened = true;
            throw new InvalidOperationException();
        }, default));
        Assert.False(opened);
    }

    [Fact]
    public void Preview_html_encodes_workbook_data()
    {
        using var fixture = new Workbook(["1","A","<script>alert(1)</script>","R1","192.0.2.1"]);
        using var output = new StringWriter();
        InventoryView.Write(output, fixture.Preview());
        Assert.Contains("&lt;SCRIPT&gt;", output.ToString());
        Assert.DoesNotContain("<SCRIPT>", output.ToString());
    }

    private sealed class Workbook : IDisposable
    {
        private readonly string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
        public string Database => path + ".sqlite";
        public Workbook(params string[][] rows)
        {
            using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
            void Add(string name, string text)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open());
                writer.Write(text);
            }
            Add("[Content_Types].xml", """<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>""");
            Add("_rels/.rels", """<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
            Add("xl/workbook.xml", """<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="IP" sheetId="1" r:id="rId1"/></sheets></workbook>""");
            Add("xl/_rels/workbook.xml.rels", """<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/></Relationships>""");
            var all = new[] { new[] { "STT", "NGĂN", "THIẾT BỊ", "CHỨC NĂNG", "ĐỊA CHỈ IP" } }.Concat(rows);
            Add("xl/worksheets/sheet1.xml", "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>" +
                string.Concat(all.Select((r, i) => $"<row r=\"{i + 1}\">" +
                    string.Concat(r.Select((v, j) => $"<c r=\"{(char)('A' + j)}{i + 1}\" t=\"inlineStr\"><is><t>{SecurityElement.Escape(v)}</t></is></c>")) +
                    "</row>")) + "</sheetData></worksheet>");
        }
        public InventoryPreview Preview() => ExcelInventory.Preview(path, "IP", 1, "LAB");
        public void Dispose()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            File.Delete(path);
            File.Delete(Database);
        }
    }
}
