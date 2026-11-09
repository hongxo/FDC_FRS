using System.Globalization;
using System.Text.Json;
using Fdc.Collector.Pilot;

internal static class InventoryCommands
{
    public static void Run(string[] args)
    {
        if (args is ["inventory-review", var db, var reviewFile])
        {
            var review = JsonSerializer.Deserialize<InventoryReview>(File.ReadAllText(reviewFile))
                ?? throw new InvalidDataException("Review required.");
            new InventoryStore(db).RecordReview(review);
            Console.WriteLine("Classification review recorded. Independent test approval is pending.");
            return;
        }
        if (args.Length < 6 || args[0] is not ("inventory-preview" or "inventory-commit"))
            throw new ArgumentException("inventory-preview workbook sheet header station private.html [mapping.json] | inventory-commit workbook sheet header station db reviewedHash [mapping.json|-] [--valid-only]");
        var commit = args[0] == "inventory-commit";
        if ((!commit && args.Length > 7) || (commit && (args.Length < 7 || args.Length > 9 || (args.Length == 9 && args[8] != "--valid-only"))))
            throw new ArgumentException("Invalid inventory arguments.");
        var index = commit ? 7 : 6;
        var mapping = args.Length > index && args[index] != "-" ? JsonSerializer.Deserialize<InventoryMapping>(File.ReadAllText(args[index])) : null;
        var preview = ExcelInventory.Preview(args[1], args[2], int.Parse(args[3], CultureInfo.InvariantCulture), args[4], mapping);
        if (commit)
            Console.WriteLine(new InventoryStore(args[5]).Commit(preview, args[6], args.Length == 9) ? "Imported" : "Already imported");
        else
        {
            using var output = new StreamWriter(new FileStream(args[5], FileMode.CreateNew));
            InventoryView.Write(output, preview);
            Console.WriteLine($"Preview {preview.Id}. Accepted rows {preview.Devices.Count}. Issues {preview.Issues.Count}.");
        }
    }
}
