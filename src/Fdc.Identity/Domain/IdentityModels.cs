using System.Collections.Immutable;

namespace Fdc.Identity.Domain;

public enum ScopeKind { Corporation, ManagementUnit, Station, GlobalAdministration }
public enum CredentialKind { HumanSession, CollectorCertificate, LegacyApiKey }
public sealed record AuthorityVersion(long Policy, long Catalog, long Hierarchy, Guid RestoreEpoch);
public sealed record UserIdentity(Guid Id, string PersonReference, bool Active, long SecurityVersion, long GrantVersion);
public sealed record HumanSession(Guid UserId, bool Revoked, DateTimeOffset IdleUntil, DateTimeOffset AbsoluteUntil,
    long SecurityVersion, long GrantVersion, AuthorityVersion Version, DateTimeOffset? MfaAt);
public sealed record RoleDefinition(Guid Id, string Code, string Name, string Description, bool IsSystemSeed,
    bool Active, long Version, DateTimeOffset? ValidUntil = null)
{
    public static RoleDefinition Create(string code, string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 80 || !code.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.'))
            throw new ArgumentException("Invalid immutable role code.");
        ValidateName(name);
        if (description.Length > 2000 || description.Any(char.IsControl)) throw new ArgumentException("Invalid description.");
        return new(Guid.NewGuid(), code, name, description, false, false, 1);
    }
    public RoleDefinition Rename(string name) { ValidateName(name); return this with { Name = name, Version = checked(Version + 1) }; }
    public RoleDefinition CopyAsDraft(string code, string name) => Create(code, name, Description);
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 160 || name.Any(char.IsControl)) throw new ArgumentException("Invalid role name.");
    }
}
public sealed record RoleAssignment(Guid Id, Guid UserId, Guid RoleId, bool Active, DateTimeOffset From, DateTimeOffset Until);
public sealed record PermissionDescriptor(string Code, bool Approved, bool Implemented, long CatalogVersion,
    bool RequiresMfa, TimeSpan? MaximumMfaAge, bool ReviewIndependent = false, bool ApprovalIndependent = false,
    bool GlobalAdministrative = false);
public sealed record RolePermission(Guid RoleId, string PermissionCode);
public sealed record StationGrant(Guid Id, Guid UserRoleId, ScopeKind Scope, string ScopeId, bool Active,
    DateTimeOffset From, DateTimeOffset Until, long HierarchyVersion);
public sealed record GrantFunction(Guid GrantId, string PermissionCode);
public sealed record AdministrativeDelegation(Guid UserRoleId, string TargetPopulation, string PermissionCode,
    bool Active, DateTimeOffset From, DateTimeOffset Until);
public sealed record TrustedResource(string StationId, string CorporationId, string ManagementUnitId,
    long HierarchyVersion, ImmutableArray<string> ReviewedAncestorUnits);
public sealed record WorkflowContext(string Revision, string? CreatorPerson, string? ReviewerPerson,
    ImmutableHashSet<string> BeneficiaryPersons);
public sealed record AuthorizationSnapshot(UserIdentity User, HumanSession Session, AuthorityVersion CurrentVersion,
    ImmutableArray<RoleDefinition> Roles, ImmutableArray<RoleAssignment> Assignments,
    ImmutableArray<PermissionDescriptor> Permissions, ImmutableArray<RolePermission> RolePermissions,
    ImmutableArray<StationGrant> Grants, ImmutableArray<GrantFunction> Functions,
    ImmutableArray<AdministrativeDelegation> Delegations);
public sealed record AccessRequest(string PermissionCode, CredentialKind Credential, TrustedResource? Resource = null,
    string? AdministrativeTarget = null, WorkflowContext? Workflow = null);
public sealed record AccessDecision(bool Allowed, string Reason)
{
    public static AccessDecision Deny(string reason) => new(false, reason);
    public static AccessDecision Permit() => new(true, "AUTHORIZED");
}
