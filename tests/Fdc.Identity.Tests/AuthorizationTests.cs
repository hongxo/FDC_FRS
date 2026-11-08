using System.Collections.Immutable;
using Fdc.Identity.Domain;
using Fdc.Identity.Security;
using Fdc.Identity.Application;

namespace Fdc.Identity.Tests;

public sealed class AuthorizationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 5, 0, 0, TimeSpan.Zero);
    private const string Action = "candidate.approve";
    private static readonly Guid User = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid Role = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid Assignment = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid Grant = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private readonly PermissionEvaluator evaluator = new();
    private static AuthorityVersion Version => new(1, 1, 1, Guid.Parse("50000000-0000-0000-0000-000000000001"));
    private static TrustedResource Resource => new("A1", "CORP", "UNIT-A", 1, []);
    private static WorkflowContext Workflow => new("revision-1", "creator", "reviewer", ImmutableHashSet<string>.Empty);
    private static AccessRequest Request => new(Action, CredentialKind.HumanSession, Resource, Workflow: Workflow);
    private static AuthorizationSnapshot State => new(
        new(User, "approver", true, 1, 1), new(User, false, Now.AddMinutes(15), Now.AddHours(8), 1, 1, Version, Now), Version,
        [new(Role, "arbitrary-custom-code", "Any display name", "", false, true, 1)],
        [new(Assignment, User, Role, true, Now.AddHours(-1), Now.AddHours(1))],
        [new(Action, true, true, 1, true, TimeSpan.FromMinutes(5), ApprovalIndependent: true)],
        [new(Role, Action)], [new(Grant, Assignment, ScopeKind.Station, "A1", true, Now.AddHours(-1), Now.AddHours(1), 1)],
        [new(Grant, Action)], []);

    [Fact] public void Custom_role_is_not_limited_to_seven_names() => Assert.True(evaluator.Evaluate(State, Request, Now).Allowed);
    [Fact] public void Role_names_do_not_confer_permissions() => Assert.False(evaluator.Evaluate(State with { RolePermissions = [] }, Request, Now).Allowed);
    [Fact] public void Role_permission_A_cannot_combine_with_assignment_B_scope()
    {
        var otherRole = Guid.NewGuid(); var otherAssignment = Guid.NewGuid();
        var state = State with { Roles = State.Roles.Add(new(otherRole, "second", "Second", "", false, true, 1)),
            Assignments = State.Assignments.Add(new(otherAssignment, User, otherRole, true, Now.AddDays(-1), Now.AddDays(1))),
            Grants = [State.Grants[0] with { UserRoleId = otherAssignment }] };
        Assert.False(evaluator.Evaluate(state, Request, Now).Allowed);
    }
    [Theory][InlineData("B1")][InlineData("")]
    public void Trusted_object_station_must_match_scope(string station) => Assert.False(evaluator.Evaluate(State, Request with { Resource = Resource with { StationId = station } }, Now).Allowed);
    [Fact] public void Role_deactivation_cuts_all_assignments() => Assert.False(evaluator.Evaluate(State with { Roles = [State.Roles[0] with { Active = false }] }, Request, Now).Allowed);
    [Fact] public void Role_expiry_is_exclusive_boundary() => Assert.False(evaluator.Evaluate(State with { Roles = [State.Roles[0] with { ValidUntil = Now }] }, Request, Now).Allowed);
    [Fact] public void Assignment_expiry_is_exclusive_boundary() => Assert.False(evaluator.Evaluate(State with { Assignments = [State.Assignments[0] with { Until = Now }] }, Request, Now).Allowed);
    [Fact] public void Grant_expiry_is_exclusive_boundary() => Assert.False(evaluator.Evaluate(State with { Grants = [State.Grants[0] with { Until = Now }] }, Request, Now).Allowed);
    [Fact] public void Future_grant_is_denied() => Assert.False(evaluator.Evaluate(State with { Grants = [State.Grants[0] with { From = Now.AddSeconds(1) }] }, Request, Now).Allowed);
    [Fact] public void Function_subset_is_required() => Assert.False(evaluator.Evaluate(State with { Functions = [] }, Request, Now).Allowed);
    [Fact] public void Stale_hierarchy_is_denied() => Assert.False(evaluator.Evaluate(State, Request with { Resource = Resource with { HierarchyVersion = 2 } }, Now).Allowed);
    [Fact] public void Creator_cannot_approve_with_custom_role() => Assert.Equal("SEPARATION_OF_DUTIES", evaluator.Evaluate(State, Request with { Workflow = Workflow with { CreatorPerson = "approver" } }, Now).Reason);
    [Fact] public void Reviewer_cannot_approve_same_revision() => Assert.False(evaluator.Evaluate(State, Request with { Workflow = Workflow with { ReviewerPerson = "approver" } }, Now).Allowed);
    [Fact] public void Beneficiary_cannot_approve_own_escalation() => Assert.False(evaluator.Evaluate(State, Request with { Workflow = Workflow with { BeneficiaryPersons = ImmutableHashSet.Create("approver") } }, Now).Allowed);
    [Fact] public void Independent_action_requires_workflow() => Assert.False(evaluator.Evaluate(State, Request with { Workflow = null }, Now).Allowed);
    [Fact] public void Unapproved_permission_is_denied() => Assert.False(evaluator.Evaluate(State with { Permissions = [State.Permissions[0] with { Approved = false }] }, Request, Now).Allowed);
    [Fact] public void Missing_backend_enforcement_is_denied() => Assert.False(evaluator.Evaluate(State with { Permissions = [State.Permissions[0] with { Implemented = false }] }, Request, Now).Allowed);
    [Fact] public void Unknown_permission_is_denied() => Assert.False(evaluator.Evaluate(State, Request with { PermissionCode = "user.manage" }, Now).Allowed);
    [Fact] public void Catalog_version_mismatch_is_denied() => Assert.False(evaluator.Evaluate(State with { Permissions = [State.Permissions[0] with { CatalogVersion = 2 }] }, Request, Now).Allowed);
    [Fact] public void Old_policy_snapshot_session_is_denied() => Assert.False(evaluator.Evaluate(State with { CurrentVersion = Version with { Policy = 2 } }, Request, Now).Allowed);
    [Fact] public void Restored_old_session_epoch_is_denied() => Assert.False(evaluator.Evaluate(State with { CurrentVersion = Version with { RestoreEpoch = Guid.NewGuid() } }, Request, Now).Allowed);
    [Fact] public void Revoked_session_is_denied() => Assert.False(evaluator.Evaluate(State with { Session = State.Session with { Revoked = true } }, Request, Now).Allowed);
    [Fact] public void Disabled_user_is_denied() => Assert.False(evaluator.Evaluate(State with { User = State.User with { Active = false } }, Request, Now).Allowed);
    [Fact] public void Grant_version_change_invalidates_stale_session() => Assert.False(evaluator.Evaluate(State with { User = State.User with { GrantVersion = 2 } }, Request, Now).Allowed);
    [Fact] public void Mfa_expiry_boundary_denied() => Assert.False(evaluator.Evaluate(State with { Session = State.Session with { MfaAt = Now.AddMinutes(-5) } }, Request, Now).Allowed);
    [Fact] public void Future_mfa_proof_is_denied() => Assert.False(evaluator.Evaluate(State with { Session = State.Session with { MfaAt = Now.AddSeconds(1) } }, Request, Now).Allowed);
    [Theory][InlineData(CredentialKind.CollectorCertificate)][InlineData(CredentialKind.LegacyApiKey)]
    public void Machine_or_legacy_credentials_cannot_authorize_human_action(CredentialKind kind) => Assert.False(evaluator.Evaluate(State, Request with { Credential = kind }, Now).Allowed);
    [Fact] public void Missing_authority_fails_closed() => Assert.Equal("AUTHORITY_UNAVAILABLE", evaluator.Evaluate(null, Request, Now).Reason);
    [Fact] public void Unconfigured_restore_epoch_fails_closed() => Assert.False(evaluator.Evaluate(State with { CurrentVersion = Version with { RestoreEpoch = Guid.Empty } }, Request, Now).Allowed);
    [Theory][InlineData(ScopeKind.Corporation, "CORP", true)][InlineData(ScopeKind.ManagementUnit, "UNIT-A", true)][InlineData(ScopeKind.ManagementUnit, "UNIT-B", false)]
    public void Corporation_and_unit_scopes_are_explicit(ScopeKind kind, string id, bool expected)
        => Assert.Equal(expected, evaluator.Evaluate(State with { Grants = [State.Grants[0] with { Scope = kind, ScopeId = id }] }, Request, Now).Allowed);
    [Fact] public void Ancestor_unit_requires_reviewed_hierarchy_membership()
    {
        var state = State with { Grants = [State.Grants[0] with { Scope = ScopeKind.ManagementUnit, ScopeId = "PARENT" }] };
        Assert.False(evaluator.Evaluate(state, Request, Now).Allowed);
        Assert.True(evaluator.Evaluate(state, Request with { Resource = Resource with { ReviewedAncestorUnits = ["PARENT"] } }, Now).Allowed);
    }
    [Fact] public void Station_grant_does_not_grant_global_administration() => Assert.False(evaluator.Evaluate(State, Request with { Resource = null, AdministrativeTarget = "population-A" }, Now).Allowed);
    [Fact] public void Administrative_delegation_is_assignment_and_target_bound()
    {
        const string admin = "role.permission.manage";
        var state = State with { Permissions = [State.Permissions[0] with { Code = admin, GlobalAdministrative = true }],
            RolePermissions = [new(Role, admin)], Delegations = [new(Assignment, "population-A", admin, true, Now.AddDays(-1), Now.AddDays(1))] };
        Assert.True(evaluator.Evaluate(state, Request with { PermissionCode = admin, Resource = null, AdministrativeTarget = "population-A" }, Now).Allowed);
        Assert.False(evaluator.Evaluate(state, Request with { PermissionCode = admin, Resource = null, AdministrativeTarget = "population-B" }, Now).Allowed);
    }
    [Fact] public void Administrative_delegation_cannot_change_a_station_permission_scope_kind()
    {
        var state = State with { Delegations = [new(Assignment, "population-A", Action, true, Now.AddDays(-1), Now.AddDays(1))] };
        Assert.False(evaluator.Evaluate(state, Request with { Resource = null, AdministrativeTarget = "population-A" }, Now).Allowed);
    }
    [Fact] public void Ied_write_is_forbidden_even_if_every_catalog_flag_is_true()
    {
        const string code = "ied.configuration.write";
        var state = State with { Permissions = [new(code, true, true, 1, false, null)], RolePermissions = [new(Role, code)], Functions = [new(Grant, code)] };
        Assert.Equal("OT_OPERATION_FORBIDDEN", evaluator.Evaluate(state, Request with { PermissionCode = code }, Now).Reason);
    }
    [Fact] public void New_role_has_no_active_authority_and_copy_carries_no_assignment()
    {
        var original = RoleDefinition.Create("custom", "Custom"); var copy = original.CopyAsDraft("copy", "Copy");
        Assert.False(original.Active); Assert.False(copy.Active); Assert.False(copy.IsSystemSeed); Assert.NotEqual(original.Id, copy.Id);
        Assert.Equal(original.Code, original.Rename("Renamed").Code);
    }
    [Fact] public async Task Pending_D09_never_invokes_mutation_callback()
    {
        var invoked = false;
        await Assert.ThrowsAsync<OwnerDecisionRequiredException>(() => new PendingD09CommitBoundary().CommitAsync(User, _ => { invoked = true; return Task.FromResult(1); }, default));
        Assert.False(invoked);
    }
}
