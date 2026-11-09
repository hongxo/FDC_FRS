using System.Globalization;
using System.Net;
using System.Text.Json;
using Fdc.Core.Lab;
using Fdc.Collector.Pilot;

internal static class ReadinessCommands
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void Run(string[] args)
    {
        if (args is ["readiness-evaluate", var draft, var output])
        {
            var input = new FileInfo(draft);
            if (input.Length > 128 * 1024) throw new InvalidDataException("Draft size limit.");
            using var source = input.OpenRead();
            var permit = JsonSerializer.Deserialize<LabPermitDraft>(source, new JsonSerializerOptions
            {
                UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
            }) ?? throw new InvalidDataException("Draft required.");
            var result = LabReadinessGate.Evaluate(permit, DateTimeOffset.UtcNow);
            using var target = new FileStream(output, FileMode.CreateNew);
            JsonSerializer.Serialize(target, result, JsonOptions);
            Console.WriteLine(result.Status);
            return;
        }
        if (args is not ["readiness-inventory", var workbook, var sheet, var header, var station, var html])
            throw new ArgumentException("readiness-inventory workbook sheet header station new-private.html | readiness-evaluate private-draft.json new-result.json");

        var preview = ExcelInventory.Preview(workbook, sheet, int.Parse(header, CultureInfo.InvariantCulture), station);
        var rows = preview.Devices.Select(d => new
        {
            InventoryId = $"{preview.Id}:{d.Row}",
            SourceFile = Path.GetFileName(workbook), SourceSheet = preview.Sheet, SourceRow = d.Row,
            StationCode = d.Station, BayCode = d.Bay, FunctionCode = d.Code, Vendor = d.Vendor.ToString(),
            d.Model, d.Firmware, DeviceType = d.DeviceType.ToString(), d.IedName, IpAddress = d.Address,
            InterfaceName = (string?)null, MmsEndpoint = (string?)null, FileDirectory = (string?)null,
            VerificationStatus = "PENDING_VERIFICATION", VerifiedBy = (string?)null,
            VerifiedAtUtc = (DateTimeOffset?)null, EvidenceReference = (string?)null
        });
        var data = JsonSerializer.Serialize(new { PreviewHash = preview.Id, SourceFile = Path.GetFileName(workbook), SourceSheet = preview.Sheet, Devices = rows, preview.Issues });
        var assembly = typeof(ReadinessCommands).Assembly;
        using var template = new StreamReader(assembly.GetManifestResourceStream(
            assembly.GetManifestResourceNames().Single(name => name.EndsWith(".ReadinessUi.html", StringComparison.Ordinal)))!);
        using var targetHtml = new StreamWriter(new FileStream(html, FileMode.CreateNew));
        targetHtml.Write(template.ReadToEnd().Replace("__INVENTORY_DATA__", WebUtility.HtmlEncode(data), StringComparison.Ordinal));
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            Accepted = preview.Devices.Count,
            Issues = preview.Issues.GroupBy(issue => issue.Code).ToDictionary(group => group.Key, group => group.Count()),
            MissingFields = preview.Issues.SelectMany(issue => issue.MissingFields).GroupBy(field => field)
                .ToDictionary(group => group.Key, group => group.Count()),
            InvalidFields = preview.Issues.SelectMany(issue => issue.InvalidFields).GroupBy(field => field)
                .ToDictionary(group => group.Key, group => group.Count()),
            MissingModel = preview.Devices.Count(d => d.Model is null),
            MissingFirmware = preview.Devices.Count(d => d.Firmware is null)
        }));
    }
}
