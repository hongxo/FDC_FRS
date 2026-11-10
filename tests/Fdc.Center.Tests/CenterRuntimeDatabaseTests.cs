using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Fdc.Center;
using Fdc.Center.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Fdc.Center.Tests;
[Collection("Center database lab")]
public sealed class CenterRuntimeDatabaseTests : Fdc.Testing.A10IamSqlTest
{
 private static bool Owned => Environment.GetEnvironmentVariable("FDC_RUN_A11_RUNTIME") == "1";
 private static string LegacyConnection()
 {
  if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException();var path=Environment.GetEnvironmentVariable("FDC_CENTER_RUNTIME_SECRET_FILE")!;var plain=ProtectedData.Unprotect(File.ReadAllBytes(path),null,DataProtectionScope.CurrentUser);try{return Encoding.UTF8.GetString(plain);}finally{CryptographicOperations.ZeroMemory(plain);}
 }
 [CenterRuntimeFact] public async Task Dedicated_runtime_principal_denies_authority_dml_secret_reads_and_unscoped_aggregates()
 {
  await using var profile = Owned ? await A11RuntimeProfile.CreateAsync() : null;
  profile?.AssertWrongEntropyDenied();
  await using var db=new SqlConnection(profile?.ReadConnection() ?? LegacyConnection());await db.OpenAsync();await using(var cmd=new SqlCommand("SELECT COALESCE(IS_SRVROLEMEMBER('sysadmin'),0)+COALESCE(IS_MEMBER('db_owner'),0)",db))Assert.Equal(0,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
  foreach(var sql in new[]{"SELECT * FROM iam.PasswordCredential","SELECT * FROM iam.MfaCredential","DELETE FROM iam.Role","UPDATE iam.AuthorityVersion SET PolicyConfigured=1","DELETE FROM audit.SecurityEvent","SELECT * FROM ingest.vRecordSummary","SELECT * FROM catalog.vStationInventory","DELETE FROM ingest.DisturbanceRecord"})
  {await using var command=new SqlCommand(sql,db);var error=await Assert.ThrowsAsync<SqlException>(()=>command.ExecuteNonQueryAsync());Assert.Equal(229,error.Number);}
 }
 [CenterRuntimeFact] public async Task Actual_center_starts_against_new_database_with_non_sa_principal_and_pending_boundaries()
 {
  await using var profile = Owned ? await A11RuntimeProfile.CreateAsync() : null;
  using var key=RSA.Create(2048);var req=new CertificateRequest("CN=localhost",key,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);var names=new SubjectAlternativeNameBuilder();names.AddIpAddress(IPAddress.Loopback);req.CertificateExtensions.Add(names.Build());req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection{new("1.3.6.1.5.5.7.3.1")},true));
  using var original=req.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1),DateTimeOffset.UtcNow.AddHours(1));using var cert=X509CertificateLoader.LoadPkcs12(original.Export(X509ContentType.Pkcs12),null,X509KeyStorageFlags.UserKeySet|X509KeyStorageFlags.Exportable);
  var root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../src/Fdc.Center"));
  await using var app=CenterApplication.Build(new WebApplicationOptions{Args=[],EnvironmentName="Testing",ContentRootPath=root},b=>{b.Configuration.AddInMemoryCollection(new Dictionary<string,string?>{["IdentityRuntime:Enabled"]="true",["CenterDatabase:Enabled"]="true",["ConnectionStrings:FdcIdentity"]=profile?.ReadConnection() ?? LegacyConnection()});if(profile is not null)b.Services.AddSingleton(new CenterPersistenceRegistration.OwnedRuntimeFixture(profile.RunId));b.Logging.ClearProviders();b.WebHost.ConfigureKestrel(s=>s.Listen(IPAddress.Loopback,0,l=>l.UseHttps(cert)));});
  try
  {
   await app.StartAsync();using var scope=app.Services.CreateScope();Assert.NotNull(scope.ServiceProvider.GetRequiredService<CenterDbContext>());Assert.IsType<PendingStationDataAuthority>(scope.ServiceProvider.GetRequiredService<IStationDataAuthority>());
   var address=app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();var pin=cert.GetCertHashString(HashAlgorithmName.SHA256);using var transport=new HttpClientHandler{UseCookies=false,AllowAutoRedirect=false,ServerCertificateCustomValidationCallback=(_,peer,_,_)=>peer!=null&&peer.GetCertHashString(HashAlgorithmName.SHA256)==pin};using var client=new HttpClient(transport){BaseAddress=new Uri(address)};
   using var ui=await client.GetAsync("/identity/");Assert.Equal(HttpStatusCode.OK,ui.StatusCode);
   // A high-entropy but nonexistent session forces real SQL lookup on this new DB, not just no-cookie short circuit.
   client.DefaultRequestHeaders.Add("Cookie","__Host-FdcSession="+Fdc.Identity.Security.OpaqueProofs.Create());using var api=await client.GetAsync("/api/identity/v1/admin/users");Assert.Equal(HttpStatusCode.Unauthorized,api.StatusCode);using var machine=await client.GetAsync("/api/stations/SYNTHETIC");Assert.Equal(HttpStatusCode.Forbidden,machine.StatusCode);
  }
  finally{await app.StopAsync();}
 }
 [Fact] public void Center_database_cannot_be_enabled_with_sa_or_legacy_keys()
 {
  Assert.Throws<InvalidOperationException>(()=>CenterApplication.Build(new WebApplicationOptions{Args=[],EnvironmentName="Testing"},b=>b.Configuration.AddInMemoryCollection(new Dictionary<string,string?>{["CenterDatabase:Enabled"]="true"})));
  Assert.Throws<InvalidOperationException>(()=>CenterApplication.Build(new WebApplicationOptions{Args=[],EnvironmentName="Testing"},b=>b.Configuration.AddInMemoryCollection(new Dictionary<string,string?>{["CenterDatabase:Enabled"]="true",["IdentityRuntime:Enabled"]="true",["ConnectionStrings:FdcIdentity"]="Server=lpc:.;Database=FDC_Center_Lab;User ID=sa"})));
 }
}
internal sealed class CenterRuntimeFactAttribute:FactAttribute
{
 public CenterRuntimeFactAttribute(){if(OperatingSystem.IsWindows() && Fdc.Testing.A10IamSqlTest.Enabled && Environment.GetEnvironmentVariable("FDC_RUN_A11_RUNTIME")=="1")return;if(Environment.GetEnvironmentVariable("FDC_CENTER_CONFIRMED_LAB")!="FDC_Center_Lab"||string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FDC_CENTER_RUNTIME_SECRET_FILE")))Skip="Explicit Center runtime lab/DPAPI artifact required.";}
}
