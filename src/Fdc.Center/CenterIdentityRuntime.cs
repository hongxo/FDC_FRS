using System.Data.Common;
using System.Net;
using Fdc.Identity.Api;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Center;

// Actual Center startup branch; deliberately limited to loopback Development/Lab while D01/D04/D05 are pending.
// Configuration selects integration, never grants permission or approves a policy.
public static class CenterIdentityRuntime
{
    public static WebApplication Build(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing") && !builder.Environment.IsEnvironment("Lab"))
            throw new InvalidOperationException("IAM activation requires an approved deployment baseline; this integration is Development/Lab only.");
        var connection = builder.Configuration.GetConnectionString("FdcIdentity");
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Supply the IAM SQL connection through the approved secret channel.");
        try
        {
            if (new SqlConnectionStringBuilder(connection).ApplicationIntent != ApplicationIntent.ReadWrite) throw new ArgumentException();
        }
        catch (ArgumentException) { throw new InvalidOperationException("A valid primary-only IAM SQL connection is required."); }
        builder.AddCenterLabPersistence(connection);
        // Per Human lane, not global RequireCertificate inherited from legacy machine setup.
        builder.WebHost.ConfigureKestrel(server => server.ConfigureHttpsDefaults(https => https.ClientCertificateMode = ClientCertificateMode.AllowCertificate));
        builder.Services.AddDbContextFactory<IamDbContext>(options => options.UseSqlServer(connection).EnableSensitiveDataLogging(false).EnableDetailedErrors(false));
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddScoped<IHumanSessionAuthorityReader, SqlSessionAuthorityReader>();
        builder.Services.AddAuthentication("Fdc.Human").AddScheme<AuthenticationSchemeOptions, HumanSessionHandler>("Fdc.Human", _ => { });
        builder.Services.AddAuthorization(options => options.AddPolicy("Fdc.Human", policy => policy.AddAuthenticationSchemes("Fdc.Human").RequireAuthenticatedUser()));
        builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-Token"; options.Cookie.Name = "__Host-FdcCsrf"; options.Cookie.SecurePolicy = CookieSecurePolicy.Always; });
        builder.Services.AddScoped<IRoleAdministrationAuthority, PendingD04RoleAuthority>();
        builder.Services.AddScoped<IRoleQueries, SqlRoleQueries>(); builder.Services.AddScoped<RoleManagementService>();
        builder.Services.AddScoped<IManagementReadAuthority, PendingManagementReadAuthority>();
        builder.Services.AddScoped<IManagementQueries, SqlManagementQueries>(); builder.Services.AddScoped<ManagementQueryService>();
        builder.Services.AddSingleton<IReviewedPermissionPolicy, PendingPermissionPolicy>();
        builder.Services.AddSingleton<IOperationMappingReleaseVerifier, PendingOperationMappingReleaseVerifier>();
        builder.Services.AddSingleton<OperationPermissionResolver>();
        builder.Services.AddSingleton<ITrustedResourceResolver, PendingCenterResourceResolver>();
        builder.Services.AddScoped<ICurrentAuthorityReader, SqlCurrentAuthorityReader>();
        builder.Services.AddSingleton<PermissionEvaluator>(); builder.Services.AddScoped<Fdc.Identity.Application.AuthorizationService>();
        builder.Services.AddScoped<IAuthorizedCommitBoundary, PendingD09CommitBoundary>();
        builder.Services.AddSingleton<ISqlCommitGatePlanProvider, PendingSqlCommitGatePlanProvider>();
        builder.Services.AddSingleton<ISessionRevocationPolicy, PendingSessionRevocationPolicy>();
        builder.Services.AddScoped<SqlSessionRevocationService>();
        builder.Services.AddSingleton<IAuthenticationPolicyProvider, PendingAuthenticationPolicy>();
        builder.Services.AddSingleton<ILoginTransactionStore, PendingLoginTransactionStore>();
        builder.Services.AddSingleton<IMfaPolicyEvaluator, PendingMfaPolicy>();
        builder.Services.AddSingleton<IRoleMutationAuthority, PendingRoleMutationAuthority>();
        // No Store/BundleReceiver/InboxWorker registration in this branch: operational storage remains untouched.
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self'; connect-src 'self'; frame-ancestors 'none'; base-uri 'none'; object-src 'none'";
            if (!context.Request.IsHttps || context.Connection.RemoteIpAddress is not { } remote || !IPAddress.IsLoopback(remote))
            { context.Response.StatusCode = 403; return; }
            if (context.Request.Path.StartsWithSegments("/api/stations"))
            { context.Response.StatusCode = 403; return; } // Machine ingress is never available on this Human lane.
            try { await next(); }
            catch (OwnerDecisionRequiredException) { await Error(context, "OWNER_POLICY_REQUIRED"); }
            catch (AuthorityUnavailableException) { await Error(context, "AUTHORITY_UNAVAILABLE"); }
            catch (DbException) { await Error(context, "AUTHORITY_UNAVAILABLE"); }
        });
        // No forwarded-header middleware: no proxy header becomes TLS, source identity or a principal.
        app.UseWhen(context => context.Request.Path.StartsWithSegments("/identity"), branch => { branch.UseDefaultFiles(); branch.UseStaticFiles(); });
        app.UseAuthentication(); app.UseAuthorization();
        app.MapIdentityRoleQueries(); app.MapIdentityManagementQueries();
        // Existing business API identities preserved, but no data handler runs before reviewed station mapping exists.
        app.MapGet("/api/status", BlockBusiness).RequireAuthorization("Fdc.Human");
        app.MapGet("/api/events", BlockBusiness).RequireAuthorization("Fdc.Human");
        app.MapPost("/api/commands", BlockBusiness).RequireAuthorization("Fdc.Human");
        app.MapGet("/api/records/{id}/download", BlockBusiness).RequireAuthorization("Fdc.Human");
        app.MapGet("/api/records/{id}/waveform", BlockBusiness).RequireAuthorization("Fdc.Human");
        app.MapGet("/", () => Results.Redirect("/identity/"));
        return app;
    }
    private static IResult BlockBusiness(HttpContext context) => Results.Json(new { code = "STATION_OPERATION_MAPPING_PENDING", correlationId = context.TraceIdentifier }, statusCode: 503);
    private static async Task Error(HttpContext context, string code)
    {
        if (context.Response.HasStarted) { context.Abort(); return; }
        context.Response.StatusCode = 503; await context.Response.WriteAsJsonAsync(new { code, correlationId = context.TraceIdentifier });
    }
    private sealed class PendingCenterResourceResolver : ITrustedResourceResolver
    {
        public Task<Fdc.Identity.Domain.TrustedResource?> ResolveAsync(IamDbContext db, string objectId, Fdc.Identity.Domain.AuthorityVersion authority, CancellationToken ct) =>
            throw new OwnerDecisionRequiredException("D04: trusted business object station binding and supported operation mapping");
    }
}
