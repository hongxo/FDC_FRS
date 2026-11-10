using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fdc.Center.Tests;

// A real loopback HTTPS reverse proxy. Public responses can be cached by URL.
// Private/no-store responses always go to the authority service with the caller's cookie.
internal sealed class A10ReadProxy : IAsyncDisposable
{
    private WebApplication app = null!;
    private HttpClient upstream = null!;
    private readonly ConcurrentDictionary<string, byte[]> cache = new();
    public Uri Origin { get; private set; } = null!;
    public int CacheEntries => cache.Count;
    private int forwarded;
    public int Forwarded => forwarded;
    public static async Task<A10ReadProxy> StartAsync(Uri target, X509Certificate2 certificate)
    {
        if (target.Scheme != "https" || target.Host != "127.0.0.1") throw new InvalidOperationException("Loopback target required.");
        var result = new A10ReadProxy();
        var pin = certificate.GetCertHashString(HashAlgorithmName.SHA256);
        result.upstream = new HttpClient(new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (_, peer, _, _) => peer?.GetCertHashString(HashAlgorithmName.SHA256) == pin });
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0, listener => listener.UseHttps(certificate)));
        result.app = builder.Build();
        result.app.MapGet("/{**path}", async (HttpContext h) =>
        {
            var key = h.Request.Path + h.Request.QueryString;
            if (result.cache.TryGetValue(key, out var stored))
            {
                await h.Response.Body.WriteAsync(stored, h.RequestAborted);
                return;
            }
            var address = target.GetLeftPart(UriPartial.Authority) + h.Request.Path + h.Request.QueryString;
            using var request = new HttpRequestMessage(HttpMethod.Get, address);
            if (h.Request.Headers.Cookie.Count > 0) request.Headers.TryAddWithoutValidation("Cookie", h.Request.Headers.Cookie.ToString());
            Interlocked.Increment(ref result.forwarded);
            using var response = await result.upstream.SendAsync(request, h.RequestAborted);
            var bytes = await response.Content.ReadAsByteArrayAsync(h.RequestAborted);
            h.Response.StatusCode = (int)response.StatusCode;
            h.Response.ContentType = response.Content.Headers.ContentType?.ToString();
            foreach (var name in new[] { "Cache-Control", "Vary", "X-Correlation-Id", "X-Human-Session-View" })
                if (response.Headers.TryGetValues(name, out var values)) h.Response.Headers[name] = string.Join(", ", values);
            var policy = response.Headers.CacheControl;
            if (policy?.Public == true && !policy.NoStore && !policy.Private && policy.MaxAge > TimeSpan.Zero)
                result.cache.TryAdd(key, bytes);
            await h.Response.Body.WriteAsync(bytes, h.RequestAborted);
        });
        await result.app.StartAsync();
        result.Origin = new(result.app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
        return result;
    }
    public async ValueTask DisposeAsync()
    {
        if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
        upstream?.Dispose();
    }
}
