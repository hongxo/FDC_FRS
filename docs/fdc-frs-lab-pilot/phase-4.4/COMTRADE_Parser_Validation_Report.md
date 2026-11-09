# COMTRADE parser validation

Fixture kiểm tra ASCII 1991/1999, ASCII/BINARY/BINARY32/FLOAT32 2013, multi-rate, digital bits, analog scaling, time multiplier, UTF-8 BOM và ISO-8859-1 đã chọn rõ. Negative tests kiểm tra record width, sample count, timestamp, encoding, dữ liệu không hữu hạn, line quá dài và cancellation. Parser giữ timestamp gốc và tỷ số primary/secondary như metadata.

Ngày 2026-11-09 (Asia/Bangkok). Source `1ad4f0a`. Evidence và lệnh tái lập xem [Evidence](Evidence.md). Reviewer độc lập chưa được chỉ định.
