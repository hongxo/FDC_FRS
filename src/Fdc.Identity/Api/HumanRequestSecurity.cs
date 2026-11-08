using Fdc.Identity.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Fdc.Identity.Api;

public sealed class HumanRequestSecurity
{
    private readonly string origin;
    public HumanRequestSecurity(Uri approvedOrigin)
    {
        if (!approvedOrigin.IsAbsoluteUri || approvedOrigin.Scheme != "https" || approvedOrigin.AbsolutePath != "/" ||
            approvedOrigin.Query.Length != 0 || approvedOrigin.Fragment.Length != 0 || approvedOrigin.UserInfo.Length != 0)
            throw new ArgumentException("Exact HTTPS origin required.");
        origin = approvedOrigin.GetLeftPart(UriPartial.Authority);
    }
    public async Task<bool> ValidateMutationAsync(HttpContext context, IAntiforgery antiforgery)
    {
        // Trust TLS/listener and host only after the separately reviewed proxy boundary. Forwarded headers are never credentials.
        if (!context.Request.IsHttps || context.Connection.ClientCertificate is not null ||
            context.Request.Headers.Origin.Count != 1 ||
            !string.Equals(context.Request.Headers.Origin[0], origin, StringComparison.Ordinal) ||
            !string.Equals($"https://{context.Request.Host}", origin, StringComparison.Ordinal)) return false;
        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method) || HttpMethods.IsOptions(context.Request.Method)) return false;
        try { await antiforgery.ValidateRequestAsync(context); return true; }
        catch (AntiforgeryValidationException) { return false; }
    }
    public static CookieOptions Cookie(SameSiteMode reviewedSameSite)
    {
        if (reviewedSameSite is not SameSiteMode.Strict and not SameSiteMode.Lax) throw new ArgumentException("Reviewed SameSite policy required.");
        return new() { Secure = true, HttpOnly = true, Path = "/", SameSite = reviewedSameSite, IsEssential = true };
    }
    public static void WriteSession(HttpContext context, string token, SameSiteMode reviewedSameSite)
    {
        if (!context.Request.IsHttps || !OpaqueProofs.IsValid(token)) throw new InvalidOperationException("HTTPS and valid session proof required.");
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Cookies.Append("__Host-FdcSession", token, Cookie(reviewedSameSite));
    }
}
