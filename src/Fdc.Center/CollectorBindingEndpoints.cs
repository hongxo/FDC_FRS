using Fdc.Identity.Api;
using Microsoft.AspNetCore.Antiforgery;

namespace Fdc.Center;

public static class CollectorBindingEndpoints
{
    public static void MapCollectorBindings(this WebApplication app)
    {
        app.MapGet("/identity/collector-bindings.html", () => Results.Stream(
            typeof(CollectorBindingEndpoints).Assembly.GetManifestResourceStream("Fdc.Center.IdentityUi.collector-bindings.html")!,
            "text/html; charset=utf-8")).RequireAuthorization("Fdc.Human");
        var group = app.MapGroup("/api/business/v1/collector-binding-requests").RequireAuthorization("Fdc.Human");
        group.MapGet("/", (HttpContext h, CollectorBindingService service, CancellationToken ct) =>
            LabWorkflowEndpoints.Guard(h, async (digest, _) => Results.Ok(await service.ListAsync(digest, ct))));
        group.MapPost("/", (HttpContext h, CollectorBindingService service, ILabHttpOrigin origin, IAntiforgery csrf, CancellationToken ct) =>
            LabWorkflowEndpoints.Guard(h, async (digest, actor) =>
            {
                if (!await new HumanRequestSecurity(origin.Demand()).ValidateMutationAsync(h, csrf)) return Results.Forbid();
                var body = await h.Request.ReadFromJsonAsync<CollectorBindingDraft>(ct);
                if (body is null || !Guid.TryParse(h.Request.Headers["Idempotency-Key"], out var id) || id != body.RequestId)
                    return Results.BadRequest();
                return Results.Ok(await service.RequestAsync(digest, actor, body, h.TraceIdentifier, ct));
            }));
        group.MapPost("/{id:guid}", (HttpContext h, CollectorBindingService service, ILabHttpOrigin origin,
            IAntiforgery csrf, Guid id, CancellationToken ct) =>
            LabWorkflowEndpoints.Guard(h, async (digest, actor) =>
            {
                if (!await new HumanRequestSecurity(origin.Demand()).ValidateMutationAsync(h, csrf)) return Results.Forbid();
                if (h.Request.Headers.IfMatch.Count != 1) return Results.StatusCode(428);
                var body = await h.Request.ReadFromJsonAsync<BindingDecision>(ct);
                if (body is null || h.Request.Headers.IfMatch.ToString() != body.ETag) return Results.BadRequest();
                return Results.Ok(await service.TransitionAsync(digest, actor, id, body, h.TraceIdentifier, ct));
            }));
    }
}
