using Fdc.Identity.Application;
using Fdc.Identity.Security;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

// Own-user revocation only, no arbitrary target UserId and no host registration. Default policy refuses execution.
public sealed class SqlSessionRevocationService(IamDbContext db, IAuthorizedCommitBoundary boundary,
    ISessionRevocationPolicy policy, TimeProvider clock)
{
    public async Task<SessionRevocationResult> RevokeAsync(string sessionDigest, SessionRevocationOperation operation, CancellationToken ct)
    {
        if (!Enum.IsDefined(operation)) throw new ArgumentException("Unknown revocation operation.");
        await policy.DemandConfiguredAsync(operation, ct);
        if (!SqlSessionAuthorityReader.IsDigest(sessionDigest)) return new(false, 0, 0);
        try
        {
            await SqlSessionAuthorityReader.RequireWritablePrimaryAsync(db, ct);
            var initial = await SqlSessionAuthorityReader.ReadCoreAsync(db, sessionDigest, ct);
            if (initial is null || !SessionValidity.IsCurrent(initial.User, initial.Session, initial.Authority, clock.GetUtcNow())) return new(false, 0, 0);
            return await boundary.CommitAsync(initial.User.Id, async token =>
            {
                if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Revocation requires the same DbContext authority transaction.");
                var current = await SqlSessionAuthorityReader.ReadCoreAsync(db, sessionDigest, token);
                if (current is null || current.User.Id != initial.User.Id || !SessionValidity.IsCurrent(current.User, current.Session, current.Authority, clock.GetUtcNow()))
                    return new SessionRevocationResult(false, 0, 0);
                var correlation = await policy.DemandAsync(operation, current, token);
                if (string.IsNullOrWhiteSpace(correlation) || correlation.Length > 128 || correlation.Any(char.IsControl)) throw new InvalidOperationException("Server correlation ID required.");
                var user = await db.Set<UserRow>().SingleAsync(x => x.Id == current.User.Id, token);
                await db.Entry(user).ReloadAsync(token);
                if (!user.Active || user.SecurityVersion != current.User.SecurityVersion || user.GrantVersion != current.User.GrantVersion)
                    throw new UnauthorizedAccessException("Authority changed before revocation.");
                var sessions = await db.Set<SessionRow>().Where(x => x.UserId == user.Id && !x.Revoked &&
                    (operation == SessionRevocationOperation.Everywhere || x.Id == current.SessionId)).ToListAsync(token);
                foreach (var session in sessions) { await db.Entry(session).ReloadAsync(token); session.Revoked = true; session.Version = checked(session.Version + 1); }
                var now = clock.GetUtcNow();
                if (operation == SessionRevocationOperation.Everywhere)
                {
                    user.SecurityVersion = checked(user.SecurityVersion + 1); user.Version = checked(user.Version + 1);
                    var challenges = await db.Set<ChallengeRow>().Where(x => x.UserId == user.Id && x.ConsumedAt == null).ToListAsync(token);
                    foreach (var challenge in challenges) { await db.Entry(challenge).ReloadAsync(token); challenge.ConsumedAt = now; challenge.Version = checked(challenge.Version + 1); }
                }
                var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = user.Id,
                    EventCode = operation == SessionRevocationOperation.Everywhere ? "iam.session.revoke.all" : "iam.session.revoke.current",
                    OutcomeCode = "revoked", CorrelationId = correlation, PolicyVersion = current.Authority.Policy, RestoreEpoch = current.Authority.RestoreEpoch };
                db.Add(audit); db.Add(new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
                // Revocation, security version/challenge invalidation and required audit/outbox share one SaveChanges.
                await db.SaveChangesAsync(token);
                return new SessionRevocationResult(true, sessions.Count, user.SecurityVersion);
            }, ct);
        }
        catch (Exception error) when (SqlAuthorityErrors.IsUnavailable(error)) { throw new AuthorityUnavailableException(); }
    }
}
