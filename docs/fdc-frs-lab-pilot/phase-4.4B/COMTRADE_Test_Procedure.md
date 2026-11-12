# Runbook COMTRADE

1. Người quản lý Lab và OT ghi run ID, người giám sát, thiết bị/firmware/interface, target/cổng/thư mục, nguồn đọc, thời gian, timeout/retry/băng thông, ngưỡng SCADA và stop procedure. Kiểm tra scope có hiệu lực và mã tham chiếu độc lập trước khi tạo adapter.
2. Chụp baseline SCADA và đồng hồ. Chọn một file set có sẵn do Lab duyệt. Giới hạn một association và chỉ discovery/list/read. Không tạo disturbance, điều khiển hoặc xóa file.
3. Ghi kích thước, thời gian UTC, danh mục .CFG/.DAT và .HDR/.INF nếu có. Stage spool riêng, tính SHA-256 và kiểm tra cặp, số kênh, samples, format, timestamp. Đối chiếu digest/size với manifest và nguồn tham chiếu khi có.
4. Đẩy qua mTLS station-bound nếu được duyệt. Đối chiếu manifest, SQL metadata, file store, audit/outbox và ACK. Thử lại cùng manifest để xác minh không nhân bản. Thử ngắt/khởi động lại trên fixture cô lập trước. Mọi thử lỗi tác động mạng thật cần phiếu riêng.
5. Người giám sát so baseline với during/after, ghi stop reason và ký nhận. Nếu lỗi, dừng tải, giữ spool/manifest/quarantine và evidence. Không xóa bản ghi IED hoặc reset SQL.
