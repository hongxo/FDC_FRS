using Fdc.Center.Persistence;
using Fdc.Identity.Application;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace Fdc.Center;

public static class CenterPersistenceRegistration
{
 public static void AddCenterLabPersistence(this WebApplicationBuilder builder,string iamConnection)
 {
  if(!builder.Configuration.GetValue<bool>("CenterDatabase:Enabled"))return;
  var options=new SqlConnectionStringBuilder(iamConnection);
  if(options.InitialCatalog!="FDC_Center_Lab"||options.DataSource!="lpc:."||(!options.IntegratedSecurity&&options.UserID.Equals("sa",StringComparison.OrdinalIgnoreCase)))
   throw new InvalidOperationException("Center lab integration requires the explicit local Center database and a non-sa runtime principal.");
  // One configured connection for both contexts; no independent secondary IAM authority is enabled.
  builder.Services.AddDbContext<CenterDbContext>(o=>o.UseSqlServer(iamConnection).EnableSensitiveDataLogging(false));
  builder.Services.AddScoped<IMachineRecordAuthority,PendingMachineRecordAuthority>();
  builder.Services.AddScoped<IStationDataAuthority,PendingStationDataAuthority>();
  builder.Services.AddScoped<SqlSoeIngestion>();
  builder.Services.AddScoped<SqlStationRecords>();builder.Services.AddScoped<SqlRecordIngestion>();
  builder.Services.AddHostedService<CenterRuntimeDatabaseProbe>();
 }
 private sealed class CenterRuntimeDatabaseProbe(IServiceScopeFactory scopes):IHostedService
 {
  public async Task StartAsync(CancellationToken ct)
  {
   using var scope=scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<CenterDbContext>();
   var valid=await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN DB_NAME()=N'FDC_Center_Lab' AND CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND COALESCE(IS_SRVROLEMEMBER('sysadmin'),0)=0 AND COALESCE(IS_MEMBER('db_owner'),0)=0 AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) AND EXISTS(SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id=s.schema_id WHERE s.name=N'ingest' AND t.name=N'DisturbanceRecord') THEN 1 ELSE 0 END AS [Value]").SingleAsync(ct);
   if(valid!=1)throw new InvalidOperationException("Center Lab identity/schema/runtime least-privilege guard failed.");
  }
  public Task StopAsync(CancellationToken ct)=>Task.CompletedTask;
 }
}
