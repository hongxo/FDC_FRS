using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.RegularExpressions;
namespace Fdc.Center.Persistence.Migrations;
[DbContext(typeof(CenterDbContext))]
[Migration("202611080005_RecordSourceSnapshot")]
public sealed class RecordSourceSnapshot : Migration
{
 protected override void Up(MigrationBuilder b)=>Add(b,"005_RecordSourceSnapshot.up.sql");
 protected override void Down(MigrationBuilder b)=>Add(b,"005_RecordSourceSnapshot.down.sql");
 private static void Add(MigrationBuilder b,string suffix)
 {
  var assembly=typeof(RecordSourceSnapshot).Assembly; using var stream=assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(x=>x.EndsWith(suffix,StringComparison.Ordinal)))!;using var reader=new StreamReader(stream);
  foreach(var sql in Regex.Split(reader.ReadToEnd(),@"^\s*GO\s*$",RegexOptions.Multiline|RegexOptions.IgnoreCase)) if(!string.IsNullOrWhiteSpace(sql))b.Sql(sql);
 }
}
