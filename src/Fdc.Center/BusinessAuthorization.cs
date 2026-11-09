using System.Data;
using Fdc.Center.Persistence;
using Fdc.Identity.Application;
using Fdc.Identity.Domain;
using Fdc.Identity.Infrastructure;
using Fdc.Identity.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fdc.Center;

public enum BusinessReadOperation { Dashboard, Station, Ied, Record, RecordDownload, Collector }

public interface IReviewedBusinessOperationPolicy
{
    // A reviewed release must bind the operation to a catalog entry and current authority versions.
    string DemandPermission(BusinessReadOperation operation, AuthorityVersion version);
}

public sealed class PendingBusinessOperationPolicy : IReviewedBusinessOperationPolicy
{
    public string DemandPermission(BusinessReadOperation operation, AuthorityVersion version) =>
        throw new OwnerDecisionRequiredException("D04: reviewed business operation and object mapping required.");
}

public sealed class BusinessObject
{
    public Guid Id { get; init; }
    public Guid StationId { get; init; }
    public required string Label { get; init; }
}
public sealed record BusinessDownload(string Name, Stream Content);
public sealed class BusinessFile { public Guid Id { get; init; } public required string Name { get; init; } public long SizeBytes { get; init; } }

public interface IReviewedBusinessFileSource
{
    // Read only from the reviewed private store. Enforce the cap before allocation and reject
    // traversal/symlinks. ObjectKey is a database reference, never an HTTP parameter.
    Task<Stream?> OpenVerifiedAsync(string objectKey, long size, string sha256, int cap, CancellationToken ct);
}

public sealed class PendingBusinessFileSource : IReviewedBusinessFileSource
{
    public Task<Stream?> OpenVerifiedAsync(string objectKey, long size, string sha256, int cap, CancellationToken ct) =>
        throw new OwnerDecisionRequiredException("D10/E03: approved private storage and download audit contract required.");
}

