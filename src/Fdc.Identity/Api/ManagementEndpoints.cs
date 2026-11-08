using Fdc.Identity.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fdc.Identity.Api;

// Opt-in canonical read adapters only. No host calls this extension; D01/D04/D05 remain activation dependencies.
public static class ManagementEndpoints
{
    public static IEndpointRouteBuilder MapIdentityManagementQueries(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity/v1/admin").RequireAuthorization("Fdc.Human");
        group.MapGet("/users", (HttpContext h, ManagementQueryService s, string? search, bool? active, int? offset, int? limit, CancellationToken ct) =>
            Guard(h, async () => Results.Ok(await s.UsersAsync(search, active, Window(offset, limit), ct))));
        group.MapGet("/users/{id:guid}", (HttpContext h, ManagementQueryService s, Guid id, CancellationToken ct) =>
            Guard(h, async () => await s.UserAsync(id, ct) is { } row ? Results.Ok(row) : Results.NotFound()));
        group.MapGet("/users/{id:guid}/roles", (HttpContext h, ManagementQueryService s, Guid id, int? offset, int? limit, CancellationToken ct) =>
            Guard(h, async () => Results.Ok(await s.RolesAsync(id, Window(offset, limit), ct))));
        group.MapGet("/station-grants", (HttpContext h, ManagementQueryService s, Guid userId, Guid userRoleId, int? offset, int? limit, CancellationToken ct) =>
            Guard(h, async () => Results.Ok(await s.GrantsAsync(userId, userRoleId, Window(offset, limit), ct))));
        group.MapGet("/permission-catalog", (HttpContext h, ManagementQueryService s, int? offset, int? limit, CancellationToken ct) =>
            Guard(h, async () => Results.Ok(await s.PermissionsAsync(Window(offset, limit), ct))));
        group.MapGet("/security-audit", (HttpContext h, ManagementQueryService s, string? action, int? offset, int? limit, CancellationToken ct) =>
            Guard(h, async () => Results.Ok(await s.AuditAsync(action, Window(offset, limit), ct))));
        group.MapGet("/security-audit/{id:guid}", (HttpContext h, ManagementQueryService s, Guid id, CancellationToken ct) =>
            Guard(h, async () => await s.AuditEventAsync(id, ct) is { } row ? Results.Ok(row) : Results.NotFound()));
        group.MapGet("/change-requests/{id:guid}", (HttpContext h, ManagementQueryService s, Guid id, CancellationToken ct) =>
            Guard(h, async () => await s.RequestAsync(id, ct) is { } row ? Results.Ok(row) : Results.NotFound()));
        return endpoints;
    }
    private static PageWindow Window(int? offset, int? limit) => new(offset ?? 0, limit ?? 50);
    private static async Task<IResult> Guard(HttpContext context, Func<Task<IResult>> action)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!context.Request.IsHttps || context.Connection.ClientCertificate is not null ||
            !context.User.Identities.Any(x => x.IsAuthenticated && x.AuthenticationType == "Fdc.Human")) return Results.Unauthorized();
        try { return await action(); }
        catch (OwnerDecisionRequiredException) { return Results.Json(new { code = "BLOCKED_BY_D04", correlationId = context.TraceIdentifier }, statusCode: 503); }
        catch (AuthorityUnavailableException) { return Results.Json(new { code = "AUTHORITY_UNAVAILABLE", correlationId = context.TraceIdentifier }, statusCode: 503); }
        catch (UnauthorizedAccessException) { return Results.Forbid(); }
        catch (ArgumentException) { return Results.BadRequest(new { code = "INVALID_REQUEST", correlationId = context.TraceIdentifier }); }
    }
}
