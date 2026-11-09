using Fdc.Identity.Application;

namespace Fdc.Center;

internal static class HumanIdentityUi
{
    private static readonly (string Screen, ManagementReadOperation Operation)[] ManagementScreens =
    [
        ("users", ManagementReadOperation.Users),
        ("permissions", ManagementReadOperation.PermissionCatalog),
        ("grants", ManagementReadOperation.StationGrants),
        ("audit", ManagementReadOperation.SecurityAudit),
        ("requests", ManagementReadOperation.ChangeRequest)
    ];

    internal static void MapHumanIdentityUi(this WebApplication app)
    {
        // Public status is invariant: no account lookup, request body, token, or runtime activation.
        app.MapGet("/api/identity/v1/ui/auth-status", (HttpContext context) =>
        {
            if (context.Connection.ClientCertificate is not null) return Results.Unauthorized();
            return Results.Ok(new { login = "BLOCKED_BY_POLICY", mfa = "BLOCKED_BY_POLICY",
                recovery = "BLOCKED_BY_POLICY", profile = "BLOCKED_BY_POLICY", logout = "BLOCKED_BY_POLICY" });
        }).AllowAnonymous();

        app.MapGet("/identity/management.html", () =>
        {
            var assembly = typeof(HumanIdentityUi).Assembly;
            var html = assembly.GetManifestResourceStream("Fdc.Center.IdentityUi.management.html")
                ?? throw new InvalidOperationException("Missing identity management page.");
            return Results.Stream(html, "text/html; charset=utf-8");
        }).RequireAuthorization("Fdc.Human");

        app.MapGet("/api/identity/v1/ui/navigation", async (IManagementReadAuthority management,
            IRoleAdministrationAuthority roles, CancellationToken ct) =>
        {
            var available = new List<string>();
            foreach (var (screen, operation) in ManagementScreens)
                if (await MayRead(async () => { await management.DemandAsync(operation, ct); }))
                    available.Add(screen);
            if (await MayRead(async () => { await roles.DemandReadAsync(ct); }))
                available.Add("roles");
            return Results.Ok(new { screens = available });
        }).RequireAuthorization("Fdc.Human");
    }

    private static async Task<bool> MayRead(Func<Task> demand)
    {
        try { await demand(); return true; }
        catch (OwnerDecisionRequiredException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }
}
