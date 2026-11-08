using Fdc.Identity.Api;
using Fdc.Identity.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fdc.Identity.Tests;

public sealed class EndpointContractTests
{
    [Fact] public async Task Adapter_exposes_only_canonical_read_routes_with_explicit_human_policy()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<RoleManagementService>();
        await using var app = builder.Build();
        app.MapIdentityRoleQueries();
        // Inspect metadata only: NO StartAsync, listener, real session, SQL connection or mock login.
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(s => s.Endpoints).OfType<RouteEndpoint>().ToArray();
        Assert.Equal(3, endpoints.Length);
        foreach (var endpoint in endpoints)
        {
            Assert.StartsWith("/api/identity/v1/admin/roles", endpoint.RoutePattern.RawText);
            Assert.Contains(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(), a => a.Policy == "Fdc.Human");
            Assert.Equal(["GET"], endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods);
        }
    }
}
