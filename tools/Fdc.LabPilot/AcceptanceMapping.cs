using System.Globalization;
using System.Text.Json;
using Fdc.Collector.Pilot;

internal static class AcceptanceMapping
{
    public static void Run(string[] args)
    {
        if (args is not ["acceptance-map", var workbook, var sheet, var header, var station, var output])
            throw new ArgumentException("acceptance-map workbook sheet header station new-private-json");

        var preview = ExcelInventory.Preview(workbook, sheet, int.Parse(header, CultureInfo.InvariantCulture), station);
        var mapping = new
        {
            EvidenceKind = "WORKBOOK_PREVIEW_ONLY",
            PreviewHash = preview.Id,
            preview.Issues,
            Rows = preview.Devices.Select(device => new
            {
                ExcelRow = device.Row,
                device.Station,
                device.Bay,
                Function = device.Code,
                Vendor = device.Vendor.ToString(),
                device.Model,
                DeviceType = device.DeviceType.ToString(),
                IpAddress = device.Address,
                device.IedName,
                device.Firmware,
                device.Protocol,
                SourceDocument = $"{Path.GetFileName(workbook)}#{sheet}:{device.Row}",
                Confidence = "UNVERIFIED_IMPORT",
                VerifiedBy = (string?)null,
                VerifiedAt = (DateTimeOffset?)null,
                device.ApprovedForTesting
            })
        };

        using var file = new FileStream(output, FileMode.CreateNew);
        JsonSerializer.Serialize(file, mapping, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"Private mapping exported. Accepted {preview.Devices.Count}. Issues {preview.Issues.Count}. Approved 0.");
    }
}
