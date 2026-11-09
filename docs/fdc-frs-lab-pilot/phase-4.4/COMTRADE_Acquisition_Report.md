# COMTRADE acquisition

Collector ghép CFG/DAT theo basename và giữ HDR/INF nếu có. File qua thư mục `.part`, kiểm tra size và SHA-256, đối chiếu metadata trước/sau, parse và đưa vào spool theo content identity. Duplicate giữ manifest đầu tiên. SHA-256 nội bộ kiểm tra nhất quán bytes. Native simulator có đối chiếu trực tiếp byte với fixture gốc.

Ngày 2026-11-09 (Asia/Bangkok). Source `1ad4f0a`. Evidence và lệnh tái lập xem [Evidence](Evidence.md). Reviewer độc lập chưa được chỉ định.
