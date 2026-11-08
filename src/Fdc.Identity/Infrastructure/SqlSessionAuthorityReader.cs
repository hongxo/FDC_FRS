using System.Data.Common;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

public sealed record CurrentHumanSession(Guid SessionId, UserIdentity User, HumanSession Session, AuthorityVersion Authority);
public interface IHumanSessionAuthorityReader
{
    Task<CurrentHumanSession?> ReadAsync(string digest, CancellationToken ct);
}

// Single SQL statement joins the current session, user, person and configured authority. No local authority cache.
// Not registered in Center; caller must supply primary-only least-privilege connection and approved D01/D05 wiring.
public sealed class SqlSessionAuthorityReader(IDbContextFactory<IamDbContext> factory) : IHumanSessionAuthorityReader
{
    public async Task<CurrentHumanSession?> ReadAsync(string digest, CancellationToken ct)
    {
        if (!IsDigest(digest)) return null;
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            await RequireWritablePrimaryAsync(db, ct);
            return await ReadCoreAsync(db, digest, ct);
        }
        catch (Exception error) when (SqlAuthorityErrors.IsUnavailable(error)) { throw new AuthorityUnavailableException(); }
    }
    internal static bool IsDigest(string digest) => digest.Length == 64 && digest.All(c => char.IsAsciiHexDigit(c) && !char.IsLower(c));
    internal static async Task RequireWritablePrimaryAsync(IamDbContext db, CancellationToken ct)
    {
        var options = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (options.ApplicationIntent != ApplicationIntent.ReadWrite) throw new AuthorityUnavailableException();
        var state = await db.Database.SqlQueryRaw<string>("SELECT CONVERT(nvarchar(60), DATABASEPROPERTYEX(DB_NAME(), 'Updateability')) AS [Value]").SingleAsync(ct);
        if (state != "READ_WRITE") throw new AuthorityUnavailableException();
    }
    internal static async Task<CurrentHumanSession?> ReadCoreAsync(IamDbContext db, string digest, CancellationToken ct)
    {
        var row = await (from s in db.Set<SessionRow>().AsNoTracking()
            join u in db.Set<UserRow>() on s.UserId equals u.Id
            join p in db.Set<PersonReferenceRow>() on u.PersonReferenceId equals p.Id
            from a in db.Set<AuthorityVersionRow>()
            where s.HandleDigest == digest && a.AuthorityKey == "Center" && a.PolicyConfigured
            select new { S = s, U = u, P = p.PersonKey, A = a }).SingleOrDefaultAsync(ct);
        if (row is null) return null;
        var authority = new AuthorityVersion(row.A.PolicyVersion, row.A.CatalogVersion, row.A.HierarchyVersion, row.A.RestoreEpoch);
        var session = new HumanSession(row.U.Id, row.S.Revoked, row.S.IdleUntil, row.S.AbsoluteUntil,
            row.S.SecurityVersion, row.S.GrantVersion,
            new(row.S.PolicyVersion, row.S.CatalogVersion, row.S.HierarchyVersion, row.S.RestoreEpoch), row.S.MfaAt);
        return new(row.S.Id, new(row.U.Id, row.P, row.U.Active, row.U.SecurityVersion, row.U.GrantVersion), session, authority);
    }
}
