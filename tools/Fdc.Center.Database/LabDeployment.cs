using System.Security.Cryptography;
using System.Text;
using Fdc.Center.Persistence;
using Fdc.Identity.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace Fdc.Center.Database;
public static class LabDeployment
{
 public static async Task RunAsync(string[] args)
 {
  if(args.Length!=7)throw new ArgumentException("deploy <reviewed-IAM-up-SHA256> <reviewed-Center-up-SHA256> <reviewed-UTC-up-SHA256> <reviewed-SOE-up-SHA256> <reviewed-snapshot-up-SHA256> <reviewed-station-FK-up-SHA256>");
  var connection=Environment.GetEnvironmentVariable("FDC_CENTER_LAB_CONNECTION")??throw new InvalidOperationException("External lab credential required.");
  var settings=new SqlConnectionStringBuilder(connection);
  if(settings.DataSource!="lpc:."||settings.InitialCatalog!="FDC_Center_Lab"||Environment.GetEnvironmentVariable("FDC_CENTER_CONFIRMED_LAB")!="FDC_Center_Lab")throw new InvalidOperationException("Exact confirmed lab required.");
  static string Resource(Type type,string suffix){var a=type.Assembly;using var s=a.GetManifestResourceStream(a.GetManifestResourceNames().Single(n=>n.EndsWith(suffix,StringComparison.Ordinal)))!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var iam=Resource(typeof(IamDbContext),"001_IamFoundation.up.sql");var center=Resource(typeof(CenterDbContext),"002_CenterDomains.up.sql");
  string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
  var stationFk=Resource(typeof(CenterDbContext),"006_OperationalEventStation.up.sql");
  if(!Hash(stationFk).Equals(args[6],StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Operational event station FK migration hash mismatch.");
  var snapshot=Resource(typeof(CenterDbContext),"005_RecordSourceSnapshot.up.sql");
  if(!Hash(snapshot).Equals(args[5],StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Snapshot migration hash mismatch.");
  var soe=Resource(typeof(CenterDbContext),"004_StationSoeEvents.up.sql");
  if(!Hash(soe).Equals(args[4],StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("SOE migration hash mismatch.");
  var utc=Resource(typeof(CenterDbContext),"003_UtcAndRuntimeMetadata.up.sql");
  if(!Hash(utc).Equals(args[3],StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("UTC migration review hash mismatch.");
  if(!Hash(iam).Equals(args[1],StringComparison.OrdinalIgnoreCase)||!Hash(center).Equals(args[2],StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Reviewed migration hash mismatch; no connection opened.");
  settings.InitialCatalog="master";await using var master=new SqlConnection(settings.ConnectionString);await master.OpenAsync();
  async Task<object?> Scalar(SqlConnection c,string sql){await using var cmd=new SqlCommand(sql,c){CommandTimeout=120};return await cmd.ExecuteScalarAsync();}
  if(Convert.ToInt32(await Scalar(master,"SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND CONVERT(int,SERVERPROPERTY('ProductMajorVersion'))=16 AND IS_SRVROLEMEMBER('sysadmin')=1 THEN 1 ELSE 0 END"))!=1)throw new InvalidOperationException("Unexpected SQL instance/version/creator principal.");
  if(await Scalar(master,"SELECT DB_ID(N'FDC_Center_Lab')") is DBNull)
  {
   await Scalar(master,"CREATE DATABASE [FDC_Center_Lab]");
   await Scalar(master,"EXEC [FDC_Center_Lab].sys.sp_addextendedproperty @name=N'Fdc.Disposable',@value=1; EXEC [FDC_Center_Lab].sys.sp_addextendedproperty @name=N'Fdc.Environment',@value=N'DevelopmentLab'; EXEC [FDC_Center_Lab].sys.sp_addextendedproperty @name=N'Fdc.Scope',@value=N'Center foundation only; no effective IAM policy activation';");
  }
  if(Convert.ToInt32(await Scalar(master,"SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process=1 AND database_id=DB_ID(N'FDC_Center_Lab')"))!=0)throw new InvalidOperationException("Target has connected application sessions; stop.");
  settings.InitialCatalog="FDC_Center_Lab";await using var check=new SqlConnection(settings.ConnectionString);await check.OpenAsync();
  if(Convert.ToInt32(await Scalar(check,"SELECT COUNT(*) FROM sys.extended_properties WHERE class=0 AND (name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1 OR name=N'Fdc.Environment' AND CONVERT(nvarchar(128),value)=N'DevelopmentLab')"))!=2)throw new InvalidOperationException("Existing target lacks exact Lab labels.");
  if(Convert.ToInt64(await Scalar(check,"SELECT COALESCE(SUM(p.row_count),0) FROM sys.dm_db_partition_stats p JOIN sys.tables t ON p.object_id=t.object_id WHERE p.index_id IN(0,1) AND t.name<>N'__EFMigrationsHistory'"))!=0)throw new InvalidOperationException("Target has data; no direct deployment allowed.");
  var backupRoot=Convert.ToString(await Scalar(master,"SELECT CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))"))??throw new InvalidOperationException("Backup path missing.");
  var backup=Path.Combine(backupRoot,"FDC_Center_Lab_before_"+Guid.NewGuid().ToString("N")+".bak");
  await using(var b=new SqlCommand("BACKUP DATABASE [FDC_Center_Lab] TO DISK=@path WITH COPY_ONLY,CHECKSUM; RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM",master){CommandTimeout=120}){b.Parameters.AddWithValue("@path",backup);await b.ExecuteNonQueryAsync();}
  await check.CloseAsync();
  async Task Confirm(DbContext db)
  {
   await db.Database.OpenConnectionAsync();await db.Database.ExecuteSqlRawAsync("EXEC sys.sp_set_session_context @key=N'Fdc.ConfirmedDisposableLab',@value=1; EXEC sys.sp_set_session_context @key=N'Fdc.ExpectedLabDatabase',@value=N'FDC_Center_Lab';");
  }
  await using(var db=new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(settings.ConnectionString).Options)){await Confirm(db);await db.Database.MigrateAsync();}
  await using(var db=new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(settings.ConnectionString).Options)){await Confirm(db);await db.Database.MigrateAsync();}
  Console.WriteLine("Explicit lab migrations completed. Predeployment COPY_ONLY/CHECKSUM backup verified. No source DB data copied; no IAM users/roles/permissions/sessions/grants seeded.");
  Console.WriteLine("Backup artifact: "+backup);
 }
}
