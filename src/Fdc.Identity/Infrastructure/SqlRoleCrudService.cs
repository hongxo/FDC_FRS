using System.Collections.Immutable;
using System.Text.Json;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fdc.Identity.Infrastructure;

// Real SQL implementation, opt-in only. No host registration; pending authority/commit implementations refuse execution.
// All writes require the SAME DbContext transaction supplied by D09. No public direct role mutation endpoint.
public sealed class SqlRoleCrudService(IamDbContext db, IAuthorizedCommitBoundary boundary, IRoleMutationAuthority authority, TimeProvider clock)
{
    public async Task<RoleRequestAccepted> SubmitAsync(RoleChangePayload payload, string idempotencyKey, CancellationToken ct)
    {
        ValidateKey(idempotencyKey);
        var actor = await authority.DemandAsync(RoleMutationOperation.SubmitRequest, ct);
        return await boundary.CommitAsync(actor.UserId, async token =>
        {
            RequireTransaction();
            actor = await SameActorAsync(actor, RoleMutationOperation.SubmitRequest, token);
            await CurrentAuthorityAsync(actor, token);
            var now = clock.GetUtcNow();
            if (actor.RequestExpiresAt <= now || actor.RequestExpiresAt.Offset != TimeSpan.Zero) throw new InvalidOperationException("Invalid request lifetime.");
            var existing = await db.Set<RoleChangeRequestRow>().AsNoTracking().SingleOrDefaultAsync(r => r.RequesterUserId == actor.UserId && r.IdempotencyKey == idempotencyKey, token);
            var expires = existing?.ExpiresAt ?? actor.RequestExpiresAt;
            var request = new RoleChangeRequest(actor.PersonKey, payload, existing?.Revision ?? 1, expires);
            if (existing is not null)
            {
                if (existing.Digest != request.Digest || existing.PolicyVersion != actor.PolicyVersion || existing.CatalogVersion != actor.CatalogVersion || now >= existing.ExpiresAt)
                    throw new InvalidOperationException("Idempotency key payload/version conflict.");
                return new RoleRequestAccepted(existing.Id, existing.Revision, existing.Digest, existing.ApprovalStatus, existing.EffectiveStatus, true);
            }
            await PreviewAsync(payload, actor, token);
            var row = new RoleChangeRequestRow { Id = request.Id, CreatedAt = now, RequesterUserId = actor.UserId,
                RoleId = payload.RoleId, Revision = request.Revision, PayloadJson = request.PayloadJson, Digest = request.Digest,
                IdempotencyKey = idempotencyKey, BaseRoleVersion = payload.BaseVersion, PolicyVersion = actor.PolicyVersion,
                CatalogVersion = actor.CatalogVersion, ExpiresAt = expires };
            db.Add(row); AddAudit(actor, "iam.role.request", "pending", now);
            await db.SaveChangesAsync(token);
            return new RoleRequestAccepted(row.Id, row.Revision, row.Digest, row.ApprovalStatus, row.EffectiveStatus, false);
        }, ct);
    }

