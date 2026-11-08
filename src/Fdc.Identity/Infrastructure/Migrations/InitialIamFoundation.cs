using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fdc.Identity.Infrastructure.Migrations;

// Reviewed design activation and confirmed lab session context are required before explicit execution.
// Kept separate from Fdc.Center; no service registration or startup migration is performed.
[DbContext(typeof(IamDbContext))]
[Migration("202611080001_InitialIamFoundation")]
public sealed class InitialIamFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => AddBatches(migrationBuilder, "001_IamFoundation.up.sql");
    protected override void Down(MigrationBuilder migrationBuilder) => AddBatches(migrationBuilder, "001_IamFoundation.down.sql");
    private static void AddBatches(MigrationBuilder builder, string suffix)
    {
        var assembly = typeof(InitialIamFoundation).Assembly;
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith(suffix, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException("Missing migration artifact.");
        using var reader = new StreamReader(stream);
        foreach (var batch in Regex.Split(reader.ReadToEnd(), @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            if (!string.IsNullOrWhiteSpace(batch)) builder.Sql(batch);
    }
}
