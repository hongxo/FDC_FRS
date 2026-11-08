using System.Security.Claims;
using Fdc.Identity.Api;
using Fdc.Identity.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fdc.Identity.Tests;

public sealed class Phase4AdapterHardeningTests
{
    [Theory][InlineData("http", 401)][InlineData("https", 503)]
    public async Task Role_adapter_enforces_https_before_pending_authority(string scheme, int expected)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton(new RoleManagementService(new PendingD04RoleAuthority(), new NeverQueries()));
        await using var app = builder.Build(); app.MapIdentityRoleQueries();
        var endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(x => x.Endpoints).OfType<RouteEndpoint>().Single(e => e.RoutePattern.RawText!.EndsWith("roles/", StringComparison.Ordinal));
        using var scope = app.Services.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Scheme = scheme; context.Request.Method = "GET"; context.Response.Body = new MemoryStream();
        // Test-only principal fixture; no runtime authentication provider, listener or SQL mock acceptance.
        context.User = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "Fdc.Human"));
        await endpoint.RequestDelegate!(context);
        Assert.Equal(expected, context.Response.StatusCode); Assert.Equal("no-store", context.Response.Headers.CacheControl);
    }
    private sealed class NeverQueries : IRoleQueries
    {
        public Task<RolePage> ListAsync(RoleReadScope scope, string? afterCode, int limit, CancellationToken ct) => throw new InvalidOperationException("Pending authority must block query.");
        public Task<RoleView?> DetailAsync(RoleReadScope scope, Guid roleId, CancellationToken ct) => throw new InvalidOperationException();
        public Task<IReadOnlyList<string>> PermissionsAsync(RoleReadScope scope, Guid roleId, CancellationToken ct) => throw new InvalidOperationException();
    }
}
