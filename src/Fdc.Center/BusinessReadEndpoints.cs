using System.Data.Common;
using Fdc.Identity.Application;

namespace Fdc.Center;

public static class BusinessReadEndpoints
{
    public static void MapBusinessReadEndpoints(this WebApplication app)
    {
        app.MapGet("/identity/business.html", () =>
        {
            var stream = typeof(BusinessReadEndpoints).Assembly.GetManifestResourceStream("Fdc.Center.IdentityUi.business.html")
                ?? throw new InvalidOperationException("Business view resource missing.");
            return Results.Stream(stream, "text/html; charset=utf-8");
        }).RequireAuthorization("Fdc.Human");
        var group = app.MapGroup("/api/business/v1").RequireAuthorization("Fdc.Human");
        group.MapGet("/dashboard", (HttpContext h, BusinessAuthorization auth, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(new { stations = await auth.DashboardAsync(digest, ct) })));

        MapObjects("/stations", BusinessReadOperation.Station);
        MapObjects("/ieds", BusinessReadOperation.Ied);
        MapObjects("/records", BusinessReadOperation.Record);
        MapObjects("/collectors", BusinessReadOperation.Collector);
        group.MapGet("/record-files/{id:guid}/download", (HttpContext h, BusinessAuthorization auth,
            IReviewedBusinessFileSource storage, Guid id, CancellationToken ct) =>
            Guard(h, async digest => await auth.DownloadAsync(digest, id, storage, h.TraceIdentifier, ct) is { } file
                ? Results.File(file.Bytes, "application/octet-stream", file.Name, enableRangeProcessing: false)
                : Results.NotFound()));
        return;

        void MapObjects(string path, BusinessReadOperation operation)
        {
            group.MapGet(path, (HttpContext h, BusinessAuthorization auth, Guid? station, int? offset, int? limit, CancellationToken ct) =>
                Guard(h, async digest => Results.Ok(await auth.ListAsync(digest, operation, station, offset ?? 0, limit ?? 50, ct))));
            group.MapGet(path + "/{id:guid}", (HttpContext h, BusinessAuthorization auth, Guid id, CancellationToken ct) =>
                Guard(h, async digest => await auth.DetailAsync(digest, operation, id, ct) is { } row
                    ? Results.Ok(row) : Results.NotFound()));
        }
    }

    private static async Task<IResult> Guard(HttpContext context, Func<string, Task<IResult>> action)
    {
        context.Response.Headers.CacheControl = "no-store";
        var human = context.User.Identities.FirstOrDefault(identity => identity.IsAuthenticated && identity.AuthenticationType == "Fdc.Human");
        var digest = human?.FindFirst("fdc.session.digest")?.Value;
        if (!context.Request.IsHttps || context.Connection.ClientCertificate is not null || string.IsNullOrWhiteSpace(digest))
            return Results.Unauthorized();
        try { return await action(digest); }
        catch (UnauthorizedAccessException) { return Results.Forbid(); }
        catch (ArgumentException) { return Results.BadRequest(new { code = "INVALID_QUERY" }); }
        catch (OwnerDecisionRequiredException) { return Results.Json(new { code = "BUSINESS_POLICY_REQUIRED" }, statusCode: 503); }
        catch (Exception error) when (error is DbException or AuthorityUnavailableException)
        { return Results.Json(new { code = "AUTHORITY_UNAVAILABLE" }, statusCode: 503); }
    }
}
