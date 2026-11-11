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
        group.MapGet("/navigation", (HttpContext h, BusinessAuthorization auth, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.NavigationAsync(digest, ct))));
        group.MapGet("/dashboard", (HttpContext h, BusinessAuthorization auth, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.OverviewAsync(digest, ct))));
        group.MapGet("/dashboard/trends", (HttpContext h, BusinessAuthorization auth, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.TrendsAsync(digest, ct))));
        group.MapGet("/dashboard/alerts", (HttpContext h, BusinessAuthorization auth, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.AlertsAsync(digest, ct))));

        MapObjects("/stations", BusinessReadOperation.Station);
        MapObjects("/ieds", BusinessReadOperation.Ied);
        MapObjects("/records", BusinessReadOperation.Record);
        MapObjects("/collectors", BusinessReadOperation.Collector);
        group.MapGet("/stations/{id:guid}/ieds", (HttpContext h, BusinessAuthorization auth, Guid id, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.IedsForStationAsync(digest, id, ct))));
        group.MapGet("/ieds/{id:guid}/collectors", (HttpContext h, BusinessAuthorization auth, Guid id, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.CollectorsForIedAsync(digest, id, ct))));
        group.MapGet("/collectors/{id:guid}/records", (HttpContext h, BusinessAuthorization auth, Guid id, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.RecordsForCollectorAsync(digest, id, ct))));
        group.MapGet("/collectors/{id:guid}/heartbeat", (HttpContext h, BusinessAuthorization auth, Guid id, int? offset, int? limit, bool? paged, CancellationToken ct) =>
            Guard(h, async digest =>
            {
                if (paged == true)
                {
                    var page = await auth.HeartbeatPageAsync(digest, id, offset ?? 0, limit ?? 50, ct);
                    return page is null ? Results.NotFound() : Results.Ok(page);
                }
                var rows = await auth.CollectorHeartbeatAsync(digest, id, offset ?? 0, limit ?? 50, ct);
                return rows is null ? Results.NotFound() : Results.Ok(rows);
            }));
        group.MapGet("/collectors/{id:guid}/transfers", (HttpContext h, BusinessAuthorization auth, Guid id, int? offset, int? limit, bool? paged, CancellationToken ct) =>
            Guard(h, async digest =>
            {
                if (paged == true)
                {
                    var page = await auth.TransferPageAsync(digest, id, offset ?? 0, limit ?? 50, ct);
                    return page is null ? Results.NotFound() : Results.Ok(page);
                }
                var rows = await auth.CollectorTransfersAsync(digest, id, offset ?? 0, limit ?? 50, ct);
                return rows is null ? Results.NotFound() : Results.Ok(rows);
            }));
        group.MapGet("/records/{id:guid}/downloadable-files", (HttpContext h, BusinessAuthorization auth, Guid id, CancellationToken ct) =>
            Guard(h, async digest => Results.Ok(await auth.DownloadableFilesAsync(digest, id, ct))));
        group.MapGet("/records/{id:guid}/waveform", (HttpContext h, BusinessAuthorization auth,
            IReviewedBusinessFileSource storage, Guid id, CancellationToken ct) =>
            Guard(h, async digest => await auth.WaveformAsync(digest, id, storage, h.TraceIdentifier, ct) is { } preview
                ? Results.Ok(preview) : Results.NotFound()));
        group.MapGet("/record-files/{id:guid}/download", (HttpContext h, BusinessAuthorization auth,
            IReviewedBusinessFileSource storage, Guid id, CancellationToken ct) =>
            Guard(h, async digest => await auth.DownloadAsync(digest, id, storage, h.TraceIdentifier, ct) is { } file
                ? Results.File(file.Content, "application/octet-stream", file.Name, enableRangeProcessing: false)
                : Results.NotFound()));
        return;

        void MapObjects(string path, BusinessReadOperation operation)
        {
            group.MapGet(path, (HttpContext h, BusinessAuthorization auth, Guid? station, int? offset, int? limit, string? search,
                Guid? iedId, DateTimeOffset? collectedFromUtc, DateTimeOffset? collectedToUtc, bool? paged, CancellationToken ct) =>
                Guard(h, async digest => paged == true
                    ? Results.Ok(await auth.PageAsync(digest, operation, station, offset ?? 0, limit ?? 50, ct, search,
                        new(iedId, collectedFromUtc, collectedToUtc)))
                    : Results.Ok(await auth.ListAsync(digest, operation, station, offset ?? 0, limit ?? 50, ct, search,
                        new(iedId, collectedFromUtc, collectedToUtc)))));
            group.MapGet(path + "/{id:guid}", (HttpContext h, BusinessAuthorization auth, Guid id, CancellationToken ct) =>
                Guard(h, async digest => await auth.DetailAsync(digest, operation, id, ct) is { } row
                    ? Results.Ok(row) : Results.NotFound()));
        }
    }

    private static async Task<IResult> Guard(HttpContext context, Func<string, Task<IResult>> action)
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["X-Correlation-Id"] = context.TraceIdentifier;
        var human = context.User.Identities.FirstOrDefault(identity => identity.IsAuthenticated && identity.AuthenticationType == "Fdc.Human");
        var digest = human?.FindFirst("fdc.session.digest")?.Value;
        if (!context.Request.IsHttps || context.Connection.ClientCertificate is not null || string.IsNullOrWhiteSpace(digest))
            return Results.Unauthorized();
        HumanViewStamp.Set(context, digest);
        try { return await action(digest); }
        catch (UnauthorizedAccessException) { return Results.Forbid(); }
        catch (ArgumentException) { return Results.BadRequest(new { code = "INVALID_QUERY" }); }
        catch (Exception error) when (error is InvalidDataException or NotSupportedException)
        { return Results.Json(new { code = "COMTRADE_PREVIEW_UNAVAILABLE" }, statusCode: 422); }
        catch (IOException) { return Results.Json(new { code = "STORAGE_UNAVAILABLE" }, statusCode: 503); }
        catch (OwnerDecisionRequiredException) { return Results.Json(new { code = "BUSINESS_POLICY_REQUIRED" }, statusCode: 503); }
        catch (Exception error) when (error is DbException or AuthorityUnavailableException)
        { return Results.Json(new { code = "AUTHORITY_UNAVAILABLE" }, statusCode: 503); }
    }
}
