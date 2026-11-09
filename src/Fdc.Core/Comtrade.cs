using System.Globalization;

namespace Fdc.Core;

public static class Comtrade
{
    public static string? Validate(string cfgPath, string datPath)
    {
        var lines = File.ReadAllLines(cfgPath);
        if (lines.Length < 8) throw new InvalidDataException("CFG thiếu dữ liệu.");
        var channels = lines[1].Split(',');
        int Parse(string value) => int.Parse(value.Trim().TrimEnd('A', 'D', 'a', 'd'), CultureInfo.InvariantCulture);
        if (channels.Length < 3) throw new InvalidDataException("CFG thiếu số kênh.");
        var analog = Parse(channels[1]); var digital = Parse(channels[2]);
        if (analog < 0 || digital < 0 || analog + digital != Parse(channels[0])) throw new InvalidDataException("Số kênh CFG không hợp lệ.");
        var frequencyIndex = checked(2 + analog + digital);
        if (frequencyIndex + 2 >= lines.Length) throw new InvalidDataException("CFG thiếu tần số lấy mẫu.");
        var rates = Parse(lines[frequencyIndex + 1]);
        if (rates is < 0 or > 10000) throw new InvalidDataException("Số tốc độ lấy mẫu không hợp lệ.");
        var timingIndex = frequencyIndex + 2 + rates;
        if (timingIndex + 2 >= lines.Length) throw new InvalidDataException("CFG thiếu thời gian hoặc định dạng DAT.");
        var format = lines[timingIndex + 2].Trim().ToUpperInvariant();
        long expected = rates == 0 ? 0 : long.Parse(lines[timingIndex - 1].Split(',')[1].Trim(), CultureInfo.InvariantCulture);
        if (expected < 0) throw new InvalidDataException("Số mẫu âm.");
        var length = new FileInfo(datPath).Length;
        if (length == 0) throw new InvalidDataException("DAT rỗng.");
        var recordSize = format switch
        {
            "BINARY" => checked(8 + 2 * analog + 2 * ((digital + 15) / 16)),
            "BINARY32" or "FLOAT32" => checked(8 + 4 * analog + 2 * ((digital + 15) / 16)),
            "ASCII" => 0,
            _ => throw new InvalidDataException($"Định dạng DAT chưa hỗ trợ: {format}")
        };
        if (recordSize > 0 && (length % recordSize != 0 || (expected > 0 && length / recordSize != expected)))
            throw new InvalidDataException("Kích thước DAT không khớp số kênh/số mẫu CFG.");
        if (format == "ASCII")
        {
            long count = 0;
            foreach (var line in File.ReadLines(datPath).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var values = line.Split(','); count++;
                if (values.Length != 2 + analog + digital || !long.TryParse(values[0], out var sample) || sample != count)
                    throw new InvalidDataException("Dòng DAT ASCII không hợp lệ.");
                foreach (var value in values.Skip(1))
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || !double.IsFinite(number))
                        throw new InvalidDataException("DAT chứa giá trị không hợp lệ.");
            }
            if (count == 0 || (expected > 0 && count != expected)) throw new InvalidDataException("Số mẫu DAT không khớp CFG.");
        }
        // Preserve the raw IED timestamp. Do not invent a timezone when the CFG has none.
        return lines[timingIndex + 1].Trim();
    }
}
