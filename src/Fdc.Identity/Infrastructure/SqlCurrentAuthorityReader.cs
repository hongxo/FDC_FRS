using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Security;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

public interface IReviewedPermissionPolicy
{
    // Only a reviewed server mapping may supply MFA/SoD/scope semantics missing from the draft catalog.
    PermissionDescriptor? Resolve(PermissionRow permission, AuthorityVersion authority);
}
public sealed class PendingPermissionPolicy : IReviewedPermissionPolicy
{
    public PermissionDescriptor? Resolve(PermissionRow permission, AuthorityVersion authority) => null;
}
public interface ITrustedResourceResolver
{
    // Resolve actual object ownership on the same database/transaction, never infer from a client station ID.
    Task<TrustedResource?> ResolveAsync(IamDbContext db, string objectId, AuthorityVersion authority, CancellationToken ct);
}

// Coherent read-only snapshot used by the Lab host; never D09's mutation/revoke commit gate.
public sealed class SqlCurrentAuthorityReader(IDbContextFactory<IamDbContext> factory,
    IReviewedPermissionPolicy policy, ITrustedResourceResolver resources, TimeProvider clock) : ICurrentAuthorityReader
{
    public async Task<(AuthorizationSnapshot Snapshot, TrustedResource? Resource)?> ReadAsync(string sessionDigest, string? objectId, CancellationToken ct)
    {
        if (!SqlSessionAuthorityReader.IsDigest(sessionDigest)) return null;
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            await SqlSessionAuthorityReader.RequireWritablePrimaryAsync(db, ct);
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var result = await new SqlTransactionalAuthorityReader(policy, resources, clock)
                .ReadAsync(db, sessionDigest, objectId, ct);
            await tx.CommitAsync(ct);
            return result;
        }
        catch (Exception error) when (SqlAuthorityErrors.IsUnavailable(error)) { throw new AuthorityUnavailableException(); }
    }
}
