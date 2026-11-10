using System.Security.Cryptography;
using System.Text;

namespace Fdc.Center;

internal static class HumanViewStamp
{
    internal static void Set(HttpContext context, string sessionDigest)
    {
        // A one-way display fingerprint cannot authenticate a request.
        context.Response.Headers["X-Human-Session-View"] = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(sessionDigest)));
        context.Response.Headers.Vary = "Cookie";
    }
}