    public async Task<RoleApplyResult> ApplyReviewedAsync(Guid requestId, long revision, string digest, byte[] expectedRowVersion, CancellationToken ct)
    {
        if (revision < 1 || digest.Length != 64 || expectedRowVersion.Length != 8) throw new ArgumentException("Revision, digest and rowversion required.");
        var actor = await authority.DemandAsync(RoleMutationOperation.ApplyReviewedRequest, ct);
        return await boundary.CommitAsync(actor.UserId, async token =>
        {
            RequireTransaction();
            actor = await SameActorAsync(actor, RoleMutationOperation.ApplyReviewedRequest, token);
            var versions = await CurrentAuthorityAsync(actor, token);
            var row = await db.Set<RoleChangeRequestRow>().SingleOrDefaultAsync(r => r.Id == requestId, token) ?? throw new InvalidOperationException("Request not available.");
            await db.Entry(row).ReloadAsync(token);
            await authority.DemandReviewedRequestAsync(actor, row, token);
            if (row.Revision != revision || row.Digest != digest) throw new InvalidOperationException("Revision conflict.");
            var payload = JsonSerializer.Deserialize<RoleChangePayload>(row.PayloadJson) ?? throw new InvalidOperationException("Invalid stored payload.");
            var targetId = payload.Kind is RoleChangeKind.Create or RoleChangeKind.Clone ? row.Id : row.RoleId ?? throw new InvalidOperationException("Target role required.");
            if (row.EffectiveStatus == "Applied")
            {
                var applied = await db.Set<RoleRow>().SingleAsync(r => r.Id == targetId, token);
                return new RoleApplyResult(row.Id, targetId, applied.Version, true);
            }
            if (row.ApprovalStatus != "Approved" || row.EffectiveStatus != "NotApplied" || row.PolicyVersion != actor.PolicyVersion ||
                row.CatalogVersion != actor.CatalogVersion || clock.GetUtcNow() >= row.ExpiresAt || !row.RowVersion.AsSpan().SequenceEqual(expectedRowVersion))
                throw new InvalidOperationException("Unapproved, expired or stale request.");
            var requester = await (from u in db.Set<UserRow>() join p in db.Set<PersonReferenceRow>() on u.PersonReferenceId equals p.Id
                                   where u.Id == row.RequesterUserId && u.Active select p.PersonKey).SingleOrDefaultAsync(token);
            if (requester is null || new RoleChangeRequest(requester, payload, row.Revision, row.ExpiresAt).Digest != row.Digest)
                throw new InvalidOperationException("Immutable requester/payload binding changed.");
            var preview = await PreviewAsync(payload, actor, token);
            var now = clock.GetUtcNow();
            RoleRow role;
            if (payload.Kind is RoleChangeKind.Create or RoleChangeKind.Clone)
            {
                role = new() { Id = targetId, CreatedAt = now, RoleCode = preview.Draft.Code, RoleName = preview.Draft.Name,
                    Description = preview.Draft.Description, IsActive = false, IsSystemSeed = false, CreatedBy = row.RequesterUserId };
                db.Add(role);
            }
            else
            {
                role = await db.Set<RoleRow>().SingleAsync(r => r.Id == targetId, token);
                await db.Entry(role).ReloadAsync(token);
                if (role.Version != payload.BaseVersion) throw new InvalidOperationException("Role version changed.");
                if (payload.Kind == RoleChangeKind.Rename) role.RoleName = preview.Draft.Name;
                if (payload.Kind == RoleChangeKind.Activate)
                {
                    if (role.ValidUntil <= now) throw new InvalidOperationException("Cannot reactivate expired role.");
                    role.IsActive = true;
                }
                if (payload.Kind == RoleChangeKind.Deactivate) role.IsActive = false;
                role.Version = checked(role.Version + 1); role.UpdatedAt = now; role.UpdatedBy = actor.UserId;
            }
            if (payload.Kind is RoleChangeKind.PermissionDiff or RoleChangeKind.Clone)
            {
                var current = await db.Set<RolePermissionRow>().Where(x => x.RoleId == role.Id).ToListAsync(token);
                var permissions = await db.Set<PermissionRow>().Where(p => preview.ProposedPermissions.Contains(p.PermissionCode) && p.IsApproved && p.IsImplemented && p.CatalogVersion == actor.CatalogVersion).ToListAsync(token);
                if (permissions.Count != preview.ProposedPermissions.Length) throw new InvalidOperationException("Catalog changed.");
                var wanted = permissions.Select(x => x.Id).ToHashSet();
                db.RemoveRange(current.Where(x => !wanted.Contains(x.PermissionId)));
                foreach (var permission in permissions.Where(p => current.All(x => x.PermissionId != p.Id)))
                    db.Add(new RolePermissionRow { Id = Guid.NewGuid(), CreatedAt = now, RoleId = role.Id, PermissionId = permission.Id });
            }
            var impacted = await (from u in db.Set<UserRow>() where db.Set<UserRoleRow>().Any(a => a.UserId == u.Id && a.RoleId == role.Id) select u).ToListAsync(token);
            foreach (var user in impacted) { await db.Entry(user).ReloadAsync(token); user.GrantVersion = checked(user.GrantVersion + 1); user.Version = checked(user.Version + 1); }
            versions.PolicyVersion = checked(versions.PolicyVersion + 1); versions.Version = checked(versions.Version + 1);
            row.EffectiveStatus = "Applied"; row.Version = checked(row.Version + 1);
            db.Add(new RoleHistoryRow { Id = Guid.NewGuid(), CreatedAt = now, Version = role.Version, RoleId = role.Id,
                RequestId = row.Id, Revision = row.Revision, Digest = row.Digest,
                StateJson = JsonSerializer.Serialize(new { role.RoleCode, role.RoleName, role.IsActive, Permissions = preview.ProposedPermissions }) });
            AddAudit(actor, "iam.role.apply", "applied", now);
            await db.SaveChangesAsync(token);
            return new RoleApplyResult(row.Id, role.Id, role.Version, false);
        }, ct);
    }

