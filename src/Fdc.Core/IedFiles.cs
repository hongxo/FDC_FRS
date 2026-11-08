using System.Runtime.InteropServices;

namespace Fdc.Core;

public interface IIedSession : IDisposable
{
    IReadOnlyList<RemoteFile> List(string directory);
    void Download(string remotePath, string destination, long maxBytes, CancellationToken ct);
}
public interface IIedFactory { IIedSession Connect(Ied ied); }

public sealed class NativeIedFactory : IIedFactory, IDisposable
{
    private readonly Dictionary<string, NativeIedSession> sessions = [];
    private readonly object gate = new();
    public IIedSession Connect(Ied ied)
    {
        lock (gate)
        {
            if (sessions.TryGetValue(ied.Id, out var current) && !current.IsConnected) { current.Dispose(); sessions.Remove(ied.Id); }
            if (!sessions.TryGetValue(ied.Id, out current)) sessions[ied.Id] = current = new NativeIedSession(ied);
            return new Lease(current);
        }
    }
    public void Dispose() { lock (gate) { foreach (var session in sessions.Values) session.Dispose(); sessions.Clear(); } }
    private sealed class Lease(NativeIedSession session) : IIedSession, IReportSession
    {
        public IReadOnlyList<RemoteFile> List(string directory) => session.List(directory);
        public void Download(string path, string destination, long maxBytes, CancellationToken ct) => session.Download(path, destination, maxBytes, ct);
        public void Subscribe(ReportOptions options, Action<ReportPacket> callback) => session.Subscribe(options, callback);
        public void Dispose() { }
    }
    public static void VerifyLibrary()
    {
        var path = Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "iec61850.dll" : "libiec61850.so");
        var handle = NativeLibrary.Load(path);
        try
        {
            foreach (var symbol in new[] { "IedConnection_create", "IedConnection_connect", "IedConnection_destroy", "IedConnection_getFileDirectory", "IedConnection_getFile", "LinkedList_getNext", "LinkedList_destroyDeep", "FileDirectoryEntry_destroy", "IedConnection_getRCBValues", "IedConnection_installReportHandler", "ClientReport_getDataSetValues" })
                NativeLibrary.GetExport(handle, symbol);
        }
        finally { NativeLibrary.Free(handle); }
    }
}

