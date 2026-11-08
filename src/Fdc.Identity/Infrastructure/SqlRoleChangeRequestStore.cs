using Fdc.Identity.Application;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

// A real current-authority D09 boundary must own the transaction and recheck before this callback.
// PendingD09CommitBoundary blocks before any connection/write. There is no in-memory runtime fallback.
public sealed class SqlRoleChangeRequestStore(IamDbContext db, IAuthorizedCommitBoundary boundary)
{
    public Task<Guid> PersistAsync(Guid actorUserId, RoleChangeRequestRow request, SecurityEventRow audit, CancellationToken ct)
        => boundary.CommitAsync(actorUserId, async token =>
        {
            if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("D09 must supply the active SQL transaction.");
            if (request.RequesterUserId != actorUserId || audit.ActorUserId != actorUserId) throw new UnauthorizedAccessException("Actor binding mismatch.");
            db.Set<RoleChangeRequestRow>().Add(request);
            db.Set<SecurityEventRow>().Add(audit);
            db.Set<AuditOutboxRow>().Add(new() { Id = Guid.NewGuid(), EventId = audit.Id, CreatedAt = audit.CreatedAt });
            // One SaveChanges inside the SAME boundary transaction; a mandatory audit/outbox error propagates.
            await db.SaveChangesAsync(token);
            return request.Id;
        }, ct);
}
