using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Fdc.Collector.Pilot;

// Separate from Core.NativeIedSession: no Report/RCB, dataset, control, write or delete exports.
// API signatures verified against pinned libiec61850 6e7a603c / v1.6.2.1 headers.
public sealed class MmsReadOnlyAdapter : IIedFileAcquisitionAdapter
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();
    private readonly object sync = new();
    private readonly EvidenceKind evidenceKind;
    private readonly IedTarget target;
    private readonly LabReadProfile profile;
    private readonly ILabReadAuthorization authorization;
    private readonly TimeProvider clock;
    private readonly SemaphoreSlim gate;
    private readonly Native native;
    private IntPtr connection;
    private DateTimeOffset nextOperation;
    private bool disposed;
    public EvidenceKind EvidenceKind => evidenceKind;

    public MmsReadOnlyAdapter(string absoluteLibraryPath, string expectedLibrarySha256, IedTarget target,
        LabReadProfile profile, ILabReadAuthorization authorization, TimeProvider clock, EvidenceKind evidenceKind, CancellationToken ct)
    {
        this.target = target; this.profile = profile; this.authorization = authorization; this.clock = clock; this.evidenceKind = evidenceKind;
        if (!Enum.IsDefined(evidenceKind) || evidenceKind == EvidenceKind.OFFLINE_FIXTURE || (evidenceKind == EvidenceKind.SIMULATED) != System.Net.IPAddress.IsLoopback(target.Address))
            throw new InvalidOperationException("Native simulation must be loopback; it is never real IED evidence.");
        Demand(); // Before native load, semaphore acquisition and especially before any connection.
        if (!Path.IsPathFullyQualified(absoluteLibraryPath)) throw new ArgumentException("Absolute pinned native library required.");
        using (var stream = File.OpenRead(absoluteLibraryPath))
            if (!string.Equals(Convert.ToHexStringLower(SHA256.HashData(stream)), expectedLibrarySha256, StringComparison.Ordinal))
                throw new InvalidDataException("Native binary fingerprint mismatch.");
        gate = Gates.GetOrAdd(AcquisitionSafety.Endpoint(target), _ => new(1, 1));
        if (!gate.Wait(0, ct)) throw new InvalidOperationException("One active acquisition per IED endpoint is permitted.");
        try
        {
            native = new Native(absoluteLibraryPath);
            try
            {
                connection = native.Create();
                if (connection == IntPtr.Zero) throw new IOException("Native connection allocation failed.");
                native.SetConnectTimeout(connection, (uint)profile.ConnectTimeoutMs);
                native.SetRequestTimeout(connection, (uint)profile.RequestTimeoutMs);
                Demand(); ct.ThrowIfCancellationRequested();
                native.Connect(connection, out var error, target.Address.ToString(), target.Port);
                Check(error); ct.ThrowIfCancellationRequested();
            }
            catch { if (connection != IntPtr.Zero) native.Destroy(connection); native.Dispose(); throw; }
        }
        catch { gate.Release(); throw; }
    }
    private void Demand()
    {
        var now = clock.GetUtcNow(); AcquisitionSafety.Validate(target, profile, now);
        authorization.Demand(target, profile, now);
    }
    private void BeforeOperation(CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var delay = nextOperation - clock.GetUtcNow();
        if (delay > TimeSpan.Zero && clock.GetUtcNow() + delay >= profile.Until) throw new InvalidOperationException("Read window would expire during rate limiting.");
        if (delay > TimeSpan.Zero && ct.WaitHandle.WaitOne(delay)) ct.ThrowIfCancellationRequested();
        ct.ThrowIfCancellationRequested(); Demand();
        nextOperation = clock.GetUtcNow() + profile.MinimumOperationInterval;
    }
    public CapabilityObservation ProbeCapabilities(CancellationToken ct)
    {
        try
        {
            _ = ListFiles(ct);
            return new(CapabilityState.SUCCESS, CapabilityState.SUCCESS, CapabilityState.SUCCESS,
                CapabilityState.NOT_TESTED, CapabilityState.NOT_TESTED, evidenceKind, clock.GetUtcNow(),
                "MMS association and directory response only; no file download, edition or COMTRADE confirmed.");
        }
        catch (IedFileServiceException e)
        {
            return new(CapabilityState.SUCCESS, e.State, e.State, CapabilityState.NOT_TESTED,
                CapabilityState.NOT_TESTED, evidenceKind, clock.GetUtcNow(), e.Message);
        }
    }
    public IReadOnlyList<IedFileMetadata> ListFiles(CancellationToken ct) { lock (sync) return ListCore(ct); }
    private IReadOnlyList<IedFileMetadata> ListCore(CancellationToken ct)
    {
        var files = new List<IedFileMetadata>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        string? cursor = null;
        bool more;
        do
        {
            BeforeOperation(ct);
            var list = native.Directory(connection, out var error, target.Directory, cursor, out more);
            var last = cursor;
            try
            {
                Check(error);
                for (var node = list == IntPtr.Zero ? IntPtr.Zero : native.Next(list); node != IntPtr.Zero; node = native.Next(node))
                {
                    ct.ThrowIfCancellationRequested();
                    if (names.Count >= profile.MaximumEntries) throw new InvalidDataException("Directory bound exceeded.");
                    var entry = native.Data(node);
                    var name = Marshal.PtrToStringUTF8(native.Name(entry)) ?? throw new InvalidDataException("Missing remote name.");
                    AcquisitionSafety.ValidateRemotePath(name, name.EndsWith('/'));
                    if (!names.Add(name)) throw new InvalidDataException("Non-progressing or duplicate directory entry.");
                    last = name;
                    if (name.EndsWith('/')) continue;
                    var path = name.Contains('/') ? name : target.Directory.TrimEnd('/') + "/" + name;
                    var prefix = target.Directory.TrimEnd('/') + "/";
                    if (!path.StartsWith(prefix, StringComparison.Ordinal)) throw new InvalidDataException("File outside approved directory.");
                    var modified = native.Modified(entry);
                    files.Add(new(path, native.Size(entry), modified == 0 ? null : DateTimeOffset.FromUnixTimeMilliseconds(checked((long)modified))));
                }
            }
            finally { if (list != IntPtr.Zero) native.DestroyList(list, native.DestroyEntry); }
            if (more && (last is null || last == cursor)) throw new InvalidDataException("Empty/non-progressing MMS continuation.");
            cursor = last;
        } while (more);
        return files;
    }
    public IedFileMetadata GetFileMetadata(string remotePath, CancellationToken ct) =>
        ListFiles(ct).SingleOrDefault(x => x.RemotePath == remotePath) ?? throw new FileNotFoundException("File not in approved directory response.");
    public void DownloadFile(string remotePath, Stream destination, CancellationToken ct, IProgress<long>? progress = null)
    { lock (sync) DownloadCore(remotePath, destination, ct, progress); }
    private void DownloadCore(string remotePath, Stream destination, CancellationToken ct, IProgress<long>? progress)
    {
        AcquisitionSafety.ValidateRemotePath(remotePath);
        var metadata = GetFileMetadata(remotePath, ct); // No arbitrary client path outside listing.
        if (metadata.Size < 0 || metadata.Size > profile.MaximumFileBytes) throw new InvalidDataException("File exceeds approved bound.");
        BeforeOperation(ct);
        long total = 0; Exception? failure = null; var timer = Stopwatch.StartNew();
        Native.Handler handler = (_, buffer, count) =>
        {
            try
            {
                ct.ThrowIfCancellationRequested(); Demand();
                if (count > 1024 * 1024 || count > profile.MaximumFileBytes - total) throw new InvalidDataException("Native transfer bound exceeded.");
                var bytes = new byte[(int)count]; Marshal.Copy(buffer, bytes, 0, bytes.Length);
                destination.Write(bytes); total += count; progress?.Report(total);
                var delay = TimeSpan.FromSeconds((double)total / profile.BytesPerSecond) - timer.Elapsed;
                if (delay > TimeSpan.Zero && clock.GetUtcNow() + delay >= profile.Until) throw new InvalidOperationException("Read window would expire during download throttle.");
                if (delay > TimeSpan.Zero && ct.WaitHandle.WaitOne(delay)) ct.ThrowIfCancellationRequested();
                return true;
            }
            catch (Exception e) { failure = e; return false; } // Never unwind managed exception through C.
        };
        var received = native.GetFile(connection, out var error, remotePath, handler, IntPtr.Zero);
        GC.KeepAlive(handler);
        if (failure is OperationCanceledException cancelled) throw cancelled;
        if (failure is not null) throw new IOException("Transfer interrupted; local partial must not be promoted.", failure);
        Check(error);
        if (received != total || total != metadata.Size) throw new InvalidDataException("Partial/changed file size.");
        var after = GetFileMetadata(remotePath, ct);
        if (after.Size != metadata.Size || after.Modified != metadata.Modified) throw new InvalidDataException("Source changed during read.");
    }
    public string GetVendorDiagnostics() => "Common MMS file adapter; vendor/model/firmware are inventory claims until independently verified; no protocol fallback.";
    private static void Check(int error) { if (error != 0) throw new IedFileServiceException(error); }
    public void Dispose() { lock (sync) DisposeCore(); }
    private void DisposeCore()
    {
        if (disposed) return;
        disposed = true;
        try { native.Destroy(connection); connection = IntPtr.Zero; }
        finally { native.Dispose(); gate.Release(); }
    }

    private sealed class Native : IDisposable
    {
        private readonly IntPtr library;
        public Native(string path)
        {
            library = NativeLibrary.Load(path);
            try
            {
                Create = Export<CreateFn>("IedConnection_create"); Destroy = Export<PointerAction>("IedConnection_destroy");
                Connect = Export<ConnectFn>("IedConnection_connect");
                SetConnectTimeout = Export<TimeoutFn>("IedConnection_setConnectTimeout"); SetRequestTimeout = Export<TimeoutFn>("IedConnection_setRequestTimeout");
                Directory = Export<DirectoryFn>("IedConnection_getFileDirectoryEx"); GetFile = Export<FileFn>("IedConnection_getFile");
                Next = Export<PointerFn>("LinkedList_getNext"); Data = Export<PointerFn>("LinkedList_getData");
                Name = Export<PointerFn>("FileDirectoryEntry_getFileName"); Size = Export<SizeFn>("FileDirectoryEntry_getFileSize");
                Modified = Export<ModifiedFn>("FileDirectoryEntry_getLastModified");
                DestroyEntry = Export<PointerAction>("FileDirectoryEntry_destroy"); DestroyList = Export<ListDestroyFn>("LinkedList_destroyDeep");
            }
            catch { NativeLibrary.Free(library); throw; }
        }
        private T Export<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
        public readonly CreateFn Create; public readonly PointerAction Destroy, DestroyEntry;
        public readonly ConnectFn Connect; public readonly TimeoutFn SetConnectTimeout, SetRequestTimeout;
        public readonly DirectoryFn Directory; public readonly FileFn GetFile;
        public readonly PointerFn Next, Data, Name; public readonly SizeFn Size; public readonly ModifiedFn Modified; public readonly ListDestroyFn DestroyList;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr CreateFn();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PointerAction(IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr PointerFn(IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint SizeFn(IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate ulong ModifiedFn(IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void TimeoutFn(IntPtr self, uint timeout);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ConnectFn(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string host, int port);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr DirectoryFn(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string directory, [MarshalAs(UnmanagedType.LPUTF8Str)] string? cursor, [MarshalAs(UnmanagedType.I1)] out bool more);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] public delegate bool Handler(IntPtr parameter, IntPtr buffer, uint count);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint FileFn(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, Handler handler, IntPtr parameter);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ListDestroyFn(IntPtr list, PointerAction destroy);
        public void Dispose() => NativeLibrary.Free(library);
    }
}
