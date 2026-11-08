using System.Collections.Immutable;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

public sealed class SeedAndStoreTests
{
    [Fact] public async Task Pending_seed_manifest_does_not_install_defaults()
    {
        var manifest = new SeedManifest("V1", 1, null, [new(null, null, "Viewer", [])]);
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => SeedManifestValidator.ValidateAsync(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(manifest), new PendingSeedApprovalVerifier(), ImmutableHashSet<string>.Empty, default));
    }
    [Fact] public async Task Merely_claiming_approval_does_not_enable_seed()
    {
        var manifest = new SeedManifest("V1", 1, "unverified", [new(Guid.NewGuid(), "synthetic", "Synthetic", [])]);
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => SeedManifestValidator.ValidateAsync(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(manifest), new PendingSeedApprovalVerifier(), ImmutableHashSet<string>.Empty, default));
    }
    [Fact] public async Task Pending_D09_does_not_open_sql_or_add_audit_rows()
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true;Encrypt=true").Options);
        var actor = Guid.NewGuid(); var store = new SqlRoleChangeRequestStore(db, new PendingD09CommitBoundary());
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => store.PersistAsync(actor,
            new() { RequesterUserId = actor, PayloadJson = "{}", Digest = new('0', 64), IdempotencyKey = "synthetic" },
            new() { ActorUserId = actor, EventCode = "role.request", OutcomeCode = "pending", CorrelationId = "synthetic" }, default));
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Null(db.Database.CurrentTransaction);
    }
}
