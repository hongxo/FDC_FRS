using System.Collections.Immutable;
using Fdc.Identity.Application;
namespace Fdc.Identity.Tests;
public sealed class OperationMappingTests
{
    private static readonly ImmutableHashSet<string> Catalog = ["role.read", "role.permission.manage"];
    private static OperationMappingRelease Release(IdentityOperation operation = IdentityOperation.RoleRead, string code = "role.read", OperationScope scope = OperationScope.Station) =>
        new("lab-fixture", 1, 1, ImmutableDictionary<IdentityOperation, OperationRule>.Empty.Add(operation, new(code, scope, null, true, true)));
    [Fact] public void Pending_owner_verifier_denies_even_complete_configuration() => Assert.Throws<OwnerDecisionRequiredException>(() => new OperationPermissionResolver(new PendingOperationMappingReleaseVerifier()).Resolve(IdentityOperation.RoleRead, Release(), 1, 1, Catalog));
    [Theory] [InlineData("*")] [InlineData("unknown.permission")] [InlineData("ied.configuration.write")]
    public void Unimplemented_wildcard_and_ot_deny_cannot_map(string code) => Assert.Throws<OwnerDecisionRequiredException>(() => new OperationPermissionResolver(new FixtureReview()).Resolve(IdentityOperation.RoleRead, Release(code: code), 1, 1, Catalog));
    [Fact] public void Manage_permission_never_implies_another_operation() => Assert.Throws<OwnerDecisionRequiredException>(() => new OperationPermissionResolver(new FixtureReview()).Resolve(IdentityOperation.RolePermissionApply, Release(IdentityOperation.RolePermissionRequest, "role.permission.manage"), 1, 1, Catalog));
    [Fact] public void Stale_policy_denied() => Assert.Throws<OwnerDecisionRequiredException>(() => new OperationPermissionResolver(new FixtureReview()).Resolve(IdentityOperation.RoleRead, Release(), 2, 1, Catalog));
    [Fact] public void Delegation_requires_explicit_function() => Assert.Throws<OwnerDecisionRequiredException>(() => new OperationPermissionResolver(new FixtureReview()).Resolve(IdentityOperation.RoleRead, Release(scope: OperationScope.AdministrativeDelegation), 1, 1, Catalog));
    [Fact] public void Exact_mapping_is_metadata_not_an_authorization_decision() => Assert.Equal("role.read", new OperationPermissionResolver(new FixtureReview()).Resolve(IdentityOperation.RoleRead, Release(), 1, 1, Catalog).PermissionCode);
    private sealed class FixtureReview : IOperationMappingReleaseVerifier { public bool IsReviewed(OperationMappingRelease release) => true; }
}