    private async Task<RoleProposalPreview> PreviewAsync(RoleChangePayload payload, RoleMutationActor actor, CancellationToken ct)
    {
        var row = payload.RoleId is { } id ? await db.Set<RoleRow>().AsNoTracking().SingleOrDefaultAsync(r => r.Id == id, ct) : null;
        RoleDefinition? role = row is null ? null : new(row.Id, row.RoleCode, row.RoleName, row.Description, row.IsSystemSeed, row.IsActive, row.Version, row.ValidUntil);
        var codes = row is null ? [] : await (from rp in db.Set<RolePermissionRow>() join p in db.Set<PermissionRow>() on rp.PermissionId equals p.Id
                                              where rp.RoleId == row.Id select p.PermissionCode).ToListAsync(ct);
        var occupied = await db.Set<RoleRow>().Where(r => r.RoleCode == payload.NewCode).Select(r => r.RoleCode).ToListAsync(ct);
        var deployed = await db.Set<PermissionRow>().AsNoTracking().Where(p => p.IsApproved && p.IsImplemented && p.CatalogVersion == actor.CatalogVersion).Select(p => p.PermissionCode).ToListAsync(ct);
        return RoleProposalValidator.Validate(payload, role, codes.ToImmutableHashSet(StringComparer.Ordinal), occupied.ToImmutableHashSet(StringComparer.Ordinal),
            new(actor.CatalogVersion, actor.SupportedPermissions.Intersect(deployed)));
    }
    private async Task<AuthorityVersionRow> CurrentAuthorityAsync(RoleMutationActor actor, CancellationToken ct)
    {
        var row = await db.Set<AuthorityVersionRow>().SingleOrDefaultAsync(a => a.AuthorityKey == "Center" && a.PolicyConfigured, ct);
        if (row is not null) await db.Entry(row).ReloadAsync(ct);
        if (row is null || actor.UserId == Guid.Empty || string.IsNullOrWhiteSpace(actor.PersonKey) || row.RestoreEpoch == Guid.Empty ||
            !row.PolicyConfigured || !await (from u in db.Set<UserRow>() join p in db.Set<PersonReferenceRow>() on u.PersonReferenceId equals p.Id
                where u.Id == actor.UserId && u.Active && p.PersonKey == actor.PersonKey select u.Id).AnyAsync(ct) ||
            row.PolicyVersion != actor.PolicyVersion || row.CatalogVersion != actor.CatalogVersion ||
            string.IsNullOrWhiteSpace(actor.CorrelationId) || actor.CorrelationId.Length > 128 || actor.CorrelationId.Any(char.IsControl))
            throw new AuthorityUnavailableException();
        return row;
    }
    private void AddAudit(RoleMutationActor actor, string action, string outcome, DateTimeOffset now)
    {
        var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = actor.UserId, EventCode = action,
            OutcomeCode = outcome, CorrelationId = actor.CorrelationId, PolicyVersion = actor.PolicyVersion,
            RestoreEpoch = db.Set<AuthorityVersionRow>().Local.Single(x => x.AuthorityKey == "Center").RestoreEpoch };
        db.Add(audit); db.Add(new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
    }
    private void RequireTransaction() { if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("D09 transaction must own this DbContext."); }
    private async Task<RoleMutationActor> SameActorAsync(RoleMutationActor before, RoleMutationOperation operation, CancellationToken ct)
    {
        var current = await authority.DemandAsync(operation, ct);
        if (current.UserId != before.UserId || current.PersonKey != before.PersonKey) throw new UnauthorizedAccessException("Actor changed while entering authority gate.");
        return current;
    }
    private static void ValidateKey(string key)
    { if (key.Length is < 8 or > 128 || key.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_')) throw new ArgumentException("Invalid idempotency key."); }
}
