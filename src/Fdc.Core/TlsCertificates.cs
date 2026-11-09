using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Fdc.Core;

public static class TlsCertificates
{
    // Schannel needs a temporary CNG key container on Windows; ephemeral keys work on Linux.
    public static X509Certificate2 LoadPrivateCertificate(string path, string password) =>
        X509CertificateLoader.LoadPkcs12FromFile(path, password, OperatingSystem.IsWindows() ? X509KeyStorageFlags.UserKeySet : X509KeyStorageFlags.EphemeralKeySet);
    public const string ClientAuth = "1.3.6.1.5.5.7.3.2";
    public const string ServerAuth = "1.3.6.1.5.5.7.3.1";
    public static string Fingerprint(X509Certificate2 certificate) => Convert.ToHexStringLower(SHA256.HashData(certificate.RawData));
    public static bool Matches(X509Certificate2 certificate, string expected) =>
        string.Equals(Fingerprint(certificate), expected.Replace(":", ""), StringComparison.OrdinalIgnoreCase);
    public static bool Validate(X509Certificate2 certificate, X509Certificate2 root, string purpose, bool checkRevocation,
        X509Chain? presentedChain = null)
    {
        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.Add(root);
        chain.ChainPolicy.ApplicationPolicy.Add(new Oid(purpose));
        chain.ChainPolicy.RevocationMode = checkRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
        if (presentedChain != null)
            foreach (var element in presentedChain.ChainElements) chain.ChainPolicy.ExtraStore.Add(element.Certificate);
        return chain.Build(certificate);
    }
    public static HttpClient CreateClient(CollectorOptions options)
    {
        var handler = new HttpClientHandler();
        if (options.Mtls.Enabled)
        {
            var client = LoadPrivateCertificate(options.Mtls.CertificatePath, options.Mtls.CertificatePassword);
            if (!client.HasPrivateKey) throw new InvalidOperationException("Chứng thư FDC thiếu private key.");
            var root = X509CertificateLoader.LoadCertificateFromFile(options.Mtls.TrustedCaPath);
            handler.ClientCertificates.Add(client);
            handler.ServerCertificateCustomValidationCallback = (_, certificate, chain, errors) =>
                certificate != null && (errors & (SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable)) == 0
                && Validate(certificate, root, ServerAuth, options.Mtls.CheckRevocation, chain);
        }
        var http = new HttpClient(handler) { BaseAddress = new Uri(options.CenterUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(60) };
        http.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
        return http;
    }
}
