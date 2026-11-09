using System.Collections.Immutable;

namespace Fdc.Collector.Pilot;

public sealed record InventoryTestScope(string Batch, int Row, string Supervisor, string StopProcedure,
    string TestPlan, LabReadProfile ReadProfile, ImmutableHashSet<string> ApprovedPaths, int MaximumAttempts);

// The host supplies an independently trusted Lab-owner authorizer. The default implementation
// remains PendingLabReadAuthorization. Import and manual classification never provide that trust.
public interface IInventoryScopeAuthority
{
    void Demand(IedTarget target, InventoryTestScope scope, bool download, DateTimeOffset now);
}

public sealed class PendingInventoryScopeAuthority : IInventoryScopeAuthority
{
    public void Demand(IedTarget target, InventoryTestScope scope, bool download, DateTimeOffset now) =>
        throw new UnauthorizedAccessException("Independent approval of exact inventory and file scope required.");
}

public sealed class InventoryTestGate(InventoryStore store, IInventoryScopeAuthority authority, TimeProvider clock)
{
    public void Demand(InventoryPreview preview, IedTarget target, InventoryTestScope scope, bool download)
    {
        var device = preview.Devices.SingleOrDefault(d => d.Row == scope.Row);
        if (preview.Id != scope.Batch || device is null || device.Address != target.Address.ToString() || device.Station != target.Station ||
            new[] { scope.Supervisor, scope.StopProcedure, scope.TestPlan }.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Inventory row and complete operational scope required.");
        var review = store.LatestReview(scope.Batch, scope.Row);
        if (review is null || review.DeviceType != LabDeviceType.PROTECTION_RELAY || review.IedName != target.IedId ||
            !review.Iec61850 || (download && (!review.FileService || scope.ApprovedPaths is null || scope.ApprovedPaths.Count == 0)))
            throw new InvalidOperationException("Manual relay and capability review required.");
        var now = clock.GetUtcNow();
        AcquisitionSafety.Validate(target, scope.ReadProfile, now);
        if (download)
            foreach (var path in scope.ApprovedPaths)
            {
                AcquisitionSafety.ValidateRemotePath(path);
                if (!path.StartsWith(target.Directory.TrimEnd('/') + "/", StringComparison.Ordinal))
                    throw new InvalidOperationException("File outside approved directory.");
            }
        if (scope.MaximumAttempts is < 1 or > 10) throw new InvalidOperationException("Explicit retry limit required.");
        authority.Demand(target, scope, download, now);
    }

    public CapabilityObservation Probe(InventoryPreview preview, IedTarget target, InventoryTestScope scope,
        Func<IIedFileAcquisitionAdapter> open, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Demand(preview, target, scope, false);
        using var adapter = open();
        return adapter.ProbeCapabilities(ct);
    }

    public IReadOnlyList<PilotCollectedRecord> Collect(InventoryPreview preview, IedTarget target, InventoryTestScope scope,
        Func<IIedFileAcquisitionAdapter> open, PilotAcquisition acquisition, int attempts, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (attempts != scope.MaximumAttempts) throw new InvalidOperationException("Retry limit differs from approved scope.");
        Demand(preview, target, scope, true);
        using var adapter = new ScopedAdapter(open(), scope.ApprovedPaths.WithComparer(StringComparer.Ordinal), () => Demand(preview, target, scope, true));
        return acquisition.Collect(adapter, target, attempts, ct);
    }

    private sealed class ScopedAdapter(IIedFileAcquisitionAdapter inner, ImmutableHashSet<string> paths, Action demand) : IIedFileAcquisitionAdapter
    {
        public EvidenceKind EvidenceKind => inner.EvidenceKind;
        public CapabilityObservation ProbeCapabilities(CancellationToken ct)
        {
            demand();
            var observation = inner.ProbeCapabilities(ct);
            if (observation.MmsConnected != CapabilityState.SUCCESS || observation.FileService != CapabilityState.SUCCESS)
                throw new InvalidOperationException("Successful MMS association and File Services observation required before acquisition.");
            return observation;
        }
        public IReadOnlyList<IedFileMetadata> ListFiles(CancellationToken ct)
        { demand(); return inner.ListFiles(ct).Where(f => paths.Contains(f.RemotePath)).ToArray(); }
        public IedFileMetadata GetFileMetadata(string path, CancellationToken ct)
        { Check(path); return inner.GetFileMetadata(path, ct); }
        public void DownloadFile(string path, Stream destination, CancellationToken ct, IProgress<long>? progress = null)
        { Check(path); inner.DownloadFile(path, destination, ct, progress); }
        private void Check(string path)
        {
            demand();
            if (!paths.Contains(path)) throw new UnauthorizedAccessException("File outside approved test scope.");
        }
        public string GetVendorDiagnostics() => inner.GetVendorDiagnostics();
        public void Dispose() => inner.Dispose();
    }
}
