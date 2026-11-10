using System.Globalization;
using System.Text;

namespace Fdc.Core;

public sealed record ComtradeLimits(int MaxCfgBytes, long MaxDatBytes, int MaxChannels,
    int MaxRates, long MaxSamples, int PreviewPoints)
{
    public void Validate()
    {
        if (MaxCfgBytes is < 1 or > 1048576 || MaxDatBytes is < 1 or > 104857600 ||
            MaxChannels is < 1 or > 256 || MaxRates is < 1 or > 10000 ||
            MaxSamples is < 1 or > 5000000 || PreviewPoints is < 2 or > 10000)
            throw new ArgumentException("Invalid parser safety bounds.");
    }
}
public sealed record AnalogChannel(int Index, string Name, string Phase, string Unit, double A, double B,
    string PrimarySecondary, double? Primary, double? Secondary);
public sealed record SamplingRate(double Hertz, long LastSample);
public sealed record ComtradePoint(long Sample, double TimeMs, double[] Analog, int[] Digital);
public sealed record ComtradePreview(string Station, string Device, string Revision, string Format,
    string StartTimeRaw, string TriggerTimeRaw, double TimeMultiplier, double NominalHz,
    IReadOnlyList<AnalogChannel> Analog, IReadOnlyList<string> Digital, IReadOnlyList<SamplingRate> Rates,
    long Samples, IReadOnlyList<ComtradePoint> Points, bool PreviewDecimated, string Encoding,
    string Scaling = "a*x+b in declared channel units; primary/secondary metadata preserved, no automatic ratio conversion");

