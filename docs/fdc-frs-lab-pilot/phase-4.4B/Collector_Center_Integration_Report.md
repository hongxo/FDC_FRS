# Collector → Center

Tái dùng adapter read-only, spool SQLite, upload intake máy, outbox và dashboard offline từ 4.4/4.4A. Harness tách OFFLINE_FIXTURE khỏi local simulation. Test thực thi lab kiểm tra upload theo station binding, checksum, idempotency, ACK và tính toàn vẹn SQL. Xem ma trận cho kết quả từng phạm vi.

`tools/Invoke-LabAcceptance.ps1 -RunId <mới> -Workbook <đường-dẫn-riêng>` chạy offline. Thêm `-LocalIntegration` chỉ khi đã cấu hình local shared-memory `FDC_Center_Lab`, DPAPI fixture và SHA-256 binary simulator/native theo biến môi trường của tests. Thêm `-Browser` để chạy kiểm tra dashboard với Playwright cài sẵn. Harness không chạy migration. Evidence đặt trong `tmp/phase4-4b/<run-id>`. Nội dung thô không đưa lên GitHub.

Local mTLS fixture có PKI/identity của phiên test. Integrated OT security BLOCKED khi D01/D07 và trust topology còn thiếu phê duyệt.

Postcheck local-01 trên SQL Lab ghi 54 bảng người dùng gồm metadata, 68 FK, 0 FK vô hiệu hoặc không được trust và 0 hàng ứng dụng. DBCC CHECKCONSTRAINTS không báo lỗi. Hồi quy local-02 có 297 PASS, 24 SKIP, 0 FAIL. Kiểm tra riêng xác nhận 7 liên kết trên dashboard tổng hợp.