// File reads and explicitly assigned RCB subscriptions. No control, setting or delete API.
public sealed partial class NativeIedSession : IIedSession, IReportSession
{
    private IntPtr connection;
    private readonly object operationGate = new();
    private readonly Ied ied;
    public bool IsConnected { get { lock (operationGate) return connection != IntPtr.Zero && Native.IedConnection_getState(connection) == 2; } }
    public NativeIedSession(Ied ied)
    {
        this.ied = ied;
        connection = Native.IedConnection_create();
        if (connection == IntPtr.Zero) throw new IOException("Không tạo được phiên libiec61850.");
        try
        {
            Native.IedConnection_setConnectTimeout(connection, 5000);
            Native.IedConnection_setRequestTimeout(connection, 15000);
            Native.IedConnection_connect(connection, out var error, ied.Host, ied.Port);
            Check(error);
        }
        catch { Dispose(); throw; }
    }
    public IReadOnlyList<RemoteFile> List(string directory)
    { lock (operationGate) return ListCore(directory); }
    private IReadOnlyList<RemoteFile> ListCore(string directory)
    {
        var list = Native.IedConnection_getFileDirectory(connection, out var error, directory);
        try
        {
            Check(error); var files = new List<RemoteFile>();
            if (list == IntPtr.Zero) return files;
            for (var node = Native.LinkedList_getNext(list); node != IntPtr.Zero; node = Native.LinkedList_getNext(node))
            {
                var entry = Native.LinkedList_getData(node);
                var name = Marshal.PtrToStringUTF8(Native.FileDirectoryEntry_getFileName(entry)) ?? "";
                if (name.EndsWith('/') || name.EndsWith('\\')) continue;
                // Vendors return absolute paths, directory-qualified paths or basenames.
                var normalized = name.Replace('\\', '/');
                var prefix = directory.Replace('\\', '/').TrimEnd('/') + "/";
                var path = normalized.Contains('/') ? normalized : prefix + normalized;
                var modified = Native.FileDirectoryEntry_getLastModified(entry);
                files.Add(new(path, Native.FileDirectoryEntry_getFileSize(entry),
                    modified == 0 ? null : DateTimeOffset.FromUnixTimeMilliseconds(checked((long)modified))));
            }
            return files;
        }
        finally { if (list != IntPtr.Zero) Native.LinkedList_destroyDeep(list, Native.FileDirectoryEntry_destroy); }
    }
    public void Download(string remotePath, string destination, long maxBytes, CancellationToken ct)
    { lock (operationGate) DownloadCore(remotePath, destination, maxBytes, ct); }
    private void DownloadCore(string remotePath, string destination, long maxBytes, CancellationToken ct)
    {
        using var output = File.Create(destination); long total = 0; Exception? failure = null;
        Native.FileHandler handler = (_, buffer, count) =>
        {
            try
            {
                ct.ThrowIfCancellationRequested(); total += count;
                if (total > maxBytes) throw new InvalidDataException("File vượt giới hạn dung lượng.");
                var bytes = new byte[checked((int)count)]; Marshal.Copy(buffer, bytes, 0, bytes.Length); output.Write(bytes);
                return true;
            }
            catch (Exception ex) { failure = ex; return false; }
        };
        Native.IedConnection_getFile(connection, out var error, remotePath, handler, IntPtr.Zero);
        GC.KeepAlive(handler);
        if (failure != null) throw new IOException("Tải file bị ngắt.", failure);
        Check(error); output.Flush(true);
    }
    private static void Check(int error) { if (error != 0) throw new IOException($"MMS error {error}"); }
    public void Dispose()
    {
        lock (operationGate)
        {
        if (connection == IntPtr.Zero) return;
        StopReports();
        Native.IedConnection_destroy(connection); connection = IntPtr.Zero;
        }
    }
    private static partial class Native
    {
        private const string Lib = "iec61850";
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal delegate bool FileHandler(IntPtr parameter, IntPtr buffer, uint bytesRead);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void DeleteHandler(IntPtr entry);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr IedConnection_create();
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int IedConnection_getState(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void IedConnection_destroy(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void IedConnection_setConnectTimeout(IntPtr self, uint timeout);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void IedConnection_setRequestTimeout(IntPtr self, uint timeout);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void IedConnection_connect(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string host, int port);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr IedConnection_getFileDirectory(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string directory);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern uint IedConnection_getFile(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, FileHandler handler, IntPtr parameter);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr LinkedList_getNext(IntPtr list);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr LinkedList_getData(IntPtr list);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void LinkedList_destroyDeep(IntPtr list, DeleteHandler handler);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr FileDirectoryEntry_getFileName(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern uint FileDirectoryEntry_getFileSize(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern ulong FileDirectoryEntry_getLastModified(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void FileDirectoryEntry_destroy(IntPtr self);
    }
}

public sealed class DemoIedFactory(string root) : IIedFactory
{
    public IIedSession Connect(Ied ied) => new DemoIedSession(Path.Combine(root, ied.Id));
    private sealed class DemoIedSession(string directory) : IIedSession
    {
        public IReadOnlyList<RemoteFile> List(string _) => Directory.Exists(directory)
            ? Directory.GetFiles(directory).Select(p => new RemoteFile("/COMTRADE/" + Path.GetFileName(p), new FileInfo(p).Length, File.GetLastWriteTimeUtc(p))).ToArray()
            : [];
        public void Download(string remotePath, string destination, long maxBytes, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested(); var source = Path.Combine(directory, Identity.FileName(remotePath));
            if (new FileInfo(source).Length > maxBytes) throw new InvalidDataException("File vượt giới hạn.");
            File.Copy(source, destination, true);
        }
        public void Dispose() { }
    }
}