// Bounded, strict parser for an explicit tested subset. Does not classify faults or alter input bytes.
public static class BoundedComtradeReader
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    public static ComtradePreview Read(string cfgPath, string datPath, ComtradeLimits limits, string encoding,
        CancellationToken ct = default)
    {
        limits.Validate(); ct.ThrowIfCancellationRequested();
        if (!Path.GetFileNameWithoutExtension(cfgPath).Equals(Path.GetFileNameWithoutExtension(datPath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("CFG/DAT basename mismatch.");
        using var cfg = File.OpenRead(cfgPath);
        using var dat = File.OpenRead(datPath);
        return Read(cfg, dat, limits, encoding, ct);
    }

    // Caller owns both verified streams. Read from the beginning without closing them.
    public static ComtradePreview Read(Stream cfg, Stream input, ComtradeLimits limits, string encoding,
        CancellationToken ct = default)
    {
        limits.Validate();
        ct.ThrowIfCancellationRequested();
        if (!cfg.CanRead || !cfg.CanSeek || !input.CanRead || !input.CanSeek ||
            cfg.Position != 0 || input.Position != 0)
            throw new ArgumentException("Readable seekable CFG/DAT streams at position zero required.");
        if (cfg.Length <= 0 || cfg.Length > limits.MaxCfgBytes) throw new InvalidDataException("CFG size bound.");
        var bytes = new byte[(int)cfg.Length]; cfg.ReadExactly(bytes);
        var decoder = encoding switch { "UTF-8" => new UTF8Encoding(false, true), "ISO-8859-1" => Encoding.Latin1, _ => throw new NotSupportedException("Encoding not tested/supported.") };
        string text;
        try { text = decoder.GetString(bytes).TrimStart('\uFEFF'); }
        catch (DecoderFallbackException e) { throw new InvalidDataException("Invalid configured CFG encoding; raw bytes retained.", e); }
        if (text.Contains('\0')) throw new InvalidDataException("NUL in CFG.");
        var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1])) lines.RemoveAt(lines.Count - 1);
        string[] Line(int index, int minimum)
        {
            if (index < 0 || index >= lines.Count || lines[index].Length > 16384 || lines[index].Contains('"'))
                throw new InvalidDataException("Missing/oversize/unsupported quoted CFG line.");
            var cells = lines[index].Split(',').Select(x => x.Trim()).ToArray();
            if (cells.Length < minimum) throw new InvalidDataException("Missing CFG fields.");
            return cells;
        }
        long Integer(string s) => long.TryParse(s, NumberStyles.Integer, Culture, out var x) ? x : throw new InvalidDataException("Invalid integer.");
        int Bit(string s) { var value = Integer(s); return value is 0 or 1 ? (int)value : throw new InvalidDataException("Digital DAT must be 0 or 1."); }
        double Real(string s) => double.TryParse(s, NumberStyles.Float, Culture, out var x) && double.IsFinite(x) ? x : throw new InvalidDataException("Non-finite/invalid numeric value.");
        var header = Line(0, 2); var revision = header.Length == 2 ? "1991" : header[2];
        if (revision is not ("1991" or "1999" or "2013")) throw new NotSupportedException("COMTRADE revision not tested/supported.");
        var counts = Line(1, 3);
        if (!counts[1].EndsWith('A') || !counts[2].EndsWith('D')) throw new InvalidDataException("Invalid channel suffix.");
        var na = Integer(counts[1][..^1]); var nd = Integer(counts[2][..^1]);
        if (na < 0 || nd < 0 || na + nd != Integer(counts[0]) || na + nd is < 1 || na + nd > limits.MaxChannels)
            throw new InvalidDataException("Channel count bound/mismatch.");
        var analog = new List<AnalogChannel>(); var digital = new List<string>();
        for (var i = 0; i < na; i++)
        {
            var row = Line(2 + i, 10);
            if (Integer(row[0]) != i + 1 || string.IsNullOrWhiteSpace(row[1])) throw new InvalidDataException("Invalid analog channel index/name.");
            var a = Real(row[5]); var b = Real(row[6]); _ = Real(row[7]);
            if (Real(row[8]) > Real(row[9])) throw new InvalidDataException("Invalid analog range.");
            double? primary = null, secondary = null; var ps = "unspecified";
            if (revision != "1991")
            {
                if (row.Length < 13 || Real(row[10]) <= 0 || Real(row[11]) <= 0 || row[12] is not ("P" or "S")) throw new InvalidDataException("Invalid ratio/PS metadata.");
                primary = Real(row[10]); secondary = Real(row[11]); ps = row[12];
            }
            analog.Add(new(i + 1, row[1], row[2], row[4], a, b, ps, primary, secondary));
        }
        for (var i = 0; i < nd; i++)
        {
            var row = Line(checked(2 + (int)na + i), revision == "1991" ? 3 : 5);
            if (Integer(row[0]) != i + 1 || string.IsNullOrWhiteSpace(row[1]) || Integer(row[^1]) is not (0 or 1)) throw new InvalidDataException("Invalid digital metadata.");
            digital.Add(row[1]);
        }
        var f = checked(2 + (int)na + (int)nd);
        var hz = Real(Line(f, 1)[0]); if (hz <= 0) throw new InvalidDataException("Invalid nominal frequency.");
        var rateCount = Integer(Line(f + 1, 1)[0]);
        if (rateCount < 0 || rateCount > limits.MaxRates) throw new InvalidDataException("Sampling rate count bound.");
        var rates = new List<SamplingRate>(); long previousEnd = 0;
        for (var i = 0; i < rateCount; i++)
        {
            var row = Line(f + 2 + i, 2); var rate = Real(row[0]); var end = Integer(row[1]);
            if (rate <= 0 || end <= previousEnd || end > limits.MaxSamples) throw new InvalidDataException("Invalid sampling-rate segment.");
            rates.Add(new(rate, end)); previousEnd = end;
        }
        var timing = checked(f + 2 + (int)rateCount);
        var startRaw = string.Join(',', Line(timing, 2)); var triggerRaw = string.Join(',', Line(timing + 1, 2));
        static DateTime ParseTime(string value) => DateTime.TryParseExact(value,
            ["d/M/yyyy,H:m:s", "d/M/yyyy,H:m:s.FFFFFFF"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? DateTime.SpecifyKind(dt, DateTimeKind.Unspecified) : throw new InvalidDataException("Invalid COMTRADE timestamp.");
        _ = ParseTime(startRaw); _ = ParseTime(triggerRaw); // No guessed timezone or UTC conversion.
        var format = Line(timing + 2, 1)[0];
        if (format is not ("ASCII" or "BINARY" or "BINARY32" or "FLOAT32")) throw new NotSupportedException("DAT encoding not supported.");
        if (revision != "2013" && format is "BINARY32" or "FLOAT32") throw new NotSupportedException("32-bit formats require tested 2013 profile.");
        var multiplier = revision == "1991" ? 1d : Real(Line(timing + 3, 1)[0]);
        if (multiplier <= 0) throw new InvalidDataException("Invalid time multiplier.");
        // 2013 time-code/local-code and quality/leap fields are preserved as raw trailing CFG,
        // not interpreted as a timezone. Reject unexpected number of trailing lines.
        var minimumLines = timing + (revision == "1991" ? 3 : 4);
        if (lines.Count != minimumLines && !(revision == "2013" && lines.Count == minimumLines + 2))
            throw new InvalidDataException("Unsupported/malformed trailing CFG fields.");
        if (input.Length <= 0 || input.Length > limits.MaxDatBytes) throw new InvalidDataException("DAT size bound.");
        var expected = rates.Count == 0 ? (long?)null : rates[^1].LastSample;
        var points = new List<ComtradePoint>(); long samples = 0; double lastTime = -1;
        var stride = Math.Max(1, (long)Math.Ceiling((expected ?? limits.MaxSamples) / (double)(limits.PreviewPoints - 1)));
        ComtradePoint? last = null;
        double Derived(long sample)
        {
            if (sample == 1) return 0;
            long before = 0; double elapsed = 0;
            foreach (var rate in rates)
            {
                var intervals = Math.Min(sample, rate.LastSample) - Math.Max(before, 1);
                if (intervals > 0) elapsed += intervals * 1000d / rate.Hertz;
                if (sample <= rate.LastSample) return elapsed;
                before = rate.LastSample;
            }
            throw new InvalidDataException("No sampling rate for missing timestamp.");
        }
        void Consume(long sample, long timestamp, double[] raw, int[] bits)
        {
            ct.ThrowIfCancellationRequested();
            if (sample != samples + 1 || sample > limits.MaxSamples || timestamp < 0 || timestamp > uint.MaxValue || bits.Any(x => x is not (0 or 1)))
                throw new InvalidDataException("Invalid sample order/count/timestamp/digital.");
            var time = timestamp == uint.MaxValue || timestamp == 0 ? Derived(sample) : timestamp * multiplier / 1000d;
            if (!double.IsFinite(time) || time <= lastTime) throw new InvalidDataException("Non-monotonic time.");
            var scaled = raw.Select((value, i) => analog[i].A * value + analog[i].B).ToArray();
            if (raw.Any(x => !double.IsFinite(x)) || scaled.Any(x => !double.IsFinite(x))) throw new InvalidDataException("Non-finite waveform.");
            samples++; lastTime = time; last = new(sample, time, scaled, bits);
            if ((sample - 1) % stride == 0 && points.Count < limits.PreviewPoints - 1) points.Add(last);
        }
        if (format == "ASCII")
        {
            // No ReadLine on untrusted DAT: cap each line before allocation can grow unbounded.
            using var reader = new StreamReader(input, decoder, false, 4096, leaveOpen: true);
            if (reader.Peek() == '\uFEFF') _ = reader.Read();
            var line = new StringBuilder();
            void Row()
            {
                if (line.Length == 0) return;
                var values = line.ToString().TrimEnd('\r').Split(','); line.Clear();
                if (values.Length != 2 + na + nd) throw new InvalidDataException("DAT column mismatch.");
                Consume(Integer(values[0]), Integer(values[1]), values.Skip(2).Take((int)na).Select(Real).ToArray(), values.Skip(2 + (int)na).Select(Bit).ToArray());
            }
            while (reader.Read() is var ch && ch != -1)
            {
                ct.ThrowIfCancellationRequested();
                if (ch == '\n') Row();
                else { if (line.Length >= 16384) throw new InvalidDataException("DAT line length bound."); line.Append((char)ch); }
            }
            Row();
        }
        else
        {
            var width = checked(8 + (format == "BINARY" ? 2 : 4) * (int)na + 2 * (((int)nd + 15) / 16));
            if (input.Length % width != 0 || input.Length / width > limits.MaxSamples) throw new InvalidDataException("Binary record width/count mismatch.");
            using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
            while (input.Position < input.Length)
            {
                var sample = reader.ReadUInt32(); var timestamp = reader.ReadUInt32();
                var raw = new double[(int)na]; var bits = new int[(int)nd];
                for (var i = 0; i < na; i++) raw[i] = format switch { "BINARY" => (double)reader.ReadInt16(), "BINARY32" => (double)reader.ReadInt32(), _ => (double)reader.ReadSingle() };
                for (var i = 0; i < nd; i += 16) { var word = reader.ReadUInt16(); for (var j = 0; j < 16 && i + j < nd; j++) bits[i + j] = (word >> j) & 1; }
                Consume(sample, timestamp, raw, bits);
            }
        }
        if (samples == 0 || expected is { } n && samples != n) throw new InvalidDataException("Final sample count mismatch.");
        if (last is not null && (points.Count == 0 || points[^1].Sample != last.Sample)) points.Add(last);
        return new(header[0], header[1], revision, format, startRaw, triggerRaw, multiplier, hz,
            analog, digital, rates, samples, points, points.Count < samples, encoding);
    }
}
