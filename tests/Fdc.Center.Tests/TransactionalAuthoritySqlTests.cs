using System.Data;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Center.Tests;

[Collection("Center database lab")]
public sealed class TransactionalAuthoritySqlTests
{
    [CenterFoundationFact]
    public async Task Borrowed_authority_read_observes_transaction_local_revoke_and_never_commits_fixture()
    {
        await using var disposable = A9DisposableSql.Enabled ? await A9DisposableSql.CreateAsync() : null;
        await using var db = await OpenGuarded(disposable);
        var userId = Guid.NewGuid();
        await using (var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable))
        {
            var now = DateTimeOffset.UtcNow;
            var person = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = now, PersonKey = "SYNTHETIC-READ-" + userId.ToString("N") };
            var user = new UserRow { Id = userId, CreatedAt = now, PersonReferenceId = person.Id, NormalizedUsername = "SYNTHETIC-READ-" + userId.ToString("N"), Active = true };
            var epoch = Guid.NewGuid();
            var digest = OpaqueProofs.Digest(OpaqueProofs.Create(), "session");
            var session = new SessionRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = user.Id, HandleDigest = digest,
                IdleUntil = now.AddMinutes(5), AbsoluteUntil = now.AddMinutes(10), SecurityVersion = 1, GrantVersion = 1,
                PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = epoch };
            db.AddRange(person, user, session, new AuthorityVersionRow { Id = Guid.NewGuid(), CreatedAt = now, RestoreEpoch = epoch, PolicyConfigured = true });
            await db.SaveChangesAsync();
            var resource = new SameTransactionResource(db, tx);
            var reader = new SqlTransactionalAuthorityReader(new PendingPermissionPolicy(), resource, TimeProvider.System);
            var current = await reader.ReadAsync(db, digest, "SYNTHETIC-OBJECT", default);
            Assert.NotNull(current);
            Assert.Equal(userId, current.Value.Snapshot.User.Id);
            Assert.Empty(current.Value.Snapshot.Permissions); // No mapping becomes effective.
            Assert.Equal(1, resource.Calls);
            Assert.Same(tx, db.Database.CurrentTransaction);

            user.GrantVersion++;
            await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync(db, digest, "SYNTHETIC-OBJECT", default));
            await db.SaveChangesAsync();
            Assert.Null(await reader.ReadAsync(db, digest, "SYNTHETIC-OBJECT", default));
            Assert.Equal(1, resource.Calls); // Stale session stops before resource resolution.
            Assert.Same(tx, db.Database.CurrentTransaction);
            await tx.RollbackAsync();
        }
        db.ChangeTracker.Clear();
        Assert.False(await db.Set<UserRow>().AnyAsync(x => x.Id == userId));
        Assert.False(await db.Set<SessionRow>().AnyAsync(x => x.UserId == userId));
        Assert.False(await db.Set<AuthorityVersionRow>().AnyAsync());
    }

    [CenterFoundationFact]
    public async Task Missing_session_read_does_not_consume_or_dispose_borrowed_transaction()
    {
        await using var disposable = A9DisposableSql.Enabled ? await A9DisposableSql.CreateAsync() : null;
        await using var db = await OpenGuarded(disposable);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var reader = new SqlTransactionalAuthorityReader(new PendingPermissionPolicy(), new SameTransactionResource(db, tx), TimeProvider.System);
        Assert.Null(await reader.ReadAsync(db, new string('A', 64), null, default));
        Assert.Same(tx, db.Database.CurrentTransaction);
        Assert.Equal(1, await db.Database.SqlQueryRaw<int>("SELECT 1 AS [Value]").SingleAsync());
        await tx.RollbackAsync();
    }

    [AuthorityProfileFact]
    public async Task Empty_lab_session_miss_profile_is_not_login_or_capacity_acceptance()
    {
        await using var disposable = A9DisposableSql.Enabled ? await A9DisposableSql.CreateAsync() : null;
        await using var guarded = await OpenGuarded(disposable);
        var options = (disposable?.Options<IamDbContext>() ??
            new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(guarded.Database.GetConnectionString())).Options;
        var reader = new SqlSessionAuthorityReader(new ProfileFactory(options));
        var digest = new string('A', 64);
        for (var i = 0; i < 5; i++) Assert.Null(await reader.ReadAsync(digest, default));
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        var cpuBefore = process.TotalProcessorTime;
        var memoryBefore = process.WorkingSet64;
        var workloads = new List<object>();
        foreach (var concurrency in new[] { 1, 10 })
        {
            var measurements = new System.Collections.Concurrent.ConcurrentBag<double>();
            var total = System.Diagnostics.Stopwatch.StartNew();
            await Parallel.ForEachAsync(Enumerable.Range(0, 100), new ParallelOptions { MaxDegreeOfParallelism = concurrency }, async (_, ct) =>
            {
                var timer = System.Diagnostics.Stopwatch.StartNew();
                Assert.Null(await reader.ReadAsync(digest, ct));
                measurements.Add(timer.Elapsed.TotalMilliseconds);
            });
            total.Stop();
            var samples = measurements.Order().ToArray();
            double P(double percentile) => samples[(int)Math.Ceiling(samples.Length * percentile) - 1];
            workloads.Add(new { Concurrency = concurrency, Samples = samples, P50Ms = P(.50), P95Ms = P(.95), P99Ms = P(.99), QueriesPerSecond = samples.Length / total.Elapsed.TotalSeconds, UnexpectedErrors = 0 });
        }
        process.Refresh();
        var output = Environment.GetEnvironmentVariable("FDC_CENTER_EVIDENCE")!;
        Directory.CreateDirectory(output);
        await File.WriteAllTextAsync(Path.Combine(output, "authority-miss-profile.json"), System.Text.Json.JsonSerializer.Serialize(new
        {
            Scope = "Actual SqlSessionAuthorityReader negative lookup only; empty local lab, not successful login/MFA, protected API, full authorization, D09, SLA or fleet capacity acceptance",
            Environment = disposable is null
                ? "Windows; local SQL Server 2022; named lab; one testhost"
                : "Windows; local SQL Server 2022; owned disposable database; one testhost; pooling disabled",
            TimestampUtc = DateTimeOffset.UtcNow, Dataset = "0 users, 0 sessions, 0 configured authority; nonexistent digest; no writes", Warmup = 5,
            Workloads = workloads, ProcessCpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            ProcessWorkingSetBefore = memoryBefore, ProcessWorkingSetAfter = process.WorkingSet64,
            MissingMetrics = "SQL CPU/RAM, disk IO and network counters NOT_COLLECTED; no approved workload or latency thresholds"
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed class ProfileFactory(DbContextOptions<IamDbContext> options) : IDbContextFactory<IamDbContext>
    {
        public IamDbContext CreateDbContext() => new(options);
    }

    private static async Task<IamDbContext> OpenGuarded(A9DisposableSql? disposable)
    {
        if (disposable is not null) return new IamDbContext(disposable.Options<IamDbContext>().Options);
        var connection = Environment.GetEnvironmentVariable("FDC_CENTER_LAB_CONNECTION") ?? throw new InvalidOperationException("Explicit lab connection required.");
        var settings = new SqlConnectionStringBuilder(connection);
        if (settings.DataSource != "lpc:." || settings.InitialCatalog != "FDC_Center_Lab") throw new InvalidOperationException("Unexpected lab target.");
        var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
        try
        {
            var guard = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN DB_NAME()=N'FDC_Center_Lab' AND CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Environment' AND CONVERT(nvarchar(128),value)=N'DevelopmentLab') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
            Assert.Equal(1, guard);
            Assert.False(await db.Set<UserRow>().AnyAsync());
            Assert.False(await db.Set<AuthorityVersionRow>().AnyAsync());
            return db;
        }
        catch { await db.DisposeAsync(); throw; }
    }

    private sealed class SameTransactionResource(IamDbContext owner, IDbContextTransaction tx) : ITrustedResourceResolver
    {
        public int Calls;
        public Task<TrustedResource?> ResolveAsync(IamDbContext db, string objectId, AuthorityVersion authority, CancellationToken ct)
        {
            Assert.Same(owner, db); Assert.Same(tx, db.Database.CurrentTransaction);
            Assert.Same(tx.GetDbTransaction().Connection, db.Database.GetDbConnection());
            Calls++;
            return Task.FromResult<TrustedResource?>(new("SYNTHETIC-STATION", "SYNTHETIC-CORP", "SYNTHETIC-UNIT", authority.Hierarchy, []));
        }
    }
}

internal sealed class AuthorityProfileFactAttribute : FactAttribute
{
    public AuthorityProfileFactAttribute()
    {
        if ((!A9DisposableSql.Enabled && Environment.GetEnvironmentVariable("FDC_CENTER_CONFIRMED_LAB") != "FDC_Center_Lab") ||
            Environment.GetEnvironmentVariable("FDC_RUN_AUTHORITY_PROFILE") != "1" ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FDC_CENTER_EVIDENCE")))
            Skip = "Explicit disposable Lab and negative-lookup profile/evidence opt-in required.";
    }
}
