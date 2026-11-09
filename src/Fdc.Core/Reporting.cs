using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace Fdc.Core;

public sealed record ReportPacket(string IedId, string RcbReference, bool Trigger, int SettleSeconds, IReadOnlyList<SoeEvent> Events);
public interface IReportSession { void Subscribe(ReportOptions options, Action<ReportPacket> callback); }
public sealed class ReportInbox
{
    private readonly Channel<ReportPacket> channel = Channel.CreateBounded<ReportPacket>(new BoundedChannelOptions(10000) { FullMode = BoundedChannelFullMode.Wait });
    private long dropped;
    public long Dropped => Interlocked.Read(ref dropped);
    public void Publish(ReportPacket packet) { if (!channel.Writer.TryWrite(packet)) Interlocked.Increment(ref dropped); }
    public IAsyncEnumerable<ReportPacket> ReadAllAsync(CancellationToken ct) => channel.Reader.ReadAllAsync(ct);
}

public sealed partial class NativeIedSession
{
    private sealed record Subscription(ReportOptions Options, IntPtr Rcb, Native.ReportCallback Callback);
    private readonly Dictionary<string, Subscription> subscriptions = [];
    public void Subscribe(ReportOptions options, Action<ReportPacket> callback)
    {
        lock (operationGate)
        {
            if (subscriptions.ContainsKey(options.RcbReference)) return;
            var rcb = Native.IedConnection_getRCBValues(connection, out var error, options.RcbReference, IntPtr.Zero);
            Check(error);
            var installed = false;
            try
            {
                if (Native.ClientReportControlBlock_getRptEna(rcb) || (!Native.ClientReportControlBlock_isBuffered(rcb) && Native.ClientReportControlBlock_getResv(rcb)))
                    throw new IOException("RCB đang được sử dụng; không chiếm RCB của client khác.");
                var dataset = Text(Native.ClientReportControlBlock_getDataSetReference(rcb)).Replace('$', '.');
                var directory = Native.IedConnection_getDataSetDirectory(connection, out error, dataset, IntPtr.Zero);
                var references = new List<string>();
                try
                {
                    Check(error);
                    if (directory != IntPtr.Zero)
                        for (var node = Native.LinkedList_getNext(directory); node != IntPtr.Zero; node = Native.LinkedList_getNext(node)) references.Add(Text(Native.LinkedList_getData(node)));
                }
                finally { if (directory != IntPtr.Zero) Native.LinkedList_destroy(directory); }
                Native.ReportCallback handler = (_, report) =>
                {
                    // Copy all native data inside the callback; never let a managed exception cross C ABI.
                    try { callback(CopyReport(options, references, report)); }
                    catch (Exception ex)
                    {
                        try
                        {
                            var now = DateTimeOffset.UtcNow;
                            callback(new(ied.Id, options.RcbReference, false, options.SettleSeconds,
                                [new(Guid.NewGuid().ToString("N"), "", ied.Id, options.RcbReference, "$decode-error", ex.Message,
                                    "IEC61850.Report.DecodeError", now, null, "FDC receive time only", null, null, 0, null, null, null, true)]));
                        }
                        catch { /* The worker also exposes queue overflow counters in telemetry. */ }
                    }
                };
                Native.IedConnection_installReportHandler(connection, options.RcbReference, Text(Native.ClientReportControlBlock_getRptId(rcb)), handler, IntPtr.Zero);
                installed = true;
                uint mask = 2 | 256 | 32;
                Native.ClientReportControlBlock_setTrgOps(rcb, 1 | 2 | 16);
                Native.ClientReportControlBlock_setOptFlds(rcb, Native.ClientReportControlBlock_isBuffered(rcb) ? 255 : 159);
                if (Native.ClientReportControlBlock_isBuffered(rcb) && options.ResumeEntryId is { Length: > 0 } cursor)
                {
                    var bytes = Convert.FromHexString(cursor); var value = Native.MmsValue_newOctetString(bytes.Length, bytes.Length);
                    try { Native.MmsValue_setOctetString(value, bytes, bytes.Length); Native.ClientReportControlBlock_setEntryId(rcb, value); mask |= 4096; }
                    finally { Native.MmsValue_delete(value); }
                }
                if (!Native.ClientReportControlBlock_isBuffered(rcb)) { Native.ClientReportControlBlock_setResv(rcb, true); mask |= 4; }
                Native.ClientReportControlBlock_setRptEna(rcb, true);
                Native.IedConnection_setRCBValues(connection, out error, rcb, mask, true); Check(error);
                subscriptions.Add(options.RcbReference, new(options, rcb, handler));
            }
            catch
            {
                if (installed && Native.IedConnection_getState(connection) == 2)
                {
                    Native.ClientReportControlBlock_setRptEna(rcb, false);
                    Native.ClientReportControlBlock_setResv(rcb, false);
                    Native.IedConnection_setRCBValues(connection, out _, rcb, Native.ClientReportControlBlock_isBuffered(rcb) ? 2u : 6u, true);
                }
                if (installed) Native.IedConnection_uninstallReportHandler(connection, options.RcbReference);
                Native.ClientReportControlBlock_destroy(rcb); throw;
            }
        }
    }
    private ReportPacket CopyReport(ReportOptions options, List<string> references, IntPtr report)
    {
        var received = DateTimeOffset.UtcNow;
        var reportTime = Native.ClientReport_hasTimestamp(report) ? DateTimeOffset.FromUnixTimeMilliseconds(checked((long)Native.ClientReport_getTimestamp(report))) : (DateTimeOffset?)null;
        var seq = Native.ClientReport_hasSeqNum(report) ? Native.ClientReport_getSeqNum(report) : (ushort?)null;
        var conf = Native.ClientReport_hasConfRev(report) ? Native.ClientReport_getConfRev(report) : (uint?)null;
        var overflow = Native.ClientReport_hasBufOvfl(report) && Native.ClientReport_getBufOvfl(report);
        var subSequence = Native.ClientReport_hasSubSeqNum(report) ? Native.ClientReport_getSubSeqNum(report) : (ushort?)null;
        var moreSegments = subSequence != null && Native.ClientReport_getMoreSeqmentsFollow(report);
        var entry = Native.ClientReport_getEntryId(report); string? entryId = null;
        if (entry != IntPtr.Zero)
        {
            var bytes = new byte[Native.MmsValue_getOctetStringSize(entry)];
            Marshal.Copy(Native.MmsValue_getOctetStringBuffer(entry), bytes, 0, bytes.Length); entryId = Convert.ToHexStringLower(bytes);
        }
        var values = Native.ClientReport_getDataSetValues(report); var events = new List<SoeEvent>(); var trigger = false;
        var count = values == IntPtr.Zero ? 0 : Native.MmsValue_getArraySize(values);
        var reportIdentity = entryId ?? $"{seq}|{reportTime:O}|{received:O}";
        for (var index = 0; index < count; index++)
        {
            var reason = Native.ClientReport_getReasonForInclusion(report, index); if (reason == 0) continue;
            var reference = Text(Native.ClientReport_getDataReference(report, index));
            if (reference.Length == 0) reference = index < references.Count ? references[index] : $"$index:{index}";
            var value = Native.MmsValue_getElement(values, index); if (value == IntPtr.Zero) continue;
            var buffer = new StringBuilder(8192); Native.MmsValue_printToBuffer(value, buffer, buffer.Capacity);
            var (timestamp, timeQuality, quality) = GetTimestamp(value, 0);
            var source = timestamp != null ? "IED data timestamp" : reportTime != null ? "IED report timestamp" : "FDC receive time only";
            var text = buffer.ToString();
            var id = Identity.Hash($"{ied.Id}|{options.RcbReference}|{reportIdentity}|{subSequence}|{index}|{reference}|{text}");
            events.Add(new(id, "", ied.Id, options.RcbReference, reference, text, "IEC61850.Report", received,
                timestamp ?? reportTime, source, timeQuality, quality, reason, entryId, seq, conf, overflow, subSequence, moreSegments));
            var normalized = reference.Replace('$', '.');
            var isTrigger = options.TriggerReferences?.Any(x => normalized.Contains(x, StringComparison.Ordinal))
                ?? (normalized.Contains(".RcdMade", StringComparison.Ordinal) || normalized.Contains(".FltNum", StringComparison.Ordinal));
            // GI/integrity snapshots do not denote new disturbances. Data/quality changes only.
            if (isTrigger && (reason & 3) != 0) trigger = true;
        }
        if (overflow)
            events.Add(new(Identity.Hash($"{ied.Id}|{options.RcbReference}|{reportIdentity}|gap"), "", ied.Id, options.RcbReference,
                "$buffer-overflow", "IED báo mất dữ liệu trong bộ đệm report", "IEC61850.Report.Gap", received, reportTime,
                reportTime != null ? "IED report timestamp" : "FDC receive time only", null, null, 0, entryId, seq, conf, true));
        return new(ied.Id, options.RcbReference, trigger, options.SettleSeconds, events);
    }
    private static (DateTimeOffset?, byte?, string?) GetTimestamp(IntPtr value, int depth)
    {
        if (depth > 8) return (null, null, null);
        var type = Native.MmsValue_getType(value);
        if (type == 14) return (DateTimeOffset.FromUnixTimeMilliseconds(checked((long)Native.MmsValue_getUtcTimeInMs(value))), Native.MmsValue_getUtcTimeQuality(value), null);
        DateTimeOffset? time = null; byte? tq = null; string? quality = null;
        if (type is 0 or 1)
            for (var i = 0; i < Native.MmsValue_getArraySize(value); i++)
            {
                var child = Native.MmsValue_getElement(value, i); if (child == IntPtr.Zero) continue;
                var found = GetTimestamp(child, depth + 1);
                if (found.Item1 != null) { time = found.Item1; tq = found.Item2; }
                // Preserve a 13-bit IEC quality field when supplied with a structured value.
                if (Native.MmsValue_getType(child) == 3 && Native.MmsValue_getBitStringSize(child) == 13)
                { var buffer = new StringBuilder(128); Native.MmsValue_printToBuffer(child, buffer, buffer.Capacity); quality = buffer.ToString(); }
            }
        return (time, tq, quality);
    }
    private static string Text(IntPtr value) => value == IntPtr.Zero ? "" : Marshal.PtrToStringUTF8(value) ?? "";
    private void StopReports()
    {
        foreach (var subscription in subscriptions.Values)
        {
            if (Native.IedConnection_getState(connection) == 2)
            {
                Native.ClientReportControlBlock_setRptEna(subscription.Rcb, false);
                Native.IedConnection_setRCBValues(connection, out _, subscription.Rcb, 2, true);
            }
            Native.IedConnection_uninstallReportHandler(connection, subscription.Options.RcbReference);
            Native.ClientReportControlBlock_destroy(subscription.Rcb);
            GC.KeepAlive(subscription.Callback);
        }
        subscriptions.Clear();
    }
    private static partial class Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void ReportCallback(IntPtr parameter, IntPtr report);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr IedConnection_getRCBValues(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string reference, IntPtr update);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr IedConnection_getDataSetDirectory(IntPtr self, out int error, [MarshalAs(UnmanagedType.LPUTF8Str)] string reference, IntPtr deletable);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void LinkedList_destroy(IntPtr list);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void ClientReportControlBlock_destroy(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr ClientReportControlBlock_getRptId(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr ClientReportControlBlock_getDataSetReference(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReportControlBlock_getRptEna(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReportControlBlock_getResv(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReportControlBlock_isBuffered(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void ClientReportControlBlock_setResv(IntPtr self, [MarshalAs(UnmanagedType.I1)] bool enabled);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void ClientReportControlBlock_setRptEna(IntPtr self, [MarshalAs(UnmanagedType.I1)] bool enabled);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void ClientReportControlBlock_setTrgOps(IntPtr self, int options);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void ClientReportControlBlock_setOptFlds(IntPtr self, int options);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void ClientReportControlBlock_setEntryId(IntPtr self, IntPtr value);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr MmsValue_newOctetString(int size, int maxSize);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void MmsValue_setOctetString(IntPtr self, byte[] value, int size);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void MmsValue_delete(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void IedConnection_setRCBValues(IntPtr self, out int error, IntPtr rcb, uint mask, [MarshalAs(UnmanagedType.I1)] bool singleRequest);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void IedConnection_installReportHandler(IntPtr self, [MarshalAs(UnmanagedType.LPUTF8Str)] string reference, [MarshalAs(UnmanagedType.LPUTF8Str)] string rptId, ReportCallback callback, IntPtr parameter);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void IedConnection_uninstallReportHandler(IntPtr self, [MarshalAs(UnmanagedType.LPUTF8Str)] string reference);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr ClientReport_getDataSetValues(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr ClientReport_getDataReference(IntPtr self, int index);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int ClientReport_getReasonForInclusion(IntPtr self, int index);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr ClientReport_getEntryId(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern ulong ClientReport_getTimestamp(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern ushort ClientReport_getSeqNum(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern uint ClientReport_getConfRev(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReport_hasTimestamp(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReport_hasSeqNum(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReport_hasConfRev(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReport_hasBufOvfl(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReport_getBufOvfl(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReport_hasSubSeqNum(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern ushort ClientReport_getSubSeqNum(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] internal static extern bool ClientReport_getMoreSeqmentsFollow(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int MmsValue_getArraySize(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr MmsValue_getElement(IntPtr self, int index);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int MmsValue_getType(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int MmsValue_getBitStringSize(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern ulong MmsValue_getUtcTimeInMs(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern byte MmsValue_getUtcTimeQuality(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int MmsValue_getOctetStringSize(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr MmsValue_getOctetStringBuffer(IntPtr self);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr MmsValue_printToBuffer(IntPtr self, StringBuilder buffer, int size);
    }
}
