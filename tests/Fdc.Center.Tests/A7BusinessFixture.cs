using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Fdc.Center;
using Fdc.Center.Persistence;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Center.Tests;

internal sealed class A7BrowserFactAttribute : FactAttribute
{
    public A7BrowserFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("FDC_RUN_A7") != "1" ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FDC_A7_SQL_MASTER")))
            Skip = "Explicit fresh disposable SQL and browser opt-in required.";
    }
}

internal sealed class A7BusinessFixture : IAsyncDisposable, IReviewedPermissionPolicy, IReviewedBusinessOperationPolicy, ITrustedResourceResolver
{
    private readonly string master;
    private readonly string root;
    private Guid grantId;
    private bool created;
    public string Name { get; }
    public string Connection { get; }
    public string Signal => Path.Combine(root, "revoke");
    public PrivateComtradeFileSource Storage { get; }
    public object Ids { get; private set; } = new();

    private A7BusinessFixture(SqlConnectionStringBuilder settings)
    {
        Name = "FdcA7_" + Guid.NewGuid().ToString("N");
        settings.Pooling = false;
        settings.InitialCatalog = "master";
        master = settings.ConnectionString;
        settings.InitialCatalog = Name;
        Connection = settings.ConnectionString;
        root = Directory.CreateTempSubdirectory("FdcA7-").FullName;
        Storage = new(root, Directory.CreateDirectory(Path.Combine(root, "spool")).FullName);
    }

    public static async Task<A7BusinessFixture> CreateAsync()
    {
        var settings = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FDC_A7_SQL_MASTER"));
        if (settings.DataSource != "lpc:." || settings.InitialCatalog != "master")
            throw new InvalidOperationException("Exact local master profile required.");
        var fixture = new A7BusinessFixture(settings);
        try
        {
            await using (var master = new SqlConnection(fixture.master))
            {
                await master.OpenAsync();
                await using var identity = new SqlCommand("SELECT CONVERT(nvarchar(128), SERVERPROPERTY('MachineName'))", master);
                if ((string?)await identity.ExecuteScalarAsync() != "XOTH-ETC-2") throw new InvalidOperationException("Unexpected Lab host.");
                await using var create = new SqlCommand($"CREATE DATABASE [{fixture.Name}]", master);
                await create.ExecuteNonQueryAsync();
                fixture.created = true;
            }
            await using var iam = fixture.Iam();
            await iam.Database.OpenConnectionAsync();
            await iam.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_set_session_context @key=N'Fdc.ConfirmedDisposableLab',@value=1; EXEC sys.sp_set_session_context @key=N'Fdc.ExpectedLabDatabase',@value={fixture.Name};");
            await iam.Database.MigrateAsync();
            await using var db = fixture.Center();
            // Model-generated Center fixture only; operational migrations remain guarded to their named target.
            await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            return fixture;
        }
        catch { await fixture.DisposeAsync(); throw; }
    }

