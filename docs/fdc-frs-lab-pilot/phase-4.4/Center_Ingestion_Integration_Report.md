# Center ingestion integration

Contract `/api/pilot/v1/ingestion` gồm initiate, PUT file, status và complete. `PilotFileIntake` lưu ticket, dùng staging và kiểm tra lại binding, size, SHA-256 và parser trước khi gọi SQL writer hiện có. Test loopback dùng certificate tạm và xác nhận một record/outbox khi replay. Upload gián đoạn thử lại theo file. Route pilot hiện chỉ map trong host test.

Ngày 2026-11-09 (Asia/Bangkok). Source `1ad4f0a`. Evidence và lệnh tái lập xem [Evidence](Evidence.md). Reviewer độc lập chưa được chỉ định.
