using Fdc.Identity.Domain;

namespace Fdc.Identity.Security;

// Operates only on an authoritative snapshot obtained by the server. Never accept a snapshot from an HTTP body.
public sealed class PermissionEvaluator
{
    public AccessDecision Evaluate(AuthorizationSnapshot? state, AccessRequest request, DateTimeOffset now)
    {
        if (request.Credential != CredentialKind.HumanSession) return AccessDecision.Deny("HUMAN_SCHEME_REQUIRED");
        if (state is null) return AccessDecision.Deny("AUTHORITY_UNAVAILABLE");
        if (state.CurrentVersion.Policy < 1 || state.CurrentVersion.Catalog < 1 || state.CurrentVersion.Hierarchy < 1 || state.CurrentVersion.RestoreEpoch == Guid.Empty)
            return AccessDecision.Deny("AUTHORITY_UNCONFIGURED");
        if (request.PermissionCode == "ied.configuration.write") return AccessDecision.Deny("OT_OPERATION_FORBIDDEN");
        var (user, session) = (state.User, state.Session);
        if (!user.Active || string.IsNullOrWhiteSpace(user.PersonReference) || user.Id != session.UserId) return AccessDecision.Deny("USER_INACTIVE");
        if (session.Revoked || now >= session.IdleUntil || now >= session.AbsoluteUntil) return AccessDecision.Deny("SESSION_EXPIRED_OR_REVOKED");
        if (session.Version != state.CurrentVersion || user.SecurityVersion != session.SecurityVersion || user.GrantVersion != session.GrantVersion)
            return AccessDecision.Deny("AUTHORITY_VERSION_CHANGED");
        var descriptors = state.Permissions.Where(p => p.Code == request.PermissionCode).ToArray();
        if (descriptors.Length != 1 || !descriptors[0].Approved || !descriptors[0].Implemented || descriptors[0].CatalogVersion != state.CurrentVersion.Catalog)
            return AccessDecision.Deny("PERMISSION_UNSUPPORTED");
        var permission = descriptors[0];
        if (permission.GlobalAdministrative != (request.AdministrativeTarget is not null)
            || permission.GlobalAdministrative && request.Resource is not null)
            return AccessDecision.Deny("PERMISSION_SCOPE_KIND_MISMATCH");
        if (permission.RequiresMfa && (permission.MaximumMfaAge is not { } age || age <= TimeSpan.Zero || session.MfaAt is not { } mfa || mfa > now || now - mfa >= age))
            return AccessDecision.Deny("STEP_UP_REQUIRED");
        if (permission.ReviewIndependent || permission.ApprovalIndependent)
        {
            var workflow = request.Workflow;
            if (workflow is null || string.IsNullOrWhiteSpace(workflow.Revision) || string.IsNullOrWhiteSpace(workflow.CreatorPerson))
                return AccessDecision.Deny("WORKFLOW_REQUIRED");
            if (workflow.CreatorPerson == user.PersonReference || workflow.BeneficiaryPersons.Contains(user.PersonReference))
                return AccessDecision.Deny("SEPARATION_OF_DUTIES");
            if (permission.ApprovalIndependent && (string.IsNullOrWhiteSpace(workflow.ReviewerPerson) || workflow.ReviewerPerson == user.PersonReference || workflow.CreatorPerson == workflow.ReviewerPerson))
                return AccessDecision.Deny("SEPARATION_OF_DUTIES");
        }
        foreach (var assignment in state.Assignments.Where(a => a.UserId == user.Id && a.Active && a.From <= now && now < a.Until))
        {
            var role = state.Roles.SingleOrDefault(r => r.Id == assignment.RoleId);
            if (role is null || !role.Active || role.Version < 1 || role.ValidUntil <= now || !state.RolePermissions.Any(p => p.RoleId == role.Id && p.PermissionCode == permission.Code)) continue;
            if (request.AdministrativeTarget is { } target)
            {
                if (request.Resource is not null) return AccessDecision.Deny("AMBIGUOUS_SCOPE");
                if (state.Delegations.Any(d => d.UserRoleId == assignment.Id && d.TargetPopulation == target && d.PermissionCode == permission.Code && d.Active && d.From <= now && now < d.Until)) return AccessDecision.Permit();
                continue;
            }
            if (request.Resource is not { } resource || string.IsNullOrWhiteSpace(resource.StationId)
                || string.IsNullOrWhiteSpace(resource.CorporationId) || string.IsNullOrWhiteSpace(resource.ManagementUnitId)
                || resource.HierarchyVersion != state.CurrentVersion.Hierarchy) continue;
            foreach (var grant in state.Grants.Where(g => g.UserRoleId == assignment.Id && g.Active && g.From <= now && now < g.Until && g.HierarchyVersion == resource.HierarchyVersion))
            {
                if (!state.Functions.Any(f => f.GrantId == grant.Id && f.PermissionCode == permission.Code)) continue;
                var matches = grant.Scope switch
                {
                    ScopeKind.Station => grant.ScopeId == resource.StationId,
                    ScopeKind.ManagementUnit => grant.ScopeId == resource.ManagementUnitId || resource.ReviewedAncestorUnits.Contains(grant.ScopeId),
                    ScopeKind.Corporation => grant.ScopeId == resource.CorporationId,
                    _ => false
                };
                if (matches) return AccessDecision.Permit();
            }
        }
        return AccessDecision.Deny("SCOPE_OR_PERMISSION_DENIED");
    }
}
