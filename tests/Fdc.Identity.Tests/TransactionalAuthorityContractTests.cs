using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

public sealed class TransactionalAuthorityContractTests
{
    [Fact] public async Task Borrowed_reader_requires_existing_transaction_before_any_sql()
    {
        await using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
            .UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true").Options);
        var reader = new SqlTransactionalAuthorityReader(new PendingPermissionPolicy(), new NoResources(), TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync(db, new string('A', 64), null, default));
        Assert.Null(db.Database.CurrentTransaction);
    }
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("lowercase")]
    public async Task Malformed_digest_never_opens_factory_or_throws_null_reference(string? digest)
    {
        var factory = new NeverFactory();
        Assert.Null(await new SqlSessionAuthorityReader(factory).ReadAsync(digest!, default));
        Assert.Null(await new SqlCurrentAuthorityReader(factory, new PendingPermissionPolicy(), new NoResources(), TimeProvider.System).ReadAsync(digest!, null, default));
    }
    private sealed class NeverFactory : IDbContextFactory<IamDbContext>
    {
        public IamDbContext CreateDbContext() => throw new InvalidOperationException("Must reject before opening SQL.");
    }
    private sealed class NoResources : ITrustedResourceResolver
    {
        public Task<TrustedResource?> ResolveAsync(IamDbContext db, string objectId, AuthorityVersion authority, CancellationToken ct) => throw new InvalidOperationException();
    }
}
