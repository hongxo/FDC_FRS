using System.Collections.Immutable;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;

namespace Fdc.Identity.Tests;

public sealed class ManagementProposalTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-11-08T00:00:00Z");
    private static readonly Guid ScopeId = Guid.NewGuid();
    private static readonly RoleDefinition Role = new(Guid.NewGuid(), "synthetic-custom", "Anything", "", false, true, 1);
    private static readonly UserIdentity User = new(Guid.NewGuid(), "target-person", true, 1, 1);
    private static readonly RoleAssignment Assignment = new(Guid.NewGuid(), User.Id, Role.Id, true, Now.AddHours(-1), Now.AddHours(1));
    private static readonly ReviewedChangeCatalog Catalog = new(1, ImmutableHashSet.Create(StringComparer.Ordinal, "station.view", "ied.view"));
    private static CurrentGrantEnvelope Envelope => new(User, Assignment, Role, 1, 1, 1, [ScopeId], Catalog.SupportedCodes, Catalog.SupportedCodes, Now.AddHours(1));
    private static StationGrantProposal Grant => new(Assignment.Id, ScopeId, ["station.view"], Now, Now.AddMinutes(30), 1, 1, 1, "synthetic reason");
    private static RoleChangePayload Payload => new(RoleChangeKind.PermissionDiff, Role.Id, null, null, 1, 1, ["ied.view"], [], "synthetic reason");
    [Fact] public void Create_is_inactive_and_zero_permission_preview()
    {
        var preview = RoleProposalValidator.Validate(Payload with { Kind = RoleChangeKind.Create, RoleId = null, BaseVersion = 0, NewCode = "custom-new", Name = "Custom New", Add = [] }, null, new HashSet<string>(), new HashSet<string>(), Catalog);
        Assert.False(preview.Draft.Active); Assert.False(preview.Draft.IsSystemSeed); Assert.Empty(preview.ProposedPermissions);
    }
    [Fact] public void Duplicate_retired_role_code_rejected() => Assert.Throws<InvalidOperationException>(() =>
        RoleProposalValidator.Validate(Payload with { Kind = RoleChangeKind.Clone, NewCode = "retired", Name = "copy", Add = [] }, Role, Catalog.SupportedCodes, new HashSet<string> { "retired" }, Catalog));
    [Theory][InlineData("*")][InlineData("station.*")][InlineData("unapproved.action")][InlineData("ied.configuration.write")]
    public void Unsupported_wildcard_or_ot_permission_rejected(string code) => Assert.Throws<UnauthorizedAccessException>(() =>
        RoleProposalValidator.Validate(Payload with { Add = [code] }, Role, ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, Catalog));
    [Fact] public void Clone_has_new_id_and_no_assignment_or_grant_fields()
    {
        var preview = RoleProposalValidator.Validate(Payload with { Kind = RoleChangeKind.Clone, NewCode = "new-copy", Name = "copy", Add = [] }, Role, Catalog.SupportedCodes, ImmutableHashSet<string>.Empty, Catalog);
        Assert.NotEqual(Role.Id, preview.Draft.Id); Assert.False(preview.Draft.Active); Assert.False(preview.Draft.IsSystemSeed);
        Assert.Equal(Catalog.SupportedCodes.Count, preview.ProposedPermissions.Length);
        Assert.DoesNotContain(typeof(RoleProposalPreview).GetProperties(), p => p.Name is "UserRole" or "StationGrant" or "AdministrativeDelegation");
    }
    [Fact] public void Activate_preview_does_not_activate_effective_role()
    {
        var inactive = Role with { Active = false };
        Assert.False(RoleProposalValidator.Validate(Payload with { Kind = RoleChangeKind.Activate, Add = [] }, inactive, Catalog.SupportedCodes, ImmutableHashSet<string>.Empty, Catalog).Draft.Active);
    }
    [Fact] public void Stale_role_version_rejected() => Assert.Throws<InvalidOperationException>(() =>
        RoleProposalValidator.Validate(Payload with { BaseVersion = 2 }, Role, Catalog.SupportedCodes, ImmutableHashSet<string>.Empty, Catalog));
    [Fact] public void Permission_diff_requires_explicit_change_kind() => Assert.Throws<ArgumentException>(() =>
        RoleProposalValidator.Validate(Payload with { Kind = RoleChangeKind.Rename, Name = "new name" }, Role, Catalog.SupportedCodes, ImmutableHashSet<string>.Empty, Catalog));
    [Fact] public void Valid_grant_preview_has_function_diff_and_stable_digest()
    {
        var first = StationGrantProposalValidator.Validate(Grant, Envelope, "actor", ImmutableHashSet<string>.Empty, Now);
        var second = StationGrantProposalValidator.Validate(Grant with { Functions = ["station.view", "station.view"] }, Envelope, "actor", ImmutableHashSet<string>.Empty, Now);
        Assert.Equal(first.Digest, second.Digest); Assert.Equal(new[] { "station.view" }, first.Added.ToArray());
    }
    [Fact] public void Self_grant_by_same_person_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant, Envelope, "target-person"));
    [Fact] public void Cross_role_assignment_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant, Envelope with { Assignment = Assignment with { RoleId = Guid.NewGuid() } }));
    [Fact] public void Cross_subject_assignment_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant, Envelope with { Assignment = Assignment with { UserId = Guid.NewGuid() } }));
    [Fact] public void Cross_assignment_grant_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant with { UserRoleId = Guid.NewGuid() }, Envelope));
    [Fact] public void Unknown_scope_or_bulk_envelope_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant with { ScopeId = Guid.NewGuid() }, Envelope));
    [Fact] public void Function_not_in_same_role_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant, Envelope with { CurrentRolePermissions = ImmutableHashSet<string>.Empty }));
    [Fact] public void Inactive_role_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant, Envelope with { Role = Role with { Active = false } }));
    [Fact] public void Expired_assignment_rejected() => Assert.Throws<UnauthorizedAccessException>(() => Validate(Grant, Envelope with { Assignment = Assignment with { Until = Now } }));
    [Fact] public void Grant_expiry_beyond_assignment_rejected() => Assert.Throws<ArgumentException>(() => Validate(Grant with { Until = Now.AddHours(2) }, Envelope));
    [Fact] public void Grant_expiry_beyond_role_rejected() => Assert.Throws<ArgumentException>(() => Validate(Grant, Envelope with { Role = Role with { ValidUntil = Now.AddMinutes(10) } }));
    [Fact] public void Expired_grant_rejected() => Assert.Throws<ArgumentException>(() => Validate(Grant with { From = Now.AddMinutes(-2), Until = Now }, Envelope));
    [Fact] public void Grant_version_conflict_rejected() => Assert.Throws<InvalidOperationException>(() => Validate(Grant with { BaseVersion = 2 }, Envelope));
    [Fact] public void Hierarchy_change_rejected() => Assert.Throws<InvalidOperationException>(() => Validate(Grant with { HierarchyVersion = 2 }, Envelope));
    [Fact] public void Catalog_change_rejected() => Assert.Throws<InvalidOperationException>(() => Validate(Grant with { CatalogVersion = 2 }, Envelope));
    [Fact] public void Adjacent_lifetimes_do_not_overlap() => Assert.False(StationGrantProposalValidator.Overlaps(Now, Now.AddMinutes(1), Now.AddMinutes(1), Now.AddMinutes(2)));
    [Fact] public void Overlapping_lifetimes_detected() => Assert.True(StationGrantProposalValidator.Overlaps(Now, Now.AddMinutes(2), Now.AddMinutes(1), Now.AddMinutes(3)));
    private static GrantProposalPreview Validate(StationGrantProposal proposal, CurrentGrantEnvelope current, string actor = "actor") =>
        StationGrantProposalValidator.Validate(proposal, current, actor, ImmutableHashSet<string>.Empty, Now);
}
