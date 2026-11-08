using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fdc.Identity.Infrastructure;

// No connection is opened by model construction; no startup migration or automatic seed.
public sealed class IamDbContext(DbContextOptions<IamDbContext> options) : DbContext(options)
{
 protected override void OnModelCreating(ModelBuilder model)
 {
  Configure(model.Entity<PersonReferenceRow>(), "PersonReference", "iam");
  Configure(model.Entity<UserRow>(), "User", "iam");
  Configure(model.Entity<PasswordCredentialRow>(), "PasswordCredential", "iam");
  Configure(model.Entity<RoleRow>(), "Role", "iam");
  Configure(model.Entity<PermissionRow>(), "Permission", "iam");
  Configure(model.Entity<RolePermissionRow>(), "RolePermission", "iam");
  Configure(model.Entity<UserRoleRow>(), "UserRole", "iam");
  Configure(model.Entity<CorporationRow>(), "Corporation", "catalog");
  Configure(model.Entity<ManagementUnitRow>(), "ManagementUnit", "catalog");
  Configure(model.Entity<StationRow>(), "Station", "catalog");
  Configure(model.Entity<ScopeRow>(), "Scope", "catalog");
  Configure(model.Entity<StationGrantRow>(), "StationGrant", "iam");
  Configure(model.Entity<GrantFunctionRow>(), "GrantFunction", "iam");
  Configure(model.Entity<AdministrativeDelegationRow>(), "AdministrativeDelegation", "iam");
  Configure(model.Entity<SessionRow>(), "Session", "iam");
  Configure(model.Entity<MfaCredentialRow>(), "MfaCredential", "iam");
  Configure(model.Entity<ChallengeRow>(), "Challenge", "iam");
  Configure(model.Entity<RoleChangeRequestRow>(), "RoleChangeRequest", "iam");
  Configure(model.Entity<RoleChangeApprovalRow>(), "RoleChangeApproval", "iam");
  Configure(model.Entity<AuthorityVersionRow>(), "AuthorityVersion", "iam");
  Configure(model.Entity<SecurityEventRow>(), "SecurityEvent", "audit");
  Configure(model.Entity<AuditOutboxRow>(), "AuditOutbox", "ops");
  Configure(model.Entity<SeedInstallationRow>(), "SeedInstallation", "ops");
  Configure(model.Entity<RoleHistoryRow>(), "RoleHistory", "ops");
  model.Entity<UserRow>().HasOne<PersonReferenceRow>().WithMany().HasForeignKey(x => x.PersonReferenceId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<PasswordCredentialRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RolePermissionRow>().HasOne<RoleRow>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RolePermissionRow>().HasOne<PermissionRow>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<UserRoleRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<UserRoleRow>().HasOne<RoleRow>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<ManagementUnitRow>().HasOne<CorporationRow>().WithMany().HasForeignKey(x => x.CorporationId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<ManagementUnitRow>().HasOne<ManagementUnitRow>().WithMany().HasForeignKey(x => x.ParentUnitId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<StationRow>().HasOne<ManagementUnitRow>().WithMany().HasForeignKey(x => x.ManagementUnitId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<ScopeRow>().HasOne<CorporationRow>().WithMany().HasForeignKey(x => x.CorporationId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<ScopeRow>().HasOne<ManagementUnitRow>().WithMany().HasForeignKey(x => x.ManagementUnitId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<ScopeRow>().HasOne<StationRow>().WithMany().HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<StationGrantRow>().HasOne<UserRoleRow>().WithMany().HasForeignKey(x => x.UserRoleId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<StationGrantRow>().HasOne<ScopeRow>().WithMany().HasForeignKey(x => x.ScopeId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<GrantFunctionRow>().HasOne<StationGrantRow>().WithMany().HasForeignKey(x => x.StationGrantId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<GrantFunctionRow>().HasOne<PermissionRow>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<AdministrativeDelegationRow>().HasOne<UserRoleRow>().WithMany().HasForeignKey(x => x.UserRoleId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<AdministrativeDelegationRow>().HasOne<PermissionRow>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<SessionRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<MfaCredentialRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<ChallengeRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleChangeRequestRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.RequesterUserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleChangeRequestRow>().HasOne<RoleRow>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleChangeApprovalRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.ApproverUserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleChangeApprovalRow>().HasOne<PersonReferenceRow>().WithMany().HasForeignKey(x => x.ApproverPersonReferenceId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<SecurityEventRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<AuditOutboxRow>().HasOne<SecurityEventRow>().WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleHistoryRow>().HasOne<RoleRow>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<PersonReferenceRow>().HasIndex(x => x.PersonKey).IsUnique();
  model.Entity<UserRow>().HasIndex(x => x.NormalizedUsername).IsUnique();
  model.Entity<UserRow>().HasIndex(x => x.PersonReferenceId).IsUnique();
  model.Entity<PasswordCredentialRow>().HasIndex(x => x.UserId).IsUnique();
  model.Entity<RoleRow>().HasIndex(x => x.RoleCode).IsUnique();
  model.Entity<PermissionRow>().HasIndex(x => x.PermissionCode).IsUnique();
  model.Entity<RolePermissionRow>().HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
  model.Entity<GrantFunctionRow>().HasIndex(x => new { x.StationGrantId, x.PermissionId }).IsUnique();
  model.Entity<SessionRow>().HasIndex(x => x.HandleDigest).IsUnique();
  model.Entity<MfaCredentialRow>().HasIndex(x => x.CredentialReference).IsUnique();
  model.Entity<ChallengeRow>().HasIndex(x => x.ProofDigest).IsUnique();
  model.Entity<AuthorityVersionRow>().HasIndex(x => x.AuthorityKey).IsUnique();
  model.Entity<RoleChangeRequestRow>().HasIndex(x => new { x.RequesterUserId, x.IdempotencyKey }).IsUnique();
  model.Entity<RoleChangeApprovalRow>().HasIndex(x => new { x.RequestId, x.Revision, x.ApproverPersonReferenceId }).IsUnique();
  model.Entity<AuditOutboxRow>().HasIndex(x => x.EventId).IsUnique();
  model.Entity<SeedInstallationRow>().HasIndex(x => x.ManifestId).IsUnique();
  model.Entity<RoleHistoryRow>().HasIndex(x => new { x.RoleId, x.Version }).IsUnique();
  model.Entity<RoleChangeRequestRow>().HasAlternateKey(x => new { x.Id, x.Revision });
  model.Entity<UserRow>().HasAlternateKey(x => new { x.Id, x.PersonReferenceId });
  model.Entity<RoleChangeApprovalRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => new { x.ApproverUserId, x.ApproverPersonReferenceId }).HasPrincipalKey(x => new { x.Id, x.PersonReferenceId }).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleChangeApprovalRow>().HasOne<RoleChangeRequestRow>().WithMany().HasForeignKey(x => new { x.RequestId, x.Revision }).HasPrincipalKey(x => new { x.Id, x.Revision }).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleHistoryRow>().HasOne<RoleChangeRequestRow>().WithMany().HasForeignKey(x => new { x.RequestId, x.Revision }).HasPrincipalKey(x => new { x.Id, x.Revision }).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleRow>().Property(x => x.RoleCode).UseCollation("Latin1_General_100_BIN2").HasMaxLength(80);
  // SQL Server tables with triggers must not use the direct OUTPUT clause for SaveChanges.
  model.Entity<RoleRow>().ToTable("Role", "iam", t => t.UseSqlOutputClause(false));
  model.Entity<RoleChangeRequestRow>().ToTable("RoleChangeRequest", "iam", t => t.UseSqlOutputClause(false));
  model.Entity<SecurityEventRow>().ToTable("SecurityEvent", "audit", t => t.UseSqlOutputClause(false));
  model.Entity<RoleRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleRow>().HasOne<UserRow>().WithMany().HasForeignKey(x => x.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
  model.Entity<RoleRow>().Property(x => x.RoleName).HasMaxLength(160);
  model.Entity<RoleRow>().Property(x => x.Description).HasMaxLength(2000);
  model.Entity<PermissionRow>().Property(x => x.PermissionCode).UseCollation("Latin1_General_100_BIN2").HasMaxLength(120);
  model.Entity<SessionRow>().Property(x => x.HandleDigest).HasMaxLength(64).IsFixedLength();
  model.Entity<ChallengeRow>().Property(x => x.ProofDigest).HasMaxLength(64).IsFixedLength();
  model.Entity<RoleChangeRequestRow>().Property(x => x.Digest).HasMaxLength(64).IsFixedLength();
  model.Entity<RoleChangeRequestRow>().Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");
  model.Entity<RoleHistoryRow>().Property(x => x.StateJson).HasColumnType("nvarchar(max)");
  model.Entity<PasswordCredentialRow>().Property(x => x.EncodedHash).HasMaxLength(2048);
  model.Entity<MfaCredentialRow>().ToTable("MfaCredential", "iam", t => t.HasCheckConstraint("CK_MfaCredential_Material", "([Kind] = N'WebAuthn' AND [PublicKey] IS NOT NULL AND [EncryptedSecret] IS NULL) OR ([Kind] = N'Totp' AND [EncryptedSecret] IS NOT NULL AND [KeyVersion] IS NOT NULL AND [PublicKey] IS NULL)"));
  model.Entity<ScopeRow>().ToTable("Scope", "catalog", t => t.HasCheckConstraint("CK_Scope_Target", "([Kind]=N'Corporation' AND [CorporationId] IS NOT NULL AND [ManagementUnitId] IS NULL AND [StationId] IS NULL) OR ([Kind]=N'ManagementUnit' AND [CorporationId] IS NULL AND [ManagementUnitId] IS NOT NULL AND [StationId] IS NULL) OR ([Kind]=N'Station' AND [CorporationId] IS NULL AND [ManagementUnitId] IS NULL AND [StationId] IS NOT NULL)"));
  model.Entity<UserRoleRow>().ToTable("UserRole", "iam", t => t.HasCheckConstraint("CK_UserRole_Validity", "[ValidUntil] > [ValidFrom]"));
  model.Entity<StationGrantRow>().ToTable("StationGrant", "iam", t => t.HasCheckConstraint("CK_StationGrant_Validity", "[ValidUntil] > [ValidFrom]"));
  model.Entity<AdministrativeDelegationRow>().ToTable("AdministrativeDelegation", "iam", t => t.HasCheckConstraint("CK_AdministrativeDelegation_Validity", "[ValidUntil] > [ValidFrom]"));
  model.Entity<RoleChangeRequestRow>().ToTable("RoleChangeRequest", "iam", t => t.HasCheckConstraint("CK_RoleChangeRequest_Json", "ISJSON([PayloadJson])=1"));
  model.Entity<SessionRow>().HasIndex(x => new { x.UserId, x.Revoked, x.AbsoluteUntil });
  model.Entity<UserRoleRow>().HasIndex(x => new { x.UserId, x.Active, x.ValidUntil });
  model.Entity<StationGrantRow>().HasIndex(x => new { x.UserRoleId, x.Active, x.ValidUntil });
  model.Entity<AuditOutboxRow>().HasIndex(x => new { x.ArchivedAt, x.CreatedAt });
 }
 private static void Configure<T>(EntityTypeBuilder<T> builder, string table, string schema) where T : IamRow
 {
  builder.ToTable(table, schema, t => { t.HasCheckConstraint($"CK_{table}_Version", "[Version] > 0"); t.HasCheckConstraint($"CK_{table}_UtcCreated", "DATEPART(TZOFFSET,[CreatedAt])=0"); });
  builder.HasKey(x => x.Id); builder.Property(x => x.Id).ValueGeneratedNever();
  builder.Property(x => x.RowVersion).IsRowVersion(); builder.Property(x => x.CreatedAt).HasPrecision(7);
  foreach (var property in typeof(T).GetProperties().Where(x => x.PropertyType == typeof(string))) builder.Property(property.Name).HasMaxLength(256);
  foreach (var property in typeof(T).GetProperties().Where(x => x.PropertyType == typeof(DateTimeOffset) || x.PropertyType == typeof(DateTimeOffset?)))
   builder.ToTable(table, schema, t => t.HasCheckConstraint($"CK_{table}_Utc_{property.Name}", $"[{property.Name}] IS NULL OR DATEPART(TZOFFSET,[{property.Name}])=0"));
 }
}
