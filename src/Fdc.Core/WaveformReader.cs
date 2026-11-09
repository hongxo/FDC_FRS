using System.Globalization;
using System.IO.Compression;

namespace Fdc.Core;

public sealed record WavePoint(double TimeMs, double Value);
public sealed record AnalogTrace(string Name, string Phase, string Unit, string Scaling,
    double? Rms, double? Peak, double? Minimum, double? Maximum, long ValidSamples, IReadOnlyList<WavePoint> Points);
public sealed record DigitalTrace(string Name, int Transitions, bool Truncated, IReadOnlyList<WavePoint> Points);
public sealed record WaveformView(string Station, string Device, string Format, double NominalFrequency,
    long Samples, double DurationMs, double? TriggerMs, string StartTimeRaw, string TriggerTimeRaw,
    double WindowStartMs, double WindowEndMs, IReadOnlyList<AnalogTrace> Analog, IReadOnlyList<DigitalTrace> Digital);

public static class WaveformReader
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    public static WaveformView Read(string archive, int maxPoints = 2000, double? fromMs = null, double? toMs = null)
    {
        maxPoints = Math.Clamp(maxPoints, 100, 10000);
        using var zip = ZipFile.OpenRead(archive);
        var cfgEntry = zip.Entries.Single(x => x.Name.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase));
        var datEntry = zip.Entries.Single(x => x.Name.EndsWith(".dat", StringComparison.OrdinalIgnoreCase));
        if (cfgEntry.Length > 1024 * 1024 || datEntry.Length > 300L * 1024 * 1024) throw new InvalidDataException("COMTRADE vượt giới hạn xem.");
        string[] lines; using (var reader = new StreamReader(cfgEntry.Open())) lines = reader.ReadToEnd().Replace("\r", "").Split('\n');
        var station = lines[0].Split(','); var channels = lines[1].Split(',');
        int Number(string value) => int.Parse(value.Trim().TrimEnd('A', 'D'), Culture);
        double Real(string value) => double.Parse(value.Trim(), Culture);
        var na = Number(channels[1]); var nd = Number(channels[2]);
        if (na < 0 || nd < 0 || na + nd > 256) throw new InvalidDataException("Viewer hỗ trợ tối đa 256 kênh.");
        var analog = new List<AnalogBuilder>(); var digital = new List<DigitalBuilder>();
        for (var i = 0; i < na; i++)
        {
            var c = lines[2 + i].Split(','); var scale = c.Length > 12 ? c[12].Trim().ToUpperInvariant() : "unspecified";
            analog.Add(new(c[1], c[2], c[4], scale, Real(c[5]), Real(c[6]), maxPoints));
        }
        for (var i = 0; i < nd; i++) digital.Add(new(lines[2 + na + i].Split(',')[1], maxPoints));
        var freqIndex = 2 + na + nd; var frequency = Real(lines[freqIndex]); var rateCount = Number(lines[freqIndex + 1]);
        if (rateCount is < 0 or > 10000) throw new InvalidDataException("CFG có rate count không hợp lệ.");
        var rates = new List<(double Rate, long End)>();
        for (var i = 0; i < rateCount; i++)
        {
            var rate = lines[freqIndex + 2 + i].Split(','); rates.Add((Real(rate[0]), long.Parse(rate[1], Culture)));
        }
        var timing = freqIndex + 2 + rateCount; var format = lines[timing + 2].Trim().ToUpperInvariant();
        var multiplier = lines.Length > timing + 3 && !string.IsNullOrWhiteSpace(lines[timing + 3]) ? Real(lines[timing + 3]) : 1;
        if (!double.IsFinite(multiplier) || multiplier <= 0) throw new InvalidDataException("TIMEMULT không hợp lệ.");
        long expected = rates.Count > 0 ? rates[^1].End : 0;
        if (format != "ASCII" && expected == 0)
        {
            var width = 8 + (format == "BINARY" ? 2 : 4) * na + 2 * ((nd + 15) / 16); expected = datEntry.Length / width;
        }
        if (expected <= 0 && format == "ASCII")
        { using var counter = new StreamReader(datEntry.Open()); while (counter.ReadLine() is { } line) if (!string.IsNullOrWhiteSpace(line)) expected++; }
        var startRaw = lines[timing].Trim(); var triggerRaw = lines[timing + 1].Trim();
        double? triggerMs = ParseTime(startRaw) is { } start && ParseTime(triggerRaw) is { } trigger ? (trigger - start).TotalMilliseconds : null;
        var lower = fromMs ?? 0; var upper = toMs ?? double.MaxValue;
        if (!double.IsFinite(lower) || (toMs != null && !double.IsFinite(upper)) || lower < 0 || upper <= lower) throw new ArgumentException("Khoảng thời gian xem không hợp lệ.");
        var estimatedSamples = (double)expected;
        if ((fromMs != null || toMs != null) && rates.Count > 0 && rates.All(x => x.Rate > 0))
        {
            var end = toMs ?? DerivedTime(expected, rates);
            estimatedSamples = Math.Min(expected, Math.Max(1, (end - lower) * rates.Max(x => x.Rate) / 1000.0 + 2));
        }
        var buckets = Math.Max(1L, (long)Math.Ceiling(estimatedSamples / (maxPoints / 2.0)));
        long sampleCount = 0; double lastTime = 0; double? previous = null;
        void Consume(long sample, uint timestamp, double[] values, int[] bits)
        {
            if (sample != sampleCount + 1) throw new InvalidDataException("Số thứ tự mẫu không liên tục.");
            var time = timestamp is 0 or uint.MaxValue ? DerivedTime(sample, rates) : timestamp * multiplier / 1000.0;
            if (previous != null && time <= previous) throw new InvalidDataException("Thời gian mẫu không tăng.");
            previous = time; lastTime = time; sampleCount++;
            if (time < lower || time > upper) return;
            for (var i = 0; i < na; i++) analog[i].Add(sample / buckets, time, values[i]);
            for (var i = 0; i < nd; i++) digital[i].Add(time, bits[i]);
        }
        using var input = datEntry.Open();
        if (format == "ASCII")
        {
            using var reader = new StreamReader(input);
            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cells = line.Split(',');
                if (cells.Length != 2 + na + nd) throw new InvalidDataException("DAT ASCII sai số cột.");
                var values = cells.Skip(2).Take(na).Select(Real).ToArray();
                var bits = cells.Skip(2 + na).Select(Number).ToArray();
                if (bits.Any(x => x is not (0 or 1))) throw new InvalidDataException("Kênh số phải là 0/1.");
                Consume(long.Parse(cells[0], Culture), uint.Parse(cells[1], Culture), values, bits);
            }
        }
        else
        {
            if (format is not ("BINARY" or "BINARY32" or "FLOAT32")) throw new InvalidDataException("Viewer chưa hỗ trợ định dạng DAT này.");
            var width = 8 + (format == "BINARY" ? 2 : 4) * na + 2 * ((nd + 15) / 16);
            if (datEntry.Length % width != 0) throw new InvalidDataException("DAT sai chiều dài mẫu.");
            using var reader = new BinaryReader(input);
            for (long row = 0; row < datEntry.Length / width; row++)
            {
                var sample = reader.ReadUInt32(); var timestamp = reader.ReadUInt32(); var values = new double[na]; var bits = new int[nd];
                for (var i = 0; i < na; i++) values[i] = format switch { "BINARY" => reader.ReadInt16(), "BINARY32" => reader.ReadInt32(), _ => reader.ReadSingle() };
                for (var wordIndex = 0; wordIndex < (nd + 15) / 16; wordIndex++)
                {
                    var word = reader.ReadUInt16();
                    for (var bit = 0; bit < 16 && wordIndex * 16 + bit < nd; bit++) bits[wordIndex * 16 + bit] = (word >> bit) & 1;
                }
                Consume(sample, timestamp, values, bits);
            }
        }
        if (sampleCount == 0 || (expected > 0 && sampleCount != expected)) throw new InvalidDataException("Số mẫu không khớp CFG.");
        return new(station[0], station.ElementAtOrDefault(1) ?? "", format, frequency, sampleCount, lastTime, triggerMs,
            startRaw, triggerRaw, lower, Math.Min(upper, lastTime), analog.Select(x => x.Build()).ToArray(), digital.Select(x => x.Build()).ToArray());
    }
    private static DateTime? ParseTime(string value) => DateTime.TryParseExact(value,
        ["dd/MM/yyyy,HH:mm:ss.FFFFFFF", "dd/MM/yyyy,HH:mm:ss", "d/M/yyyy,H:mm:ss.FFFFFFF"], Culture, DateTimeStyles.None, out var time) ? time : null;
    private static double DerivedTime(long sample, List<(double Rate, long End)> rates)
    {
        if (sample == 1) return 0;
        double milliseconds = 0; long previousEnd = 0;
        foreach (var (rate, end) in rates)
        {
            if (rate <= 0 || !double.IsFinite(rate)) throw new InvalidDataException("DAT không có timestamp; sample rate không hợp lệ.");
            var intervals = Math.Min(sample, end) - Math.Max(previousEnd, 1);
            if (intervals > 0) milliseconds += intervals * 1000.0 / rate;
            if (sample <= end) return milliseconds;
            previousEnd = end;
        }
        throw new InvalidDataException("Không suy ra được thời gian mẫu.");
    }
    private sealed class AnalogBuilder(string name, string phase, string unit, string scale, double a, double b, int maxPoints)
    {
        private readonly List<WavePoint> points = []; private WavePoint? low; private WavePoint? high; private WavePoint? last;
        private long bucket = -1, count; private double squares, minimum = double.MaxValue, maximum = double.MinValue;
        public void Add(long nextBucket, double time, double raw)
        {
            var value = a * raw + b; if (!double.IsFinite(value)) return;
            count++; squares += value * value; minimum = Math.Min(minimum, value); maximum = Math.Max(maximum, value);
            if (nextBucket != bucket) { Flush(); bucket = nextBucket; }
            var point = new WavePoint(time, value);
            last = point;
            if (low == null || value < low.Value) low = point;
            if (high == null || value > high.Value) high = point;
        }
        private void Flush()
        {
            if (low != null && high != null)
            {
                if (low.TimeMs <= high.TimeMs) { points.Add(low); if (high.TimeMs != low.TimeMs) points.Add(high); }
                else { points.Add(high); points.Add(low); }
                if (points.Count > maxPoints + 2)
                {
                    var reduced = new List<WavePoint>();
                    for (var i = 0; i < points.Count; i += 4)
                    {
                        var block = points.Skip(i).Take(4).ToArray();
                        var min = block.MinBy(x => x.Value)!; var max = block.MaxBy(x => x.Value)!;
                        foreach (var point in new[] { min, max }.Distinct().OrderBy(x => x.TimeMs)) reduced.Add(point);
                    }
                    points.Clear(); points.AddRange(reduced);
                }
            }
            low = high = null;
        }
        public AnalogTrace Build() { Flush(); if (last != null && (points.Count == 0 || points[^1].TimeMs != last.TimeMs)) points.Add(last); return new(name, phase, unit, scale, count == 0 ? null : Math.Sqrt(squares / count), count == 0 ? null : Math.Max(Math.Abs(minimum), Math.Abs(maximum)), count == 0 ? null : minimum, count == 0 ? null : maximum, count, points); }
    }
    private sealed class DigitalBuilder(string name, int maxPoints)
    {
        private readonly List<WavePoint> points = []; private int changes; private WavePoint? last;
        public void Add(double time, int value)
        {
            if (last != null && last.Value != value) changes++;
            if ((last == null || last.Value != value) && points.Count < maxPoints) points.Add(new(time, value));
            last = new(time, value);
        }
        public DigitalTrace Build() { if (last != null) points.Add(last); return new(name, changes, changes >= maxPoints, points); }
    }
}
