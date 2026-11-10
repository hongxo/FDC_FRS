using System.Security.Cryptography;
using System.Text.Json;
using Fdc.Center.Persistence;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Center;

// Explicitly driven candidate worker. Startup creates no background execution or device adapter.
public sealed class LabExportCoordinator(CenterDbContext center, LabWorkflowService workflow, BusinessAuthorization authority,
    ISqlCommitGatePlanProvider gates, TimeProvider clock)
{
    public Task<Guid> RequestAsync(string digest, Guid actor, Guid jobId, string permit, int revision, string correlation, CancellationToken ct) =>
        workflow.MutateAsync(digest, actor, BusinessReadOperation.LabExport, async (iam, lab, scope, snapshot, token) =>
        {
            if (jobId == Guid.Empty || actor != snapshot.User.Id) throw new UnauthorizedAccessException();
            var row = await lab.Permits.AsNoTracking().SingleOrDefaultAsync(item => item.PermitId == permit &&
                item.PermitVersion == revision && item.IsSynthetic == true && item.StationId != null &&
                scope.Contains(item.StationId.Value), token) ?? throw new UnauthorizedAccessException();
            var existing = await lab.Exports.SingleOrDefaultAsync(item => item.Id == jobId, token);
            if (existing is not null)
            {
                if (existing.ActorUserId != actor || existing.PermitId != permit || existing.PermitVersion != revision)
                    throw new LabConflictException();
                return existing.Id;
            }
            lab.Add(new LabExportJob { Id = jobId, ActorUserId = actor, PermitId = permit, PermitVersion = revision,
                StationId = row.StationId!.Value, SessionDigest = digest,
                ExpiresUtc = new[] { clock.GetUtcNow().AddMinutes(5), snapshot.Session.AbsoluteUntil, snapshot.Session.IdleUntil }.Min() });
            await lab.SaveChangesAsync(token);
            await AuditAsync(iam, actor, "requested", correlation, snapshot.CurrentVersion, token);
            return jobId;
        }, ct);

    public async Task<bool> PrepareAsync(Guid id, Guid worker, string correlation, CancellationToken ct)
    {
        if (worker == Guid.Empty) throw new ArgumentException("Worker identity required.");
        await using var lookup = Context();
        var reference = await lookup.Exports.AsNoTracking().SingleAsync(row => row.Id == id, ct);
        try
        {
            return await workflow.MutateAsync(reference.SessionDigest, reference.ActorUserId, BusinessReadOperation.LabExport,
                async (iam, lab, scope, snapshot, token) =>
                {
                    var job = await Locked(lab, id, token);
                    if (job.State == "Completed") return false;
                    if (job.State == "Quarantined" || job.ExpiresUtc <= clock.GetUtcNow()) return false;
                    if (job.State == "Preparing" && job.LeaseUntilUtc > clock.GetUtcNow()) return false;
                    if (!scope.Contains(job.StationId) || job.ActorUserId != snapshot.User.Id) throw new UnauthorizedAccessException();
                    var permit = await lab.Permits.AsNoTracking().SingleAsync(row => row.PermitId == job.PermitId &&
                        row.PermitVersion == job.PermitVersion && row.IsSynthetic == true && row.StationId == job.StationId, token);
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(new { permit.PermitId, permit.PermitVersion,
                        permit.StationId, permit.IedId, permit.DraftStatus, permit.DraftJson, permit.PermitDigest, permit.ResultDigest });
                    if (bytes.Length > 65536) throw new ArgumentException("Artifact exceeds bounded SQL staging.");
                    job.State = "Preparing";
                    job.LeaseOwner = worker;
                    job.LeaseUntilUtc = clock.GetUtcNow().AddMinutes(1);
                    job.Payload = bytes;
                    job.PayloadDigest = Convert.ToHexStringLower(SHA256.HashData(bytes));
                    await lab.SaveChangesAsync(token);
                    await AuditAsync(iam, job.ActorUserId, "prepared", correlation, snapshot.CurrentVersion, token);
                    return true;
                }, ct);
        }
        catch (UnauthorizedAccessException) { await QuarantineAsync(reference, worker, correlation, ct); return false; }
    }

    public async Task<bool> ReleaseAsync(Guid id, Guid worker, string correlation, CancellationToken ct)
    {
        await using var lookup = Context();
        var reference = await lookup.Exports.AsNoTracking().SingleAsync(row => row.Id == id, ct);
        try
        {
            return await workflow.MutateAsync(reference.SessionDigest, reference.ActorUserId, BusinessReadOperation.LabExport,
                async (iam, lab, scope, snapshot, token) =>
                {
                    var job = await Locked(lab, id, token);
                    if (job.State != "Preparing" || job.LeaseOwner != worker || job.LeaseUntilUtc <= clock.GetUtcNow()) return false;
                    if (!scope.Contains(job.StationId) || job.ActorUserId != snapshot.User.Id || job.ExpiresUtc <= clock.GetUtcNow())
                        throw new UnauthorizedAccessException();
                    job.State = "Completed";
                    job.LeaseOwner = null;
                    job.LeaseUntilUtc = null;
                    await lab.SaveChangesAsync(token);
                    await AuditAsync(iam, job.ActorUserId, "released", correlation, snapshot.CurrentVersion, token);
                    return true;
                }, ct);
        }
        catch (UnauthorizedAccessException) { await QuarantineAsync(reference, worker, correlation, ct); return false; }
    }

    public Task<byte[]?> DownloadAsync(string digest, Guid id, string correlation, CancellationToken ct) =>
        authority.AuthorizedLabAsync<byte[]?>(digest, BusinessReadOperation.LabExport, async (iam, scope, snapshot, token) =>
        {
            await using var lab = Context();
            await lab.Database.UseTransactionAsync(center.Database.CurrentTransaction!.GetDbTransaction(), token);
            var job = await lab.Exports.AsNoTracking().SingleOrDefaultAsync(row => row.Id == id &&
                row.ActorUserId == snapshot.User.Id && scope.Contains(row.StationId), token);
            if (job?.State != "Completed" || job.ExpiresUtc <= clock.GetUtcNow() || job.Payload is null ||
                Convert.ToHexStringLower(SHA256.HashData(job.Payload)) != job.PayloadDigest) return null;
            await AuditAsync(iam, job.ActorUserId, "download.prepared", correlation, snapshot.CurrentVersion, token);
            return job.Payload;
        }, ct);

    private async Task QuarantineAsync(LabExportJob reference, Guid worker, string correlation, CancellationToken ct)
    {
        await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>().UseSqlServer(center.Database.GetDbConnection()).Options);
        await new SqlAuthorityCommitBoundary(iam, gates).CommitAsync(reference.ActorUserId, async token =>
        {
            await using var lab = Context();
            await lab.Database.UseTransactionAsync(iam.Database.CurrentTransaction!.GetDbTransaction(), token);
            var job = await Locked(lab, reference.Id, token);
            if (job.State == "Completed" || job.State == "Quarantined" ||
                (job.LeaseOwner is not null && job.LeaseOwner != worker)) return false;
            job.State = "Quarantined";
            job.Payload = null;
            job.PayloadDigest = null;
            job.LeaseOwner = null;
            job.LeaseUntilUtc = null;
            await lab.SaveChangesAsync(token);
            var version = await iam.Set<AuthorityVersionRow>().AsNoTracking().SingleAsync(token);
            await AuditAsync(iam, null, "quarantined", correlation,
                new(version.PolicyVersion, version.CatalogVersion, version.HierarchyVersion, version.RestoreEpoch), token);
            return true;
        }, ct);
    }

    private LabWorkflowDbContext Context() => new(new DbContextOptionsBuilder<LabWorkflowDbContext>()
        .UseSqlServer(center.Database.GetDbConnection()).Options);
    private static Task<LabExportJob> Locked(LabWorkflowDbContext lab, Guid id, CancellationToken ct) =>
        lab.Exports.FromSqlInterpolated($"SELECT * FROM labprep.ExportJob WITH(UPDLOCK,HOLDLOCK) WHERE Id={id}").SingleAsync(ct);
    private async Task AuditAsync(IamDbContext iam, Guid? actor, string outcome, string correlation, AuthorityVersion version, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var row = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = actor,
            EventCode = "lab.export." + outcome, OutcomeCode = outcome, CorrelationId = correlation,
            PolicyVersion = version.Policy, RestoreEpoch = version.RestoreEpoch };
        iam.AddRange(row, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = row.Id });
        await iam.SaveChangesAsync(ct);
    }
}
