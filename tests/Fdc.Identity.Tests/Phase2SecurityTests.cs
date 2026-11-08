using System.Text.Encodings.Web;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fdc.Identity.Api;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Fdc.Identity.Tests;

public sealed class Phase2SecurityTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-11-08T00:00:00Z");
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly AuthorityVersion Version = new(1, 1, 1, Guid.NewGuid());
    private static readonly UserIdentity User = new(UserId, "synthetic-person", true, 1, 1);
    private static readonly HumanSession Session = new(UserId, false, Now.AddMinutes(1), Now.AddHours(1), 1, 1, Version, null);
    private static readonly PasswordProofs Passwords = new(1000, 256); // Test cost only; no runtime selection.

    [Fact] public void Hashes_are_salted_and_passwords_are_verified()
    {
        var a = Passwords.Hash("synthetic-test-password");
        var b = Passwords.Hash("synthetic-test-password");
        Assert.NotEqual(a, b);
        Assert.DoesNotContain("synthetic-test-password", a);
        Assert.Equal(PasswordVerificationResult.Success, Passwords.Verify(a, "synthetic-test-password"));
        Assert.Equal(PasswordVerificationResult.Failed, Passwords.Verify(a, "wrong"));
    }
    [Theory][InlineData("")][InlineData("garbage")][InlineData("AQ==")]
    public void Malformed_hashes_are_rejected(string encoded) => Assert.Equal(PasswordVerificationResult.Failed, Passwords.Verify(encoded, "test"));
    [Fact] public void Higher_cost_requests_rehash_without_rejecting_correct_password()
    {
        var low = new PasswordProofs(1, 256).Hash("synthetic");
        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded, Passwords.Verify(low, "synthetic"));
    }
    [Fact] public void Overlong_password_does_not_reach_expensive_verification() =>
        Assert.Equal(PasswordVerificationResult.Failed, Passwords.Verify(Passwords.Hash("test"), new string('x', 257)));
    [Fact] public void Opaque_proofs_are_random_canonical_and_purpose_bound()
    {
        var proof = OpaqueProofs.Create();
        Assert.True(OpaqueProofs.IsValid(proof)); Assert.NotEqual(proof, OpaqueProofs.Create());
        Assert.Equal(64, OpaqueProofs.Digest(proof, "session").Length);
        Assert.NotEqual(OpaqueProofs.Digest(proof, "session"), OpaqueProofs.Digest(proof, "reset"));
    }
    [Theory][InlineData(null)][InlineData("")][InlineData("short")][InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+")][InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa=")]
    public void Invalid_opaque_proofs_are_rejected(string? value) => Assert.False(OpaqueProofs.IsValid(value));
    [Fact] public void Current_session_is_valid() => Assert.True(SessionValidity.IsCurrent(User, Session, Version, Now));
    [Fact] public void Disabled_user_denied() => Assert.False(SessionValidity.IsCurrent(User with { Active = false }, Session, Version, Now));
    [Fact] public void Revoked_session_denied() => Assert.False(SessionValidity.IsCurrent(User, Session with { Revoked = true }, Version, Now));
    [Fact] public void Idle_boundary_denied() => Assert.False(SessionValidity.IsCurrent(User, Session with { IdleUntil = Now }, Version, Now));
    [Fact] public void Absolute_boundary_denied() => Assert.False(SessionValidity.IsCurrent(User, Session with { AbsoluteUntil = Now }, Version, Now));
    [Fact] public void Security_version_change_denied() => Assert.False(SessionValidity.IsCurrent(User with { SecurityVersion = 2 }, Session, Version, Now));
    [Fact] public void Grant_version_change_denied() => Assert.False(SessionValidity.IsCurrent(User with { GrantVersion = 2 }, Session, Version, Now));
    [Fact] public void Policy_version_change_denied() => Assert.False(SessionValidity.IsCurrent(User, Session, Version with { Policy = 2 }, Now));
    [Fact] public void Restore_epoch_change_denied() => Assert.False(SessionValidity.IsCurrent(User, Session, Version with { RestoreEpoch = Guid.NewGuid() }, Now));
    [Fact] public void Empty_epoch_denied() => Assert.False(SessionValidity.IsCurrent(User, Session with { Version = Version with { RestoreEpoch = Guid.Empty } }, Version with { RestoreEpoch = Guid.Empty }, Now));
    [Fact] public void Mismatched_user_denied() => Assert.False(SessionValidity.IsCurrent(User, Session with { UserId = Guid.NewGuid() }, Version, Now));
    [Fact] public void Session_cookie_has_host_prefix_constraints()
    {
        var options = HumanRequestSecurity.Cookie(SameSiteMode.Strict);
        Assert.True(options.Secure); Assert.True(options.HttpOnly); Assert.Null(options.Domain); Assert.Equal("/", options.Path);
        Assert.Throws<ArgumentException>(() => HumanRequestSecurity.Cookie(SameSiteMode.None));
        Assert.Throws<ArgumentException>(() => new HumanRequestSecurity(new Uri("http://human.test")));
    }
    [Fact] public async Task Cross_origin_is_rejected_before_antiforgery()
    {
        var context = Request(); context.Request.Headers.Origin = "https://evil.test";
        var csrf = new Csrf(false);
        Assert.False(await new HumanRequestSecurity(new Uri("https://human.test")).ValidateMutationAsync(context, csrf));
        Assert.Equal(0, csrf.Calls);
    }
    [Fact] public async Task Missing_csrf_is_rejected_even_same_origin()
    {
        Assert.False(await new HumanRequestSecurity(new Uri("https://human.test")).ValidateMutationAsync(Request(), new Csrf(false)));
    }
    [Fact] public async Task Same_origin_valid_csrf_passes_guard() =>
        Assert.True(await new HumanRequestSecurity(new Uri("https://human.test")).ValidateMutationAsync(Request(), new Csrf(true)));
    [Fact] public async Task Real_antiforgery_provider_rejects_missing_proof_and_accepts_bound_token_pair()
    {
        var services = new ServiceCollection(); services.AddLogging();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddAntiforgery(options => { options.Cookie.Name = "__Host-FdcCsrf"; options.Cookie.SecurePolicy = CookieSecurePolicy.Always; options.HeaderName = "X-Fdc-Csrf"; });
        using var provider = services.BuildServiceProvider();
        var csrf = provider.GetRequiredService<IAntiforgery>();
        var guard = new HumanRequestSecurity(new Uri("https://human.test"));
        var missing = Request(); missing.RequestServices = provider;
        Assert.False(await guard.ValidateMutationAsync(missing, csrf));
        var initial = Request(); initial.RequestServices = provider;
        var tokens = csrf.GetAndStoreTokens(initial);
        var valid = Request(); valid.RequestServices = provider;
        valid.Request.Headers.Cookie = "__Host-FdcCsrf=" + tokens.CookieToken;
        valid.Request.Headers["X-Fdc-Csrf"] = tokens.RequestToken;
        Assert.True(await guard.ValidateMutationAsync(valid, csrf));
    }
    [Fact] public async Task Forwarded_header_does_not_turn_http_into_trusted_https()
    {
        var context = Request(); context.Request.Scheme = "http"; context.Request.Headers["X-Forwarded-Proto"] = "https";
        Assert.False(await new HumanRequestSecurity(new Uri("https://human.test")).ValidateMutationAsync(context, new Csrf(true)));
    }
    [Fact] public async Task Every_handler_call_checks_current_authority_and_revoke_is_seen()
    {
        var reader = new Reader();
        var first = await Handler(RequestWithCookie(), reader);
        Assert.True((await first.AuthenticateAsync()).Succeeded);
        reader.Current = reader.Current! with { Session = Session with { Revoked = true } };
        var second = await Handler(RequestWithCookie(), reader);
        Assert.False((await second.AuthenticateAsync()).Succeeded); Assert.Equal(2, reader.Calls);
    }
    [Fact] public async Task Authority_outage_challenges_503_without_authenticated_principal()
    {
        var reader = new Reader { Unavailable = true }; var context = RequestWithCookie();
        var handler = await Handler(context, reader);
        Assert.False((await handler.AuthenticateAsync()).Succeeded);
        await handler.ChallengeAsync(new()); Assert.Equal(503, context.Response.StatusCode);
    }
    [Fact] public async Task Forged_proxy_identity_headers_are_ignored()
    {
        var context = Request(); context.Request.Headers["X-User"] = "admin"; context.Request.Headers["X-Role"] = "admin";
        var reader = new Reader(); var handler = await Handler(context, reader);
        Assert.False((await handler.AuthenticateAsync()).Succeeded); Assert.Equal(0, reader.Calls);
    }
    [Fact] public async Task Cookie_on_http_is_rejected_before_sql()
    {
        var context = RequestWithCookie(); context.Request.Scheme = "http"; var reader = new Reader();
        Assert.False((await (await Handler(context, reader)).AuthenticateAsync()).Succeeded); Assert.Equal(0, reader.Calls);
    }
    [Fact] public async Task Machine_certificate_with_human_cookie_is_rejected_before_sql()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=synthetic-test-collector", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(Now.AddMinutes(-1), Now.AddMinutes(1));
        var context = RequestWithCookie(); context.Connection.ClientCertificate = certificate; var reader = new Reader();
        Assert.False((await (await Handler(context, reader)).AuthenticateAsync()).Succeeded); Assert.Equal(0, reader.Calls);
    }
    [Fact] public async Task Duplicate_cookies_are_rejected_before_sql()
    {
        var context = RequestWithCookie(); context.Request.Headers.Cookie += "; __Host-FdcSession=" + OpaqueProofs.Create();
        var reader = new Reader(); Assert.False((await (await Handler(context, reader)).AuthenticateAsync()).Succeeded); Assert.Equal(0, reader.Calls);
    }
    [Fact] public async Task Pending_auth_policy_never_opens_mutation_store()
    {
        var service = new Fdc.Identity.Application.AuthenticationService(new PendingAuthenticationPolicy(), new PendingLoginTransactionStore(), Passwords, new Clock());
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => service.LoginAsync("SYNTHETIC", "test", "source", default));
    }
    [Fact] public async Task Pending_login_store_never_executes_callback()
    {
        var called = false;
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new PendingLoginTransactionStore().ExecuteAsync((_, _) => { called = true; return Task.FromResult(new LoginResult(LoginOutcome.SessionCreated)); }, default));
        Assert.False(called);
    }
    [Fact] public async Task Pending_mfa_policy_blocks_activation() =>
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new PendingMfaPolicy().DemandConfiguredAsync(default));
    [Fact] public void Login_result_string_never_contains_proof() => Assert.DoesNotContain("synthetic-secret", new LoginResult(LoginOutcome.MfaRequired, "synthetic-secret").ToString());
    [Fact] public async Task Invalid_digest_never_creates_sql_context()
    {
        var factory = new NoDatabaseFactory();
        Assert.Null(await new SqlSessionAuthorityReader(factory).ReadAsync("bad", default)); Assert.Equal(0, factory.Calls);
    }
    [Fact] public async Task Readonly_replica_intent_is_rejected_before_connecting()
    {
        var factory = new ReadOnlyFactory();
        await Assert.ThrowsAsync<AuthorityUnavailableException>(() => new SqlSessionAuthorityReader(factory).ReadAsync(new string('A', 64), default));
    }
    [Fact] public void Unreviewed_permission_metadata_cannot_grant()
    {
        var row = new PermissionRow { PermissionCode = "synthetic.view", Module = "test", Action = "view", ScopeRequirement = "Station", SoDPolicy = "none", RiskLevel = "low", IsApproved = true, IsImplemented = true, CatalogVersion = 1 };
        Assert.Null(new PendingPermissionPolicy().Resolve(row, Version));
    }

    private static DefaultHttpContext Request()
    {
        var context = new DefaultHttpContext(); context.Request.Scheme = "https"; context.Request.Host = new("human.test");
        context.Request.Method = "POST"; context.Request.Headers.Origin = "https://human.test";
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(); return context;
    }
    private static DefaultHttpContext RequestWithCookie() { var context = Request(); context.Request.Headers.Cookie = "__Host-FdcSession=" + OpaqueProofs.Create(); return context; }
    private static async Task<HumanSessionHandler> Handler(HttpContext context, Reader reader)
    {
        var handler = new HumanSessionHandler(new Monitor(), NullLoggerFactory.Instance, UrlEncoder.Default, reader, new Clock());
        await handler.InitializeAsync(new("Fdc.Human", null, typeof(HumanSessionHandler)), context); return handler;
    }
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class Reader : IHumanSessionAuthorityReader
    {
        public CurrentHumanSession? Current = new(Guid.NewGuid(), User, Session, Version); public int Calls; public bool Unavailable;
        public Task<CurrentHumanSession?> ReadAsync(string digest, CancellationToken ct) { Calls++; if (Unavailable) throw new AuthorityUnavailableException(); return Task.FromResult(Current); }
    }
    private sealed class Monitor : IOptionsMonitor<AuthenticationSchemeOptions>
    {
        public AuthenticationSchemeOptions CurrentValue { get; } = new();
        public AuthenticationSchemeOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<AuthenticationSchemeOptions, string?> listener) => null;
    }
    private sealed class Csrf(bool valid) : IAntiforgery
    {
        public int Calls;
        public Task ValidateRequestAsync(HttpContext context) { Calls++; if (!valid) throw new AntiforgeryValidationException("Invalid proof."); return Task.CompletedTask; }
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext context) => throw new NotSupportedException();
        public AntiforgeryTokenSet GetTokens(HttpContext context) => throw new NotSupportedException();
        public Task<bool> IsRequestValidAsync(HttpContext context) => Task.FromResult(valid);
        public void SetCookieTokenAndHeader(HttpContext context) => throw new NotSupportedException();
    }
    private sealed class NoDatabaseFactory : IDbContextFactory<IamDbContext>
    {
        public int Calls;
        public IamDbContext CreateDbContext() { Calls++; throw new InvalidOperationException("No database expected."); }
    }
    private sealed class ReadOnlyFactory : IDbContextFactory<IamDbContext>
    {
        public IamDbContext CreateDbContext() => new(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=invalid;Database=synthetic;Integrated Security=true;ApplicationIntent=ReadOnly").Options);
    }
}
