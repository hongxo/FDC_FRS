using System.Security.Cryptography;
using System.Text;
using Fdc.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Fdc.Identity.Security;

// No policy defaults: the host must supply a reviewed cost and length budget before registration.
public sealed class PasswordProofs
{
    private readonly PasswordHasher<object> hasher;
    private readonly int maximumLength;
    private readonly object subject = new();
    public PasswordProofs(int iterations, int maximumLength)
    {
        if (iterations < 1 || maximumLength < 1) throw new ArgumentOutOfRangeException(nameof(iterations));
        this.maximumLength = maximumLength;
        hasher = new(Options.Create(new PasswordHasherOptions
        { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3, IterationCount = iterations }));
    }
    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length > maximumLength) throw new ArgumentException("Invalid password length.");
        return hasher.HashPassword(subject, password);
    }
    public PasswordVerificationResult Verify(string encodedHash, string password)
    {
        if (string.IsNullOrEmpty(encodedHash) || encodedHash.Length > 2048 || string.IsNullOrEmpty(password) || password.Length > maximumLength)
            return PasswordVerificationResult.Failed;
        try { return hasher.VerifyHashedPassword(subject, encodedHash, password); }
        catch (Exception ex) when (ex is FormatException or ArgumentException or CryptographicException or OverflowException)
        { return PasswordVerificationResult.Failed; }
    }
}

public static class OpaqueProofs
{
    public static string Create() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    public static bool IsValid(string? token)
    {
        if (token is null || token.Length != 43 || token.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_')) return false;
        try { return WebEncoders.Base64UrlEncode(WebEncoders.Base64UrlDecode(token)) == token; }
        catch (FormatException) { return false; }
    }
    public static string Digest(string token, string purpose)
    {
        if (!IsValid(token) || string.IsNullOrWhiteSpace(purpose) || purpose.Length > 80 || purpose.Any(char.IsControl))
            throw new ArgumentException("Invalid opaque proof.");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("Fdc.Identity/v1\0" + purpose + "\0" + token)));
    }
}

public static class SessionValidity
{
    public static bool IsCurrent(UserIdentity user, HumanSession session, AuthorityVersion authority, DateTimeOffset now) =>
        user.Active && user.Id == session.UserId && !string.IsNullOrWhiteSpace(user.PersonReference) &&
        !session.Revoked && now < session.IdleUntil && now < session.AbsoluteUntil &&
        user.SecurityVersion > 0 && user.GrantVersion > 0 && authority.Policy > 0 && authority.Catalog > 0 &&
        authority.Hierarchy > 0 && authority.RestoreEpoch != Guid.Empty && session.Version == authority &&
        session.SecurityVersion == user.SecurityVersion && session.GrantVersion == user.GrantVersion;
}