public sealed class BusinessAuthorization(CenterDbContext db, IReviewedBusinessOperationPolicy operations,
    IReviewedPermissionPolicy permissionPolicy, ITrustedResourceResolver resourceResolver, PermissionEvaluator evaluator,
    TimeProvider clock)
{
    public Task<int> DashboardAsync(string digest, CancellationToken ct) =>
        ReadAsync(digest, BusinessReadOperation.Dashboard, (iam, stations, _, token) =>
            iam.Set<StationRow>().CountAsync(row => stations.Contains(row.Id), token), ct);

    public Task<List<BusinessObject>> ListAsync(string digest, BusinessReadOperation operation, Guid? station, int offset, int limit,
        CancellationToken ct, string? search = null)
    {
        if (offset is < 0 or > 10000 || limit is < 1 or > 100) throw new ArgumentException("Invalid page.");
        if (search?.Length > 80 || search?.Any(char.IsControl) == true) throw new ArgumentException("Invalid search.");
        return ReadAsync(digest, operation, (iam, stations, _, token) =>
        {
            var rows = Objects(iam, operation, stations, station);
            if (!string.IsNullOrWhiteSpace(search)) rows = rows.Where(row => row.Label.Contains(search));
            return rows.OrderBy(row => row.Id).Skip(offset).Take(limit).ToListAsync(token);
        }, ct);
    }

    public Task<BusinessObject?> DetailAsync(string digest, BusinessReadOperation operation, Guid id, CancellationToken ct) =>
        ReadAsync(digest, operation, (iam, stations, _, token) =>
            Objects(iam, operation, stations, null).SingleOrDefaultAsync(row => row.Id == id, token), ct);

    public Task<List<BusinessFile>> DownloadableFilesAsync(string digest, Guid recordId, CancellationToken ct) =>
        ReadAsync(digest, BusinessReadOperation.RecordDownload, (_, stations, _, token) =>
            db.Set<RecordFile>().AsNoTracking().Where(row => row.RecordId == recordId && stations.Contains(row.StationId) &&
                row.ValidationStatus == "Verified" && row.SizeBytes > 0 && row.SizeBytes <= 16 * 1024 * 1024 &&
                (row.Extension == ".cfg" || row.Extension == ".dat" || row.Extension == ".hdr" || row.Extension == ".inf"))
                .OrderBy(row => row.Name).Take(100)
                .Select(row => new BusinessFile { Id = row.Id, Name = row.Name, SizeBytes = row.SizeBytes }).ToListAsync(token), ct);

    public async Task<BusinessDownload?> DownloadAsync(string digest, Guid fileId, IReviewedBusinessFileSource storage,
        string correlationId, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is not null) throw new InvalidOperationException("Download must own its audit transaction.");
        Stream? prepared = null;
        try
        {
            var result = await ReadAsync<BusinessDownload?>(digest, BusinessReadOperation.RecordDownload, async (iam, stations, snapshot, token) =>
            {
                const int cap = 16 * 1024 * 1024;
                var row = await (from item in db.Set<RecordFile>().AsNoTracking()
                                 join record in db.Set<DisturbanceRecord>().AsNoTracking()
                                     on new { Id = item.RecordId, item.StationId } equals new { record.Id, record.StationId }
                                 where item.Id == fileId && stations.Contains(item.StationId) && item.ValidationStatus == "Verified"
                                 select new { File = item, record.ManifestId }).SingleOrDefaultAsync(token);
                if (row is null) return null;
                var file = row.File;
                if (file.Extension is not (".cfg" or ".dat" or ".hdr" or ".inf") ||
                    file.SizeBytes is <= 0 or > cap || file.Name != Fdc.Core.Identity.FileName(file.Name) ||
                    Path.GetExtension(file.Name).ToLowerInvariant() != file.Extension ||
                    file.StorageObjectKey != $"records/{row.ManifestId}/{file.Name}") return null;

                prepared = await storage.OpenVerifiedAsync(file.StorageObjectKey, file.SizeBytes, file.Sha256, cap, token);
                if (prepared is null) return null;
                var now = clock.GetUtcNow();
                var audit = new SecurityEventRow { Id = Guid.NewGuid(), CreatedAt = now, ActorUserId = snapshot.User.Id,
                    EventCode = "business.record.download.prepared", OutcomeCode = "prepared", CorrelationId = correlationId,
                    PolicyVersion = snapshot.CurrentVersion.Policy, RestoreEpoch = snapshot.CurrentVersion.RestoreEpoch };
                iam.AddRange(audit, new AuditOutboxRow { Id = Guid.NewGuid(), CreatedAt = now, EventId = audit.Id });
                await iam.SaveChangesAsync(token);
                return new BusinessDownload(file.Name, prepared);
            }, ct);
            prepared = null; // Committed result now owns the verified temporary stream.
            return result;
        }
        finally { if (prepared is not null) await prepared.DisposeAsync(); }
    }

    private IQueryable<BusinessObject> Objects(IamDbContext iam, BusinessReadOperation operation, Guid[] stations, Guid? station)
    {
        // The station filter comes from SQL authority, intersected with the optional client filter.
        if (station is { } filter) stations = stations.Where(id => id == filter).ToArray();
        return operation switch
        {
            BusinessReadOperation.Station => iam.Set<StationRow>().AsNoTracking().Where(row => stations.Contains(row.Id))
                .Select(row => new BusinessObject { Id = row.Id, StationId = row.Id, Label = row.Code }),
            BusinessReadOperation.Ied => db.Set<IedDevice>().AsNoTracking().Where(row => stations.Contains(row.StationId))
                .Select(row => new BusinessObject { Id = row.Id, StationId = row.StationId, Label = row.DisplayName }),
            BusinessReadOperation.Record => db.Set<DisturbanceRecord>().AsNoTracking().Where(row => stations.Contains(row.StationId))
                .Select(row => new BusinessObject { Id = row.Id, StationId = row.StationId, Label = row.ManifestId }),
            BusinessReadOperation.Collector => db.Set<CollectorRegistration>().AsNoTracking().Where(row => stations.Contains(row.StationId))
                .Select(row => new BusinessObject { Id = row.Id, StationId = row.StationId, Label = row.Code }),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    private async Task<T> ReadAsync<T>(string digest, BusinessReadOperation operation,
        Func<IamDbContext, Guid[], AuthorizationSnapshot, CancellationToken, Task<T>> query, CancellationToken ct)
    {
        var existing = db.Database.CurrentTransaction;
        await using var owned = existing is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        var transaction = (existing ?? owned)!.GetDbTransaction();
        if (transaction.IsolationLevel != IsolationLevel.Serializable)
            throw new InvalidOperationException("Business reads require a serializable authority snapshot.");
        await using var iam = new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
            .UseSqlServer(db.Database.GetDbConnection()).Options);
        await iam.Database.UseTransactionAsync(transaction, ct);
        var current = await new SqlTransactionalAuthorityReader(permissionPolicy, resourceResolver, clock)
            .ReadAsync(iam, digest, null, ct);
        if (current is null) throw new UnauthorizedAccessException();
        var snapshot = current.Value.Snapshot;
        var code = operations.DemandPermission(operation, snapshot.CurrentVersion);
        if (!snapshot.Permissions.Any(permission => permission.Code == code && permission.Approved && permission.Implemented))
            throw new UnauthorizedAccessException();

        // Query only grant candidates. Ancestor-unit inheritance is deliberately excluded until reviewed.
        var stationIds = ScopeIds(snapshot, ScopeKind.Station);
        var unitIds = ScopeIds(snapshot, ScopeKind.ManagementUnit);
        var corporationIds = ScopeIds(snapshot, ScopeKind.Corporation);
        var candidates = await (from station in iam.Set<StationRow>().AsNoTracking()
                                join unit in iam.Set<ManagementUnitRow>().AsNoTracking() on station.ManagementUnitId equals unit.Id
                                where stationIds.Contains(station.Id) || unitIds.Contains(unit.Id) || corporationIds.Contains(unit.CorporationId)
                                select new { station.Id, Unit = unit.Id, unit.CorporationId, station.HierarchyVersion }).ToListAsync(ct);
        Guid[] ScopeAt(DateTimeOffset now) => candidates.Where(row => evaluator.Evaluate(snapshot, new AccessRequest(code, CredentialKind.HumanSession,
                new TrustedResource(row.Id.ToString("D"), row.CorporationId.ToString("D"), row.Unit.ToString("D"), row.HierarchyVersion, [])), now).Allowed)
            .Select(row => row.Id).ToArray();
        var scope = ScopeAt(clock.GetUtcNow());
        var result = await query(iam, scope, snapshot, ct);
        var completedAt = clock.GetUtcNow();
        if (!SessionValidity.IsCurrent(snapshot.User, snapshot.Session, snapshot.CurrentVersion, completedAt) ||
            !scope.ToHashSet().SetEquals(ScopeAt(completedAt)))
            throw new UnauthorizedAccessException();
        if (owned is not null) await owned.CommitAsync(ct);
        return result;
    }

    private static Guid[] ScopeIds(AuthorizationSnapshot snapshot, ScopeKind kind) =>
        snapshot.Grants.Where(grant => grant.Scope == kind && Guid.TryParseExact(grant.ScopeId, "D", out _))
            .Select(grant => Guid.Parse(grant.ScopeId)).Distinct().ToArray();
}
