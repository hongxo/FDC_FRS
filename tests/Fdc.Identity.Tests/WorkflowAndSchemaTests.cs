using System.Collections.Immutable;
using Fdc.Identity.Application;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Tests;

public sealed class WorkflowAndSchemaTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 5, 0, 0, TimeSpan.Zero);
    private static RoleChangePayload Payload => new(RoleChangeKind.PermissionDiff, Guid.NewGuid(), null, null, 1, 1,
        ["candidate.review"], [], "Synthetic test request");
    [Fact] public void Digest_is_stable_for_order_and_duplicate_codes()
    {
        var payload = Payload with { Add = ["station.view", "ied.view"] };
        var a = new RoleChangeRequest("requester", payload, 1, Now.AddMinutes(1));
        var b = new RoleChangeRequest("requester", payload with { Add = ["ied.view", "station.view", "ied.view"] }, 1, Now.AddMinutes(1));
        Assert.Equal(a.Digest, b.Digest);
    }
    [Fact] public void Contradictory_diff_is_rejected() => Assert.Throws<ArgumentException>(() => new RoleChangeRequest("requester", Payload with { Remove = ["candidate.review"] }, 1, Now.AddMinutes(1)));
    [Fact] public void Request_digest_binds_actor_revision_and_expiry()
    {
        var payload = Payload;
        var original = new RoleChangeRequest("requester", payload, 1, Now.AddMinutes(1));
        Assert.NotEqual(original.Digest, new RoleChangeRequest("other-person", payload, 1, Now.AddMinutes(1)).Digest);
        Assert.NotEqual(original.Digest, new RoleChangeRequest("requester", payload, 2, Now.AddMinutes(1)).Digest);
        Assert.NotEqual(original.Digest, new RoleChangeRequest("requester", payload, 1, Now.AddMinutes(2)).Digest);
    }
    [Fact] public void Requester_and_beneficiaries_cannot_approve()
    {
        var request = new RoleChangeRequest("requester", Payload, 1, Now.AddMinutes(1));
        Assert.Throws<UnauthorizedAccessException>(() => request.RecordApproval("requester", 1, request.Digest, ImmutableHashSet<string>.Empty, true, Now));
        Assert.Throws<UnauthorizedAccessException>(() => request.RecordApproval("beneficiary", 1, request.Digest, ImmutableHashSet.Create("beneficiary"), true, Now));
    }
    [Fact] public void Stale_digest_and_duplicate_person_votes_are_rejected()
    {
        var request = new RoleChangeRequest("requester", Payload, 1, Now.AddMinutes(1));
        Assert.Throws<InvalidOperationException>(() => request.RecordApproval("reviewer", 1, "wrong", ImmutableHashSet<string>.Empty, true, Now));
        request.RecordApproval("reviewer", 1, request.Digest, ImmutableHashSet<string>.Empty, true, Now);
        Assert.Throws<InvalidOperationException>(() => request.RecordApproval("reviewer", 1, request.Digest, ImmutableHashSet<string>.Empty, true, Now));
    }
    [Fact] public void Approval_does_not_remain_usable_after_revoke_or_new_beneficiary()
    {
        var request = new RoleChangeRequest("requester", Payload, 1, Now.AddMinutes(1));
        request.RecordApproval("reviewer", 1, request.Digest, ImmutableHashSet<string>.Empty, true, Now);
        Assert.False(request.CanApply(1, request.Digest, 1, ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, Now));
        Assert.False(request.CanApply(1, request.Digest, 1, ImmutableHashSet.Create("reviewer"), ImmutableHashSet.Create("reviewer"), Now));
        Assert.False(request.CanApply(1, request.Digest, 1, ImmutableHashSet<string>.Empty, ImmutableHashSet.Create("reviewer"), request.ExpiresAt));
    }
    [Fact] public void Missing_current_approval_authority_is_denied()
    {
        var request = new RoleChangeRequest("requester", Payload, 1, Now.AddMinutes(1));
        Assert.Throws<UnauthorizedAccessException>(() => request.RecordApproval("reviewer", 1, request.Digest, ImmutableHashSet<string>.Empty, false, Now));
    }
    [Fact] public void Sql_model_generates_constraints_without_connecting_to_any_server()
    {
        using var db = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer("Server=unconfigured.invalid;Database=NeverConnect;Integrated Security=true;Encrypt=true").Options);
        var sql = db.Database.GenerateCreateScript();
        Assert.Contains("CREATE TABLE [iam].[StationGrant]", sql);
        Assert.Contains("FOREIGN KEY ([UserRoleId])", sql);
        Assert.Contains("CK_MfaCredential_Material", sql);
        Assert.Contains("[RowVersion] rowversion", sql);
        Assert.Contains("UNIQUE INDEX", sql);
        Assert.DoesNotContain("INSERT INTO", sql);
        Assert.Equal(24, db.Model.GetEntityTypes().Count());
    }
}
