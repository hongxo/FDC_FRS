using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fdc.Center;
using Microsoft.Extensions.Configuration;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fdc.Center.Tests;

// Actual loopback HTTPS -> real Human handler -> real SQL session -> canonical adapter. No mock authentication.
// Session is an explicit rollback-only lab fixture, not a claim that Login/MFA issuance is complete.
public sealed class CenterHttpsSqlLabTests
{
    [CenterSqlLabFact] public async Task Actual_https_and_sql_deny_forged_identity_revoked_cookie_and_unavailable_authority()
    {
        var connection = Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION") ?? throw new InvalidOperationException("Lab required.");
        var settings = new SqlConnectionStringBuilder(connection);
        if (settings.DataSource != "lpc:." || settings.InitialCatalog != "FdcIam_DevelopmentLab") throw new InvalidOperationException("Unexpected target.");
        await using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
        var verified = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND DB_NAME()=N'FdcIam_DevelopmentLab' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) THEN 1 ELSE 0 END AS [Value]").SingleAsync();
        Assert.Equal(1, verified); Assert.False(await db.Set<UserRow>().AnyAsync());
        await using var tx = await db.Database.BeginTransactionAsync();
        var now = DateTimeOffset.UtcNow; var epoch = Guid.NewGuid(); var raw = OpaqueProofs.Create();
        var person = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = now, PersonKey = "http-sql-fixture" };
        var user = new UserRow { Id = Guid.NewGuid(), CreatedAt = now, PersonReferenceId = person.Id, NormalizedUsername = "HTTP-SQL-FIXTURE", Active = true };
        var session = new SessionRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = user.Id, HandleDigest = OpaqueProofs.Digest(raw, "session"),
            IdleUntil = now.AddMinutes(5), AbsoluteUntil = now.AddMinutes(10), SecurityVersion = 1, GrantVersion = 1, PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = epoch };
        db.AddRange(person, user, session, new AuthorityVersionRow { Id = Guid.NewGuid(), CreatedAt = now, PolicyConfigured = true, RestoreEpoch = epoch });
        await db.SaveChangesAsync();
        using var key = RSA.Create(2048);
        var certificateRequest = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var names = new SubjectAlternativeNameBuilder(); names.AddIpAddress(IPAddress.Loopback); certificateRequest.CertificateExtensions.Add(names.Build());
        certificateRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, true));
        using var certificate = certificateRequest.CreateSelfSigned(now.AddMinutes(-1), now.AddDays(1));
        // SChannel needs a user-key context on Windows. No trusted-store install or certificate/key file export;
        // the temporary key context is owned by this disposable imported test certificate.
        using var serverCertificate = X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pkcs12), null, X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);
        var factory = new SharedSqlFactory(db, connection);
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Fdc.Center"));
        await using var app = CenterApplication.Build(new WebApplicationOptions { Args = [], EnvironmentName = "Testing", ContentRootPath = root }, builder =>
        {
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
                ["IdentityRuntime:Enabled"] = "true", ["ConnectionStrings:FdcIdentity"] = connection });
            builder.Logging.ClearProviders(); builder.Logging.AddConsole(); builder.Logging.SetMinimumLevel(LogLevel.Warning);
            builder.WebHost.ConfigureKestrel(server => server.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(serverCertificate)));
            builder.Services.AddSingleton<IDbContextFactory<IamDbContext>>(factory);
        });
        try
        {
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            var pin = certificate.GetCertHashString(HashAlgorithmName.SHA256);
            using var transport = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false,
                ServerCertificateCustomValidationCallback = (_, peer, _, _) => peer is not null && peer.GetCertHashString(HashAlgorithmName.SHA256) == pin };
            using var client = new HttpClient(transport) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.Add("X-Forwarded-User", "forged-admin");
            using var missing = await client.GetAsync("/api/identity/v1/admin/roles/"); Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
            foreach (var route in new[] { "/identity/management.html", "/api/identity/v1/ui/navigation", "/api/lab/v1/readiness" })
            {
                using var anonymous = await client.GetAsync(route);
                Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
            }
            using (var managementPage = await Get(client, raw, "/identity/management.html"))
            {
                Assert.Equal(HttpStatusCode.OK, managementPage.StatusCode);
                Assert.Contains("management.mjs", await managementPage.Content.ReadAsStringAsync());
                Assert.True(managementPage.Headers.CacheControl?.NoStore);
            }
            using (var navigation = await Get(client, raw, "/api/identity/v1/ui/navigation"))
            {
                Assert.Equal(HttpStatusCode.OK, navigation.StatusCode);
                Assert.Equal("{\"screens\":[]}", await navigation.Content.ReadAsStringAsync());
            }
            using (var lab = await Get(client, raw, "/api/lab/v1/readiness"))
            {
                Assert.Equal(HttpStatusCode.ServiceUnavailable, lab.StatusCode);
                Assert.Contains("LAB_READINESS_STATION_POLICY_PENDING", await lab.Content.ReadAsStringAsync());
            }
            using (var mutation = new HttpRequestMessage(HttpMethod.Post, "/api/identity/v1/admin/roles/"))
            {
                mutation.Headers.Add("Cookie", "__Host-FdcSession=" + raw);
                using var denied = await client.SendAsync(mutation);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, denied.StatusCode);
            }
            using (var humanOnMachine = await Get(client, raw, "/api/stations/fixture"))
                Assert.Equal(HttpStatusCode.Forbidden, humanOnMachine.StatusCode);
            using var invalid = await Get(client, OpaqueProofs.Create()); Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
            using var valid = await Get(client, raw);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, valid.StatusCode);
            var body = await valid.Content.ReadAsStringAsync(); Assert.Contains("OWNER_POLICY_REQUIRED", body);
            Assert.False(body.Contains(raw, StringComparison.Ordinal), "Response must not contain raw proof.");
            foreach (var route in new[] { "/api/status", "/api/events", "/api/records/fixture/download", "/api/records/fixture/waveform" })
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, route);
                request.Headers.Add("Cookie", "__Host-FdcSession=" + raw);
                using var response = await client.SendAsync(request);
                Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
                Assert.Contains("STATION_OPERATION_MAPPING_PENDING", await response.Content.ReadAsStringAsync());
            }
            using (var machine = await client.GetAsync("/api/stations/fixture")) Assert.Equal(HttpStatusCode.Forbidden, machine.StatusCode);
            using (var staticUi = await client.GetAsync("/identity/"))
            {
                Assert.Equal(HttpStatusCode.OK, staticUi.StatusCode);
                Assert.Contains("entry.js", await staticUi.Content.ReadAsStringAsync());
                Assert.Contains("no-store", staticUi.Headers.CacheControl!.ToString());
            }
            using (var legacyUi = await client.GetAsync("/dashboard.html")) Assert.Equal(HttpStatusCode.NotFound, legacyUi.StatusCode);
            foreach (var route in new[] { "/api/identity/v1/login", "/api/identity/v1/auth/login" })
            { using var response = await client.GetAsync(route); Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); }
            if (Environment.GetEnvironmentVariable("FDC_RUN_CENTER_BROWSER") == "1")
            {
                var start = new System.Diagnostics.ProcessStartInfo("node") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                start.ArgumentList.Add(Path.Combine(root, "../../tests/Fdc.Center.Tests/browser/live-center.cjs"));
                start.Environment["FDC_CENTER_TEST_URL"] = address;
                start.Environment["FDC_CENTER_TEST_COOKIE"] = raw;
                using var browser = System.Diagnostics.Process.Start(start)!;
                var stdout = browser.StandardOutput.ReadToEndAsync(); var stderr = browser.StandardError.ReadToEndAsync();
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                try { await browser.WaitForExitAsync(deadline.Token); }
                catch { browser.Kill(entireProcessTree: true); throw; }
                Assert.True(browser.ExitCode == 0, (await stdout) + (await stderr));
            }
            var currentIdle = session.IdleUntil;
            session.IdleUntil = now.AddSeconds(-1); await db.SaveChangesAsync();
            using (var expired = await Get(client, raw, "/identity/management.html"))
                Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
            session.IdleUntil = currentIdle;
            user.Active = false; await db.SaveChangesAsync();
            using var disabled = await Get(client, raw); Assert.Equal(HttpStatusCode.Unauthorized, disabled.StatusCode);
            user.Active = true; session.Revoked = true; await db.SaveChangesAsync();
            using var revoked = await Get(client, raw); Assert.Equal(HttpStatusCode.Unauthorized, revoked.StatusCode);
            using (var revokedPage = await Get(client, raw, "/identity/management.html"))
                Assert.Equal(HttpStatusCode.Unauthorized, revokedPage.StatusCode);
            factory.Unavailable = true;
            using var unavailable = await Get(client, raw); Assert.Equal(HttpStatusCode.ServiceUnavailable, unavailable.StatusCode);
            Assert.False((await unavailable.Content.ReadAsStringAsync()).Contains("SqlException", StringComparison.Ordinal));
        }
        finally { await app.StopAsync(); await tx.RollbackAsync(); }
        Assert.False(await db.Set<UserRow>().AsNoTracking().AnyAsync()); Assert.False(await db.Set<SessionRow>().AsNoTracking().AnyAsync());
    }
    private static async Task<HttpResponseMessage> Get(HttpClient client, string proof, string path = "/api/identity/v1/admin/roles/")
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("Cookie", "__Host-FdcSession=" + proof); return await client.SendAsync(request);
    }
    private sealed class SharedSqlFactory(IamDbContext db, string connection) : IDbContextFactory<IamDbContext>
    {
        public volatile bool Unavailable;
        public IamDbContext CreateDbContext()
        {
            if (Unavailable)
            {
                var settings = new SqlConnectionStringBuilder(connection) { InitialCatalog = "FdcIam_Missing_" + Guid.NewGuid().ToString("N"), ConnectTimeout = 2, Pooling = false };
                return new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(settings.ConnectionString).Options);
            }
            var context = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(db.Database.GetDbConnection()).Options);
            context.Database.UseTransaction(db.Database.CurrentTransaction!.GetDbTransaction()); return context;
        }
    }
}


internal sealed class CenterSqlLabFactAttribute : FactAttribute
{
    public CenterSqlLabFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("FDC_IAM_CONFIRMED_LAB") != "FdcIam_DevelopmentLab" || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION")))
            Skip = "Explicit disposable SQL lab authorization and connection required.";
    }
}
