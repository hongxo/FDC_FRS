using Fdc.Identity.Application;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

// Read within the caller's reviewed login transaction. The reader makes no lockout or issuance decision.
public sealed record SqlPasswordCredential(Guid UserId, bool Active, string EncodedHash, string AlgorithmVersion,
    long SecurityVersion, long CredentialVersion)
{
    public override string ToString() => "SqlPasswordCredential [redacted]";
}

public sealed class SqlPasswordCredentialReader(IamDbContext db)
{
    public async Task<SqlPasswordCredential?> ReadAsync(string normalizedUsername, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(normalizedUsername) || normalizedUsername.Length > 256 ||
            normalizedUsername.Any(char.IsControl))
            throw new ArgumentException("Canonical username required.", nameof(normalizedUsername));
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Credential read requires the caller's authority transaction.");

        try
        {
            await SqlSessionAuthorityReader.RequireWritablePrimaryAsync(db, ct);
            return await (from user in db.Set<UserRow>().AsNoTracking()
                          join credential in db.Set<PasswordCredentialRow>().AsNoTracking() on user.Id equals credential.UserId
                          where user.NormalizedUsername == normalizedUsername
                          select new SqlPasswordCredential(user.Id, user.Active, credential.EncodedHash,
                              credential.AlgorithmVersion, user.SecurityVersion, credential.Version)).SingleOrDefaultAsync(ct);
        }
        catch (Exception error) when (SqlAuthorityErrors.IsUnavailable(error)) { throw new AuthorityUnavailableException(); }
    }
}
