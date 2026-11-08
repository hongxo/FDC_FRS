using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

public sealed class SessionRevocationTests
{
    [Fact] public async Task Pending_revocation_policy_blocks_before_database()
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        var service = new SqlSessionRevocationService(db, new PendingD09CommitBoundary(), new PendingSessionRevocationPolicy(), TimeProvider.System);
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => service.RevokeAsync(new string('A', 64), SessionRevocationOperation.Current, default));
        Assert.Empty(db.ChangeTracker.Entries());
    }
    [Fact] public async Task Unknown_operation_is_not_mapped_to_logout_all()
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        var service = new SqlSessionRevocationService(db, new PendingD09CommitBoundary(), new PendingSessionRevocationPolicy(), TimeProvider.System);
        await Assert.ThrowsAsync<ArgumentException>(() => service.RevokeAsync(new string('A', 64), (SessionRevocationOperation)999, default));
    }
}
