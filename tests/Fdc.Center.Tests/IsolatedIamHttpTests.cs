using System.Collections.Immutable;
using Fdc.Center;
using Fdc.Center.Persistence;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fdc.Identity.Api;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fdc.Center.Tests;

public sealed partial class CenterHttpsSqlLabTests
{
    [CenterSqlLabFact(browser: true)]
    public Task Browser_password_mfa_cookie_profile_logout_and_reuse_against_disposable_sql() =>
        RunBrowserAsync(null);

    [A7BrowserFact]
    public async Task Browser_business_authority_and_download_against_fresh_disposable_sql()
    {
        await using var fixture = await A7BusinessFixture.CreateAsync();
        await RunBrowserAsync(fixture);
    }

    private static async Task RunBrowserAsync(A7BusinessFixture? business)
    {
        var connection = business?.Connection ?? Environment.GetEnvironmentVariable("FDC_IAM_LAB_CONNECTION")!;
        var target = new SqlConnectionStringBuilder(connection);
        Assert.Equal("lpc:.", target.DataSource);
        Assert.Equal(business?.Name ?? "FdcIam_DevelopmentLab", target.InitialCatalog);
        await using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(connection).Options);
        if (business is null)
            Assert.Equal(1, await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN DB_NAME()=N'FdcIam_DevelopmentLab' AND CONVERT(nvarchar(128),SERVERPROPERTY('MachineName'))=N'XOTH-ETC-2' AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'Fdc.Disposable' AND TRY_CONVERT(bit,value)=1) THEN 1 ELSE 0 END AS [Value]").SingleAsync());
        Assert.False(await db.Set<UserRow>().AnyAsync());
        await using var tx = business is null ? await db.Database.BeginTransactionAsync() : null;
        var now = DateTimeOffset.UtcNow;
        var epoch = Guid.NewGuid();
        var proofs = new PasswordProofs(1, 256);
        var password = OpaqueProofs.Create();
        var factor = OpaqueProofs.Create();
        var person = new PersonReferenceRow { Id = Guid.NewGuid(), CreatedAt = now, PersonKey = "a5-browser-fixture" };
        var user = new UserRow { Id = Guid.NewGuid(), CreatedAt = now, PersonReferenceId = person.Id,
            NormalizedUsername = business is null ? "A5-BROWSER-FIXTURE" : "demo.operator", Active = true };
        db.AddRange(person, user, new AuthorityVersionRow { Id = Guid.NewGuid(), CreatedAt = now, PolicyConfigured = true, RestoreEpoch = epoch },
            new PasswordCredentialRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = user.Id,
                EncodedHash = proofs.Hash(password), AlgorithmVersion = "test-only" });
        var role = new RoleRow { Id = Guid.NewGuid(), CreatedAt = now, RoleCode = "a5-fixture-role", RoleName = "Test-only role", IsActive = true };
        var permission = new PermissionRow { Id = Guid.NewGuid(), CreatedAt = now, PermissionCode = "a5.fixture.read",
            Module = "fixture", Action = "read", ScopeRequirement = "Station", SoDPolicy = "fixture",
            RiskLevel = "fixture", CatalogVersion = 1, IsApproved = true, IsImplemented = true, ApprovalReference = "test-only" };
        var assignment = new UserRoleRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = user.Id, RoleId = role.Id, Active = true,
            ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddMinutes(5), ApprovalReference = "test-only" };
        db.AddRange(role, permission, assignment, new RolePermissionRow { Id = Guid.NewGuid(), CreatedAt = now,
            RoleId = role.Id, PermissionId = permission.Id });
        await db.SaveChangesAsync();
        if (business is not null) await business.SeedAsync(db, role.Id, assignment.Id, permission.Id, epoch);

        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder(); san.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(san.Build());
        using var certificate = request.CreateSelfSigned(now.AddMinutes(-1), now.AddHours(1));
        using var serverCert = X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pkcs12), null,
            X509KeyStorageFlags.UserKeySet);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(o => o.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(serverCert)));
        builder.Services.AddSingleton(TimeProvider.System);
        if (business is null)
            builder.Services.AddSingleton<IDbContextFactory<IamDbContext>>(new SharedSqlFactory(db, connection));
        else
        {
            builder.Services.AddDbContextFactory<IamDbContext>(options => options.UseSqlServer(connection));
            builder.Services.AddDbContext<CenterDbContext>(options => options.UseSqlServer(connection));
            builder.Services.AddScoped<BusinessAuthorization>();
            builder.Services.AddSingleton<PermissionEvaluator>();
            builder.Services.AddSingleton<IReviewedPermissionPolicy>(business);
            builder.Services.AddSingleton<IReviewedBusinessOperationPolicy>(business);
            builder.Services.AddSingleton<ITrustedResourceResolver>(business);
            builder.Services.AddSingleton<IReviewedBusinessFileSource>(business.Storage);
        }
        builder.Services.AddScoped<IHumanSessionAuthorityReader, SqlSessionAuthorityReader>();
        builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<IamDbContext>>().CreateDbContext());
        builder.Services.AddScoped<IManagementQueries, SqlManagementQueries>();
        builder.Services.AddScoped<IRoleQueries, SqlRoleQueries>();
        builder.Services.AddScoped<ManagementQueryService>();
        builder.Services.AddScoped<RoleManagementService>();
        var access = new TestOnlyReadAuthority(user.Id, role.Id, assignment.Id, permission.PermissionCode);
        builder.Services.AddSingleton<IManagementReadAuthority>(access);
        builder.Services.AddSingleton<IRoleAdministrationAuthority>(access);
        var flow = new TestOnlySqlFlow(db, proofs, epoch, factor);
        builder.Services.AddSingleton<IHumanAuthenticationFlow>(flow);
        builder.Services.AddAuthentication("Fdc.Human").AddScheme<AuthenticationSchemeOptions, HumanSessionHandler>("Fdc.Human", _ => { });
        builder.Services.AddAuthorization(options => options.AddPolicy("Fdc.Human", policy => policy.RequireAuthenticatedUser()));
        builder.Services.AddAntiforgery(options => {
            options.HeaderName = "X-CSRF-Token";
            options.Cookie.Name = "__Host-A5Csrf";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        await using var app = builder.Build();
        using var serial = new SemaphoreSlim(1);
        app.Use(async (context, next) =>
        {
            await serial.WaitAsync(context.RequestAborted);
            try { context.Response.Headers.CacheControl = "no-store"; await next(); }
            finally { serial.Release(); }
        });
        app.UseAuthentication(); app.UseAuthorization();
        Uri? origin = null;
        app.MapHumanAuthenticationCandidates(() => origin ?? throw new InvalidOperationException(), SameSiteMode.Strict);
        if (business is not null)
        {
            app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "../../../../../src/Fdc.Center/wwwroot/identity"))),
                RequestPath = "/identity",
                OnPrepareResponse = response => response.Context.Response.Headers.CacheControl = "no-store"
            });
            app.MapBusinessReadEndpoints();
        }
        app.MapIdentityManagementQueries();
        app.MapIdentityRoleQueries();
        app.MapGet("/fixture/profile", async (HttpContext h, IamDbContext context, CancellationToken ct) =>
        {
            if (!Guid.TryParse(h.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Results.Unauthorized();
            var current = await context.Set<UserRow>().AsNoTracking().Where(u => u.Id == id && u.Active)
                .Select(u => new { u.Id, username = u.NormalizedUsername, u.Active }).SingleOrDefaultAsync(ct);
            return current is null ? Results.Unauthorized() : Results.Ok(current);
        })
            .RequireAuthorization("Fdc.Human");
        app.MapGet("/", () => Results.Content("<!doctype html><meta charset=utf-8><title>Isolated IAM fixture</title><h1>TEST ONLY · IAM HTTP</h1><p id=status>Ready</p>", "text/html"));

        try
        {
            await app.StartAsync();
            origin = new(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
            var script = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tests/Fdc.Center.Tests/browser/isolated-iam.cjs"));
            var start = new System.Diagnostics.ProcessStartInfo("node") { UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true };
            start.ArgumentList.Add(script);
            start.Environment["FDC_A5_ORIGIN"] = origin.ToString();
            start.Environment["FDC_A5_USER"] = user.NormalizedUsername;
            start.Environment["FDC_A5_PASSWORD"] = password;
            start.Environment["FDC_A5_FACTOR"] = factor;
            if (business is not null)
            {
                start.Environment["FDC_A7_IDS"] = System.Text.Json.JsonSerializer.Serialize(business.Ids);
                // Browser waits for a marker; revoke through a fresh SQL context without exposing a test HTTP write route.
                start.Environment["FDC_A7_SIGNAL"] = business.Signal;
            }
            using var browser = System.Diagnostics.Process.Start(start)!;
            var output = browser.StandardOutput.ReadToEndAsync();
            var errors = browser.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try
            {
                if (business is not null) await business.RevokeWhenSignaledAsync(browser, timeout.Token);
                await browser.WaitForExitAsync(timeout.Token);
            }
            catch { browser.Kill(entireProcessTree: true); throw; }
            Assert.True(browser.ExitCode == 0, (await output) + (await errors));

            var session = await db.Set<SessionRow>().AsNoTracking().SingleAsync();
            Assert.True(session.Revoked);
            Assert.NotNull(session.MfaAt);
            Assert.NotNull(await db.Set<ChallengeRow>().AsNoTracking().Select(c => c.ConsumedAt).SingleAsync());
            var audit = await (from e in db.Set<SecurityEventRow>() join o in db.Set<AuditOutboxRow>() on e.Id equals o.EventId
                               orderby e.EventCode select e.EventCode).ToArrayAsync();
            Assert.Equal(business is null ? ["fixture.login.mfa", "fixture.logout"] :
                ["business.record.download.prepared", "business.record.download.prepared", "fixture.login.mfa", "fixture.logout"], audit);
            if (business is not null && Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") is { } businessEvidence)
            {
                using var artifact = System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(businessEvidence, "a7-browser.json")));
                var businessResult = artifact.RootElement.GetProperty("business");
                var expected = new[] { businessResult.GetProperty("correlation").GetString(), businessResult.GetProperty("cfgCorrelation").GetString() };
                var prepared = await (from e in db.Set<SecurityEventRow>() join o in db.Set<AuditOutboxRow>() on e.Id equals o.EventId
                                      where e.EventCode == "business.record.download.prepared" select e.CorrelationId).ToArrayAsync();
                Assert.Equal(expected.Order(StringComparer.Ordinal), prepared.Order(StringComparer.Ordinal));
                await File.WriteAllTextAsync(Path.Combine(businessEvidence, "a7-sql-verification.json"),
                    System.Text.Json.JsonSerializer.Serialize(new { correlation = prepared, outboxLinked = true, grantRevoked = true, sessionRevoked = true }));
            }
            if (business is null && Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") is { } evidence)
                await File.WriteAllTextAsync(Path.Combine(evidence, "a5-sql-verification.json"),
                    """{"fixture":"rollback-only","sessionRevoked":true,"challengeConsumed":true,"auditOutbox":["fixture.login.mfa","fixture.logout"],"correlation":"a5-isolated"}""");
        }
        finally { await app.StopAsync(); if (tx is not null) await tx.RollbackAsync(); }
        if (business is null)
        {
            Assert.False(await db.Set<UserRow>().AsNoTracking().AnyAsync());
            Assert.False(await db.Set<SessionRow>().AsNoTracking().AnyAsync());
        }
    }

    private sealed class TestOnlyReadAuthority(Guid user, Guid role, Guid assignment, string permission)
        : IManagementReadAuthority, IRoleAdministrationAuthority
    {
        public Task<ManagementReadScope> DemandAsync(ManagementReadOperation operation, CancellationToken ct) =>
            Task.FromResult(new ManagementReadScope([user], [role], [assignment], [], [], [],
                ImmutableHashSet.Create(StringComparer.Ordinal, permission)));
        public Task<RoleReadScope> DemandReadAsync(CancellationToken ct) =>
            Task.FromResult(new RoleReadScope([role], [user]));
    }

    private sealed class TestOnlySqlFlow(IamDbContext db, PasswordProofs proofs, Guid epoch, string factor)
        : IHumanAuthenticationFlow, ILoginTransactionStore, ILoginTransaction, IAuthenticationPolicyProvider
    {
        public Task<AuthenticationPolicy> GetAsync(CancellationToken ct) => Task.FromResult(
            new AuthenticationPolicy(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), 1, 1,
                TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), "test-only"));

        public Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct) =>
            new Fdc.Identity.Application.AuthenticationService(this, this, proofs, TimeProvider.System).LoginAsync(username, password, "test-only", ct);
        public async Task<LoginResult> ExecuteAsync(Func<ILoginTransaction, CancellationToken, Task<LoginResult>> action, CancellationToken ct)
        {
            if (db.Database.CurrentTransaction is not null) return await action(this, ct);
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var result = await action(this, ct);
            await tx.CommitAsync(ct);
            return result;
        }
        public Task<bool> IsSourceLimitedAsync(string trustedSourceKey, AuthenticationPolicy policy, CancellationToken ct) => Task.FromResult(false);
        public Task RecordFailureAsync(Guid? userId, string trustedSourceKey, AuthenticationPolicy policy, CancellationToken ct) => Task.CompletedTask;
        public async Task<CredentialCandidate?> ReadForUpdateAsync(string normalizedUsername, CancellationToken ct) =>
            await new SqlPasswordCredentialReader(db).ReadAsync(normalizedUsername, ct) is { } row
                ? new(row.UserId, row.Active, row.EncodedHash, false) : null;

        public async Task<LoginResult> CompletePasswordProofAsync(SessionIssue proposed, AuthenticationPolicy policy, string? upgradedHash, CancellationToken ct)
        {
            var challenge = OpaqueProofs.Create();
            var now = DateTimeOffset.UtcNow;
            db.Add(new ChallengeRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = proposed.UserId, Purpose = "a5-test-only",
                ProofDigest = OpaqueProofs.Digest(challenge, "a5-challenge"), ExpiresAt = now.AddMinutes(1) });
            await db.SaveChangesAsync(ct);
            return new(LoginOutcome.MfaRequired, challenge);
        }

        public async Task<LoginResult> VerifyMfaAsync(string challenge, string factorResponse, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;
            var digest = OpaqueProofs.Digest(challenge, "a5-challenge");
            var record = await db.Set<ChallengeRow>().SingleOrDefaultAsync(c => c.ProofDigest == digest && c.Purpose == "a5-test-only", ct);
            if (record is null || record.ConsumedAt is not null || record.ExpiresAt <= now ||
                !CryptographicOperations.FixedTimeEquals(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(factorResponse)),
                    SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(factor))))
                return new(LoginOutcome.Rejected);
            var user = await db.Set<UserRow>().SingleAsync(u => u.Id == record.UserId, ct);
            if (!user.Active) return new(LoginOutcome.Rejected);
            record.ConsumedAt = now;
            var raw = OpaqueProofs.Create();
            db.Add(new SessionRow { Id = Guid.NewGuid(), CreatedAt = now, UserId = user.Id, HandleDigest = OpaqueProofs.Digest(raw, "session"),
                IdleUntil = now.AddMinutes(2), AbsoluteUntil = now.AddMinutes(3), MfaAt = now, SecurityVersion = user.SecurityVersion,
                GrantVersion = user.GrantVersion, PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1, RestoreEpoch = epoch });
            Audit(user.Id, "fixture.login.mfa", now);
            await db.SaveChangesAsync(ct);
            return new(LoginOutcome.SessionCreated, raw);
        }

        public async Task<bool> LogoutAsync(string currentSessionDigest, CancellationToken ct)
        {
            var session = await db.Set<SessionRow>().SingleOrDefaultAsync(s => s.HandleDigest == currentSessionDigest && !s.Revoked, ct);
            if (session is null) return false;
            session.Revoked = true;
            Audit(session.UserId, "fixture.logout", DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(ct);
            return true;
        }

        private void Audit(Guid userId, string action, DateTimeOffset now)
        {
            var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = userId, EventCode = action,
                OutcomeCode = "fixture", CorrelationId = "a5-isolated", PolicyVersion = 1, RestoreEpoch = epoch };
            db.AddRange(audit, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
        }
    }
}
