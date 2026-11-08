namespace Fdc.Identity.Infrastructure;

public abstract class IamRow
{
 public Guid Id { get; set; }
 public long Version { get; set; } = 1;
 public DateTimeOffset CreatedAt { get; set; }
 public byte[] RowVersion { get; set; } = [];
}

public sealed class PersonReferenceRow : IamRow
{
 public required string PersonKey { get; set; }
}

public sealed class UserRow : IamRow
{
 public Guid PersonReferenceId { get; set; } public required string NormalizedUsername { get; set; } public bool Active { get; set; } public long SecurityVersion { get; set; } = 1; public long GrantVersion { get; set; } = 1;
}

public sealed class PasswordCredentialRow : IamRow
{
 public Guid UserId { get; set; } public required string EncodedHash { get; set; } public required string AlgorithmVersion { get; set; }
}

public sealed class RoleRow : IamRow
{
 public Guid? CreatedBy { get; set; }
 public Guid? UpdatedBy { get; set; }
 public DateTimeOffset? UpdatedAt { get; set; }
 public required string RoleCode { get; set; } public required string RoleName { get; set; } public string Description { get; set; } = ""; public bool IsSystemSeed { get; set; } public bool IsActive { get; set; } public DateTimeOffset? ValidUntil { get; set; }
}

public sealed class PermissionRow : IamRow
{
 public required string PermissionCode { get; set; } public required string Module { get; set; } public required string Action { get; set; } public required string ScopeRequirement { get; set; } public required string SoDPolicy { get; set; } public required string RiskLevel { get; set; } public bool IsApproved { get; set; } public bool IsImplemented { get; set; } public long CatalogVersion { get; set; } public string? ApprovalReference { get; set; }
}

public sealed class RolePermissionRow : IamRow
{
 public Guid RoleId { get; set; } public Guid PermissionId { get; set; }
}

public sealed class UserRoleRow : IamRow
{
 public Guid UserId { get; set; } public Guid RoleId { get; set; } public bool Active { get; set; } public DateTimeOffset ValidFrom { get; set; } public DateTimeOffset ValidUntil { get; set; } public required string ApprovalReference { get; set; }
}

public sealed class CorporationRow : IamRow
{
 public required string Code { get; set; }
}

public sealed class ManagementUnitRow : IamRow
{
 public Guid CorporationId { get; set; } public Guid? ParentUnitId { get; set; } public required string Code { get; set; }
}

public sealed class StationRow : IamRow
{
 public Guid ManagementUnitId { get; set; } public required string Code { get; set; } public long HierarchyVersion { get; set; }
}

public sealed class ScopeRow : IamRow
{
 public string Kind { get; set; } = "Station"; public Guid? CorporationId { get; set; } public Guid? ManagementUnitId { get; set; } public Guid? StationId { get; set; }
}

public sealed class StationGrantRow : IamRow
{
 public Guid UserRoleId { get; set; } public Guid ScopeId { get; set; } public bool Active { get; set; } public DateTimeOffset ValidFrom { get; set; } public DateTimeOffset ValidUntil { get; set; } public long GrantVersion { get; set; } = 1; public long HierarchyVersion { get; set; } public required string ApprovalReference { get; set; }
}

public sealed class GrantFunctionRow : IamRow
{
 public Guid StationGrantId { get; set; } public Guid PermissionId { get; set; }
}

public sealed class AdministrativeDelegationRow : IamRow
{
 public Guid UserRoleId { get; set; } public Guid PermissionId { get; set; } public required string TargetPopulation { get; set; } public bool Active { get; set; } public DateTimeOffset ValidFrom { get; set; } public DateTimeOffset ValidUntil { get; set; } public required string ApprovalReference { get; set; }
}

public sealed class SessionRow : IamRow
{
 public Guid UserId { get; set; } public required string HandleDigest { get; set; } public bool Revoked { get; set; } public DateTimeOffset IdleUntil { get; set; } public DateTimeOffset AbsoluteUntil { get; set; } public DateTimeOffset? MfaAt { get; set; } public long SecurityVersion { get; set; } public long GrantVersion { get; set; } public long PolicyVersion { get; set; } public long CatalogVersion { get; set; } public long HierarchyVersion { get; set; } public Guid RestoreEpoch { get; set; }
}

public sealed class MfaCredentialRow : IamRow
{
 public Guid UserId { get; set; } public required string Kind { get; set; } public required string CredentialReference { get; set; } public byte[]? PublicKey { get; set; } public byte[]? EncryptedSecret { get; set; } public string? KeyVersion { get; set; } public long? LastAcceptedStep { get; set; } public bool Active { get; set; }
}

public sealed class ChallengeRow : IamRow
{
 public Guid UserId { get; set; } public required string ProofDigest { get; set; } public required string Purpose { get; set; } public DateTimeOffset ExpiresAt { get; set; } public DateTimeOffset? ConsumedAt { get; set; }
}

public sealed class RoleChangeRequestRow : IamRow
{
 public Guid RequesterUserId { get; set; } public Guid? RoleId { get; set; } public long Revision { get; set; } public required string PayloadJson { get; set; } public required string Digest { get; set; } public required string IdempotencyKey { get; set; } public long BaseRoleVersion { get; set; } public long PolicyVersion { get; set; } public long CatalogVersion { get; set; } public DateTimeOffset ExpiresAt { get; set; } public string ApprovalStatus { get; set; } = "Pending"; public string EffectiveStatus { get; set; } = "NotApplied";
}

public sealed class RoleChangeApprovalRow : IamRow
{
 public Guid RequestId { get; set; } public long Revision { get; set; } public Guid ApproverUserId { get; set; } public Guid ApproverPersonReferenceId { get; set; } public required string RequestDigest { get; set; } public required string Decision { get; set; }
}

public sealed class AuthorityVersionRow : IamRow
{
 public string AuthorityKey { get; set; } = "Center"; public long PolicyVersion { get; set; } = 1; public long CatalogVersion { get; set; } = 1; public long HierarchyVersion { get; set; } = 1; public Guid RestoreEpoch { get; set; } public bool PolicyConfigured { get; set; }
}

public sealed class SecurityEventRow : IamRow
{
 public Guid? ActorUserId { get; set; } public required string EventCode { get; set; } public required string OutcomeCode { get; set; } public required string CorrelationId { get; set; } public long PolicyVersion { get; set; } public Guid RestoreEpoch { get; set; }
}

public sealed class AuditOutboxRow : IamRow
{
 public Guid EventId { get; set; } public DateTimeOffset? ArchivedAt { get; set; } public long Fence { get; set; }
}

public sealed class SeedInstallationRow : IamRow
{
 public required string ManifestId { get; set; } public required string ManifestDigest { get; set; } public required string ApprovalReference { get; set; }
}

public sealed class RoleHistoryRow : IamRow
{
 public Guid RoleId { get; set; } public Guid RequestId { get; set; } public long Revision { get; set; } public required string Digest { get; set; } public required string StateJson { get; set; }
}
