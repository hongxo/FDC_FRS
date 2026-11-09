using System.Collections.Immutable;
using Fdc.Identity.Application;

namespace Fdc.Identity.Application;

// Server-selected operations, never a client-supplied permission or a role label.
public enum IdentityOperation { RoleRead, RoleCreate, RoleEdit, RoleClone, RoleDeactivate, RolePermissionRequest, RolePermissionReview, RolePermissionApply, UserRoleAssign, UserRoleRevoke, StationGrantAssign, StationGrantRevoke, DelegationChange, SessionRevoke, SecurityAuditRead }
public enum OperationScope { Station, ManagementUnit, Corporation, AdministrativeDelegation, OwnSession }
public sealed record OperationRule(string PermissionCode, OperationScope Scope, string? DelegationFunction, bool RequiresStepUp, bool RequiresIndependentReview);
public sealed record OperationMappingRelease(string ReleaseId, long PolicyVersion, long CatalogVersion, ImmutableDictionary<IdentityOperation, OperationRule> Rules);
public interface IOperationMappingReleaseVerifier
{
    // Implement only after an immutable owner-approved release and its evidence are available.
    bool IsReviewed(OperationMappingRelease release);
}
public sealed class PendingOperationMappingReleaseVerifier : IOperationMappingReleaseVerifier
{
    public bool IsReviewed(OperationMappingRelease release) => false;
}
public sealed class OperationPermissionResolver(IOperationMappingReleaseVerifier verifier)
{
    public OperationRule Resolve(IdentityOperation operation, OperationMappingRelease release, long currentPolicy, long currentCatalog, IReadOnlySet<string> implementedCatalog)
    {
        if (!Enum.IsDefined(operation) || string.IsNullOrWhiteSpace(release.ReleaseId) || !verifier.IsReviewed(release))
            throw new OwnerDecisionRequiredException("D04: reviewed operation mapping release required");
        if (release.PolicyVersion != currentPolicy || release.CatalogVersion != currentCatalog || currentPolicy <= 0 || currentCatalog <= 0)
            throw new OwnerDecisionRequiredException("D04: stale policy/catalog release");
        if (!release.Rules.TryGetValue(operation, out var rule) || !Enum.IsDefined(rule.Scope) ||
            string.IsNullOrWhiteSpace(rule.PermissionCode) || rule.PermissionCode.Contains('*') ||
            rule.PermissionCode == "ied.configuration.write" || !implementedCatalog.Contains(rule.PermissionCode) ||
            (rule.Scope == OperationScope.AdministrativeDelegation && string.IsNullOrWhiteSpace(rule.DelegationFunction)))
            throw new OwnerDecisionRequiredException("D04: explicit implemented operation/scope/delegation mapping required");
        return rule; // Mapping is not an authorization allow; SQL authority/SoD/commit recheck still required.
    }
}
