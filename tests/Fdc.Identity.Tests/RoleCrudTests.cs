using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

public sealed class RoleCrudTests
{
    [Fact] public async Task Pending_authority_denies_before_sql_or_commit()
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        var service = new SqlRoleCrudService(db, new PendingD09CommitBoundary(), new PendingRoleMutationAuthority(), TimeProvider.System);
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => service.SubmitAsync(new(RoleChangeKind.Create, null, "new-role", "New role", 0, 1, [], [], "synthetic"), "request-key", default));
        Assert.Empty(db.ChangeTracker.Entries());
    }
    [Theory][InlineData("")][InlineData("short")][InlineData("key with spaces")][InlineData("../unsafe")]
    public async Task Invalid_idempotency_key_rejected(string key)
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        var service = new SqlRoleCrudService(db, new PendingD09CommitBoundary(), new PendingRoleMutationAuthority(), TimeProvider.System);
        await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitAsync(new(RoleChangeKind.Create, null, "new-role", "New role", 0, 1, [], [], "synthetic"), key, default));
    }
}
