using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ExcelDataReader;
using Fdc.Core;

namespace Fdc.Collector.Pilot;

public sealed record InventoryMapping(string Number = "STT", string Bay = "NGĂN", string Device = "THIẾT BỊ",
    string Function = "CHỨC NĂNG", string Address = "ĐỊA CHỈ IP");
public enum LabDeviceType { UNKNOWN, PROTECTION_RELAY, PLC, RTU, GATEWAY, SCADA_DEVICE, NETWORK_DEVICE, OTHER }
public enum LabVendor { UNKNOWN, ABB, SIEMENS, SCHNEIDER, OTHER }
public sealed record InventoryDevice(int Row, string Number, string Station, string Bay, string Device, string Code,
    string Address, LabVendor Vendor, LabDeviceType DeviceType)
{
    public string? Model { get; init; }
    public string? IedName { get; init; }
    public string? Firmware { get; init; }
    public string? Protocol { get; init; }
    public bool TestEnabled => false;
    public bool ApprovedForTesting => false;
    public string CapabilityStatus => "NOT_TESTED";
}
public sealed record InventoryIssue(int Row, string Code);
public sealed class InventoryPreview
{
    internal InventoryPreview(string id, string sheet, int header, IReadOnlyList<InventoryDevice> devices, IReadOnlyList<InventoryIssue> issues)
    { Id = id; Sheet = sheet; Header = header; Devices = devices; Issues = issues; }
    public string Id { get; }
    public string Sheet { get; }
    public int Header { get; }
    public IReadOnlyList<InventoryDevice> Devices { get; }
    public IReadOnlyList<InventoryIssue> Issues { get; }
}

public static class ExcelInventory
{
    public static InventoryPreview Preview(string file, string sheet, int header, string station, InventoryMapping? mapping = null)
    {
        mapping ??= new();
        if (header is < 1 or > 100 || string.IsNullOrWhiteSpace(sheet) || station is null || !Identity.SafeId(station))
            throw new ArgumentException("Explicit sheet, header row and station are required.");
        var extension = Path.GetExtension(file).ToLowerInvariant();
        if (extension is not (".xls" or ".xlsx")) throw new InvalidDataException("Workbook format must be XLSX or XLS.");
        using var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var length = input.Length;
        if (length is < 1 or > 8388608) throw new InvalidDataException("Workbook size limit.");
        var snapshot = new byte[(int)length];
        input.ReadExactly(snapshot);
        if (input.ReadByte() != -1) throw new InvalidDataException("Workbook changed during preview.");
        using var stream = new MemoryStream(snapshot, writable: false);
        var digest = Convert.ToHexString(SHA256.HashData(stream));
        stream.Position = 0;
        if (extension == ".xlsx")
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
                if (zip.Entries.Count > 256 || zip.Entries.Sum(e => e.Length) > 33554432)
                    throw new InvalidDataException("Workbook expansion limit.");
            stream.Position = 0;
        }
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = extension == ".xlsx" ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
        var devices = new List<InventoryDevice>();
        var issues = new List<InventoryIssue>();
        var found = false;
        do
        {
            if (reader.Name != sheet) continue;
            found = true;
            for (var i = 0; i < header; i++)
                if (!reader.Read()) throw new InvalidDataException("Header row not found.");
            if (reader.FieldCount > 256) throw new InvalidDataException("Workbook column limit.");
            string Cell(int index) => index >= reader.FieldCount ? "" :
                (Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture) ?? "").Trim().Normalize();
            var fields = new[] { mapping.Number, mapping.Bay, mapping.Device, mapping.Function, mapping.Address };
            if (fields.Any(string.IsNullOrWhiteSpace) || fields.Select(f => f.Trim().Normalize()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 5)
                throw new InvalidDataException("Mapping must specify five distinct headers.");
            var columns = fields.Select(field =>
            {
                var matches = Enumerable.Range(0, reader.FieldCount).Where(i =>
                    string.Equals(Cell(i), field.Trim().Normalize(), StringComparison.OrdinalIgnoreCase)).ToArray();
                if (matches.Length != 1) throw new InvalidDataException("A required header is absent or ambiguous.");
                return matches[0];
            }).ToArray();
            for (var row = header + 1; reader.Read(); row++)
            {
                if (row > 10100) throw new InvalidDataException("Workbook row limit.");
                var v = columns.Select(Cell).ToArray();
                if (v.All(string.IsNullOrWhiteSpace)) continue;
                if (v.Any(x => x.Length == 0 || x.Length > 120 || x.Any(char.IsControl)))
                { issues.Add(new(row, "MISSING_OR_INVALID_FIELD")); continue; }
                if (!IsIpv4(v[4])) { issues.Add(new(row, "INVALID_IPV4")); continue; }
                var device = Normalize(v[2]);
                var classification = Classify(device);
                devices.Add(new(row, v[0], station, Normalize(v[1]), device, Normalize(v[3]), v[4], classification.Vendor, classification.Type)
                { Model = classification.Type == LabDeviceType.UNKNOWN ? null : device });
            }
            break;
        } while (reader.NextResult());
        if (!found) throw new InvalidDataException("Sheet not found.");
        foreach (var group in devices.GroupBy(d => d.Address).Where(g => g.Count() > 1))
            issues.AddRange(group.Select(d => new InventoryIssue(d.Row, "DUPLICATE_IP")));
        foreach (var group in devices.GroupBy(d => d.Code).Where(g => g.Count() > 1))
            issues.AddRange(group.Select(d => new InventoryIssue(d.Row, "DUPLICATE_CODE")));
        var invalid = issues.Select(i => i.Row).ToHashSet();
        return new(Identity.Hash(System.Text.Json.JsonSerializer.Serialize(new { digest, sheet, header, station, mapping })),
            sheet, header, Array.AsReadOnly(devices.Where(d => !invalid.Contains(d.Row)).ToArray()), issues.AsReadOnly());
    }

    private static string Normalize(string value) => string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
    private static bool IsIpv4(string value)
    {
        var parts = value.Split('.');
        return parts.Length == 4 && parts.All(p => p.Length is >= 1 and <= 3 && p.All(char.IsAsciiDigit) &&
            (p.Length == 1 || p[0] != '0') && byte.TryParse(p, NumberStyles.None, CultureInfo.InvariantCulture, out _)) &&
            IPAddress.TryParse(value, out var ip) && ip.ToString() == value;
    }

    public static (LabVendor Vendor, LabDeviceType Type) Classify(string model) => Normalize(model) switch
    {
        "S7-1200" or "S7-1500" => (LabVendor.SIEMENS, LabDeviceType.PLC),
        "REF620" => (LabVendor.ABB, LabDeviceType.PROTECTION_RELAY),
        "7SJ85" => (LabVendor.SIEMENS, LabDeviceType.PROTECTION_RELAY),
        "P643" => (LabVendor.SCHNEIDER, LabDeviceType.PROTECTION_RELAY),
        "GRL200" or "SEL_451" => (LabVendor.OTHER, LabDeviceType.PROTECTION_RELAY),
        _ => (LabVendor.UNKNOWN, LabDeviceType.UNKNOWN)
    };
}
