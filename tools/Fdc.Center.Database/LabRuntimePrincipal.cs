using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
namespace Fdc.Center.Database;
public static class LabRuntimePrincipal
{
 public static async Task RunAsync(string[] args)
 {
  if(!OperatingSystem.IsWindows()||args.Length!=2||Environment.GetEnvironmentVariable("FDC_CENTER_CONFIRMED_LAB")!="FDC_Center_Lab")throw new InvalidOperationException("Explicit Windows local lab principal provisioning required.");
  var output=Path.GetFullPath(args[1]);if(File.Exists(output))throw new InvalidOperationException("Credential artifact exists; never overwrite or rotate implicitly.");
  var settings=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FDC_CENTER_LAB_CONNECTION"));if(settings.DataSource!="lpc:."||settings.InitialCatalog!="FDC_Center_Lab")throw new InvalidOperationException("Unexpected target.");
  await using var db=new SqlConnection(settings.ConnectionString);await db.OpenAsync();
  await using(var guard=new SqlCommand("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) AND EXISTS(SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId=N'202611080003_UtcAndRuntimeMetadata') THEN 1 ELSE 0 END",db))if(Convert.ToInt32(await guard.ExecuteScalarAsync())!=1)throw new InvalidOperationException("Lab/schema identity guard failed.");
  var login="FdcCenterLab_Runtime_"+Guid.NewGuid().ToString("N");var password=Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))+"aA1!";
  await using(var command=new SqlCommand("DECLARE @sql nvarchar(max)=N'CREATE LOGIN '+QUOTENAME(@name)+N' WITH PASSWORD='+QUOTENAME(@password,CHAR(39))+N',CHECK_POLICY=ON,CHECK_EXPIRATION=ON'; EXEC sys.sp_executesql @sql; SET @sql=N'CREATE USER '+QUOTENAME(@name)+N' FOR LOGIN '+QUOTENAME(@name)+N'; ALTER ROLE fdc_center_runtime ADD MEMBER '+QUOTENAME(@name); EXEC sys.sp_executesql @sql;",db))
  {command.Parameters.AddWithValue("@name",login);command.Parameters.AddWithValue("@password",password);await command.ExecuteNonQueryAsync();}
  var runtime=new SqlConnectionStringBuilder(settings.ConnectionString){UserID=login,Password=password,IntegratedSecurity=false,ApplicationName="Fdc.Center.Lab.Runtime"};
  var plain=Encoding.UTF8.GetBytes(runtime.ConnectionString);try{Directory.CreateDirectory(Path.GetDirectoryName(output)!);await File.WriteAllBytesAsync(output,ProtectedData.Protect(plain,null,DataProtectionScope.CurrentUser));}finally{CryptographicOperations.ZeroMemory(plain);}
  Console.WriteLine("Created least-privilege Lab SQL runtime login/user, only fdc_center_runtime membership. No IAM human account/grant created. Credential encrypted with Windows DPAPI CurrentUser outside Git. Never use this as production secret management.");Console.WriteLine("Encrypted credential artifact: "+output);
 }
}
