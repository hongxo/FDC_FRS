using Fdc.Center.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Fdc.Center.Tests;
public sealed class DatabaseModelTests
{
 private static CenterDbContext Db()=>new(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
 [Fact] public void All_29_domain_tables_have_key_rowversion_restrict_and_utc_constraints()
 {
  using var db=Db();var types=db.Model.GetEntityTypes().Where(e=>typeof(CenterRow).IsAssignableFrom(e.ClrType)).ToArray();Assert.Equal(29,types.Length);
  foreach(var e in types){Assert.NotNull(e.FindPrimaryKey());Assert.True(e.FindProperty("RowVersion")!.IsConcurrencyToken);Assert.All(e.GetForeignKeys(),f=>Assert.Equal(DeleteBehavior.Restrict,f.DeleteBehavior));}
  var script=db.Database.GenerateCreateScript();Assert.Contains("DATEPART(TZOFFSET",script);Assert.DoesNotContain("CREATE TABLE [iam]",script);Assert.DoesNotContain("CREATE TABLE [catalog].[Station]",script);
 }
 [Fact] public void Station_consistency_is_enforced_by_composite_foreign_keys()
 {
  using var db=Db();foreach(var type in new[]{typeof(CollectorIed),typeof(DisturbanceRecord),typeof(RecordFile),typeof(FaultEventRecord)})
   Assert.Contains(db.Model.FindEntityType(type)!.GetForeignKeys(),f=>f.Properties.Count>1&&f.Properties.Any(p=>p.Name=="StationId"));
 }
 [Fact] public void New_schemas_do_not_copy_iam_authority_or_store_binary_waveforms()
 {
  using var db=Db();Assert.DoesNotContain(db.Model.GetEntityTypes(),e=>e.GetSchema()=="iam");Assert.DoesNotContain(db.Model.GetEntityTypes().SelectMany(e=>e.GetProperties()),p=>p.ClrType==typeof(byte[])&&p.Name!="RowVersion");
 }
}
