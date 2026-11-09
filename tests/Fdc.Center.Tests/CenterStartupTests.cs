using Fdc.Center;
using Fdc.Core;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fdc.Center.Tests;

public sealed class CenterStartupTests
{
    [Fact] public async Task Identity_startup_is_actual_center_with_real_sql_reader_and_no_operational_workers()
    {
        await using var app = CenterApplication.Build(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" }, builder =>
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["IdentityRuntime:Enabled"] = "true",
                ["ConnectionStrings:FdcIdentity"] = "Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true" }));
        using var scope = app.Services.CreateScope();
        Assert.IsType<SqlSessionAuthorityReader>(scope.ServiceProvider.GetRequiredService<IHumanSessionAuthorityReader>());
        Assert.IsType<PendingD04RoleAuthority>(scope.ServiceProvider.GetRequiredService<IRoleAdministrationAuthority>());
        Assert.IsType<PendingD09CommitBoundary>(scope.ServiceProvider.GetRequiredService<IAuthorizedCommitBoundary>());
        Assert.Null(scope.ServiceProvider.GetService<Store>()); Assert.Null(scope.ServiceProvider.GetService<BundleReceiver>());
        Assert.DoesNotContain(app.Services.GetServices<IHostedService>(), service => service is InboxWorker);
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(x => x.Endpoints).OfType<RouteEndpoint>().ToArray();
        Assert.Contains(endpoints, e => e.RoutePattern.RawText == "/api/identity/v1/admin/users");
        Assert.DoesNotContain(endpoints, e => e.RoutePattern.RawText!.EndsWith("/login", StringComparison.Ordinal));
        Assert.DoesNotContain(endpoints, e => e.RoutePattern.RawText!.StartsWith("/api/stations", StringComparison.Ordinal));
        Assert.DoesNotContain(endpoints, e => e.RoutePattern.RawText!.StartsWith("/api/pilot", StringComparison.Ordinal));
    }
    [Fact] public void Missing_connection_does_not_fallback_to_demo()
    {
        Assert.Throws<InvalidOperationException>(() => CenterApplication.Build(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" }, builder =>
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["IdentityRuntime:Enabled"] = "true", ["ConnectionStrings:FdcIdentity"] = "" })));
    }
    [Fact] public void Production_activation_not_enabled_by_a_config_flag()
    {
        Assert.Throws<InvalidOperationException>(() => CenterApplication.Build(new WebApplicationOptions { Args = [], EnvironmentName = "Production" }, builder =>
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["IdentityRuntime:Enabled"] = "true" })));
    }
}
