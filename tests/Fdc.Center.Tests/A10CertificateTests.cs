using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Fdc.Center;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fdc.Center.Tests;

public sealed class A10CertificateTests
{
    [Fact]
    public async Task Signed_full_CRL_valid_revoked_unknown_and_unavailable_are_distinct_and_mTLS_fails_closed()
    {
        var now = DateTimeOffset.UtcNow;
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=A10-Test-CA-" + Guid.NewGuid().ToString("N"), key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        using var issuer = request.CreateSelfSigned(now.AddDays(-2), now.AddDays(2));
        using var valid = Issue(issuer, now, "1.3.6.1.5.5.7.3.2");
        using var revoked = Issue(issuer, now, "1.3.6.1.5.5.7.3.2");
        using var expired = Issue(issuer, now.AddDays(-1), "1.3.6.1.5.5.7.3.2");
        using var wrongEku = Issue(issuer, now, "1.3.6.1.5.5.7.3.1");
        var builder = new CertificateRevocationListBuilder();
        builder.AddEntry(revoked, now.AddMinutes(-1));
        var crl = builder.Build(issuer, BigInteger.One, now.AddHours(1), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1, now.AddMinutes(-2));
        var stale = builder.Build(issuer, new BigInteger(2), now.AddMinutes(-1), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1, now.AddMinutes(-3));
        var validator = new PinnedCrlCertificateValidator(issuer, TimeProvider.System);
        Assert.Equal(CollectorCertificateStatus.CertValid, validator.Validate(valid, crl));
        Assert.Equal(CollectorCertificateStatus.CertRevoked, validator.Validate(revoked, crl));
        Assert.Equal(CollectorCertificateStatus.CertExpired, validator.Validate(expired, crl));
        Assert.Equal(CollectorCertificateStatus.CertInvalid, validator.Validate(wrongEku, crl));
        Assert.Equal(CollectorCertificateStatus.RevocationUnavailable, validator.Validate(valid, null));
        Assert.Equal(CollectorCertificateStatus.RevocationUnknown, validator.Validate(valid, stale));
        var corrupt = crl.ToArray(); corrupt[^1] ^= 1;
        Assert.Equal(CollectorCertificateStatus.RevocationUnknown, validator.Validate(valid, corrupt));
        using var foreignKey = RSA.Create(2048);
        var foreignRequest = new CertificateRequest("CN=A10-Unknown", foreignKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var foreign = foreignRequest.CreateSelfSigned(now.AddMinutes(-1), now.AddHours(1));
        Assert.Equal(CollectorCertificateStatus.CertInvalid, validator.Validate(foreign, crl));

        using var serverKey = RSA.Create(2048);
        var serverRequest = new CertificateRequest("CN=localhost", serverKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var names = new SubjectAlternativeNameBuilder(); names.AddIpAddress(IPAddress.Loopback); serverRequest.CertificateExtensions.Add(names.Build());
        using var original = serverRequest.CreateSelfSigned(now.AddMinutes(-1), now.AddHours(1));
        using var server = X509CertificateLoader.LoadPkcs12(original.Export(X509ContentType.Pkcs12), null, X509KeyStorageFlags.UserKeySet);
        ReadOnlyMemory<byte>? available = crl;
        var observed = new System.Collections.Concurrent.ConcurrentBag<CollectorCertificateStatus>();
        var appBuilder = WebApplication.CreateBuilder();
        appBuilder.Logging.ClearProviders();
        appBuilder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0, listener => listener.UseHttps(https =>
        {
            https.ServerCertificate = server;
            https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
            https.ClientCertificateValidation = (peer, _, _) =>
            {
                var result = validator.Validate(peer, available);
                observed.Add(result);
                return result == CollectorCertificateStatus.CertValid;
            };
        })));
        await using var app = appBuilder.Build();
        var accepted = 0;
        app.MapGet("/machine", () => { Interlocked.Increment(ref accepted); return "SYNTHETIC"; });
        await app.StartAsync();
        try
        {
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            async Task<HttpResponseMessage> Send(X509Certificate2 peer)
            {
                using var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = (_, certificate, _, _) => certificate?.RawData.AsSpan().SequenceEqual(server.RawData) == true };
                handler.ClientCertificates.Add(peer);
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
                return await client.GetAsync(address + "/machine");
            }
            using var response = await Send(valid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await Assert.ThrowsAsync<HttpRequestException>(() => Send(revoked));
            available = null;
            await Assert.ThrowsAsync<HttpRequestException>(() => Send(valid));
            Assert.Equal(1, accepted);
            Assert.Contains(CollectorCertificateStatus.CertRevoked, observed);
            Assert.Contains(CollectorCertificateStatus.RevocationUnavailable, observed);
            if (Environment.GetEnvironmentVariable("FDC_UI_EVIDENCE") is { } output)
                await File.WriteAllTextAsync(Path.Combine(output, "a10-crl-mtls.json"), JsonSerializer.Serialize(new
                { Utc = DateTimeOffset.UtcNow, Commit = Environment.GetEnvironmentVariable("FDC_A5_COMMIT"),
                    Valid = 200, Revoked = "TLS handshake rejected", Unavailable = "TLS handshake rejected",
                    RequestsAccepted = accepted, Statuses = observed.Select(value => value.ToString()).ToArray(),
                    Revocation = "Signed fresh full CRL checked locally; no CRL/OCSP HTTP responder", OCSP = "NOT_IMPLEMENTED" }));
        }
        finally { await app.StopAsync(); }
    }

    private static X509Certificate2 Issue(X509Certificate2 issuer, DateTimeOffset now, string eku)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=A10-Collector-" + Guid.NewGuid().ToString("N"), key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(eku) }, true));
        using var issued = request.Create(issuer, now.AddMinutes(-5), now.AddHours(1), RandomNumberGenerator.GetBytes(16));
        using var withKey = issued.CopyWithPrivateKey(key);
        return X509CertificateLoader.LoadPkcs12(withKey.Export(X509ContentType.Pkcs12), null, X509KeyStorageFlags.UserKeySet);
    }
}
