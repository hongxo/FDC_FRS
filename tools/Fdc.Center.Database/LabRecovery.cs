using Fdc.Center.Persistence;
using Microsoft.Data.SqlClient;
namespace Fdc.Center.Database;
public static class LabRecovery
{
 public static async Task RunAsync(string[] args)
 {
  if(args.Length!=1||Environment.GetEnvironmentVariable("FDC_CENTER_CONFIRMED_LAB")!="FDC_Center_Lab")throw new InvalidOperationException("Explicit lab recovery test required.");
  var settings=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FDC_CENTER_LAB_CONNECTION"));if(settings.DataSource!="lpc:."||settings.InitialCatalog!="FDC_Center_Lab")throw new InvalidOperationException("Unexpected target.");
  await using var db=new SqlConnection(settings.ConnectionString);await db.OpenAsync();
  async Task<object?> Scalar(SqlConnection c,string sql){await using var cmd=new SqlCommand(sql,c){CommandTimeout=120};return await cmd.ExecuteScalarAsync();}
  if(Convert.ToInt32(await Scalar(db,"SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) AND NOT EXISTS(SELECT 1 FROM sys.dm_db_partition_stats p JOIN sys.tables t ON p.object_id=t.object_id WHERE p.index_id IN(0,1) AND p.row_count>0 AND t.name<>N'__EFMigrationsHistory') THEN 1 ELSE 0 END"))!=1)throw new InvalidOperationException("Recovery probe requires exact empty disposable Center lab.");
  settings.InitialCatalog="master";await using var master=new SqlConnection(settings.ConnectionString);await master.OpenAsync();var suffix=Guid.NewGuid().ToString("N");var restore="FDC_Center_Lab_Restore_"+suffix;
  var backup=Path.Combine(Convert.ToString(await Scalar(master,"SELECT CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))"))!,restore+".bak");
  await using(var cmd=new SqlCommand("BACKUP DATABASE FDC_Center_Lab TO DISK=@path WITH COPY_ONLY,CHECKSUM; RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM",master){CommandTimeout=120}){cmd.Parameters.AddWithValue("@path",backup);await cmd.ExecuteNonQueryAsync();}
  var files=new List<(string Logical,string Physical)>();await using(var cmd=new SqlCommand("SELECT name,type_desc FROM FDC_Center_Lab.sys.database_files",master))await using(var r=await cmd.ExecuteReaderAsync())while(await r.ReadAsync())
  {var isLog=r.GetString(1)=="LOG";files.Add((r.GetString(0),isLog?"log":"data"));}
  var dataRoot=Convert.ToString(await Scalar(master,"SELECT CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultDataPath'))"))!;var logRoot=Convert.ToString(await Scalar(master,"SELECT CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultLogPath'))"))!;
  if(await Scalar(master,"SELECT DB_ID(N'"+restore+"')") is not DBNull)throw new InvalidOperationException("Restore target already exists; never overwrite.");
  string Quote(string s)=>"N'"+s.Replace("'","''",StringComparison.Ordinal)+"'";
  var moves=files.Select((f,index)=>"MOVE "+Quote(f.Logical)+" TO "+Quote(Path.Combine(f.Physical=="log"?logRoot:dataRoot,restore+"_"+index+(f.Physical=="log"?".ldf":".mdf"))));
  await Scalar(master,"RESTORE DATABASE ["+restore+"] FROM DISK="+Quote(backup)+" WITH "+string.Join(",",moves)+",RECOVERY,CHECKSUM");
  settings.InitialCatalog=restore;await using var restored=new SqlConnection(settings.ConnectionString);await restored.OpenAsync();
  var tables=Convert.ToInt32(await Scalar(restored,"SELECT COUNT(*) FROM sys.tables WHERE name<>N'__EFMigrationsHistory'"));if(tables!=53)throw new InvalidOperationException("Restored schema count mismatch.");await Scalar(restored,"DBCC CHECKDB WITH PHYSICAL_ONLY,NO_INFOMSGS");await Scalar(restored,"DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS");
  // Probe empty-domain rollback transactionally on the exact authorized source; no committed DROP, IAM untouched.
  await using(var tx=(SqlTransaction)await db.BeginTransactionAsync())
  {
   var a=typeof(CenterDbContext).Assembly;using var stream=a.GetManifestResourceStream(a.GetManifestResourceNames().Single(n=>n.EndsWith("002_CenterDomains.down.sql",StringComparison.Ordinal)))!;using var reader=new StreamReader(stream);var down=reader.ReadToEnd();using var soeStream=a.GetManifestResourceStream(a.GetManifestResourceNames().Single(n=>n.EndsWith("004_StationSoeEvents.down.sql",StringComparison.Ordinal)))!;using var soeReader=new StreamReader(soeStream);down=soeReader.ReadToEnd()+"\nGO\n"+down;
   foreach(var batch in System.Text.RegularExpressions.Regex.Split(down,@"^\s*GO\s*$",System.Text.RegularExpressions.RegexOptions.Multiline|System.Text.RegularExpressions.RegexOptions.IgnoreCase))if(!string.IsNullOrWhiteSpace(batch))
   {await using var cmd=new SqlCommand("EXEC sys.sp_set_session_context @key=N'Fdc.ConfirmedDisposableLab',@value=1;EXEC sys.sp_set_session_context @key=N'Fdc.ExpectedLabDatabase',@value=N'FDC_Center_Lab';\n"+batch,db,tx);await cmd.ExecuteNonQueryAsync();}
   await using(var count=new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name<>N'__EFMigrationsHistory'",db,tx))if(Convert.ToInt32(await count.ExecuteScalarAsync())!=24)throw new InvalidOperationException("Rollback failed to preserve canonical IAM.");await tx.RollbackAsync();
  }
  if(Convert.ToInt32(await Scalar(db,"SELECT COUNT(*) FROM sys.tables WHERE name<>N'__EFMigrationsHistory'"))!=53)throw new InvalidOperationException("DDL rollback transaction was not restored.");
  Console.WriteLine("PASS backup CHECKSUM/VERIFYONLY, restore to unique new disposable DB,53 tables,physical CHECKDB,constraints; empty-domain down probe24 then ROLLBACK53. No existing DB overwritten/dropped.");Console.WriteLine("Retained disposable restore: "+restore);Console.WriteLine("Backup artifact: "+backup);
 }
}
