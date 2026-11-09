using System.Security.Claims;
using System.Text.Json;
using Fdc.Identity.Application;
using Fdc.Identity.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace Fdc.Identity.Api;

// Candidate HTTP boundary. The Center host deliberately does not map it.
// Implementations must authorize the operation and commit state/audit before returning a proof.
public interface IHumanAuthenticationFlow
{
    Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct);
    Task<LoginResult> VerifyMfaAsync(string challenge, string factorResponse, CancellationToken ct);
    Task<bool> LogoutAsync(string currentSessionDigest, CancellationToken ct);
}

public static class HumanAuthenticationEndpoints
{
    private const string SessionCookie = "__Host-FdcSession";
    private const int BodyLimit = 8192;
    private sealed record LoginInput(string? Username, string? Password);
    private sealed record MfaInput(string? Challenge, string? FactorResponse);

    public static void MapHumanAuthenticationCandidates(this IEndpointRouteBuilder app, Func<Uri> reviewedOrigin,
        SameSiteMode reviewedSameSite)
    {
        _ = HumanRequestSecurity.Cookie(reviewedSameSite);
        var group = app.MapGroup("/api/identity/v1/auth");
        group.MapGet("/csrf", (HttpContext h, IAntiforgery csrf) =>
        {
            h.Response.Headers.CacheControl = "no-store";
            var origin = reviewedOrigin().GetLeftPart(UriPartial.Authority);
            if (!h.Request.IsHttps || h.Connection.ClientCertificate is not null || "https://" + h.Request.Host != origin)
                return Results.Unauthorized();
            return Results.Ok(new { token = csrf.GetAndStoreTokens(h).RequestToken });
        }).AllowAnonymous();

        group.MapPost("/login", async (HttpContext h, IAntiforgery csrf, IHumanAuthenticationFlow flow, CancellationToken ct) =>
        {
            if (!await Guard(h, csrf, reviewedOrigin)) return Results.StatusCode(403);
            if (h.User.Identity?.IsAuthenticated == true) return Results.Conflict();
            var input = await Read<LoginInput>(h, ct);
            if (input?.Username is not { Length: > 0 and <= 256 } || input.Password is not { Length: > 0 and <= 2048 })
                return Results.BadRequest(new { code = "INVALID_REQUEST" });
            return await Complete(h, () => flow.LoginAsync(input.Username, input.Password, ct), reviewedSameSite, allowChallenge: true);
        }).AllowAnonymous();

        group.MapPost("/mfa/verify", async (HttpContext h, IAntiforgery csrf, IHumanAuthenticationFlow flow, CancellationToken ct) =>
        {
            if (!await Guard(h, csrf, reviewedOrigin)) return Results.StatusCode(403);
            if (h.User.Identity?.IsAuthenticated == true) return Results.Conflict();
            var input = await Read<MfaInput>(h, ct);
            if (!OpaqueProofs.IsValid(input?.Challenge) || input!.FactorResponse is not { Length: > 0 and <= 2048 })
                return Results.BadRequest(new { code = "INVALID_REQUEST" });
            return await Complete(h, () => flow.VerifyMfaAsync(input.Challenge!, input.FactorResponse, ct), reviewedSameSite, allowChallenge: false);
        }).AllowAnonymous();

        group.MapPost("/logout", async (HttpContext h, IAntiforgery csrf, IHumanAuthenticationFlow flow, CancellationToken ct) =>
        {
            if (!await Guard(h, csrf, reviewedOrigin)) return Results.StatusCode(403);
            var digest = h.User.FindFirstValue("fdc.session.digest");
            if (digest is null) return Results.Unauthorized();
            try
            {
                if (!await flow.LogoutAsync(digest, ct)) return Results.Unauthorized();
                h.Response.Cookies.Delete(SessionCookie, HumanRequestSecurity.Cookie(reviewedSameSite));
                return Results.NoContent();
            }
            catch (Exception error) when (error is AuthorityUnavailableException or OwnerDecisionRequiredException)
            { return Unavailable(); }
        }).RequireAuthorization("Fdc.Human");
    }

    private static async Task<bool> Guard(HttpContext h, IAntiforgery csrf, Func<Uri> origin)
    {
        h.Response.Headers.CacheControl = "no-store";
        var limit = h.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (h.Request.ContentLength > BodyLimit || limit is null || limit.IsReadOnly) return false;
        limit.MaxRequestBodySize = Math.Min(limit.MaxRequestBodySize ?? BodyLimit, BodyLimit);
        return await new HumanRequestSecurity(origin()).ValidateMutationAsync(h, csrf);
    }

    private static async Task<T?> Read<T>(HttpContext h, CancellationToken ct)
    {
        if (!h.Request.HasJsonContentType() || h.Request.ContentLength > BodyLimit) return default;

        try { return await h.Request.ReadFromJsonAsync<T>(ct); }
        catch (Exception error) when (error is JsonException or BadHttpRequestException) { return default; }
    }

    private static async Task<IResult> Complete(HttpContext h, Func<Task<LoginResult>> action, SameSiteMode sameSite, bool allowChallenge)
    {
        try
        {
            var result = await action();
            if (result.Outcome == LoginOutcome.Rejected) return Results.Json(new { code = "AUTHENTICATION_REJECTED" }, statusCode: 401);
            if (!OpaqueProofs.IsValid(result.Proof)) return Unavailable();
            if (result.Outcome == LoginOutcome.MfaRequired && allowChallenge)
                return Results.Json(new { code = "MFA_REQUIRED", challenge = result.Proof }, statusCode: 202);
            if (allowChallenge || result.Outcome != LoginOutcome.SessionCreated) return Unavailable();
            HumanRequestSecurity.WriteSession(h, result.Proof!, sameSite);
            return Results.NoContent();
        }
        catch (Exception error) when (error is AuthorityUnavailableException or OwnerDecisionRequiredException)
        { return Unavailable(); }
    }

    private static IResult Unavailable() => Results.Json(new { code = "AUTHENTICATION_UNAVAILABLE" }, statusCode: 503);
}
