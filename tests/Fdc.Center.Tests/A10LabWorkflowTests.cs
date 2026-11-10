using System.Collections.Immutable;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using Fdc.Center;
using Fdc.Center.Persistence;
using Fdc.Identity.Application;
using Fdc.Identity.Api;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Fdc.Testing;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fdc.Center.Tests;

[Collection("Center database lab")]
public sealed class A10LabWorkflowTests : A10IamSqlTest
{
    [A10Fact]
    public async Task Synthetic_lab_workflow_enforces_person_station_revision_permit_and_atomic_history_over_https()
    {
        await using var fixture = await Host.CreateAsync();
        var op = fixture.Client("demo.operator");
        var id = Guid.NewGuid();
        var draft = fixture.Draft();
        var created = await fixture.Create(op, id, draft);
        Assert.Equal("DRAFT", created.State);
        var row = created;
        row = await fixture.Move(op, row, "submit");
        Assert.Equal("SUBMITTED", row.State);
        await fixture.Denied(op, row, "review");
        await fixture.Denied(fixture.Client("demo.operator"), row, "approve");
        await fixture.Denied(fixture.Client("approver.other-station"), row, "approve");
        row = await fixture.Move(fixture.Client("demo.reviewer"), row, "review");
        await fixture.Denied(fixture.Client("demo.reviewer"), row, "approve");
        var stale = row with { ETag = created.ETag };
        await fixture.Denied(fixture.Client("demo.approver"), stale, "approve", HttpStatusCode.Conflict);
        row = await fixture.Move(fixture.Client("demo.approver"), row, "approve");
        await fixture.Denied(fixture.Client("demo.system-admin"), row, "apply");
        await fixture.Denied(fixture.Client("demo.system-admin"), row, "ready");
        await fixture.Denied(fixture.Client("demo.approver"), row, "permit", permitIed: fixture.OtherIed);
        await fixture.Denied(fixture.Client("demo.approver"), row, "permit", testType: "REAL_IED");
        row = await fixture.Move(fixture.Client("demo.approver"), row, "permit");
        row = await fixture.Move(fixture.Client("demo.system-admin"), row, "ready");
        await fixture.Denied(fixture.Client("demo.approver"), row, "apply");
        var request = Guid.NewGuid();
        var beforeApply = row;
        row = await fixture.Move(fixture.Client("demo.system-admin"), row, "apply", request);
        Assert.Equal("COMPLETED", row.State);
        var replay = await fixture.Move(fixture.Client("demo.system-admin"), beforeApply, "apply", request);
        Assert.True(replay.Replay);
        Assert.Equal(row, replay with { Replay = false });
        using var export = await fixture.Client("demo.auditor").GetAsync($"/api/lab/v1/tests/{row.Id}/1/export");
        Assert.Equal(HttpStatusCode.OK, export.StatusCode);
        Assert.True(export.Headers.CacheControl?.NoStore);
        var doc = await export.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("COMPLETED", doc.GetProperty("DraftStatus").GetString());
        Assert.NotNull(doc.GetProperty("ResultDigest").GetString());
        using var hidden = await fixture.Client("approver.other-station").GetAsync($"/api/lab/v1/tests/{row.Id}/1");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        await fixture.AssertAudit(row.Id, 8);
        await fixture.WriteEvidence("lab-http.json", new { State = row.State, Revision = row.Revision,
            SelfReview = 403, AliasAccount = "Denied by SQL unique PersonReference constraint", CrossStation = 403, StaleRevision = 409,
            MissingPermit = 403, WrongIed = 403, WrongType = 403, Apply = 200, Replay = true,
            RequiredHistory = 8, Network = "HTTPS loopback only; execution adapter computes SHA256, has no network dependency" });
    }

