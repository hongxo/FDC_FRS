using Fdc.Center.Persistence;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Center;

public sealed record CollectorBindingDraft(Guid CollectorId, long ExpectedVersion, string TargetStatus, Guid RequestId);
public sealed record BindingDecision(string Action, string ETag);

// Candidate registry lifecycle changes only. Station assignment is immutable and real registrations are denied.
public sealed class CollectorBindingService(CenterDbContext center, BusinessAuthorization authority, LabWorkflowService boundary, TimeProvider clock)
{
    public Task<List<CollectorBindingRequest>> ListAsync(string digest, CancellationToken ct) =>
        authority.AuthorizedLabAsync(digest, BusinessReadOperation.CollectorBindingRead, async (_, scope, _, token) =>
        {
            await using var db = new LabWorkflowDbContext(new DbContextOptionsBuilder<LabWorkflowDbContext>().UseSqlServer(center.Database.GetDbConnection()).Options);
            await db.Database.UseTransactionAsync(center.Database.CurrentTransaction!.GetDbTransaction(), token);
            return await db.Set<CollectorBindingRequest>().FromSqlRaw("""
                SELECT request.* FROM ops.CollectorBindingRequest request
                INNER JOIN ops.CollectorRegistration collector
                  ON collector.Id=request.CollectorId AND collector.StationId=request.StationId
                WHERE collector.IsSynthetic=1
                """).AsNoTracking().Where(row => scope.Contains(row.StationId))
                .OrderBy(row => row.Id).Take(100).ToListAsync(token);
        }, ct);

    public Task<CollectorBindingRequest> RequestAsync(string digest, Guid actor, CollectorBindingDraft draft, string correlation, CancellationToken ct)
    {
        if (draft.RequestId == Guid.Empty || draft.ExpectedVersion < 1 || draft.TargetStatus is not ("Bound" or "Revoked"))
            throw new ArgumentException("Invalid registry lifecycle request.");
        return boundary.MutateAsync(digest, actor, BusinessReadOperation.CollectorBindingRequest, async (iam, db, scope, snapshot, token) =>
        {
            if (snapshot.User.Id != actor) throw new UnauthorizedAccessException();
            var collector = await center.Set<CollectorRegistration>().SingleOrDefaultAsync(row => row.Id == draft.CollectorId &&
                row.IsSynthetic && scope.Contains(row.StationId), token) ?? throw new UnauthorizedAccessException();
            var existing = await db.Set<CollectorBindingRequest>().SingleOrDefaultAsync(row => row.Id == draft.RequestId, token);
            if (existing is not null)
            {
                if (existing.RequesterPerson != snapshot.User.PersonReference || existing.CollectorId != draft.CollectorId ||
                    existing.ExpectedConfigurationVersion != draft.ExpectedVersion || existing.TargetStatus != draft.TargetStatus)
                    throw new LabConflictException();
                return existing;
            }
            if (collector.ConfigurationVersion != draft.ExpectedVersion) throw new LabConflictException();
            var row = new CollectorBindingRequest { Id = draft.RequestId, CollectorId = collector.Id, StationId = collector.StationId,
                ExpectedConfigurationVersion = draft.ExpectedVersion, TargetStatus = draft.TargetStatus,
                RequesterPerson = snapshot.User.PersonReference, ExpiresUtc = clock.GetUtcNow().AddMinutes(15) };
            db.Add(row);
            await db.SaveChangesAsync(token);
            await SaveAudit(iam, db, row, actor, snapshot.CurrentVersion.Policy, snapshot.CurrentVersion.RestoreEpoch, correlation, token);
            if (clock.GetUtcNow() >= row.ExpiresUtc) throw new UnauthorizedAccessException();
            return row;
        }, ct, correlation);
    }

    public Task<CollectorBindingRequest> TransitionAsync(string digest, Guid actor, Guid request, BindingDecision decision, string correlation, CancellationToken ct)
    {
        var operation = decision.Action switch
        {
            "review" => BusinessReadOperation.CollectorBindingReview,
            "approve" => BusinessReadOperation.CollectorBindingApprove,
            "apply" => BusinessReadOperation.CollectorBindingApply,
            _ => throw new ArgumentException("Invalid binding action.")
        };
        return boundary.MutateAsync(digest, actor, operation, async (iam, db, scope, snapshot, token) =>
        {
            if (snapshot.User.Id != actor) throw new UnauthorizedAccessException();
            var row = await db.Set<CollectorBindingRequest>().FromSqlInterpolated(
                $"SELECT * FROM ops.CollectorBindingRequest WITH(UPDLOCK,HOLDLOCK) WHERE Id={request}").SingleOrDefaultAsync(token);
            if (row is null || !scope.Contains(row.StationId) || row.ExpiresUtc <= clock.GetUtcNow()) throw new UnauthorizedAccessException();
            if (LabWorkflowService.ETag(row.VersionStamp) != decision.ETag) throw new LabConflictException();
            var person = snapshot.User.PersonReference;
            var collector = await center.Set<CollectorRegistration>().SingleOrDefaultAsync(item => item.Id == row.CollectorId &&
                item.StationId == row.StationId && item.IsSynthetic, token) ?? throw new UnauthorizedAccessException();
            switch (decision.Action)
            {
                case "review":
                    if (row.State != "Pending" || person == row.RequesterPerson) throw new UnauthorizedAccessException();
                    row.ReviewerPerson = person; row.State = "Reviewed";
                    break;
                case "approve":
                    if (row.State != "Reviewed" || person == row.RequesterPerson || person == row.ReviewerPerson)
                        throw new UnauthorizedAccessException();
                    row.ApproverPerson = person; row.State = "Approved";
                    break;
                case "apply":
                    if (row.State != "Approved" || person == row.RequesterPerson || person == row.ReviewerPerson || person == row.ApproverPerson)
                        throw new UnauthorizedAccessException();
                    if (collector.ConfigurationVersion != row.ExpectedConfigurationVersion) throw new LabConflictException();
                    collector.ConfigurationVersion++;
                    collector.BindingStatus = row.TargetStatus;
                    collector.Active = row.TargetStatus == "Bound";
                    row.ApplierPerson = person; row.State = "Applied";
                    await center.SaveChangesAsync(token);
                    break;
            }
            await db.SaveChangesAsync(token);
            await SaveAudit(iam, db, row, actor, snapshot.CurrentVersion.Policy, snapshot.CurrentVersion.RestoreEpoch, correlation, token);
            return row;
        }, ct, correlation);
    }

    private async Task SaveAudit(IamDbContext iam, LabWorkflowDbContext db, CollectorBindingRequest request, Guid actor,
        long policy, Guid epoch, string correlation, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = actor,
            EventCode = "collector.binding." + request.State.ToLowerInvariant(), OutcomeCode = request.State,
            CorrelationId = correlation, PolicyVersion = policy, RestoreEpoch = epoch };
        iam.AddRange(audit, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
        await iam.SaveChangesAsync(ct);
        db.Add(new CollectorBindingHistory { Id = Guid.NewGuid(), RequestId = request.Id, EventId = audit.Id });
        await db.SaveChangesAsync(ct);
    }
}
