using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;

namespace Fdc.Center;

// Candidate policy for direct issuance and OCSP signed by the same pinned issuer.
// The endpoint is supplied by the caller, never discovered from peer-controlled AIA.
public sealed class PinnedOcspCertificateValidator : IDisposable
{
    private const int MaxResponseBytes = 64 * 1024;
    private readonly PinnedCrlCertificateValidator chain;
    private readonly Org.BouncyCastle.X509.X509Certificate issuer;
    private readonly TimeProvider clock;
    private readonly Uri endpoint;
    private readonly HttpClient client;

    public PinnedOcspCertificateValidator(X509Certificate2 issuer, TimeProvider clock, Uri endpoint, TimeSpan timeout)
    {
        if (!endpoint.IsAbsoluteUri || endpoint.UserInfo.Length != 0 || endpoint.Fragment.Length != 0 ||
            (endpoint.Scheme != "https" && !(endpoint.Scheme == "http" && IPAddress.TryParse(endpoint.Host, out var ip) && IPAddress.IsLoopback(ip))))
            throw new ArgumentException("An explicit HTTPS or literal loopback HTTP responder is required.", nameof(endpoint));
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromSeconds(10)) throw new ArgumentOutOfRangeException(nameof(timeout));
        chain = new(issuer, clock);
        this.issuer = new X509CertificateParser().ReadCertificate(issuer.RawData);
        this.clock = clock;
        this.endpoint = endpoint;
        client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false, UseCookies = false }) { Timeout = timeout };
    }

    public async Task<CollectorCertificateStatus> ValidateAsync(X509Certificate2 leaf, CancellationToken cancellationToken)
    {
        var status = chain.ValidateChain(leaf);
        if (status != CollectorCertificateStatus.CertValid) return status;
        var certificate = new X509CertificateParser().ReadCertificate(leaf.RawData);
        var id = new CertificateID(new Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier(Org.BouncyCastle.Asn1.Oiw.OiwObjectIdentifiers.IdSha1), issuer, certificate.SerialNumber);
        var request = new OcspReqGenerator();
        request.AddRequest(id);
        using var content = new ByteArrayContent(request.Generate().GetEncoded());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/ocsp-request");
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(client.Timeout);
        try
        {
            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            if (!response.IsSuccessStatusCode) return CollectorCertificateStatus.RevocationUnavailable;
            if (response.Content.Headers.ContentType?.MediaType != "application/ocsp-response" ||
                response.Content.Headers.ContentLength > MaxResponseBytes) return CollectorCertificateStatus.RevocationUnknown;
            await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token);
            var bytes = new byte[MaxResponseBytes + 1];
            var count = await stream.ReadAtLeastAsync(bytes, bytes.Length, false, deadline.Token);
            if (count == bytes.Length) return CollectorCertificateStatus.RevocationUnknown;
            return ValidateResponse(bytes[..count], id);
        }
        catch (Exception error) when (error is HttpRequestException or IOException ||
            error is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        { return CollectorCertificateStatus.RevocationUnavailable; }
    }

    private CollectorCertificateStatus ValidateResponse(byte[] bytes, CertificateID id)
    {
        try
        {
            var response = new OcspResp(bytes);
            var issuerKey = issuer.GetPublicKey();
            if (response.Status != OcspRespStatus.Successful ||
                response.GetResponseObject() is not BasicOcspResp basic ||
                basic.SignatureAlgorithm.Algorithm.Id is not ("1.2.840.113549.1.1.11" or "1.2.840.113549.1.1.12" or "1.2.840.113549.1.1.13") ||
                !(basic.ResponderId.Equals(new RespID(issuerKey)) || basic.ResponderId.Equals(new RespID(issuer.SubjectDN))) ||
                !basic.Verify(issuerKey) || basic.GetCriticalExtensionOids()?.Count > 0 ||
                basic.Responses is not [var single] || !single.GetCertID().Equals(id) ||
                single.GetCriticalExtensionOids()?.Count > 0)
                return CollectorCertificateStatus.RevocationUnknown;

            var now = clock.GetUtcNow().UtcDateTime;
            if (single.NextUpdate is not { } next || next <= now || next <= single.ThisUpdate ||
                single.ThisUpdate > basic.ProducedAt || basic.ProducedAt > now || now - single.ThisUpdate > TimeSpan.FromHours(1))
                return CollectorCertificateStatus.RevocationUnknown;
            return single.GetCertStatus() switch
            {
                RevokedStatus => CollectorCertificateStatus.CertRevoked,
                null => CollectorCertificateStatus.CertValid,
                _ => CollectorCertificateStatus.RevocationUnknown
            };
        }
        catch (Exception error) when (error is OcspException or IOException or ArgumentException or InvalidOperationException)
        { return CollectorCertificateStatus.RevocationUnknown; }
    }

    public void Dispose() => client.Dispose();
}