    [A10Fact]
    public async Task Synthetic_failure_and_explicit_abort_are_terminal_and_audited()
    {
        await using var fixture = await Host.CreateAsync();
        var row = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), fixture.Draft() with { SyntheticOutcome = "FAILED" });
        row = await fixture.Move(fixture.Client("demo.operator"), row, "submit");
        row = await fixture.Move(fixture.Client("demo.reviewer"), row, "review");
        row = await fixture.Move(fixture.Client("demo.approver"), row, "approve");
        row = await fixture.Move(fixture.Client("demo.approver"), row, "permit");
        row = await fixture.Move(fixture.Client("demo.system-admin"), row, "ready");
        row = await fixture.Move(fixture.Client("demo.system-admin"), row, "apply");
        Assert.Equal("FAILED", row.State);
        await fixture.Denied(fixture.Client("demo.system-admin"), row, "apply");
        var draft = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), fixture.Draft());
        var aborted = await fixture.Move(fixture.Client("demo.system-admin"), draft, "abort");
        Assert.Equal("ABORTED", aborted.State);
        await fixture.Denied(fixture.Client("demo.operator"), aborted, "submit");
        await fixture.WriteEvidence("a10-terminal-outcomes.json", new { PrescribedSyntheticFailure = "FAILED",
            ExplicitAbort = "ABORTED", TerminalReplayMutation = 403, RealExecution = false });
    }

    [A10Fact]
    public async Task Browser_collector_registry_request_review_approve_apply_uses_real_SQL_and_SoD()
    {
        await using var fixture = await Host.CreateAsync();
        var request = await fixture.RunBindingBrowserAsync();
        await fixture.AssertBindingApplied(request);
    }

    [A10Fact]
    public async Task Failed_binding_apply_discards_tracked_mutation_before_same_scope_retry()
    {
        await using var fixture = await Host.CreateAsync();
        var row = await fixture.BindingRequest(fixture.Client("demo.operator"));
        row = await fixture.BindingMove(fixture.Client("demo.reviewer"), row, "review");
        row = await fixture.BindingMove(fixture.Client("demo.approver"), row, "approve");
        await fixture.VerifyBindingRollbackReuse(row);
    }

    [A10Fact]
    public async Task Disposable_migration_rollback_refuses_existing_evidence_and_reverses_empty_optional_schemas()
    {
        await using var fixture = await Host.CreateAsync();
        await fixture.VerifyRollbackAsync();
    }

    [A10Fact]
    public async Task Concurrent_approval_missing_grants_cross_assignment_and_expired_permit_are_denied_atomically()
    {
        await using var fixture = await Host.CreateAsync();
        var row = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), fixture.Draft());
        row = await fixture.Move(fixture.Client("demo.operator"), row, "submit");
        row = await fixture.Move(fixture.Client("demo.reviewer"), row, "review");
        await fixture.Denied(fixture.Client("approver.no-grant"), row, "approve");
        await fixture.Denied(fixture.Client("approver.mixed"), row, "approve");
        await fixture.ConcurrentApprove(row);
        var expiring = fixture.Draft() with { End = fixture.Clock.GetUtcNow().AddSeconds(2) };
        var expired = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), expiring);
        fixture.Clock.Advance(TimeSpan.FromSeconds(3));
        await fixture.Denied(fixture.Client("demo.operator"), expired, "submit");
        await fixture.WriteEvidence("a10-lab-concurrency.json", new { ConcurrentApprove = new[] { 200, 409 },
            NoGrant = 403, CrossAssignment = 403, PermitExpiredWhileSessionCurrent = 403 });
    }

    [A10Fact]
    public async Task Synthetic_collector_registry_change_requires_four_independent_people_and_commits_audit()
    {
        await using var fixture = await Host.CreateAsync();
        var requester = fixture.Client("demo.operator");
        var row = await fixture.BindingRequest(requester);
        await fixture.BindingDenied(requester, row, "review");
        var reviewer = fixture.Client("demo.reviewer");
        row = await fixture.BindingMove(reviewer, row, "review");
        await fixture.BindingDenied(reviewer, row, "approve");
        var approver = fixture.Client("demo.approver");
        row = await fixture.BindingMove(approver, row, "approve");
        await fixture.BindingDenied(approver, row, "apply");
        await fixture.BindingDenied(fixture.Client("approver.other-station"), row, "apply");
        row = await fixture.BindingMove(fixture.Client("demo.system-admin"), row, "apply");
        Assert.Equal("Applied", row.State);
        await fixture.AssertBindingApplied(row.Id);
        await fixture.WriteEvidence("a10-binding.json", new { State = "Applied", IndependentPeople = 4,
            SelfReview = 403, SelfApprove = 403, SelfApply = 403, CrossStation = 403, ConfigurationVersion = 2,
            RegistryStatus = "Revoked", Active = false, AuditOutbox = 4, StationChanged = false, SyntheticOnly = true });
    }

    [A10Fact]
    public async Task Shared_HTTPS_reverse_proxy_and_browser_contexts_do_not_reuse_private_station_data_after_revoke()
    {
        await using var fixture = await Host.CreateAsync();
        var a = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), fixture.Draft());
        var b = await fixture.Create(fixture.Client("operator.other-station"), Guid.NewGuid(), fixture.Draft(true));
        await fixture.RunCacheBrowserAsync(a.Id, b.Id);
    }

    [A10Fact]
    public async Task Authorized_HTTP_path_swap_returns_zero_bytes_and_commits_correlated_denial_audit_outbox()
    {
        await using var fixture = await Host.CreateAsync(swapStorage: true);
        using var response = await fixture.Client("demo.operator").GetAsync("/api/business/v1/record-files/" + fixture.RecordFileId + "/download");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        Assert.True(fixture.Storage!.Opened);
        var correlation = response.Headers.GetValues("X-Correlation-Id").Single();
        await using var db = fixture.Iam();
        Assert.Equal(1, await (from audit in db.Set<SecurityEventRow>()
            join outbox in db.Set<AuditOutboxRow>() on audit.Id equals outbox.EventId
            where audit.EventCode == "business.record.download.verification_denied" &&
                audit.OutcomeCode == "denied" && audit.CorrelationId == correlation select audit.Id).CountAsync());
        await fixture.WriteEvidence("a10-http-path-swap.json", new { Http = 404, Bytes = 0,
            OpenedRedirect = true, SameDigestOutside = true, AuditOutboxCorrelation = correlation });
    }

    [A10Fact]
    public async Task Export_worker_barrier_rechecks_revoked_SQL_authority_and_quarantines_without_HTTP_artifact()
    {
        await using var fixture = await Host.CreateAsync();
        var row = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), fixture.Draft());
        var auditor = fixture.Client("demo.auditor");
        var completed = await fixture.ExportRequest(auditor, row);
        var firstWorker = Guid.NewGuid();
        Assert.True(await fixture.Prepare(completed, firstWorker));
        Assert.False(await fixture.Prepare(completed, Guid.NewGuid()));
        Assert.True(await fixture.Release(completed, firstWorker));
        using (var control = await auditor.GetAsync($"/api/lab/v1/tests/exports/{completed}/download"))
        {
            Assert.Equal(HttpStatusCode.OK, control.StatusCode);
            Assert.NotEmpty(await control.Content.ReadAsByteArrayAsync());
        }
        var raced = await fixture.ExportRequest(auditor, row);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var worker = Guid.NewGuid();
        var operation = Task.Run(async () =>
        {
            Assert.True(await fixture.Prepare(raced, worker));
            ready.SetResult();
            await resume.Task;
            return await fixture.Release(raced, worker);
        });
        try
        {
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await fixture.RevokeActor("demo.auditor");
        }
        finally { resume.TrySetResult(); }
        Assert.False(await operation);
        await using var lab = fixture.Lab();
        var quarantined = await lab.Exports.SingleAsync(job => job.Id == raced);
        Assert.Equal("Quarantined", quarantined.State);
        Assert.Null(quarantined.Payload);
        Assert.Null(quarantined.PayloadDigest);
        foreach (var id in new[] { raced, completed })
        {
            using var response = await auditor.GetAsync($"/api/lab/v1/tests/exports/{id}/download");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        }
        await using var iam = fixture.Iam();
        Assert.Equal(1, await (from audit in iam.Set<SecurityEventRow>()
            join outbox in iam.Set<AuditOutboxRow>() on audit.Id equals outbox.EventId
            where audit.EventCode == "lab.export.quarantined" select audit.Id).CountAsync());
        await fixture.WriteEvidence("a10-export-revoke.json", new { Barrier = "After staging commit, before release transaction",
            CompetingWorker = false, ReleasedAfterRevoke = false, State = "Quarantined", StagedPayload = "Removed",
            HttpAfterRevoke = 401, ArtifactBytes = 0, QuarantineAuditOutbox = 1, CompletedControlBeforeRevoke = 200 });
    }

    [A10Fact]
    public async Task Browser_Lab_workflow_and_idle_expiry_clear_two_sensitive_tabs_against_real_sql()
    {
        await using var fixture = await Host.CreateAsync();
        await fixture.RunBrowserAsync();
        await using var lab = fixture.Lab();
        var row = await lab.Permits.SingleAsync();
        Assert.Equal("COMPLETED", row.DraftStatus);
        await fixture.AssertAudit(row.PermitId, 8);
    }

    [A10Theory]
    [InlineData("absolute")]
    [InlineData("security")]
    [InlineData("revoked")]
    [InlineData("mfa")]
    public async Task Browser_session_authority_changes_clear_DOM_and_ignore_delayed_response(string scenario)
    {
        await using var fixture = await Host.CreateAsync();
        await fixture.RunBrowserAsync(scenario);
        await using var lab = fixture.Lab();
        Assert.Equal("COMPLETED", (await lab.Permits.SingleAsync()).DraftStatus);
    }

    [A10Fact]
    public async Task Expired_permit_revoked_actor_and_outbox_failure_never_commit_a_transition()
    {
        await using var fixture = await Host.CreateAsync();
        var row = await fixture.Create(fixture.Client("demo.operator"), Guid.NewGuid(), fixture.Draft());
        row = await fixture.Move(fixture.Client("demo.operator"), row, "submit");
        fixture.Clock.Advance(TimeSpan.FromHours(2));
        await fixture.Denied(fixture.Client("demo.reviewer"), row, "review", HttpStatusCode.Unauthorized);
        fixture.Clock.Reset();
        await using var db = fixture.Iam();
        var reviewer = await db.Set<UserRow>().SingleAsync(user => user.NormalizedUsername == "demo.reviewer");
        reviewer.GrantVersion++;
        await db.SaveChangesAsync();
        await fixture.Denied(fixture.Client("demo.reviewer"), row, "review", HttpStatusCode.Unauthorized);
        reviewer.GrantVersion--;
        await db.SaveChangesAsync();
        // Transactional failure injection with an additional test trigger. No existing trigger is disabled.
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TRIGGER ops.A10RejectOutbox ON ops.AuditOutbox AFTER INSERT AS
            BEGIN THROW 51499,'Injected A.10 audit outbox failure.',1; END
            """);
        await fixture.Denied(fixture.Client("demo.reviewer"), row, "review", HttpStatusCode.ServiceUnavailable);
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER ops.A10RejectOutbox");
        await using var lab = fixture.Lab();
        var saved = await lab.Permits.SingleAsync();
        Assert.Equal("SUBMITTED", saved.DraftStatus);
        Assert.Null(saved.ReviewerPerson);
        Assert.Equal(2, await lab.History.CountAsync());
        Assert.Equal(2, await db.Set<AuditOutboxRow>().CountAsync());
        await fixture.WriteEvidence("lab-rollback.json", new { ExpiredSession = 401, RevokedAuthority = 401,
            AuditFailure = 503, StateAfterFailure = saved.DraftStatus, History = 2, Outbox = 2 });
    }

    internal sealed class TestClock : TimeProvider
    {
        private readonly DateTimeOffset start = DateTimeOffset.UtcNow;
        private DateTimeOffset current;
        public TestClock() { current = start; }
        public override DateTimeOffset GetUtcNow() => current;
        public void Advance(TimeSpan interval) => current += interval;
        public void Reset() => current = start;
    }

    internal sealed class Host : IAsyncDisposable, IReviewedBusinessOperationPolicy, IReviewedPermissionPolicy,
        ITrustedResourceResolver, ISqlCommitGatePlanProvider, ILabHttpOrigin
    {
        private readonly Dictionary<string, (Guid Id, string Raw)> actors = new();
        private readonly List<HttpClient> clients = [];
        private WebApplication app = null!;
        private X509Certificate2 certificate = null!;
        private Uri origin = null!;
        private readonly Guid station = Guid.NewGuid(), otherStation = Guid.NewGuid(), inventory = Guid.NewGuid(), otherInventory = Guid.NewGuid(), ied = Guid.NewGuid();
        public Guid RecordFileId { get; } = Guid.NewGuid();
        public Guid CollectorId { get; } = Guid.NewGuid();
        public SwapStorage? Storage { get; private set; }
        public Guid OtherIed { get; } = Guid.NewGuid();
        public TestClock Clock { get; } = new();
        public IamDbContext Iam() => new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(Connection).Options);
        public LabWorkflowDbContext Lab() => new(new DbContextOptionsBuilder<LabWorkflowDbContext>().UseSqlServer(Connection).Options);
        public SyntheticLabDraft Draft(bool other = false) => new(other ? otherInventory : inventory, other ? otherStation : station,
            other ? OtherIed : ied, Clock.GetUtcNow().AddMinutes(-1),
            Clock.GetUtcNow().AddMinutes(30), "SYNTHETIC evidence hash only", [new string('a', 64)]);

        public static async Task<Host> CreateAsync(bool swapStorage = false)
        {
            var fixture = new Host();
            try
            {
                if (swapStorage) fixture.Storage = new SwapStorage();
                await fixture.Initialize();
                return fixture;
            }
            catch { await fixture.DisposeAsync(); throw; }
        }

        private async Task Initialize()
        {
            await using (var sql = new SqlConnection(Connection))
            {
                await sql.OpenAsync();
                var name = TargetDatabase;
                await using var context = new SqlCommand("""
                    EXEC sys.sp_set_session_context @key=N'Fdc.ConfirmedDisposableLab',@value=1;
                    EXEC sys.sp_set_session_context @key=N'Fdc.ExpectedLabDatabase',@value=@name;
                    EXEC sys.sp_set_session_context @key=N'Fdc.A10.Run',@value=@run;
                    """, sql);
                context.Parameters.AddWithValue("@name", name);
                context.Parameters.AddWithValue("@run", name["FDC_A10_TEST_".Length..]);
                await context.ExecuteNonQueryAsync();
                await A9DisposableSql.ApplyCenterMigrationsAsync(sql, name);
                var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tools/Fdc.Center.Database/lab-readiness"));
                foreach (var file in new[] { "007_LabReadinessDraft.up.sql", "008_SyntheticWorkflow.up.sql", "009_SyntheticExport.up.sql",
                    "010_SyntheticCollectorBinding.up.sql" })
                {
                    var script = await File.ReadAllTextAsync(Path.Combine(root, file));
                    if (file.StartsWith("007", StringComparison.Ordinal))
                        script = script.Replace("DB_NAME() <> N'FDC_Center_Lab'", $"DB_NAME() <> N'{name}'", StringComparison.Ordinal);
                    await using var tx = (SqlTransaction)await sql.BeginTransactionAsync();
                    foreach (var batch in Regex.Split(script, @"(?im)^\s*GO\s*$"))
                    {
                        if (string.IsNullOrWhiteSpace(batch)) continue;
                        await using var command = new SqlCommand(batch, sql, tx);
                        await command.ExecuteNonQueryAsync();
                    }
                    await tx.CommitAsync();
                }
            }
            await SeedAsync();
            using var key = RSA.Create(2048);
            var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var san = new SubjectAlternativeNameBuilder(); san.AddIpAddress(IPAddress.Loopback); request.CertificateExtensions.Add(san.Build());
            using var original = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(2));
            certificate = X509CertificateLoader.LoadPkcs12(original.Export(X509ContentType.Pkcs12), null, X509KeyStorageFlags.UserKeySet);
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0, listener => listener.UseHttps(certificate)));
            builder.Services.AddDbContextFactory<IamDbContext>(options => options.UseSqlServer(Connection));
            builder.Services.AddDbContext<CenterDbContext>(options => options.UseSqlServer(Connection));
            builder.Services.AddSingleton<TimeProvider>(Clock);
            builder.Services.AddScoped<IHumanSessionAuthorityReader, SqlSessionAuthorityReader>();
            builder.Services.AddAuthentication("Fdc.Human").AddScheme<AuthenticationSchemeOptions, HumanSessionHandler>("Fdc.Human", _ => { });
            builder.Services.AddAuthorization(options => options.AddPolicy("Fdc.Human", policy => policy.RequireAuthenticatedUser()));
            builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-Token"; options.Cookie.Name = "__Host-A10Csrf"; options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always; });
            builder.Services.AddSingleton<IReviewedPermissionPolicy>(this);
            builder.Services.AddSingleton<IReviewedBusinessOperationPolicy>(this);
            builder.Services.AddSingleton<ITrustedResourceResolver>(this);
            builder.Services.AddSingleton<ISqlCommitGatePlanProvider>(this);
            builder.Services.AddSingleton<ILabHttpOrigin>(this);
            builder.Services.AddSingleton<PermissionEvaluator>();
            builder.Services.AddScoped<BusinessAuthorization>();
            builder.Services.AddSingleton<IReviewedBusinessFileSource>(Storage is null ? new PendingBusinessFileSource() : Storage);
            builder.Services.AddScoped<LabWorkflowService>();
            builder.Services.AddScoped<LabExportCoordinator>();
            builder.Services.AddScoped<CollectorBindingService>();
            app = builder.Build();
            app.UseAuthentication(); app.UseAuthorization();
            app.UseStaticFiles(new StaticFileOptions { FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Fdc.Center/wwwroot/identity"))), RequestPath = "/identity" });
            app.MapGet("/api/identity/v1/auth/csrf", (Microsoft.AspNetCore.Http.HttpContext h, IAntiforgery csrf) =>
                Microsoft.AspNetCore.Http.Results.Ok(new { token = csrf.GetAndStoreTokens(h).RequestToken }));
            app.MapLabWorkflow();
            app.MapCollectorBindings();
            app.MapBusinessReadEndpoints();
            await app.StartAsync();
            origin = new(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
        }

        private async Task SeedAsync()
        {
            await using var db = Iam();
            var now = Clock.GetUtcNow();
            T Row<T>(T row) where T : IamRow { row.Id = Guid.NewGuid(); row.CreatedAt = now; return row; }
            var corp = Row(new CorporationRow { Code = "A10-CORP" });
            var unit = Row(new ManagementUnitRow { CorporationId = corp.Id, Code = "A10-UNIT" });
            db.AddRange(corp, unit, new StationRow { Id = station, CreatedAt = now, ManagementUnitId = unit.Id, Code = "A10-A", HierarchyVersion = 1 },
                new StationRow { Id = otherStation, CreatedAt = now, ManagementUnitId = unit.Id, Code = "A10-B", HierarchyVersion = 1 });
            var epoch = Guid.NewGuid();
            db.Add(Row(new AuthorityVersionRow { RestoreEpoch = epoch, PolicyConfigured = true }));
            var operations = new Dictionary<string, BusinessReadOperation[]>
            {
                ["demo.operator"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabRequest, BusinessReadOperation.LabSubmit,
                    BusinessReadOperation.RecordDownload],
                ["demo.reviewer"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabReview, BusinessReadOperation.LabApprove],
                ["demo.approver"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabApprove, BusinessReadOperation.LabPermit, BusinessReadOperation.LabApply],
                ["approver.second"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabApprove],
                ["approver.no-grant"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabApprove],
                ["approver.mixed"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabApprove],
                ["approver.other-station"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabApprove],
                ["operator.other-station"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabRequest, BusinessReadOperation.LabSubmit],
                ["demo.system-admin"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabReady, BusinessReadOperation.LabApply],
                ["demo.security-admin"] = [BusinessReadOperation.LabRead],
                ["demo.auditor"] = [BusinessReadOperation.LabRead, BusinessReadOperation.LabExport],
                ["demo.viewer"] = [BusinessReadOperation.LabRead]
            };
            var permissions = new Dictionary<BusinessReadOperation, PermissionRow>();
            operations["demo.operator"] = [..operations["demo.operator"], BusinessReadOperation.CollectorBindingRead, BusinessReadOperation.CollectorBindingRequest];
            operations["demo.reviewer"] = [..operations["demo.reviewer"], BusinessReadOperation.CollectorBindingRead,
                BusinessReadOperation.CollectorBindingReview, BusinessReadOperation.CollectorBindingApprove];
            operations["demo.approver"] = [..operations["demo.approver"], BusinessReadOperation.CollectorBindingRead,
                BusinessReadOperation.CollectorBindingApprove, BusinessReadOperation.CollectorBindingApply];
            operations["demo.system-admin"] = [..operations["demo.system-admin"], BusinessReadOperation.CollectorBindingRead, BusinessReadOperation.CollectorBindingApply];
            operations["approver.other-station"] = [..operations["approver.other-station"], BusinessReadOperation.CollectorBindingRead, BusinessReadOperation.CollectorBindingApply];
            foreach (var operation in operations.Values.SelectMany(value => value).Distinct())
            {
                var permission = Row(new PermissionRow { PermissionCode = Code(operation), Module = "fixture", Action = "fixture",
                    ScopeRequirement = "Station", SoDPolicy = "fixture", RiskLevel = "fixture", CatalogVersion = 1,
                    IsApproved = true, IsImplemented = true, ApprovalReference = "TEST_ONLY" });
                permissions.Add(operation, permission); db.Add(permission);
            }
            var operatorPerson = Row(new PersonReferenceRow { PersonKey = "A10-operator" });
            db.Add(operatorPerson);
            foreach (var (name, allowed) in operations)
            {
                var person = name == "demo.operator" ? operatorPerson : Row(new PersonReferenceRow { PersonKey = name });
                if (person != operatorPerson) db.Add(person);
                var user = Row(new UserRow { PersonReferenceId = person.Id, NormalizedUsername = name, Active = true });
                var role = Row(new RoleRow { RoleCode = "fixture-" + user.Id.ToString("N"), RoleName = name, IsActive = true });
                var assignment = Row(new UserRoleRow { UserId = user.Id, RoleId = role.Id, Active = true,
                    ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddHours(1), ApprovalReference = "TEST_ONLY" });
                var scope = Row(new ScopeRow { Kind = "Station", StationId = name.EndsWith(".other-station", StringComparison.Ordinal) ? otherStation : station });
                var grant = Row(new StationGrantRow { UserRoleId = assignment.Id, ScopeId = scope.Id, Active = name != "approver.no-grant",
                    ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddHours(1), HierarchyVersion = 1, ApprovalReference = "TEST_ONLY" });
                var raw = OpaqueProofs.Create();
                db.AddRange(user, role, assignment, scope, grant, Row(new SessionRow { UserId = user.Id,
                    HandleDigest = OpaqueProofs.Digest(raw, "session"), IdleUntil = now.AddMinutes(45), AbsoluteUntil = now.AddHours(1),
                    SecurityVersion = 1, GrantVersion = 1, PolicyVersion = 1, CatalogVersion = 1, HierarchyVersion = 1,
                    RestoreEpoch = epoch, MfaAt = now }));
                foreach (var operation in allowed)
                    db.AddRange(Row(new RolePermissionRow { RoleId = role.Id, PermissionId = permissions[operation].Id }),
                        Row(new GrantFunctionRow { StationGrantId = grant.Id, PermissionId = permissions[operation].Id }));
                if (name == "approver.mixed")
                {
                    var emptyRole = Row(new RoleRow { RoleCode = "empty-" + user.Id.ToString("N"), RoleName = "Empty fixture assignment", IsActive = true });
                    var emptyAssignment = Row(new UserRoleRow { UserId = user.Id, RoleId = emptyRole.Id, Active = true,
                        ValidFrom = now.AddMinutes(-1), ValidUntil = now.AddHours(1), ApprovalReference = "TEST_ONLY" });
                    db.AddRange(emptyRole, emptyAssignment);
                    grant.UserRoleId = emptyAssignment.Id;
                }
                actors.Add(name, (user.Id, raw));
            }
            await db.SaveChangesAsync();
            await using (var duplicate = Iam())
            {
                duplicate.Add(Row(new UserRow { PersonReferenceId = operatorPerson.Id, NormalizedUsername = "operator.alias", Active = true }));
                await Assert.ThrowsAsync<DbUpdateException>(() => duplicate.SaveChangesAsync());
            }
            await using var center = new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(Connection).Options);
            center.AddRange(new IedDevice { Id = ied, StationId = station, Code = "A10-IED-A", DisplayName = "Synthetic A", IsSynthetic = true },
                new IedDevice { Id = OtherIed, StationId = otherStation, Code = "A10-IED-B", DisplayName = "Synthetic B", IsSynthetic = true });
            await center.SaveChangesAsync();
            var collector = new CollectorRegistration { Id = CollectorId, StationId = station, Code = "A10-COLLECTOR", Active = true, IsSynthetic = true };
            center.AddRange(collector, new CollectorIed { StationId = station, CollectorId = CollectorId, IedId = ied, Active = true, IsSynthetic = true });
            await center.SaveChangesAsync();
            if (Storage is not null)
            {
                var record = new DisturbanceRecord { StationId = station, IedId = ied, CollectorId = collector.Id,
                    ManifestId = Storage.Manifest, ManifestSha256 = Storage.Manifest, RemoteIdentity = "SYNTHETIC",
                    CollectedAtUtc = now, TimeSource = "fixture", CorrelationId = "fixture", IsSynthetic = true };
                center.AddRange(record, new RecordFile { Id = RecordFileId, StationId = station, RecordId = record.Id, Name = "fixture.dat", Extension = ".dat",
                        StorageObjectKey = Storage.Key, SizeBytes = Storage.Bytes.Length, Sha256 = Storage.Digest, ValidationStatus = "Verified", IsSynthetic = true });
                await center.SaveChangesAsync();
            }
            await center.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT labprep.InventoryProposal(InventoryId,StationCode,SourceDigest,SourceFile,SourceSheet,SourceRow,ProposalJson)
                VALUES({inventory},N'A10-A',{SHA256.HashData("synthetic"u8)},N'fixture',N'fixture',1,{"{}"})
                """);
            await center.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT labprep.InventoryProposal(InventoryId,StationCode,SourceDigest,SourceFile,SourceSheet,SourceRow,ProposalJson)
                VALUES({otherInventory},N'A10-B',{SHA256.HashData("synthetic-B"u8)},N'fixture',N'fixture',2,{"{}"})
                """);
        }

        private static string Code(BusinessReadOperation operation) => "a10.fixture." + operation.ToString().ToLowerInvariant();
        public string DemandPermission(BusinessReadOperation operation, AuthorityVersion version) =>
            operation.ToString().StartsWith("Lab", StringComparison.Ordinal) || operation.ToString().StartsWith("CollectorBinding", StringComparison.Ordinal) ||
                operation == BusinessReadOperation.RecordDownload
                ? Code(operation) : throw new OwnerDecisionRequiredException("Unmapped fixture operation.");
        public PermissionDescriptor? Resolve(PermissionRow row, AuthorityVersion version) =>
            row.PermissionCode.StartsWith("a10.fixture.", StringComparison.Ordinal)
                ? new(row.PermissionCode, true, true, version.Catalog, true, TimeSpan.FromMinutes(5)) : null;
        public Task<TrustedResource?> ResolveAsync(IamDbContext db, string objectId, AuthorityVersion authority, CancellationToken ct) =>
            throw new InvalidOperationException("Only canonical station scope.");
        public Task<SqlCommitGatePlan> ResolveAsync(Guid actorId, CancellationToken ct) =>
            Task.FromResult(new SqlCommitGatePlan("A10SyntheticLab", true, [new(actorId, true)], 5000, IsolationLevel.Serializable));
        public Uri Demand() => origin;

        public HttpClient Client(string name)
        {
            var pin = certificate.GetCertHashString(HashAlgorithmName.SHA256);
            var transport = new HttpClientHandler { UseCookies = true, AllowAutoRedirect = false,
                ServerCertificateCustomValidationCallback = (_, peer, _, _) => peer?.GetCertHashString(HashAlgorithmName.SHA256) == pin };
            transport.CookieContainer.Add(origin, new Cookie("__Host-FdcSession", actors[name].Raw, "/", origin.Host) { Secure = true, HttpOnly = true });
            var client = new HttpClient(transport) { BaseAddress = origin, Timeout = TimeSpan.FromSeconds(15) };
            client.DefaultRequestHeaders.Add("Origin", origin.GetLeftPart(UriPartial.Authority));
            clients.Add(client); return client;
        }

        private async Task<HttpResponseMessage> Post(HttpClient client, string route, object body, Guid request, string? etag = null)
        {
            var csrf = await client.GetFromJsonAsync<JsonElement>("/api/identity/v1/auth/csrf");
            using var message = new HttpRequestMessage(HttpMethod.Post, route) { Content = JsonContent.Create(body) };
            message.Headers.Add("X-CSRF-Token", csrf.GetProperty("token").GetString());
            message.Headers.Add("Idempotency-Key", request.ToString());
            if (etag is not null) message.Headers.Add("If-Match", etag);
            return await client.SendAsync(message);
        }

        public async Task<LabTransitionResult> Create(HttpClient client, Guid request, SyntheticLabDraft draft)
        {
            using var response = await Post(client, "/api/lab/v1/tests/", draft, request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<LabTransitionResult>())!;
        }
        public async Task<Guid> ExportRequest(HttpClient client, LabTransitionResult row)
        {
            using var response = await Post(client, $"/api/lab/v1/tests/{row.Id}/{row.Revision}/exports", new { }, Guid.NewGuid());
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("jobId").GetGuid();
        }
        public async Task<CollectorBindingRequest> BindingRequest(HttpClient client)
        {
            var request = Guid.NewGuid();
            using var response = await Post(client, "/api/business/v1/collector-binding-requests/",
                new CollectorBindingDraft(CollectorId, 1, "Revoked", request), request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<CollectorBindingRequest>())!;
        }
        public async Task<CollectorBindingRequest> BindingMove(HttpClient client, CollectorBindingRequest row, string action)
        {
            var etag = LabWorkflowService.ETag(row.VersionStamp);
            using var response = await Post(client, "/api/business/v1/collector-binding-requests/" + row.Id,
                new BindingDecision(action, etag), Guid.NewGuid(), etag);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<CollectorBindingRequest>())!;
        }
        public async Task BindingDenied(HttpClient client, CollectorBindingRequest row, string action)
        {
            var etag = LabWorkflowService.ETag(row.VersionStamp);
            using var response = await Post(client, "/api/business/v1/collector-binding-requests/" + row.Id,
                new BindingDecision(action, etag), Guid.NewGuid(), etag);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        public async Task AssertBindingApplied(Guid request)
        {
            await using var center = new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(Connection).Options);
            var collector = await center.Set<CollectorRegistration>().SingleAsync(row => row.Id == CollectorId);
            Assert.Equal(station, collector.StationId);
            Assert.Equal(2, collector.ConfigurationVersion);
            Assert.Equal("Revoked", collector.BindingStatus);
            Assert.False(collector.Active);
            await using var lab = Lab();
            var events = await lab.Set<CollectorBindingHistory>().Where(row => row.RequestId == request).Select(row => row.EventId).ToArrayAsync();
            Assert.Equal(4, events.Length);
            await using var iam = Iam();
            Assert.Equal(4, await (from audit in iam.Set<SecurityEventRow>()
                join outbox in iam.Set<AuditOutboxRow>() on audit.Id equals outbox.EventId
                where events.Contains(audit.Id) select audit.Id).CountAsync());
            using var visible = await Client("demo.operator").GetAsync("/api/business/v1/collector-binding-requests/");
            Assert.Equal(HttpStatusCode.OK, visible.StatusCode);
            Assert.Equal(request, (await visible.Content.ReadFromJsonAsync<CollectorBindingRequest[]>())!.Single().Id);
            using var other = await Client("approver.other-station").GetAsync("/api/business/v1/collector-binding-requests/");
            Assert.Equal(HttpStatusCode.OK, other.StatusCode);
            Assert.Empty((await other.Content.ReadFromJsonAsync<CollectorBindingRequest[]>())!);
        }
        public async Task VerifyBindingRollbackReuse(CollectorBindingRequest row)
        {
            using var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<CollectorBindingService>();
            var center = scope.ServiceProvider.GetRequiredService<CenterDbContext>();
            var actor = actors["demo.system-admin"];
            var digest = OpaqueProofs.Digest(actor.Raw, "session");
            await using var iam = Iam();
            await iam.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER audit.A10FailBindingAudit ON audit.SecurityEvent AFTER INSERT AS
                BEGIN THROW 51499,'Injected binding audit failure.',1; END
                """);
            try
            {
                await Assert.ThrowsAsync<AuthorityUnavailableException>(() => service.TransitionAsync(digest, actor.Id, row.Id,
                    new("apply", LabWorkflowService.ETag(row.VersionStamp)), "binding-rollback", default));
                Assert.Empty(center.ChangeTracker.Entries());
                await using var independent = new CenterDbContext(new DbContextOptionsBuilder<CenterDbContext>().UseSqlServer(Connection).Options);
                Assert.Equal(1, (await independent.Set<CollectorRegistration>().SingleAsync(item => item.Id == CollectorId)).ConfigurationVersion);
            }
            finally { await iam.Database.ExecuteSqlRawAsync("DROP TRIGGER audit.A10FailBindingAudit"); }
            var applied = await service.TransitionAsync(digest, actor.Id, row.Id,
                new("apply", LabWorkflowService.ETag(row.VersionStamp)), "binding-retry", default);
            Assert.Equal("Applied", applied.State);
            await AssertBindingApplied(row.Id);
            await WriteEvidence("a10-binding-rollback.json", new { Failure = "Required audit failed", RolledBackConfiguration = 1,
                TrackerDiscarded = true, RetryInSameScope = "Applied exactly once", FinalConfiguration = 2 });
        }
        public async Task VerifyRollbackAsync()
        {
            await using var sql = new SqlConnection(Connection);
            await sql.OpenAsync();
            await using var command = new SqlCommand("EXEC sys.sp_set_session_context @key=N'Fdc.A10.Run',@value=@run;", sql);
            command.Parameters.AddWithValue("@run", TargetDatabase["FDC_A10_TEST_".Length..]);
            await command.ExecuteNonQueryAsync();
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tools/Fdc.Center.Database/lab-readiness"));
            var down007 = (await File.ReadAllTextAsync(Path.Combine(root, "007_LabReadinessDraft.down.sql")))
                .Replace("DB_NAME() <> N'FDC_Center_Lab'", $"DB_NAME() <> N'{TargetDatabase}'", StringComparison.Ordinal);
            await using (var blocked = (SqlTransaction)await sql.BeginTransactionAsync())
            {
                await using var refuse = new SqlCommand(down007, sql, blocked);
                var error = await Assert.ThrowsAsync<SqlException>(() => refuse.ExecuteNonQueryAsync());
                Assert.Equal(51443, error.Number);
                await blocked.RollbackAsync();
            }
            await using var removeInventory = new SqlCommand("DELETE labprep.InventoryProposal WHERE InventoryId IN(@a,@b)", sql);
            removeInventory.Parameters.AddWithValue("@a", inventory);
            removeInventory.Parameters.AddWithValue("@b", otherInventory);
            await removeInventory.ExecuteNonQueryAsync();
            foreach (var file in new[] { "010_SyntheticCollectorBinding.down.sql", "009_SyntheticExport.down.sql", "008_SyntheticWorkflow.down.sql", "007_LabReadinessDraft.down.sql" })
            {
                var script = file.StartsWith("007", StringComparison.Ordinal) ? down007 : await File.ReadAllTextAsync(Path.Combine(root, file));
                await using var tx = (SqlTransaction)await sql.BeginTransactionAsync();
                foreach (var batch in Regex.Split(script, @"(?im)^\s*GO\s*$"))
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    await using var step = new SqlCommand(batch, sql, tx);
                    await step.ExecuteNonQueryAsync();
                }
                await tx.CommitAsync();
            }
            await using var verify = new SqlCommand("SELECT CASE WHEN SCHEMA_ID(N'labprep') IS NULL AND OBJECT_ID(N'ops.CollectorBindingRequest') IS NULL THEN 1 ELSE 0 END", sql);
            Assert.Equal(1, Convert.ToInt32(await verify.ExecuteScalarAsync()));
            await WriteEvidence("a10-migration-rollback.json", new { ExistingInventoryRefusal = 51443, OwnedSyntheticInventoryRemoved = 2,
                ReverseOrder = "010,009,008,007", OptionalSchemasRemoved = true });
        }
        public async Task ConcurrentApprove(LabTransitionResult row)
        {
            async Task<HttpStatusCode> Attempt(string name)
            {
                var request = Guid.NewGuid();
                using var response = await Post(Client(name), $"/api/lab/v1/tests/{row.Id}/{row.Revision}/transition",
                    new LabTransition("approve", row.ETag, request), request, row.ETag);
                return response.StatusCode;
            }
            var statuses = await Task.WhenAll(Attempt("demo.approver"), Attempt("approver.second"));
            Assert.Equal(new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }.Order(), statuses.Order());
            await using var lab = Lab();
            Assert.Equal(1, await lab.History.CountAsync(history => history.PermitId == row.Id && history.Action == "approve"));
        }
        public async Task<bool> Prepare(Guid id, Guid worker)
        {
            using var scope = app.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<LabExportCoordinator>().PrepareAsync(id, worker, "prepare-" + id, default);
        }
        public async Task<bool> Release(Guid id, Guid worker)
        {
            using var scope = app.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<LabExportCoordinator>().ReleaseAsync(id, worker, "release-" + id, default);
        }
        public async Task RevokeActor(string name)
        {
            await using var db = Iam();
            var id = actors[name].Id;
            await new SqlAuthorityCommitBoundary(db, this).CommitAsync(id, async ct =>
            {
                var user = await db.Set<UserRow>().SingleAsync(row => row.Id == id, ct);
                user.GrantVersion++;
                var assignments = await db.Set<UserRoleRow>().Where(row => row.UserId == id).Select(row => row.Id).ToArrayAsync(ct);
                foreach (var grant in await db.Set<StationGrantRow>().Where(row => assignments.Contains(row.UserRoleId)).ToListAsync(ct))
                    grant.Active = false;
                await db.SaveChangesAsync(ct);
                return true;
            }, default);
        }
        public async Task<LabTransitionResult> Move(HttpClient client, LabTransitionResult row, string action, Guid? request = null)
        {
            var id = request ?? Guid.NewGuid();
            using var response = await Post(client, $"/api/lab/v1/tests/{row.Id}/{row.Revision}/transition",
                new LabTransition(action, row.ETag, id, ied, "SYNTHETIC_HASH"), id, row.ETag);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<LabTransitionResult>())!;
        }
        public async Task Denied(HttpClient client, LabTransitionResult row, string action,
            HttpStatusCode expected = HttpStatusCode.Forbidden, Guid? permitIed = null, string testType = "SYNTHETIC_HASH")
        {
            await using var lab = Lab();
            var count = await lab.History.CountAsync();
            var id = Guid.NewGuid();
            using var response = await Post(client, $"/api/lab/v1/tests/{row.Id}/{row.Revision}/transition",
                new LabTransition(action, row.ETag, id, permitIed ?? ied, testType), id, row.ETag);
            Assert.Equal(expected, response.StatusCode);
            Assert.Equal(count, await lab.History.CountAsync());
            if (expected == HttpStatusCode.Forbidden)
            {
                var correlation = response.Headers.GetValues("X-Correlation-Id").Single();
                await using var iam = Iam();
                Assert.Equal(1, await (from audit in iam.Set<SecurityEventRow>()
                    join outbox in iam.Set<AuditOutboxRow>() on audit.Id equals outbox.EventId
                    where audit.EventCode == "business.mutation.denied" && audit.CorrelationId == correlation select audit.Id).CountAsync());
            }
        }
        public async Task AssertAudit(string id, int histories)
        {
            await using var lab = Lab();
            await using var db = Iam();
            Assert.Equal(histories, await lab.History.CountAsync(row => row.PermitId == id));
            Assert.Equal(histories + 1, await db.Set<SecurityEventRow>().CountAsync(row => row.EventCode != "business.mutation.denied"));
            Assert.Equal(await db.Set<SecurityEventRow>().CountAsync(), await db.Set<AuditOutboxRow>().CountAsync());
            var correlations = await lab.History.Where(row => row.PermitId == id).Select(row => row.CorrelationId).ToArrayAsync();
            var audited = await (from audit in db.Set<SecurityEventRow>()
                join outbox in db.Set<AuditOutboxRow>() on audit.Id equals outbox.EventId
                where correlations.Contains(audit.CorrelationId) select audit.CorrelationId).ToArrayAsync();
            Assert.True(correlations.All(value => audited.Contains(value, StringComparer.Ordinal)));
        }
        public async Task WriteEvidence(string file, object observations)
        {
            if (Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") is not { } root) return;
            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(Path.Combine(root, file), JsonSerializer.Serialize(new
            { Utc = DateTimeOffset.UtcNow, Commit = Environment.GetEnvironmentVariable("FDC_A5_COMMIT"), Observations = observations }));
        }
        public async Task RunBrowserAsync(string scenario = "idle")
        {
            var root = Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") ?? throw new InvalidOperationException("Evidence directory required.");
            var signal = Path.Combine(root, "lab-expire-" + Guid.NewGuid().ToString("N"));
            var start = new System.Diagnostics.ProcessStartInfo("node")
            { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            start.ArgumentList.Add(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tests/Fdc.Center.Tests/browser/a10-lab.cjs")));
            start.Environment["FDC_A10_ORIGIN"] = origin.ToString();
            start.Environment["FDC_A10_ACTORS"] = JsonSerializer.Serialize(actors.ToDictionary(pair => pair.Key, pair => pair.Value.Raw));
            start.Environment["FDC_A10_DRAFT"] = JsonSerializer.Serialize(Draft());
            start.Environment["FDC_A10_EXPIRE"] = signal;
            start.Environment["FDC_A10_SESSION_SCENARIO"] = scenario;
            using var process = System.Diagnostics.Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            try
            {
                while (!process.HasExited)
                {
                    if (File.Exists(signal))
                    {
                        Assert.Equal(scenario, await File.ReadAllTextAsync(signal, timeout.Token));
                        if (scenario == "idle") Clock.Advance(TimeSpan.FromMinutes(46));
                        else if (scenario == "mfa") Clock.Advance(TimeSpan.FromMinutes(6));
                        else
                        {
                            await using var iam = Iam();
                            var viewerId = actors["demo.viewer"].Id;
                            if (scenario == "security")
                            {
                                var user = await iam.Set<UserRow>().SingleAsync(row => row.Id == viewerId, timeout.Token);
                                user.SecurityVersion++;
                            }
                            else
                            {
                                var session = await iam.Set<SessionRow>().SingleAsync(row => row.UserId == viewerId, timeout.Token);
                                if (scenario == "revoked") session.Revoked = true;
                                else if (scenario == "absolute") session.AbsoluteUntil = Clock.GetUtcNow().AddSeconds(1);
                                else throw new ArgumentException("Unknown clock scenario.");
                            }
                            await iam.SaveChangesAsync(timeout.Token);
                            if (scenario == "absolute") Clock.Advance(TimeSpan.FromSeconds(2));
                        }
                        await File.WriteAllTextAsync(signal + ".done", "clock-advanced", timeout.Token);
                        break;
                    }
                    await Task.Delay(30, timeout.Token);
                }
                await process.WaitForExitAsync(timeout.Token);
                Assert.True(process.ExitCode == 0, (await output) + (await error));
            }
            catch { if (!process.HasExited) process.Kill(true); throw; }
            finally
            {
                File.Delete(signal);
                File.Delete(signal + ".done");
            }
        }
        public async Task RunCacheBrowserAsync(string a, string b)
        {
            await using var proxy = await A10ReadProxy.StartAsync(origin, certificate);
            var root = Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") ?? throw new InvalidOperationException("Evidence directory required.");
            var signal = Path.Combine(root, "cache-revoke-" + Guid.NewGuid().ToString("N"));
            var start = new System.Diagnostics.ProcessStartInfo("node") { UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            start.ArgumentList.Add(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tests/Fdc.Center.Tests/browser/a10-cache.cjs")));
            start.Environment["FDC_A10_ORIGIN"] = proxy.Origin.ToString();
            start.Environment["FDC_A10_CACHE_A"] = a;
            start.Environment["FDC_A10_CACHE_B"] = b;
            start.Environment["FDC_A10_TOKEN_A"] = actors["demo.viewer"].Raw;
            start.Environment["FDC_A10_TOKEN_B"] = actors["operator.other-station"].Raw;
            start.Environment["FDC_A10_REVOKE"] = signal;
            using var process = System.Diagnostics.Process.Start(start)!;
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(70));
            try
            {
                while (!process.HasExited)
                {
                    if (File.Exists(signal))
                    {
                        Assert.Equal("revoke", await File.ReadAllTextAsync(signal, timeout.Token));
                        await RevokeActor("demo.viewer");
                        await File.WriteAllTextAsync(signal + ".done", "revoked", timeout.Token);
                        break;
                    }
                    await Task.Delay(30, timeout.Token);
                }
                await process.WaitForExitAsync(timeout.Token);
                Assert.True(process.ExitCode == 0, (await stdout) + (await stderr));
                Assert.Equal(0, proxy.CacheEntries);
                Assert.True(proxy.Forwarded > 10);
                await WriteEvidence("a10-proxy-sql.json", new { proxy.Forwarded, proxy.CacheEntries,
                    ViewerGrantVersionChanged = true, Sessions = "A/B test sessions, distinct cookies", Api = "Real HTTPS upstream with pinned certificate" });
            }
            catch { if (!process.HasExited) process.Kill(true); throw; }
            finally { File.Delete(signal); File.Delete(signal + ".done"); }
        }
        public async Task<Guid> RunBindingBrowserAsync()
        {
            var root = Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") ?? throw new InvalidOperationException("Evidence required.");
            var start = new System.Diagnostics.ProcessStartInfo("node") { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tests/Fdc.Center.Tests/browser/a10-binding.cjs")));
            start.Environment["FDC_A10_ORIGIN"] = origin.ToString();
            start.Environment["FDC_A10_ACTORS"] = JsonSerializer.Serialize(actors.ToDictionary(pair => pair.Key, pair => pair.Value.Raw));
            start.Environment["FDC_A10_COLLECTOR"] = CollectorId.ToString();
            using var process = System.Diagnostics.Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch { if (!process.HasExited) process.Kill(true); throw; }
            Assert.True(process.ExitCode == 0, (await output) + (await error));
            using var result = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "a10-binding-browser.json")));
            return result.RootElement.GetProperty("request").GetGuid();
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var client in clients) client.Dispose();
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            certificate?.Dispose();
            Storage?.Dispose();
        }
    }

    internal sealed class SwapStorage : IReviewedBusinessFileSource, IDisposable
    {
        private readonly string root = Directory.CreateTempSubdirectory("FdcA10-swap-").FullName;
        private readonly string outside = Directory.CreateTempSubdirectory("FdcA10-outside-").FullName;
        private string? junction;
        public string Manifest { get; } = new string('a', 64);
        public string Key => "records/" + Manifest + "/fixture.dat";
        public byte[] Bytes { get; } = "SYNTHETIC SAME BYTES BOTH SIDES"u8.ToArray();
        public string Digest => Convert.ToHexStringLower(SHA256.HashData(Bytes));
        public bool Opened { get; private set; }

        public async Task<Stream?> OpenVerifiedAsync(string objectKey, long size, string sha256, int cap, CancellationToken ct)
        {
            var folder = Directory.CreateDirectory(Path.Combine(root, "records", Manifest)).FullName;
            var file = Path.Combine(folder, "fixture.dat");
            await File.WriteAllBytesAsync(file, Bytes, ct);
            await File.WriteAllBytesAsync(Path.Combine(outside, "fixture.dat"), Bytes, ct);
            var spool = Directory.CreateDirectory(Path.Combine(root, "spool")).FullName;
            var source = new PrivateComtradeFileSource(root, spool);
            var result = await source.OpenVerifiedAsync(objectKey, size, sha256, cap, path =>
            {
                if (!Path.GetFullPath(folder).StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Test directory outside owned root.");
                Directory.Move(folder, folder + "-original");
                PrivateComtradeFileSourceTests.CreateDirectoryRedirect(folder, outside);
                junction = folder;
                var opened = File.OpenRead(path);
                Opened = true;
                return opened;
            }, ct);
            Assert.Empty(Directory.GetFiles(spool));
            return result;
        }
        public void Dispose()
        {
            if (junction is not null) Directory.Delete(junction);
            Directory.Delete(root, true);
            Directory.Delete(outside, true);
        }
    }
}

internal sealed class A10FactAttribute : FactAttribute
{
    public A10FactAttribute()
    {
        if (!A10IamSqlTest.Enabled) Skip = "Explicit A.10 owned disposable SQL opt-in required.";
    }
}
internal sealed class A10TheoryAttribute : TheoryAttribute
{
    public A10TheoryAttribute()
    {
        if (!A10IamSqlTest.Enabled) Skip = "Explicit A.10 owned disposable SQL opt-in required.";
    }
}
