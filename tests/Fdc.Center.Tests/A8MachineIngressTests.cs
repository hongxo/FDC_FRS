using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Fdc.Center;
using Fdc.Center.Persistence;
using Fdc.Core;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fdc.Center.Tests;

public sealed class A8MachineIngressTests
{
    [A7BrowserFact]
    public async Task Fresh_SQL_registry_and_test_CA_allow_only_bound_machine_ingress()
    {
        await using var fixture = await A7BusinessFixture.CreateAsync();
        var now = DateTimeOffset.UtcNow;
        await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(fixture.Connection).Options);
        await using var db = new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(fixture.Connection).Options);
        var corp = new CorporationRow { Id = Guid.NewGuid(), Code = "A8-MACHINE", CreatedAt = now };
        var unit = new ManagementUnitRow { Id = Guid.NewGuid(), CorporationId = corp.Id, Code = "A8-MACHINE", CreatedAt = now };
        var station = new StationRow { Id = Guid.NewGuid(), ManagementUnitId = unit.Id, Code = "A8-STATION", HierarchyVersion = 1, CreatedAt = now };
        iam.AddRange(corp, unit, station);
        await iam.SaveChangesAsync();
        var collector = new CollectorRegistration { Code = "A8-COLLECTOR", StationId = station.Id, Active = true, BindingStatus = "Bound", IsSynthetic = true };
        var ied = new IedDevice { Code = "A8-IED", DisplayName = "Fixture IED", StationId = station.Id, IsSynthetic = true };
        db.AddRange(collector, ied, new CollectorIed { StationId = station.Id, CollectorId = collector.Id, IedId = ied.Id, Active = true, IsSynthetic = true });
        await db.SaveChangesAsync();

        using var rootKey = RSA.Create(2048);
        var rootRequest = new CertificateRequest("CN=A8-test-root", rootKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        using var root = rootRequest.CreateSelfSigned(now.AddHours(-2), now.AddHours(2));
        using var client = Issue(root, TlsCertificates.ClientAuth, now.AddMinutes(-1), now.AddMinutes(30));
        using var wrongEku = Issue(root, TlsCertificates.ServerAuth, now.AddMinutes(-1), now.AddMinutes(30));
        using var expired = Issue(root, TlsCertificates.ClientAuth, now.AddMinutes(-20), now.AddMinutes(-10));
        using var server = PilotMachineBoundaryTests.Certificate("A8-server", TlsCertificates.ServerAuth);
        Assert.True(TlsCertificates.Validate(client, root, TlsCertificates.ClientAuth, false));
        Assert.False(TlsCertificates.Validate(wrongEku, root, TlsCertificates.ClientAuth, false));
        Assert.False(TlsCertificates.Validate(expired, root, TlsCertificates.ClientAuth, false));

        var pin = client.GetCertHashString(HashAlgorithmName.SHA256);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(new PilotMachineBoundaryTests.FixtureCertificatePin(pin));
        builder.Services.AddAuthentication(PilotIngestionEndpoints.MachineScheme)
            .AddScheme<AuthenticationSchemeOptions, PilotMachineBoundaryTests.PinnedFixtureHandler>(PilotIngestionEndpoints.MachineScheme, _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddDbContext<CenterDbContext>(options => options.UseSqlServer(fixture.Connection));
        var registry = new TestRegistry(fixture.Connection, collector.Id);
        builder.Services.AddSingleton<IMachineRecordAuthority>(registry);
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<SqlRecordIngestion>();
        var storage = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(fixture.Signal)!, "machine")).FullName;
        builder.Services.AddScoped(provider => new PilotFileIntake(storage,
            new(1048576, 4194304, 1024, 10, TimeSpan.FromMinutes(5), new(1048576, 1048576, 32, 32, 10000, 100), "UTF-8", 12),
            registry, provider.GetRequiredService<SqlRecordIngestion>(), TimeProvider.System, true));
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(new HttpsConnectionAdapterOptions
        {
            ServerCertificate = server, ClientCertificateMode = ClientCertificateMode.AllowCertificate,
            ClientCertificateValidation = (peer, chain, _) => TlsCertificates.Validate(peer, root, TlsCertificates.ClientAuth, false, chain)
        })));
        await using var app = builder.Build();
        app.UseAuthentication(); app.UseAuthorization(); app.MapPilotIngestionContract();
        try
        {
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            HttpClient Open(bool certificate)
            {
                var handler = new HttpClientHandler { UseCookies = false, ServerCertificateCustomValidationCallback = (_, peer, _, _) =>
                    peer?.GetCertHashString(HashAlgorithmName.SHA256) == server.GetCertHashString(HashAlgorithmName.SHA256) };
                if (certificate) handler.ClientCertificates.Add(client);
                return new(handler) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
            }
            const string route = "/api/pilot/v1/ingestion/";
            var files = new Dictionary<string, string>
            {
                ["a.cfg"] = "SYNTHETIC,IED,1999\n1,1A,0D\n1,Ia,A,,A,1,0,0,-32768,32767,1,1,P\n50\n1\n1000,2\n01/01/2026,00:00:00.000000\n01/01/2026,00:00:00.000000\nASCII\n1\n",
                ["a.dat"] = "1,0,0\n2,1000,1\n"
            };
            var digests = files.Select(row => new FileDigest(row.Key, Encoding.UTF8.GetByteCount(row.Value), Fdc.Core.Identity.Hash(row.Value))).ToArray();
            Manifest Create(string stationCode) => new(Fdc.Core.Identity.Hash(stationCode + "\n" + ied.Code + "\nfixture\n" +
                string.Join('\n', digests.Select(file => file.Name + ":" + file.Sha256))), stationCode, ied.Code, "", "", "", "",
                "fixture", now, null, "fixture", digests);
            var manifest = Create(station.Code);
            using var human = Open(false);
            human.DefaultRequestHeaders.Add("Cookie", "__Host-FdcSession=" + OpaqueProofs.Create());
            human.DefaultRequestHeaders.Add("X-Forwarded-Client-Cert", "fixture-spoof");
            using var humanDenied = await human.PostAsJsonAsync(route, manifest);
            Assert.Equal(HttpStatusCode.Unauthorized, humanDenied.StatusCode);
            using var machine = Open(true);
            using var wrong = await machine.PostAsJsonAsync(route, Create("A8-OTHER"));
            Assert.Equal(HttpStatusCode.Forbidden, wrong.StatusCode);
            Assert.Contains("STATION_BINDING_DENIED", await wrong.Content.ReadAsStringAsync());
            using var accepted = await machine.PostAsJsonAsync(route, manifest);
            Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
            var ticket = await accepted.Content.ReadFromJsonAsync<PilotUploadTicket>();
            Assert.NotNull(ticket);
            foreach (var (name, bytes) in files)
            {
                using var put = await machine.PutAsync(route + ticket.Id + "/files/" + name, new ByteArrayContent(Encoding.UTF8.GetBytes(bytes)));
                Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
            }
            using var complete = await machine.PostAsync(route + ticket.Id + "/complete", null);
            Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
            Assert.Equal(station.Id, (await db.Set<DisturbanceRecord>().SingleAsync()).StationId);
            Assert.Equal(2, await db.Set<RecordFile>().CountAsync());
            Assert.Equal(1, await db.Set<ProcessingOutbox>().CountAsync());

            collector.Active = false;
            await db.SaveChangesAsync();
            using var unregistered = await machine.PostAsJsonAsync(route, manifest);
            Assert.Equal(HttpStatusCode.Forbidden, unregistered.StatusCode);
            registry.Unavailable = true;
            using var outage = await machine.PostAsJsonAsync(route, manifest);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, outage.StatusCode);
            Assert.Contains("AUTHORITY_UNAVAILABLE", await outage.Content.ReadAsStringAsync());
            if (Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") is { } output)
                await File.WriteAllTextAsync(Path.Combine(output, "a8-mtls.json"),
                    System.Text.Json.JsonSerializer.Serialize(new { chain = "ephemeral test CA", eku = TlsCertificates.ClientAuth,
                        wrongEkuDenied = true, expiredDenied = true, human = 401, wrongStation = 403, upload = 201, completion = 200,
                        inactiveCollector = 403, registryUnavailable = 503, records = 1, files = 2, outbox = 1, utc = now }));
        }
        finally { await app.StopAsync(); }
    }

    private static X509Certificate2 Issue(X509Certificate2 root, string eku, DateTimeOffset start, DateTimeOffset end)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=A8-test-client", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(eku) }, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        using var signed = request.Create(root, start, end, RandomNumberGenerator.GetBytes(16));
        using var withKey = signed.CopyWithPrivateKey(key);
        return X509CertificateLoader.LoadPkcs12(withKey.Export(X509ContentType.Pkcs12), null, X509KeyStorageFlags.UserKeySet);
    }

    private sealed class TestRegistry(string connection, Guid pinnedCollector) : IMachineRecordAuthority
    {
        public volatile bool Unavailable;
        public async Task<MachineRecordBinding> DemandAsync(string stationCode, string iedCode, CancellationToken ct)
        {
            if (Unavailable) throw new AuthorityUnavailableException();
            await using var center = new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(connection).Options);
            await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
            var station = await iam.Set<StationRow>().Where(s => s.Code == stationCode).Select(s => s.Id).SingleOrDefaultAsync(ct);
            if (station == Guid.Empty) throw new UnauthorizedAccessException();
            var binding = await (from collector in center.Set<CollectorRegistration>()
                                 join link in center.Set<CollectorIed>() on new { CollectorId = collector.Id, collector.StationId } equals new { link.CollectorId, link.StationId }
                                 join ied in center.Set<IedDevice>() on new { Id = link.IedId, link.StationId } equals new { ied.Id, ied.StationId }
                                 where collector.Id == pinnedCollector && collector.StationId == station && collector.Active &&
                                     collector.BindingStatus == "Bound" && link.Active && ied.Active && ied.Code == iedCode
                                 select new MachineRecordBinding(station, ied.Id, collector.Id)).SingleOrDefaultAsync(ct);
            return binding ?? throw new UnauthorizedAccessException();
        }
    }
}
