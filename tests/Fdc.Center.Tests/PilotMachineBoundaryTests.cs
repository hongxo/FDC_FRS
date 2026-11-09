using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using Fdc.Center;
using Fdc.Center.Persistence;
using Fdc.Identity.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fdc.Center.Tests;

public sealed class PilotMachineBoundaryTests
{
    [Fact]
    public async Task Isolated_loopback_https_machine_contract_rejects_human_cookie_forged_headers_and_unsigned_authority()
    {
        using var server = Certificate("server", "1.3.6.1.5.5.7.3.1");
        using var collector = Certificate("SIMULATED-collector", "1.3.6.1.5.5.7.3.2");
        var pin = collector.GetCertHashString(HashAlgorithmName.SHA256);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(new FixtureCertificatePin(pin));
        builder.Services.AddAuthentication(PilotIngestionEndpoints.MachineScheme).AddScheme<AuthenticationSchemeOptions, PinnedFixtureHandler>(PilotIngestionEndpoints.MachineScheme, _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddScoped(_ => new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options));
        builder.Services.AddSingleton<IMachineRecordAuthority, PendingMachineRecordAuthority>();
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddScoped<SqlRecordIngestion>();
        builder.Services.AddScoped(provider => new PilotFileIntake(Path.Combine(Path.GetTempPath(), "FdcPilot-NEVER-CREATED"),
            new(1048576, 4194304, 1024, 10, TimeSpan.FromMinutes(1), new(1048576, 1048576, 32, 32, 1000, 20), "UTF-8", 12),
            provider.GetRequiredService<IMachineRecordAuthority>(), provider.GetRequiredService<SqlRecordIngestion>(), TimeProvider.System, true));
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(new HttpsConnectionAdapterOptions
        { ServerCertificate = server, ClientCertificateMode = ClientCertificateMode.AllowCertificate, ClientCertificateValidation = (peer, _, _) => peer.GetCertHashString(HashAlgorithmName.SHA256) == pin })));
        await using var app = builder.Build(); app.UseAuthentication(); app.UseAuthorization(); app.MapPilotIngestionContract();
        try
        {
            await app.StartAsync();
            var url = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            HttpClient Client(bool cert)
            {
                var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false, ServerCertificateCustomValidationCallback = (_, peer, _, _) => peer?.GetCertHashString(HashAlgorithmName.SHA256) == server.GetCertHashString(HashAlgorithmName.SHA256) };
                if (cert) handler.ClientCertificates.Add(collector);
                return new(handler) { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(5) };
            }
            using var human = Client(false);
            human.DefaultRequestHeaders.Add("Cookie", "__Host-FdcSession=" + Fdc.Identity.Security.OpaqueProofs.Create());
            human.DefaultRequestHeaders.Add("X-Forwarded-Client-Cert", "SIMULATED-collector");
            using var forbidden = await human.GetAsync("/api/pilot/v1/ingestion/" + Guid.NewGuid()); Assert.Equal(HttpStatusCode.Unauthorized, forbidden.StatusCode);
            using var machine = Client(true);
            machine.DefaultRequestHeaders.Add("Cookie", "__Host-FdcSession=" + Fdc.Identity.Security.OpaqueProofs.Create());
            using var mixed = await machine.GetAsync("/api/pilot/v1/ingestion/" + Guid.NewGuid()); Assert.Equal(HttpStatusCode.Forbidden, mixed.StatusCode);
            machine.DefaultRequestHeaders.Remove("Cookie");
            using var malformed = await machine.PostAsync("/api/pilot/v1/ingestion/", new StringContent("{}", System.Text.Encoding.UTF8, "application/json")); Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
            var content = new string('x', 65537);
            using var oversized = await machine.PostAsync("/api/pilot/v1/ingestion/", new StringContent(content, System.Text.Encoding.UTF8, "application/json")); Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversized.StatusCode);
        }
        finally { await app.StopAsync(); }
    }
    internal static X509Certificate2 Certificate(string name, string eku)
    {
        using var key = RSA.Create(2048); var req = new CertificateRequest("CN=" + name, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder(); san.AddIpAddress(IPAddress.Loopback); req.CertificateExtensions.Add(san.Build());
        req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(eku) }, true));
        using var original = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
        return X509CertificateLoader.LoadPkcs12(original.Export(X509ContentType.Pkcs12), null, X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);
    }
    internal sealed record FixtureCertificatePin(string Sha256);
    // Only this test host: actual pinned ephemeral TLS certificate, not mock headers or main-runtime principal.
    internal sealed class PinnedFixtureHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, FixtureCertificatePin pin) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var cert = Context.Connection.ClientCertificate;
            if (cert?.GetCertHashString(HashAlgorithmName.SHA256) != pin.Sha256) return Task.FromResult(AuthenticateResult.NoResult());
            var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, "SIMULATED-CERTIFICATE")], Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
