using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fdc.Center;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;

namespace Fdc.Center.Tests;

public sealed class A11OcspTests
{
    [Theory]
    [InlineData("good", CollectorCertificateStatus.CertValid)]
    [InlineData("revoked", CollectorCertificateStatus.CertRevoked)]
    [InlineData("unknown", CollectorCertificateStatus.RevocationUnknown)]
    [InlineData("foreign-signer", CollectorCertificateStatus.RevocationUnknown)]
    [InlineData("bad-signature", CollectorCertificateStatus.RevocationUnknown)]
    [InlineData("wrong-cert-id", CollectorCertificateStatus.RevocationUnknown)]
    [InlineData("stale", CollectorCertificateStatus.RevocationUnknown)]
    [InlineData("timeout", CollectorCertificateStatus.RevocationUnavailable)]
    [InlineData("http-500", CollectorCertificateStatus.RevocationUnavailable)]
    [InlineData("unreachable", CollectorCertificateStatus.RevocationUnavailable)]
    public async Task Issuer_signed_ocsp_over_loopback_is_fail_closed(string scenario, CollectorCertificateStatus expected)
    {
        var now = DateTimeOffset.UtcNow;
        using var issuerKey = RSA.Create(2048);
        using var foreignKey = RSA.Create(2048);
        using var leafKey = RSA.Create(2048);
        var caRequest = new CertificateRequest("CN=A11 OCSP Test CA", issuerKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        caRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        using var ca = caRequest.CreateSelfSigned(now.AddHours(-2), now.AddHours(2));
        var leafRequest = new CertificateRequest("CN=A11 Collector", leafKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        leafRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.2") }, true));
        using var leaf = leafRequest.Create(ca, now.AddMinutes(-30), now.AddMinutes(30), RandomNumberGenerator.GetBytes(16));
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(server => server.Listen(IPAddress.Loopback, 0));
        await using var app = builder.Build();
        var requests = 0;
        app.MapPost("/ocsp", async (HttpContext context) =>
        {
            Interlocked.Increment(ref requests);
            if (scenario == "http-500") return Results.StatusCode(500);
            if (scenario == "timeout") await Task.Delay(Timeout.InfiniteTimeSpan, context.RequestAborted);
            using var incoming = new MemoryStream();
            await context.Request.Body.CopyToAsync(incoming, context.RequestAborted);
            var id = new OcspReq(incoming.ToArray()).GetRequestList().Single().GetCertID();
            if (scenario == "wrong-cert-id")
                id = CertificateID.DeriveCertificateID(id, id.SerialNumber.Add(Org.BouncyCastle.Math.BigInteger.One));
            var signer = DotNetUtilities.GetRsaKeyPair(scenario is "foreign-signer" or "bad-signature" ? foreignKey : issuerKey);
            var responder = scenario == "bad-signature" ? DotNetUtilities.GetRsaKeyPair(issuerKey).Public : signer.Public;
            var generator = new BasicOcspRespGenerator(responder);
            CertificateStatus? state = scenario switch
            {
                "revoked" => new RevokedStatus(now.AddMinutes(-5).UtcDateTime),
                "unknown" => new UnknownStatus(),
                _ => null
            };
            generator.AddResponse(id, state, now.AddMinutes(-10).UtcDateTime,
                (scenario == "stale" ? now.AddMinutes(-1) : now.AddMinutes(10)).UtcDateTime, null);
            var basic = generator.Generate(new Asn1SignatureFactory("SHA256WITHRSA", signer.Private), null, now.AddMinutes(-2).UtcDateTime);
            var encoded = new OcspResponse(new OcspResponseStatus(0),
                new ResponseBytes(OcspObjectIdentifiers.PkixOcspBasic, new DerOctetString(basic.GetEncoded()))).GetDerEncoded();
            return Results.Bytes(encoded, "application/ocsp-response");
        });
        await app.StartAsync();
        var origin = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
        if (scenario == "unreachable") await app.StopAsync();
        using var validator = new PinnedOcspCertificateValidator(ca, TimeProvider.System, new Uri(origin, "/ocsp"),
            scenario == "timeout" ? TimeSpan.FromSeconds(2) : TimeSpan.FromSeconds(5));
        Assert.Equal(expected, await validator.ValidateAsync(leaf, default));
        Assert.Equal(scenario == "unreachable" ? 0 : 1, requests);
    }
}
