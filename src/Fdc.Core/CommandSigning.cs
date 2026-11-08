using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Fdc.Core;
public sealed record SignedCommand(CommandRow Command, string Signature);
public static class CommandSigning
{
    public static SignedCommand Sign(CommandRow command, string key) => new(command,
        Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), JsonSerializer.SerializeToUtf8Bytes(command))));
    public static bool Verify(SignedCommand envelope, string key)
    {
        try { return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(envelope.Signature), Convert.FromHexString(Sign(envelope.Command, key).Signature)); }
        catch (FormatException) { return false; }
    }
}
