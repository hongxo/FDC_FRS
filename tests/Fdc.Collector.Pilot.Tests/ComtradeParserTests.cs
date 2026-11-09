using System.Text;
using Fdc.Collector.Pilot;
using Fdc.Core;

namespace Fdc.Collector.Pilot.Tests;

public sealed class ComtradeParserTests
{
    [Fact] public void Utf8_dat_bom_supported_and_bytes_preserved()
    {
        using var f = new PilotFiles();
        var bytes = Encoding.UTF8.GetPreamble().Concat(File.ReadAllBytes(f.Dat)).ToArray(); File.WriteAllBytes(f.Dat, bytes);
        Assert.Equal(3, BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8").Samples);
        Assert.Equal(bytes, File.ReadAllBytes(f.Dat));
    }
    [Fact] public void Huge_digital_value_long_line_and_cancellation_rejected_without_truncation()
    {
        using var f = new PilotFiles(); File.WriteAllText(f.Dat, "1,0,1,9223372036854775807\n2,1000,2,1\n3,2000,3,0\n");
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8"));
        File.WriteAllText(f.Dat, new string('1', 16385));
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8"));
        Assert.ThrowsAny<OperationCanceledException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8", new CancellationToken(true)));
    }
    [Theory] [InlineData("ASCII")] [InlineData("BINARY")] [InlineData("BINARY32")] [InlineData("FLOAT32")]
    public void Tested_2013_formats_scale_analog_decode_digital_preserve_time_and_bytes(string format)
    {
        using var f = new PilotFiles(format); var cfg = File.ReadAllBytes(f.Cfg); var dat = File.ReadAllBytes(f.Dat);
        var view = BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8");
        Assert.Equal(3, view.Samples); Assert.Equal(format, view.Format); Assert.Equal(2, view.TimeMultiplier);
        Assert.Equal(4, view.Points[^1].TimeMs); Assert.Equal(1, view.Points[1].Digital[0]);
        Assert.Equal(format == "BINARY32" ? 33554435d : format == "FLOAT32" ? 4d : 3d, view.Points[0].Analog[0]);
        Assert.Equal("S", view.Analog[0].PrimarySecondary);
        Assert.Equal(cfg, File.ReadAllBytes(f.Cfg)); Assert.Equal(dat, File.ReadAllBytes(f.Dat));
    }
    [Fact] public void Multi_rate_missing_timestamps_are_derived_without_guessing_timezone()
    {
        using var f = new PilotFiles(); File.WriteAllText(f.Cfg, PilotFiles.Config("ASCII").Replace("1\n1000,3", "2\n1000,2\n500,3"));
        File.WriteAllText(f.Dat, "1,0,1,0\n2,0,2,1\n3,0,3,0\n");
        var view = BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8");
        Assert.Equal(new[] { 0d, 1d, 3d }, view.Points.Select(p => p.TimeMs));
        Assert.Equal("01/01/2026,00:00:00.000000", view.StartTimeRaw);
    }
    [Theory] [InlineData("missing_channel")] [InlineData("negative_rate")] [InlineData("count")]
    [InlineData("time")] [InlineData("multiplier")] [InlineData("format")]
    public void Malformed_cfg_is_rejected(string kind)
    {
        using var f = new PilotFiles(); var text = File.ReadAllText(f.Cfg);
        text = kind switch
        {
            "missing_channel" => text.Replace("2,1A,1D", "4,3A,1D"),
            "negative_rate" => text.Replace("1000,3", "-1000,3"),
            "count" => text.Replace("1000,3", "1000,4"),
            "time" => text.Replace("01/01/2026", "31/02/2026"),
            "multiplier" => text[..^2] + "NaN\n",
            "format" => text.Replace("ASCII", "EXEC"),
            _ => throw new InvalidOperationException()
        };
        File.WriteAllText(f.Cfg, text);
        if (kind == "format") Assert.Throws<NotSupportedException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8"));
        else Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8"));
    }
    [Theory] [InlineData("1,0,NaN,0\n2,1000,2,1\n3,2000,3,0\n")]
    [InlineData("1,0,1,0\n2,1000,2,2\n3,2000,3,0\n")]
    [InlineData("1,0,1,0\n3,1000,2,1\n3,2000,3,0\n")]
    [InlineData("1,0,1,0\n2,1000,2,1\n")]
    [InlineData("1,0,1,0\n2,1000,2,1\n3,500,3,0\n")]
    public void Corrupt_or_partial_ascii_never_silently_truncates(string dat)
    {
        using var f = new PilotFiles(); File.WriteAllText(f.Dat, dat);
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8"));
    }
    [Fact] public void Binary_partial_record_and_nan_rejected()
    {
        using var f = new PilotFiles("BINARY"); using (var stream = File.OpenWrite(f.Dat)) stream.SetLength(11);
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8"));
        using var floating = new PilotFiles("FLOAT32");
        using (var stream = File.OpenWrite(floating.Dat)) { stream.Position = 8; using var w = new BinaryWriter(stream); w.Write(float.NaN); }
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(floating.Cfg, floating.Dat, PilotFiles.Limits, "UTF-8"));
    }
    [Fact] public void Bounds_and_encoding_are_explicit()
    {
        using var f = new PilotFiles();
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits with { MaxCfgBytes = 5 }, "UTF-8"));
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits with { MaxDatBytes = 5 }, "UTF-8"));
        Assert.Throws<NotSupportedException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "AUTO"));
        File.WriteAllBytes(f.Cfg, Encoding.Latin1.GetBytes(PilotFiles.Config("ASCII").Replace("SYNTHETIC", "TESTÉ")));
        Assert.Throws<InvalidDataException>(() => BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8"));
        Assert.Equal("TESTÉ", BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "ISO-8859-1").Station);
    }
    [Theory] [InlineData("1991")] [InlineData("1999")]
    public void Older_ascii_profiles_are_explicitly_tested(string revision)
    {
        using var f = new PilotFiles(); var cfg = PilotFiles.Config("ASCII").Replace("2013", revision);
        if (revision == "1991") cfg = cfg.Replace(",100,1,S", "")[..^2];
        File.WriteAllText(f.Cfg, cfg);
        Assert.Equal(revision, BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits, "UTF-8").Revision);
    }
    [Fact] public void Preview_is_bounded_and_discloses_decimation()
    {
        using var f = new PilotFiles(); var v = BoundedComtradeReader.Read(f.Cfg, f.Dat, PilotFiles.Limits with { PreviewPoints = 2 }, "UTF-8");
        Assert.Equal(2, v.Points.Count); Assert.True(v.PreviewDecimated); Assert.Equal(3, v.Points[^1].Sample);
    }
}
