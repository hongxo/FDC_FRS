using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fdc.Center.Persistence;
using Fdc.Core;
using Fdc.Identity.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fdc.Center;

// Candidate contract for isolated Lab hosts only. Main Center startup does NOT call this method.
// A real deployment needs reviewed D01/D07 certificate validation/registry authority and limits.
public static class PilotIngestionEndpoints
{
    public const string MachineScheme = "Fdc.Pilot.Machine";
    public static void MapPilotIngestionContract(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pilot/v1/ingestion").RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = MachineScheme });
        group.MapPost("/", (HttpContext context, [FromServices] PilotFileIntake intake, CancellationToken ct) => Guard(context, async () =>
        {
            using var bytes = new MemoryStream(); var buffer = new byte[8192];
            while (true)
            {
                var count = await context.Request.Body.ReadAsync(buffer, ct); if (count == 0) break;
                if (bytes.Length + count > 65536) return Results.StatusCode(413);
                bytes.Write(buffer, 0, count);
            }
            var manifest = JsonSerializer.Deserialize<Manifest>(bytes.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web) { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow }) ?? throw new InvalidDataException();
            var ticket = await intake.BeginAsync(manifest, context.TraceIdentifier, ct);
            return Results.Json(ticket, statusCode: 201);
        }));
        group.MapPut("/{ticket:guid}/files/{name}", (HttpContext context, Guid ticket, string name, [FromServices] PilotFileIntake intake, CancellationToken ct) =>
            Guard(context, async () => { await intake.PutFileAsync(ticket, name, context.Request.Body, ct); return Results.NoContent(); }));
        group.MapPost("/{ticket:guid}/complete", (HttpContext context, Guid ticket, [FromServices] PilotFileIntake intake, CancellationToken ct) =>
            Guard(context, async () => Results.Json(await intake.CompleteAsync(ticket, ct))));
        group.MapGet("/{ticket:guid}", (HttpContext context, Guid ticket, [FromServices] PilotFileIntake intake, CancellationToken ct) =>
            Guard(context, async () => Results.Json(await intake.StatusAsync(ticket, ct))));
    }
    private static async Task<IResult> Guard(HttpContext context, Func<Task<IResult>> action)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!context.Request.IsHttps || context.Connection.ClientCertificate is null ||
            !context.User.Identities.Any(i => i.IsAuthenticated && i.AuthenticationType == MachineScheme) ||
            context.Request.Cookies.ContainsKey("__Host-FdcSession")) return Results.StatusCode(403);
        try { return await action(); }
        catch (OwnerDecisionRequiredException) { return Error(503, "OWNER_POLICY_REQUIRED"); }
        catch (AuthorityUnavailableException) { return Error(503, "AUTHORITY_UNAVAILABLE"); }
        catch (DbException) { return Error(503, "AUTHORITY_UNAVAILABLE"); }
        catch (UnauthorizedAccessException) { return Error(403, "STATION_BINDING_DENIED"); }
        catch (JsonException) { return Error(400, "INVALID_MANIFEST"); }
        catch (InvalidDataException) { return Error(400, "INTEGRITY_OR_MANIFEST_INVALID"); }
        catch (ArgumentException) { return Error(400, "INVALID_INPUT"); }
        catch (IOException) { return Error(503, "STORAGE_OR_TRANSFER_UNAVAILABLE"); }
        IResult Error(int status, string code) => Results.Json(new { code, correlationId = context.TraceIdentifier }, statusCode: status);
    }
}
