namespace Fdc.Center;

internal static class LabReadinessEndpoints
{
    internal static void MapLabReadiness(this WebApplication app)
    {
        // The reviewed station-operation map is pending. Do not bind request bodies or access records.
        app.MapGet("/api/lab/v1/readiness", (HttpContext context) => Results.Json(
            new { code = "LAB_READINESS_STATION_POLICY_PENDING", correlationId = context.TraceIdentifier },
            statusCode: StatusCodes.Status503ServiceUnavailable))
            .RequireAuthorization("Fdc.Human");
    }
}
