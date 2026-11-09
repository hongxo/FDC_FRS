using System.Collections.Immutable;
using Fdc.Identity.Api;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fdc.Identity.Tests;

public sealed class ManagementReadTests
{
    internal static ManagementReadScope Empty => new([], [], [], [], [], [], ImmutableHashSet<string>.Empty.WithComparer(StringComparer.Ordinal));
    [Theory][InlineData(-1, 1)][InlineData(10001, 1)][InlineData(0, 0)][InlineData(0, 101)]
    public void Invalid_page_is_rejected(int offset, int limit) => Assert.Throws<ArgumentException>(() => new PageWindow(offset, limit));
    [Fact] public async Task Default_page_cannot_bypass_limits()
    {
        var service = new ManagementQueryService(new PendingManagementReadAuthority(), Queries());
        await Assert.ThrowsAsync<ArgumentException>(() => service.UsersAsync(null, null, default, default));
    }
    [Fact] public async Task Pending_mapping_blocks_before_any_sql()
    {
        var service = new ManagementQueryService(new PendingManagementReadAuthority(), Queries());
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => service.UsersAsync(null, null, new(0, 10), default));
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => service.AuditAsync(null, new(0, 10), default));
    }
    [Fact] public async Task Unscoped_detail_is_empty_without_database_connection()
    {
        var queries = Queries();
        Assert.Null(await queries.UserAsync(Empty, Guid.NewGuid(), default));
        Assert.Null(await queries.AuditEventAsync(Empty, Guid.NewGuid(), default));
        Assert.Null(await queries.RequestAsync(Empty, Guid.NewGuid(), default));
        Assert.Empty((await queries.UserRolesAsync(Empty, Guid.NewGuid(), new(0, 10), default)).Items);
        Assert.Empty((await queries.GrantsAsync(Empty, Guid.NewGuid(), Guid.NewGuid(), new(0, 10), default)).Items);
    }
    [Fact] public void Read_dtos_have_no_credential_or_raw_payload_fields()
    {
        var types = new[] { typeof(UserView), typeof(UserRoleView), typeof(GrantView), typeof(GrantFunctionView), typeof(PermissionView), typeof(AuditView), typeof(ChangeRequestView) };
        foreach (var type in types)
        foreach (var property in type.GetProperties())
            Assert.DoesNotMatch("(?i)password|hash|secret|token|payload|privatekey", property.Name);
    }
    [Fact] public async Task Read_adapters_are_canonical_human_get_only()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], EnvironmentName = "Testing" });
        builder.Services.AddAuthorization(); builder.Services.AddScoped<ManagementQueryService>();
        await using var app = builder.Build(); app.MapIdentityManagementQueries();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(x => x.Endpoints).OfType<RouteEndpoint>().ToArray();
        Assert.Equal(9, endpoints.Length);
        Assert.Contains(endpoints, e => e.RoutePattern.RawText == "/api/identity/v1/admin/station-grants/{grantId:guid}/functions");
        foreach (var endpoint in endpoints)
        {
            Assert.StartsWith("/api/identity/v1/admin/", endpoint.RoutePattern.RawText);
            Assert.Contains(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(), a => a.Policy == "Fdc.Human");
            Assert.Equal(["GET"], endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods);
        }
        Assert.DoesNotContain(endpoints, e => e.RoutePattern.RawText!.Contains("apply", StringComparison.OrdinalIgnoreCase));
    }
    private static SqlManagementQueries Queries() => new(new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
        .UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options));
}
