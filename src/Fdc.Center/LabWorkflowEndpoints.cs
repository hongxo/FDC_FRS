using System.Data.Common;
using System.Security.Claims;
using Fdc.Identity.Api;
using Fdc.Identity.Application;
using Microsoft.AspNetCore.Antiforgery;

namespace Fdc.Center;

public interface ILabHttpOrigin { Uri Demand(); }
public sealed class PendingLabHttpOrigin : ILabHttpOrigin
{
    public Uri Demand() => throw new OwnerDecisionRequiredException("D01/D04/D08: exact reviewed Lab Human origin required.");
}

public static class LabWorkflowEndpoints
{
    public static void MapLabWorkflow(this WebApplication app)
    {
        app.MapGet("/identity/lab.html", () => Results.Stream(
            typeof(LabWorkflowEndpoints).Assembly.GetManifestResourceStream("Fdc.Center.IdentityUi.lab.html")!,
            "text/html; charset=utf-8")).RequireAuthorization("Fdc.Human");
        var group = app.MapGroup("/api/lab/v1/tests").RequireAuthorization("Fdc.Human");
        group.MapGet("/", (HttpContext h, LabWorkflowService service, CancellationToken ct) =>
            Guard(h, async (digest, _) => Results.Ok(await service.ListAsync(digest, ct))));
        group.MapGet("/{id}/{revision:int}", (HttpContext h, LabWorkflowService service, string id, int revision, CancellationToken ct) =>
            Guard(h, async (digest, _) =>
            {
                var row = await service.DetailAsync(digest, id, revision, ct);
                if (row is null) return Results.NotFound();
                h.Response.Headers.ETag = LabWorkflowService.ETag(row.VersionStamp);
                return Results.Ok(row);
            }));
        group.MapGet("/{id}/{revision:int}/history", (HttpContext h, LabWorkflowService service, string id, int revision, CancellationToken ct) =>
            Guard(h, async (digest, _) => Results.Ok(await service.HistoryAsync(digest, id, revision, ct))));
        group.MapGet("/{id}/{revision:int}/export", (HttpContext h, LabWorkflowService service, string id, int revision, CancellationToken ct) =>
            Guard(h, async (digest, _) => await service.ExportAsync(digest, id, revision, h.TraceIdentifier, ct) is { } bytes
                ? Results.File(bytes, "application/json", "synthetic-lab-evidence.json") : Results.NotFound()));
        group.MapGet("/exports/{job:guid}/download", (HttpContext h, LabExportCoordinator service, Guid job, CancellationToken ct) =>
            Guard(h, async (digest, _) => await service.DownloadAsync(digest, job, h.TraceIdentifier, ct) is { } bytes
                ? Results.File(bytes, "application/json", "synthetic-lab-evidence.json") : Results.NotFound()));
        group.MapPost("/{id}/{revision:int}/exports", (HttpContext h, LabExportCoordinator service, ILabHttpOrigin origin,
            IAntiforgery csrf, string id, int revision, CancellationToken ct) =>
            Guard(h, async (digest, actor) =>
            {
                if (!await new HumanRequestSecurity(origin.Demand()).ValidateMutationAsync(h, csrf)) return Results.Forbid();
                if (!Guid.TryParse(h.Request.Headers["Idempotency-Key"], out var key)) return Results.BadRequest(new { code = "IDEMPOTENCY_REQUIRED" });
                var job = await service.RequestAsync(digest, actor, key, id, revision, h.TraceIdentifier, ct);
                return Results.Accepted(value: new { jobId = job });
            }));
        group.MapPost("/", (HttpContext h, LabWorkflowService service, ILabHttpOrigin origin, IAntiforgery csrf, CancellationToken ct) =>
            Guard(h, async (digest, actor) =>
            {
                if (!await new HumanRequestSecurity(origin.Demand()).ValidateMutationAsync(h, csrf)) return Results.Forbid();
                if (!Guid.TryParse(h.Request.Headers["Idempotency-Key"], out var key)) return Results.BadRequest(new { code = "IDEMPOTENCY_REQUIRED" });
                var body = await h.Request.ReadFromJsonAsync<SyntheticLabDraft>(ct);
                if (body is null) return Results.BadRequest();
                return Results.Ok(await service.CreateAsync(digest, actor, key, body, h.TraceIdentifier, ct));
            }));
        group.MapPost("/{id}/{revision:int}/transition", (HttpContext h, LabWorkflowService service, ILabHttpOrigin origin,
            IAntiforgery csrf, string id, int revision, CancellationToken ct) =>
            Guard(h, async (digest, actor) =>
            {
                if (!await new HumanRequestSecurity(origin.Demand()).ValidateMutationAsync(h, csrf)) return Results.Forbid();
                if (!Guid.TryParse(h.Request.Headers["Idempotency-Key"], out var key)) return Results.BadRequest(new { code = "IDEMPOTENCY_REQUIRED" });
                if (h.Request.Headers.IfMatch.Count != 1) return Results.StatusCode(428);
                var body = await h.Request.ReadFromJsonAsync<LabTransition>(ct);
                if (body is null || body.RequestId != key || body.ETag != h.Request.Headers.IfMatch.ToString()) return Results.BadRequest();
                return Results.Ok(await service.TransitionAsync(digest, actor, id, revision, body, h.TraceIdentifier, ct));
            }));
    }

    internal static async Task<IResult> Guard(HttpContext h, Func<string, Guid, Task<IResult>> action)
    {
        h.Response.Headers.CacheControl = "no-store";
        h.Response.Headers["X-Correlation-Id"] = h.TraceIdentifier;
        var human = h.User.Identities.SingleOrDefault(identity => identity.IsAuthenticated && identity.AuthenticationType == "Fdc.Human");
        var digest = human?.FindFirst("fdc.session.digest")?.Value;
        if (!h.Request.IsHttps || h.Connection.ClientCertificate is not null || string.IsNullOrWhiteSpace(digest) ||
            !Guid.TryParse(human?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var actor)) return Results.Unauthorized();
        HumanViewStamp.Set(h, digest);
        try { return await action(digest, actor); }
        catch (UnauthorizedAccessException) { return Results.Json(new { code = "LAB_AUTHORIZATION_DENIED", correlationId = h.TraceIdentifier }, statusCode: 403); }
        catch (LabConflictException) { return Results.Json(new { code = "WORKFLOW_CONFLICT", correlationId = h.TraceIdentifier }, statusCode: 409); }
        catch (OwnerDecisionRequiredException) { return Results.Json(new { code = "LAB_POLICY_PENDING" }, statusCode: 503); }
        catch (Exception error) when (error is DbException or AuthorityUnavailableException or CommitOutcomeUnknownException)
        { return Results.Json(new { code = "AUTHORITY_UNAVAILABLE", correlationId = h.TraceIdentifier }, statusCode: 503); }
        catch (Exception error) when (error is ArgumentException or System.Text.Json.JsonException or BadHttpRequestException)
        { return Results.BadRequest(new { code = "INVALID_LAB_REQUEST" }); }
    }
}
