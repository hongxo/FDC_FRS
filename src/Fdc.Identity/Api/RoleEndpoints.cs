using Fdc.Identity.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fdc.Identity.Api;

// NOT registered in Fdc.Center. D01/D04/D05 need disposition before a host calls this extension.
// No login, mock-auth handler, API-key principal or force-apply endpoint is supplied here.
public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapIdentityRoleQueries(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity/v1/admin/roles").RequireAuthorization("Fdc.Human");
        group.MapGet("/", async (HttpContext context, RoleManagementService service, string? afterCode, int? limit, CancellationToken ct) =>
            await Guard(context, async () => Results.Ok(await service.ListAsync(afterCode, limit ?? 50, ct))));
        group.MapGet("/{id:guid}", async (HttpContext context, RoleManagementService service, Guid id, CancellationToken ct) =>
            await Guard(context, async () => await service.DetailAsync(id, ct) is { } row ? Results.Ok(row) : Results.NotFound()));
        group.MapGet("/{id:guid}/permissions", async (HttpContext context, RoleManagementService service, Guid id, CancellationToken ct) =>
            await Guard(context, async () => Results.Ok(await service.PermissionsAsync(id, ct))));
        return endpoints;
    }
    private static async Task<IResult> Guard(HttpContext context, Func<Task<IResult>> action)
    {
        context.Response.Headers.CacheControl = "no-store";
        // Scheme name must be supplied by the real server authentication handler, never a client header.
        if (!context.User.Identities.Any(i => i.IsAuthenticated && i.AuthenticationType == "Fdc.Human")) return Results.Unauthorized();
        try { return await action(); }
        catch (OwnerDecisionRequiredException) { return Results.Json(new { code = "OWNER_POLICY_REQUIRED", correlationId = context.TraceIdentifier }, statusCode: 503); }
        catch (AuthorityUnavailableException) { return Results.Json(new { code = "AUTHORITY_UNAVAILABLE", correlationId = context.TraceIdentifier }, statusCode: 503); }
        catch (UnauthorizedAccessException) { return Results.Forbid(); }
        catch (ArgumentException) { return Results.BadRequest(new { code = "INVALID_QUERY", correlationId = context.TraceIdentifier }); }
    }
}
