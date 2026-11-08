using System.Security.Claims;
using System.Text.Encodings.Web;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fdc.Identity.Api;

// Opt-in adapter only. Hosts must not register this scheme until D01/D05 policy and route contracts are reviewed.
public sealed class HumanSessionHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
    UrlEncoder encoder, IHumanSessionAuthorityReader authority, TimeProvider clock)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private bool unavailable;
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.IsHttps || Context.Connection.ClientCertificate is not null) return AuthenticateResult.Fail("Human HTTPS session required.");
        if (!Request.Cookies.TryGetValue("__Host-FdcSession", out var token)) return AuthenticateResult.NoResult();
        var occurrences = Request.Headers.Cookie.ToString().Split(';').Count(x => x.TrimStart().StartsWith("__Host-FdcSession=", StringComparison.Ordinal));
        if (occurrences != 1 || !OpaqueProofs.IsValid(token)) return AuthenticateResult.Fail("Invalid session.");
        try
        {
            var digest = OpaqueProofs.Digest(token, "session");
            var state = await authority.ReadAsync(digest, Context.RequestAborted);
            if (state is null || !SessionValidity.IsCurrent(state.User, state.Session, state.Authority, clock.GetUtcNow())) return AuthenticateResult.Fail("Invalid session.");
            var identity = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, state.User.Id.ToString("D")),
                new Claim("fdc.session.id", state.SessionId.ToString("D")),
                new Claim("fdc.session.digest", digest)], "Fdc.Human");
            return AuthenticateResult.Success(new(new ClaimsPrincipal(identity), Scheme.Name));
        }
        catch (AuthorityUnavailableException)
        {
            unavailable = true;
            return AuthenticateResult.Fail("Authority unavailable.");
        }
    }
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.CacheControl = "no-store";
        Response.StatusCode = unavailable ? 503 : 401;
        return Task.CompletedTask;
    }
}
