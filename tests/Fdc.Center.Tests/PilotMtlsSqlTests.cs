using System.Net;
using System.Security.Cryptography;
using Fdc.Center;
using Fdc.Center.Persistence;
using Fdc.Collector.Pilot;
using Fdc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fdc.Center.Tests;

public sealed partial class CenterDatabaseSqlTests
{
    [CenterFoundationFact]
    public async Task OFFLINE_FIXTURE_through_SIMULATED_pinned_mtls_API_to_SQL_outbox_and_verified_edge_ack()
    {
        await using var f = await Fixture.Create();
        using var server = PilotMachineBoundaryTests.Certificate("SIMULATED-server", "1.3.6.1.5.5.7.3.1");
        using var collector = PilotMachineBoundaryTests.Certificate("SIMULATED-collector", "1.3.6.1.5.5.7.3.2");
        var pin = collector.GetCertHashString(HashAlgorithmName.SHA256);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(new PilotMachineBoundaryTests.FixtureCertificatePin(pin));
        builder.Services.AddAuthentication(PilotIngestionEndpoints.MachineScheme).AddScheme<AuthenticationSchemeOptions, PilotMachineBoundaryTests.PinnedFixtureHandler>(PilotIngestionEndpoints.MachineScheme, _ => { });
        builder.Services.AddAuthorization(); builder.Services.AddScoped(_ => f.Db());
        builder.Services.AddSingleton<IMachineRecordAuthority>(new FixtureMachine(f));
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System); builder.Services.AddScoped<SqlRecordIngestion>();
        builder.Services.AddScoped(provider => new PilotFileIntake(Path.Combine(f.Root, "https-intake"),
            new(1048576, 4194304, 1024, 10, TimeSpan.FromMinutes(5), new(1048576, 1048576, 32, 32, 10000, 100), "UTF-8", 12),
            provider.GetRequiredService<IMachineRecordAuthority>(), provider.GetRequiredService<SqlRecordIngestion>(), TimeProvider.System, true));
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(new HttpsConnectionAdapterOptions
        { ServerCertificate = server, ClientCertificateMode = ClientCertificateMode.AllowCertificate, ClientCertificateValidation = (peer, _, _) => peer.GetCertHashString(HashAlgorithmName.SHA256) == pin })));
        await using var app = builder.Build(); app.UseAuthentication(); app.UseAuthorization(); app.MapPilotIngestionContract();
        try
        {
            var source = await f.Package(); var edge = Path.Combine(f.Root, "https-edge");
            var q = new PilotTransferQueue(edge, TimeProvider.System);
            using var adapter = new OfflineSqlFixtureFiles(Path.Combine(f.Root, "records", source.Manifest.Id));
            var spool = Path.Combine(edge, "spool");
            var record = new PilotAcquisition(q, spool, new(1048576, 1048576, 32, 32, 10000, 100), "UTF-8", 1024, 1048576,
                TimeProvider.System, new(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1)))
                .Collect(adapter, new(f.StationCode, "IED-SYNTHETIC", IPAddress.Loopback, 1, "/", null, null, null), 3, default).Single();
            await app.StartAsync();
            var url = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            var now = DateTimeOffset.UtcNow;
            var profile = new PilotCenterProfile(new Uri(url), server.GetCertHashString(HashAlgorithmName.SHA256), "SIMULATED", "TEST-NOT-OWNER-APPROVAL",
                now.AddMinutes(-1), now.AddMinutes(3), TimeSpan.FromSeconds(10), true);
            using var uploader = new PilotUploadClient(profile, collector, spool, new LoopbackTransferFixture(), TimeProvider.System);
            var lease = q.Claim("Upload", TimeSpan.FromMinutes(1))!;
            await uploader.UploadAndAcknowledgeAsync(q, lease, default);
            Assert.Equal("Acknowledged", q.Status("Upload", 10).Single().State);
            Assert.True(File.Exists(Path.Combine(record.Package.Directory, "test.dat")));
            // Lost/duplicate client completion never creates a second metadata record or outbox item.
            var replay = await uploader.UploadAsync(record.Package, default);
            await using var check = f.Db();
            var row = await check.Set<DisturbanceRecord>().SingleAsync();
            Assert.Equal(replay.CenterRecordId, row.Id); Assert.Equal(record.Manifest.Id, row.ManifestId);
            Assert.Equal(2, await check.Set<IngestionAttempt>().CountAsync()); Assert.Single(await check.Set<ProcessingOutbox>().ToListAsync());
            using var pending = new PilotUploadClient(profile, collector, spool, new PendingPilotCenterTransferAuthorization(), TimeProvider.System);
            await Assert.ThrowsAsync<InvalidOperationException>(() => pending.UploadAsync(record.Package, default));
        }
        finally { await app.StopAsync(); SqliteConnection.ClearAllPools(); }
    }
    private sealed class LoopbackTransferFixture : IPilotCenterTransferAuthorization
    {
        public void Demand(PilotCenterProfile profile, Manifest manifest, DateTimeOffset now)
        {
            if (!profile.SimulatedLoopback || !IPAddress.IsLoopback(IPAddress.Parse(profile.Origin.Host)) || profile.ReleaseId != "SIMULATED")
                throw new InvalidOperationException("No real Center permitted by fixture.");
        }
    }
}
