using System.Collections.Immutable;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Security;
using Fdc.Identity.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Fdc.Identity.Tests;

public sealed class ServiceBoundaryTests
{
    [Fact] public async Task Pending_role_policy_never_reaches_sql_queries()
    {
        var queries = new UnexpectedQueries();
        var service = new RoleManagementService(new PendingD04RoleAuthority(), queries);
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => service.ListAsync(null, 50, default));
        Assert.False(queries.Called);
    }
    [Theory][InlineData(0)][InlineData(101)][InlineData(-1)]
    public async Task Unbounded_role_queries_are_rejected(int limit)
    {
        var service = new RoleManagementService(new PendingD04RoleAuthority(), new UnexpectedQueries());
        await Assert.ThrowsAsync<ArgumentException>(() => service.ListAsync(null, limit, default));
    }
    [Fact] public async Task Every_request_reads_authority_again_and_outage_never_allows()
    {
        var reader = new UnavailableReader(); var service = new AuthorizationService(reader, new PermissionEvaluator(), TimeProvider.System);
        for (var i = 0; i < 2; i++) Assert.False((await service.DemandAsync("synthetic-digest", "station.view", CredentialKind.HumanSession, "record-1", null, null, default)).Allowed);
        Assert.Equal(2, reader.Calls);
    }
    [Fact] public async Task Machine_credentials_are_rejected_before_authority_lookup()
    {
        var reader = new UnavailableReader(); var service = new AuthorizationService(reader, new PermissionEvaluator(), TimeProvider.System);
        Assert.False((await service.DemandAsync("synthetic", "station.view", CredentialKind.CollectorCertificate, "record", null, null, default)).Allowed);
        Assert.Equal(0, reader.Calls);
    }
    [Fact] public void Migration_batches_require_exact_disposable_target_without_opening_sql()
    {
        var migration = new InitialIamFoundation();
        Assert.NotEmpty(migration.UpOperations);
        foreach (var operation in migration.UpOperations.Cast<SqlOperation>())
        {
            Assert.Contains("Fdc.ConfirmedDisposableLab", operation.Sql);
            Assert.Contains("Fdc.ExpectedLabDatabase", operation.Sql);
            Assert.Contains("COALESCE", operation.Sql);
            Assert.Contains("EXEC(N'", operation.Sql);
        }
        var down = Assert.Single(migration.DownOperations.Cast<SqlOperation>());
        Assert.Contains("Nonempty IAM foundation", down.Sql);
        Assert.True(down.Sql.IndexOf("Nonempty IAM foundation", StringComparison.Ordinal) < down.Sql.IndexOf("DROP TABLE", StringComparison.Ordinal));
    }
    private sealed class UnavailableReader : ICurrentAuthorityReader
    {
        public int Calls;
        public Task<(AuthorizationSnapshot Snapshot, TrustedResource? Resource)?> ReadAsync(string sessionDigest, string? objectId, CancellationToken ct)
        { Calls++; throw new AuthorityUnavailableException(); }
    }
    private sealed class UnexpectedQueries : IRoleQueries
    {
        public bool Called;
        public Task<RolePage> ListAsync(RoleReadScope scope, string? afterCode, int limit, CancellationToken ct) { Called = true; throw new InvalidOperationException("Must not reach persistence."); }
        public Task<RoleView?> DetailAsync(RoleReadScope scope, Guid roleId, CancellationToken ct) => throw new InvalidOperationException();
        public Task<IReadOnlyList<string>> PermissionsAsync(RoleReadScope scope, Guid roleId, CancellationToken ct) => throw new InvalidOperationException();
    }
}
