using System.Collections.Immutable;
using System.Data;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

public sealed class CommitGatePlanTests
{
    [Fact] public void Duplicate_user_locks_merge_to_strongest_mode_and_sort_canonically()
    {
        var first = Guid.Parse("00000000-0000-0000-0000-000000000001"); var last = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var plan = new SqlCommitGatePlan("Test", false, [new(last, false), new(first, false), new(last, true)], 1000, IsolationLevel.ReadCommitted).Normalize(first);
        Assert.Equal(new[] { first, last }, plan.Users.Select(x => x.UserId)); Assert.True(plan.Users[1].Exclusive);
    }
    [Theory][InlineData(0)][InlineData(-1)][InlineData(30001)]
    public void Unbounded_or_invalid_lock_timeout_is_rejected(int timeout)
    {
        var actor = Guid.NewGuid(); Assert.Throws<ArgumentException>(() => new SqlCommitGatePlan("Test", false, [new(actor, false)], timeout, IsolationLevel.ReadCommitted).Normalize(actor));
    }
    [Fact] public void Actor_must_be_in_lock_set()
    { Assert.Throws<ArgumentException>(() => new SqlCommitGatePlan("Test", false, [new(Guid.NewGuid(), false)], 1000, IsolationLevel.ReadCommitted).Normalize(Guid.NewGuid())); }
    [Fact] public void Unreviewed_isolation_and_namespace_rejected()
    {
        var actor = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new SqlCommitGatePlan("bad/name", false, [new(actor, false)], 1000, IsolationLevel.ReadCommitted).Normalize(actor));
        Assert.Throws<ArgumentException>(() => new SqlCommitGatePlan("Test", false, [new(actor, false)], 1000, IsolationLevel.ReadUncommitted).Normalize(actor));
    }
    [Fact] public async Task Pending_plan_denies_before_connection_or_callback()
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        var called = false;
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new SqlAuthorityCommitBoundary(db, new PendingSqlCommitGatePlanProvider()).CommitAsync(Guid.NewGuid(), _ => { called = true; return Task.FromResult(1); }, default));
        Assert.False(called); Assert.Null(db.Database.CurrentTransaction);
    }
    [Fact] public async Task Preexisting_uncommitted_mutation_is_not_flushed_under_a_new_actor()
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        db.Add(new RoleRow { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, RoleCode = "uncommitted", RoleName = "Must not flush" });
        var actor = Guid.NewGuid(); var called = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SqlAuthorityCommitBoundary(db, new FixturePlan(actor)).CommitAsync(actor, _ => { called = true; return Task.FromResult(1); }, default));
        Assert.False(called); Assert.Equal(EntityState.Added, Assert.Single(db.ChangeTracker.Entries()).State);
    }
    private sealed class FixturePlan(Guid actor) : ISqlCommitGatePlanProvider
    {
        public Task<SqlCommitGatePlan> ResolveAsync(Guid actorId, CancellationToken ct) => Task.FromResult(new SqlCommitGatePlan("Fixture", false, [new(actor, false)], 1000, IsolationLevel.ReadCommitted));
    }
}
