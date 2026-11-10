using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fdc.Center.Persistence;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Center;

public sealed record SyntheticLabDraft(Guid InventoryId, Guid StationId, Guid IedId, DateTimeOffset Start,
    DateTimeOffset End, string Plan, string[] EvidenceSha256);
public sealed record LabTransition(string Action, string ETag, Guid RequestId, Guid? PermitIed = null, string? TestType = null);
public sealed record LabTransitionResult(string Id, int Revision, string State, string ETag, bool Replay);
public sealed class LabConflictException : Exception;

// No device adapter or network client is accepted by this service. The sole execution type hashes a synthetic plan.
public sealed class LabWorkflowService(CenterDbContext center, BusinessAuthorization authority,
    ISqlCommitGatePlanProvider gates, TimeProvider clock)
{
    public Task<List<LabPermit>> ListAsync(string digest, CancellationToken ct) =>
        ReadAsync(digest, async (_, lab, scope, _, token) => await lab.Permits.AsNoTracking()
            .Where(row => row.IsSynthetic == true && row.StationId != null && scope.Contains(row.StationId.Value))
            .OrderBy(row => row.PermitId).ThenBy(row => row.PermitVersion).Take(100).ToListAsync(token), ct);

    public Task<LabPermit?> DetailAsync(string digest, string id, int revision, CancellationToken ct) =>
        ReadAsync(digest, (_, lab, scope, _, token) => lab.Permits.AsNoTracking()
            .SingleOrDefaultAsync(row => row.PermitId == id && row.PermitVersion == revision &&
                row.IsSynthetic == true && row.StationId != null && scope.Contains(row.StationId.Value), token), ct);

    public Task<List<LabWorkflowHistory>> HistoryAsync(string digest, string id, int revision, CancellationToken ct) =>
        ReadAsync(digest, async (_, lab, scope, _, token) =>
        {
            if (!await lab.Permits.AnyAsync(row => row.PermitId == id && row.PermitVersion == revision &&
                row.IsSynthetic == true && row.StationId != null && scope.Contains(row.StationId.Value), token)) return [];
            return await lab.History.AsNoTracking().Where(row => row.PermitId == id && row.PermitVersion == revision)
                .OrderBy(row => row.RecordedUtc).ThenBy(row => row.Id).Take(100).ToListAsync(token);
        }, ct);

    public Task<LabTransitionResult> CreateAsync(string digest, Guid actor, Guid request, SyntheticLabDraft draft,
        string correlation, CancellationToken ct)
    {
        if (request == Guid.Empty || draft.Start.Offset != TimeSpan.Zero || draft.End.Offset != TimeSpan.Zero ||
            draft.Start >= draft.End || draft.End - draft.Start > TimeSpan.FromHours(24) ||
            string.IsNullOrWhiteSpace(draft.Plan) || draft.Plan.Length > 2048 || draft.Plan.Any(char.IsControl) ||
            draft.EvidenceSha256 is not { Length: > 0 and <= 16 } ||
            draft.EvidenceSha256.Any(value => value is null || value.Length != 64 || !value.All(char.IsAsciiHexDigit)))
            throw new ArgumentException("Bounded synthetic plan, evidence hashes and UTC window required.");
        var body = JsonSerializer.Serialize(draft);
        var digestValue = Hash(body);
        return MutateAsync(digest, actor, BusinessReadOperation.LabRequest, async (iam, lab, scope, snapshot, token) =>
        {
            DemandActor(actor, snapshot);
            if (!scope.Contains(draft.StationId) || !await center.Set<IedDevice>().AnyAsync(
                row => row.Id == draft.IedId && row.StationId == draft.StationId && row.IsSynthetic, token))
                throw new UnauthorizedAccessException();
            var replay = await ReplayAsync(lab, actor, request, digestValue, scope, token);
            if (replay is not null) return replay;
            var inventory = await center.Database.SqlQuery<int>($"""
                SELECT COUNT(*) AS [Value] FROM labprep.InventoryProposal
                WHERE InventoryId={draft.InventoryId}
                  AND StationCode IN(SELECT Code FROM catalog.Station WHERE Id={draft.StationId})
                """).SingleAsync(token);
            if (inventory != 1) throw new UnauthorizedAccessException();
            var row = new LabPermit { PermitId = request.ToString("N"), PermitVersion = 1, InventoryId = draft.InventoryId,
                ScopeDigest = SHA256.HashData(Encoding.UTF8.GetBytes(body)), StartsUtc = draft.Start.UtcDateTime,
                EndsUtc = draft.End.UtcDateTime, DraftJson = body, StationId = draft.StationId, IedId = draft.IedId,
                TestType = "SYNTHETIC_HASH", IsSynthetic = true, RequesterPerson = snapshot.User.PersonReference };
            lab.Add(row);
            return await SaveAsync(iam, lab, row, snapshot, request, digestValue, "draft", correlation, token);
        }, ct, correlation);
    }

    public Task<byte[]?> ExportAsync(string digest, string id, int revision, string correlation, CancellationToken ct) =>
        authority.AuthorizedLabAsync<byte[]?>(digest, BusinessReadOperation.LabExport, async (iam, scope, snapshot, token) =>
        {
            await using var lab = await ContextAsync(token);
            var row = await lab.Permits.AsNoTracking().SingleOrDefaultAsync(item => item.PermitId == id &&
                item.PermitVersion == revision && item.IsSynthetic == true && item.StationId != null &&
                scope.Contains(item.StationId.Value), token);
            if (row is null) return null;
            var bytes = JsonSerializer.SerializeToUtf8Bytes(new { row.PermitId, row.PermitVersion, row.StationId, row.IedId,
                row.DraftStatus, row.DraftJson, row.PermitDigest, row.ResultDigest, IsSynthetic = true });
            var now = clock.GetUtcNow();
            var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = snapshot.User.Id,
                EventCode = "lab.synthetic.export.prepared", OutcomeCode = "prepared", CorrelationId = correlation,
                PolicyVersion = snapshot.CurrentVersion.Policy, RestoreEpoch = snapshot.CurrentVersion.RestoreEpoch };
            iam.AddRange(audit, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
            await iam.SaveChangesAsync(token);
            return bytes;
        }, ct);

    public Task<LabTransitionResult> TransitionAsync(string digest, Guid actor, string id, int revision,
        LabTransition transition, string correlation, CancellationToken ct)
    {
        if (transition.RequestId == Guid.Empty || revision < 1 || id.Length != 32 || !id.All(char.IsAsciiHexDigit))
            throw new ArgumentException("Invalid workflow identity.");
        var operation = transition.Action switch
        {
            "submit" => BusinessReadOperation.LabSubmit, "review" => BusinessReadOperation.LabReview,
            "approve" => BusinessReadOperation.LabApprove, "permit" => BusinessReadOperation.LabPermit,
            "ready" => BusinessReadOperation.LabReady, "apply" or "abort" => BusinessReadOperation.LabApply,
            _ => throw new ArgumentException("Unsupported transition.")
        };
        var requestDigest = Hash(JsonSerializer.Serialize(new { id, revision, transition.Action, transition.ETag, transition.PermitIed, transition.TestType }));
        return MutateAsync(digest, actor, operation, async (iam, lab, scope, snapshot, token) =>
        {
            DemandActor(actor, snapshot);
            var replay = await ReplayAsync(lab, actor, transition.RequestId, requestDigest, scope, token);
            if (replay is not null) return replay;
            var row = await lab.Permits.FromSqlInterpolated($"""
                SELECT * FROM labprep.PermitDraft WITH(UPDLOCK,HOLDLOCK) WHERE PermitId={id} AND PermitVersion={revision}
                """).SingleOrDefaultAsync(token);
            if (row?.IsSynthetic != true || row.StationId is null || !scope.Contains(row.StationId.Value))
                throw new UnauthorizedAccessException();
            if (transition.ETag != ETag(row.VersionStamp)) throw new LabConflictException();
            var now = clock.GetUtcNow().UtcDateTime;
            if (now < row.StartsUtc || now >= row.EndsUtc) throw new UnauthorizedAccessException("PERMIT_WINDOW");
            if (row.TestType != "SYNTHETIC_HASH" || !await center.Set<IedDevice>()
                .AnyAsync(ied => ied.Id == row.IedId && ied.StationId == row.StationId && ied.IsSynthetic, token))
                throw new UnauthorizedAccessException("IED_SCOPE");
            var person = snapshot.User.PersonReference;
            switch (transition.Action)
            {
                case "submit":
                    Require(row.DraftStatus == "DRAFT" && person == row.RequesterPerson);
                    row.DraftStatus = "SUBMITTED";
                    break;
                case "review":
                    Require(row.DraftStatus == "SUBMITTED" && person != row.RequesterPerson);
                    row.ReviewerPerson = person;
                    row.DraftStatus = "REVIEWED";
                    break;
                case "approve":
                    Require(row.DraftStatus == "REVIEWED" && person != row.RequesterPerson && person != row.ReviewerPerson);
                    row.ApproverPerson = person;
                    row.DraftStatus = "APPROVED";
                    break;
                case "permit":
                    Require(row.DraftStatus == "APPROVED" && person == row.ApproverPerson &&
                        transition.PermitIed == row.IedId && transition.TestType == row.TestType);
                    row.PermitDigest = Convert.ToHexStringLower(row.ScopeDigest);
                    break;
                case "ready":
                    Require(row.DraftStatus == "APPROVED" && row.PermitDigest == Convert.ToHexStringLower(row.ScopeDigest));
                    row.DraftStatus = "READY";
                    break;
                case "apply":
                    Require(row.DraftStatus == "READY" && row.PermitDigest == Convert.ToHexStringLower(row.ScopeDigest) &&
                        person != row.RequesterPerson && person != row.ReviewerPerson && person != row.ApproverPerson);
                    row.ApplierPerson = person;
                    row.DraftStatus = "SYNTHETIC_EXECUTION";
                    await SaveAsync(iam, lab, row, snapshot, Guid.NewGuid(), requestDigest, "synthetic.start", correlation, token);
                    row.ResultDigest = Hash(row.DraftJson);
                    row.DraftStatus = "COMPLETED";
                    break;
                case "abort":
                    Require(row.DraftStatus is not ("COMPLETED" or "ABORTED" or "FAILED"));
                    row.DraftStatus = "ABORTED";
                    break;
            }
            var result = await SaveAsync(iam, lab, row, snapshot, transition.RequestId, requestDigest, transition.Action, correlation, token);
            if (clock.GetUtcNow().UtcDateTime >= row.EndsUtc) throw new UnauthorizedAccessException("PERMIT_WINDOW");
            return result;
        }, ct, correlation);
    }

    private Task<T> ReadAsync<T>(string digest, Func<IamDbContext, LabWorkflowDbContext, Guid[], AuthorizationSnapshot,
        CancellationToken, Task<T>> action, CancellationToken ct) =>
        authority.AuthorizedLabAsync(digest, BusinessReadOperation.LabRead,
            async (iam, scope, snapshot, token) =>
            {
                await using var lab = await ContextAsync(token);
                return await action(iam, lab, scope, snapshot, token);
            }, ct);

    internal async Task<T> MutateAsync<T>(string digest, Guid actor, BusinessReadOperation operation,
        Func<IamDbContext, LabWorkflowDbContext, Guid[], AuthorizationSnapshot, CancellationToken, Task<T>> action,
        CancellationToken ct, string? denialCorrelation = null)
    {
        await using var gateContext = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
            .UseSqlServer(center.Database.GetDbConnection()).Options);
        try
        {
            return await new SqlAuthorityCommitBoundary(gateContext, gates).CommitAsync(actor, async token =>
            {
                await center.Database.UseTransactionAsync(gateContext.Database.CurrentTransaction!.GetDbTransaction(), token);
                try
                {
                    return await authority.AuthorizedLabAsync(digest, operation, async (iam, scope, snapshot, inner) =>
                    {
                        await using var lab = await ContextAsync(inner);
                        return await action(iam, lab, scope, snapshot, inner);
                    }, token);
                }
                finally { await center.Database.UseTransactionAsync(null, CancellationToken.None); }
            }, ct);
        }
        catch (UnauthorizedAccessException)
        {
            center.ChangeTracker.Clear();
            // A denied mutation has no effective business change. Record its outcome in a fresh gate transaction.
            await new SqlAuthorityCommitBoundary(gateContext, gates).CommitAsync(actor, async token =>
            {
                var version = await gateContext.Set<AuthorityVersionRow>().AsNoTracking().SingleAsync(token);
                var now = clock.GetUtcNow();
                var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = actor,
                    EventCode = "business.mutation.denied", OutcomeCode = operation.ToString(),
                    CorrelationId = denialCorrelation ?? Guid.NewGuid().ToString("N"), PolicyVersion = version.PolicyVersion, RestoreEpoch = version.RestoreEpoch };
                gateContext.AddRange(audit, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
                await gateContext.SaveChangesAsync(token);
                return true;
            }, ct);
            throw;
        }
        catch
        {
            center.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<LabWorkflowDbContext> ContextAsync(CancellationToken ct)
    {
        var lab = new LabWorkflowDbContext(new DbContextOptionsBuilder<LabWorkflowDbContext>()
            .UseSqlServer(center.Database.GetDbConnection()).Options);
        await lab.Database.UseTransactionAsync(center.Database.CurrentTransaction!.GetDbTransaction(), ct);
        return lab;
    }

    private async Task<LabTransitionResult> SaveAsync(IamDbContext iam, LabWorkflowDbContext lab, LabPermit row,
        AuthorizationSnapshot snapshot, Guid request, string digest, string action, string correlation, CancellationToken ct)
    {
        await lab.SaveChangesAsync(ct);
        var now = clock.GetUtcNow();
        var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = snapshot.User.Id,
            EventCode = "lab.synthetic." + action, OutcomeCode = row.DraftStatus, CorrelationId = correlation,
            PolicyVersion = snapshot.CurrentVersion.Policy, RestoreEpoch = snapshot.CurrentVersion.RestoreEpoch };
        iam.AddRange(audit, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
        lab.Add(new LabWorkflowHistory { Id = Guid.NewGuid(), PermitId = row.PermitId, PermitVersion = row.PermitVersion,
            ActorUserId = snapshot.User.Id, ActorPerson = snapshot.User.PersonReference, Action = action, State = row.DraftStatus,
            CorrelationId = correlation, RecordedUtc = now, RequestId = request, RequestDigest = digest, ReplyVersion = row.VersionStamp });
        await lab.SaveChangesAsync(ct);
        await iam.SaveChangesAsync(ct);
        return new(row.PermitId, row.PermitVersion, row.DraftStatus, ETag(row.VersionStamp), false);
    }

    private static async Task<LabTransitionResult?> ReplayAsync(LabWorkflowDbContext lab, Guid actor, Guid request,
        string digest, Guid[] scope, CancellationToken ct)
    {
        var history = await lab.History.AsNoTracking().SingleOrDefaultAsync(row => row.ActorUserId == actor && row.RequestId == request, ct);
        if (history is null) return null;
        if (history.RequestDigest != digest) throw new LabConflictException();
        if (!await lab.Permits.AnyAsync(row => row.PermitId == history.PermitId && row.PermitVersion == history.PermitVersion &&
            row.IsSynthetic == true && row.StationId != null && scope.Contains(row.StationId.Value), ct))
            throw new UnauthorizedAccessException();
        return new(history.PermitId, history.PermitVersion, history.State, ETag(history.ReplyVersion), true);
    }

    private static void DemandActor(Guid actor, AuthorizationSnapshot snapshot)
    {
        if (snapshot.User.Id != actor) throw new UnauthorizedAccessException();
    }
    private static void Require(bool condition) { if (!condition) throw new UnauthorizedAccessException("WORKFLOW_POLICY"); }
    private static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    public static string ETag(byte[] value) => "\"" + Convert.ToBase64String(value) + "\"";
}