    private IamDbContext Iam() => new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(Connection).Options);
    private CenterDbContext Center() => new(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(Connection).Options);

    public async Task SeedAsync(IamDbContext iam, Guid role, Guid assignment, Guid permission, Guid epoch)
    {
        var now = DateTimeOffset.UtcNow;
        T Row<T>(T row) where T : IamRow { row.Id = Guid.NewGuid(); row.CreatedAt = now; return row; }
        var corp = Row(new CorporationRow { Code = "A7-FIXTURE" });
        var unit = Row(new ManagementUnitRow { Code = "A7-FIXTURE", CorporationId = corp.Id });
        var a = Row(new StationRow { Code = "A7-Station-A", ManagementUnitId = unit.Id, HierarchyVersion = 1 });
        var b = Row(new StationRow { Code = "A7-Station-B", ManagementUnitId = unit.Id, HierarchyVersion = 1 });
        var scope = Row(new ScopeRow { Kind = "Station", StationId = a.Id });
        var grant = Row(new StationGrantRow { UserRoleId = assignment, ScopeId = scope.Id, Active = true,
            ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(5), HierarchyVersion = 1, ApprovalReference = "test-only" });
        grantId = grant.Id;
        iam.AddRange(corp, unit, a, b, scope, grant, Row(new GrantFunctionRow { StationGrantId = grant.Id, PermissionId = permission }));
        var download = Row(new PermissionRow { PermissionCode = "a7.fixture.download", Module = "fixture", Action = "download",
            ScopeRequirement = "Station", SoDPolicy = "fixture", RiskLevel = "fixture", CatalogVersion = 1,
            IsApproved = true, IsImplemented = true, ApprovalReference = "test-only" });
        iam.AddRange(download, Row(new RolePermissionRow { RoleId = role, PermissionId = download.Id }),
            Row(new GrantFunctionRow { StationGrantId = grant.Id, PermissionId = download.Id }));
        await iam.SaveChangesAsync();
        await using var db = Center();
        async Task<(Guid Ied, Guid Collector, Guid Record, Guid File, string Digest, Guid Cfg, string CfgDigest)> Objects(StationRow station, string suffix)
        {
            var ied = new IedDevice { StationId = station.Id, Code = "IED-" + suffix, DisplayName = "A7-IED-" + suffix, IsSynthetic = true };
            var collector = new CollectorRegistration { StationId = station.Id, Code = "A7-Collector-" + suffix, Active = true, IsSynthetic = true };
            var manifest = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(station.Code)));
            var record = new DisturbanceRecord { StationId = station.Id, IedId = ied.Id, CollectorId = collector.Id,
                ManifestId = manifest, ManifestSha256 = manifest, RemoteIdentity = "A7-" + suffix, TimeSource = "fixture",
                CollectedAtUtc = now, CorrelationId = "a7-fixture", IsSynthetic = true };
            db.AddRange(ied, collector, record, new CollectorIed { StationId = station.Id, CollectorId = collector.Id,
                IedId = ied.Id, Active = true, IsSynthetic = true });
            var folder = Directory.CreateDirectory(Path.Combine(root, "records", manifest)).FullName;
            var bytes = Encoding.UTF8.GetBytes("1,0,0\n2,1000,1\n");
            await File.WriteAllBytesAsync(Path.Combine(folder, "fixture.dat"), bytes);
            var digest = Convert.ToHexStringLower(SHA256.HashData(bytes));
            var file = new RecordFile { StationId = station.Id, RecordId = record.Id, Name = "fixture.dat", Extension = ".dat",
                SizeBytes = bytes.Length, Sha256 = digest, StorageObjectKey = $"records/{manifest}/fixture.dat",
                ValidationStatus = "Verified", IsSynthetic = true };
            var cfgBytes = Encoding.UTF8.GetBytes("SYNTHETIC,IED,1999\n1,1A,0D\n1,Ia,A,,A,1,0,0,-32768,32767,1,1,P\n50\n1\n1000,2\n01/01/2026,00:00:00.000000\n01/01/2026,00:00:00.000000\nASCII\n1\n");
            await File.WriteAllBytesAsync(Path.Combine(folder, "fixture.cfg"), cfgBytes);
            var cfgDigest = Convert.ToHexStringLower(SHA256.HashData(cfgBytes));
            var cfg = new RecordFile { StationId = station.Id, RecordId = record.Id, Name = "fixture.cfg", Extension = ".cfg",
                SizeBytes = cfgBytes.Length, Sha256 = cfgDigest, StorageObjectKey = $"records/{manifest}/fixture.cfg",
                ValidationStatus = "Verified", IsSynthetic = true };
            db.AddRange(file, cfg,
                new CollectorHeartbeat { StationId = station.Id, CollectorId = collector.Id, ReceivedAtUtc = now,
                    Status = suffix == "A" ? "Healthy" : "Offline", QueueDepth = 1, IsSynthetic = true },
                new IngestionAttempt { StationId = station.Id, RecordId = record.Id, Number = 1,
                    Outcome = suffix == "A" ? "Accepted" : "Failed", CorrelationId = "fixture", IsSynthetic = true });
            await db.SaveChangesAsync();
            return (ied.Id, collector.Id, record.Id, file.Id, digest, cfg.Id, cfgDigest);
        }
        var first = await Objects(a, "A");
        var second = await Objects(b, "B");
        await VerifyOtherRolesAsync(a.Id, b.Id, permission, epoch);
        Ids = new { stationA = a.Id, stationB = b.Id, iedA = first.Ied, iedB = second.Ied, collectorA = first.Collector,
            collectorB = second.Collector, recordA = first.Record, recordB = second.Record, fileA = first.File, fileB = second.File, digestA = first.Digest, cfgA = first.Cfg, cfgB = second.Cfg, cfgDigestA = first.CfgDigest };
    }

    private async Task VerifyOtherRolesAsync(Guid stationA, Guid stationB, Guid permissionId, Guid epoch)
    {
        await using var center = Center();
        await using var tx = await center.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
            .UseSqlServer(center.Database.GetDbConnection()).Options);
        await iam.Database.UseTransactionAsync(tx.GetDbTransaction());
        var now = DateTimeOffset.UtcNow;
        T Row<T>(T row) where T : IamRow { row.Id = Guid.NewGuid(); row.CreatedAt = now; return row; }
        var outcomes = new List<object>();
        var fixtureRoles = new[]
        {
            (User: "demo.system-admin", Permission: false, Station: (Guid?)stationA, Expected: (Guid?)null),
            (User: "demo.security-admin", Permission: true, Station: (Guid?)null, Expected: (Guid?)null),
            (User: "demo.reviewer", Permission: true, Station: (Guid?)stationA, Expected: (Guid?)stationA),
            (User: "demo.approver", Permission: true, Station: (Guid?)stationB, Expected: (Guid?)stationB),
            (User: "demo.auditor", Permission: true, Station: (Guid?)stationB, Expected: (Guid?)stationB),
            (User: "demo.viewer", Permission: true, Station: (Guid?)stationA, Expected: (Guid?)stationA)
        };
        foreach (var sample in fixtureRoles)
        {
            var person = Row(new PersonReferenceRow { PersonKey = sample.User });
            var user = Row(new UserRow { PersonReferenceId = person.Id, NormalizedUsername = sample.User, Active = true });
            var role = Row(new RoleRow { RoleCode = sample.User, RoleName = sample.User, IsActive = true });
            var assignment = Row(new UserRoleRow { UserId = user.Id, RoleId = role.Id, Active = true,
                ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(5), ApprovalReference = "test-only" });
            var digest = OpaqueProofs.Digest(OpaqueProofs.Create(), "session");
            iam.AddRange(person, user, role, assignment, Row(new SessionRow { UserId = user.Id, HandleDigest = digest,
                IdleUntil = now.AddMinutes(5), AbsoluteUntil = now.AddMinutes(5), SecurityVersion = 1, GrantVersion = 1,
                PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = epoch }));
            if (sample.Permission) iam.Add(Row(new RolePermissionRow { RoleId = role.Id, PermissionId = permissionId }));
            if (sample.Station is Guid station)
            {
                var scope = await iam.Set<ScopeRow>().SingleOrDefaultAsync(row => row.Kind == "Station" && row.StationId == station);
                if (scope is null)
                {
                    scope = Row(new ScopeRow { Kind = "Station", StationId = station });
                    iam.Add(scope);
                }
                var grant = Row(new StationGrantRow { UserRoleId = assignment.Id, ScopeId = scope.Id, Active = true,
                    ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(5), HierarchyVersion = 1, ApprovalReference = "test-only" });
                iam.AddRange(grant, Row(new GrantFunctionRow { StationGrantId = grant.Id, PermissionId = permissionId }));
            }
            await iam.SaveChangesAsync();
            var authorization = new BusinessAuthorization(center, this, this, this, new PermissionEvaluator(), TimeProvider.System);
            if (!sample.Permission)
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authorization.DashboardAsync(digest, default));
            else
            {
                var visible = await authorization.ListAsync(digest, BusinessReadOperation.Station, null, 0, 50, default);
                Assert.Equal(sample.Expected is Guid expected ? new[] { expected } : [], visible.Select(row => row.Id));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authorization.DownloadableFilesAsync(digest, Guid.NewGuid(), default));
            }
            outcomes.Add(new { identity = sample.User, scope = sample.Expected == stationA ? "A" : sample.Expected == stationB ? "B" : "none",
                permission = sample.Permission, downloadDenied = true });
        }
        await tx.RollbackAsync();
        if (Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") is { } evidence)
            await File.WriteAllTextAsync(Path.Combine(evidence, "a7-role-sql.json"), System.Text.Json.JsonSerializer.Serialize(outcomes));
    }

    public async Task RevokeWhenSignaledAsync(Process browser, CancellationToken ct)
    {
        while (!File.Exists(Signal))
        {
            if (browser.HasExited) return;
            await Task.Delay(40, ct);
        }
        await using var db = Iam();
        var grant = await db.Set<StationGrantRow>().SingleAsync(row => row.Id == grantId, ct);
        grant.Active = false;
        await db.SaveChangesAsync(ct);
        await File.WriteAllTextAsync(Signal + ".done", "revoked", ct);
    }

    public string DemandPermission(BusinessReadOperation operation, AuthorityVersion version) =>
        operation == BusinessReadOperation.RecordDownload ? "a7.fixture.download" : "a5.fixture.read";
    public PermissionDescriptor? Resolve(PermissionRow row, AuthorityVersion version) =>
        row.PermissionCode is "a5.fixture.read" or "a7.fixture.download"
            ? new(row.PermissionCode, true, true, version.Catalog, false, null) : null;
    public Task<TrustedResource?> ResolveAsync(IamDbContext db, string objectId, AuthorityVersion authority, CancellationToken ct) =>
        throw new InvalidOperationException("Only canonical station scope is used.");

    public async ValueTask DisposeAsync()
    {
        if (!created) { Directory.Delete(root, recursive: true); return; }
        await using var connection = new SqlConnection(master);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"IF DB_ID(N'{Name}') IS NOT NULL DROP DATABASE [{Name}]", connection);
        await command.ExecuteNonQueryAsync();
        await using var check = new SqlCommand("SELECT DB_ID(@name)", connection);
        check.Parameters.AddWithValue("@name", Name);
        if (await check.ExecuteScalarAsync() is not DBNull) throw new InvalidOperationException("Fixture database still exists.");
        Directory.Delete(root, recursive: true);
    }
}
