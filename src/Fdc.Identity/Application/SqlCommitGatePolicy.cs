using System.Collections.Immutable;
using System.Data;

namespace Fdc.Identity.Application;

public sealed record UserAuthorityLock(Guid UserId, bool Exclusive);
public sealed record SqlCommitGatePlan(string ResourcePrefix, bool ExclusivePolicy, ImmutableArray<UserAuthorityLock> Users,
    int LockTimeoutMilliseconds, IsolationLevel Isolation)
{
    public SqlCommitGatePlan Normalize(Guid actorId)
    {
        if (actorId == Guid.Empty || string.IsNullOrWhiteSpace(ResourcePrefix) || ResourcePrefix.Length > 80 ||
            ResourcePrefix.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '.' and not '-' and not '_') ||
            LockTimeoutMilliseconds is < 1 or > 30000 || Isolation is not IsolationLevel.ReadCommitted and not IsolationLevel.Serializable ||
            Users.IsDefaultOrEmpty || Users.Any(x => x.UserId == Guid.Empty) || !Users.Any(x => x.UserId == actorId))
            throw new ArgumentException("Incomplete authority gate plan.");
        // Strongest mode from the start; deterministic order; no mid-transaction lock upgrades.
        return this with { Users = Users.GroupBy(x => x.UserId).Select(g => new UserAuthorityLock(g.Key, g.Any(x => x.Exclusive)))
            .OrderBy(x => x.UserId.ToString("N"), StringComparer.Ordinal).ToImmutableArray() };
    }
}
public interface ISqlCommitGatePlanProvider
{
    // Trusted server operation policy, not request JSON. Roles/hierarchy/catalog require Exclusive policy gate.
    // Must provide all actors/targets and the reviewed DB-scoped resource namespace consistently across every writer.
    Task<SqlCommitGatePlan> ResolveAsync(Guid actorId, CancellationToken ct);
}
public sealed class PendingSqlCommitGatePlanProvider : ISqlCommitGatePlanProvider
{
    public Task<SqlCommitGatePlan> ResolveAsync(Guid actorId, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D09: SQL gate plan, lock namespace/order, writers and isolation disposition");
}
public sealed class CommitOutcomeUnknownException : Exception
{
    public CommitOutcomeUnknownException() : base("Commit outcome unknown; query durable idempotency result before retry.") { }
}
