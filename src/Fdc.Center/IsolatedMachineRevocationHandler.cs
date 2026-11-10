using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Fdc.Center;

// A candidate scheme for the isolated Pilot contract. No main runtime registration.
internal sealed record IsolatedMachineRevocation(
    PinnedCrlCertificateValidator Crl,
    Func<ReadOnlyMemory<byte>?> ReadCrl,
    PinnedOcspCertificateValidator Ocsp,
    Func<string, Guid?> ResolveCollector);

internal sealed class IsolatedMachineRevocationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IWebHostEnvironment environment,
    IsolatedMachineRevocation policy)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!environment.IsEnvironment("Testing") || !Request.IsHttps ||
            Context.Connection.RemoteIpAddress is not { } peer || !IPAddress.IsLoopback(peer))
            return AuthenticateResult.Fail("Isolated machine ingress required.");

        var certificate = await Context.Connection.GetClientCertificateAsync(Context.RequestAborted);
        if (certificate is null)
            return AuthenticateResult.NoResult();

        var fingerprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
        var collector = policy.ResolveCollector(fingerprint);
        if (collector is null)
            return AuthenticateResult.Fail("Unknown machine credential.");

        var crl = policy.Crl.Validate(certificate, policy.ReadCrl());
        var ocsp = crl == CollectorCertificateStatus.CertValid
            ? await policy.Ocsp.ValidateAsync(certificate, Context.RequestAborted)
            : (CollectorCertificateStatus?)null;
        Logger.LogInformation("Isolated machine revocation {CorrelationId} {CollectorId} {CrlStatus} {OcspStatus}",
            Context.TraceIdentifier, collector, crl, ocsp);
        if (crl != CollectorCertificateStatus.CertValid || ocsp != CollectorCertificateStatus.CertValid)
            return AuthenticateResult.Fail("Machine revocation evidence rejected.");

        var identity = new ClaimsIdentity([new(ClaimTypes.NameIdentifier, collector.Value.ToString())], Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }
}
