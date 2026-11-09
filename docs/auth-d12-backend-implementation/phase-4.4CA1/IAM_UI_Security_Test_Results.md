# Security verification

Xem ma trận IAM-UI-01–25. Live runtime dùng Kestrel HTTPS loopback và SQL cookie fixture rollback-only. Test không giả lập response của API, không cấp session thông qua login và không chứng nhận bất kỳ policy production nào. Các test CSRF, assignment, SoD và machine-certificate guard dùng bằng chứng UNIT/INTEGRATION riêng.

Lượt run-01 ghi nhận browser timeout sau HTTP 500 ở navigation khi tải menu và catalog đồng thời trên SQL fixture dùng chung transaction. Raw evidence giữ riêng. Bản sửa kiểm tra navigation trước khi tải màn hình và không tự tải dữ liệu cho màn hình chưa được authority cho phép.

Run-02. Build 0 warnings/0 errors. IAM 243 PASS / 1 SKIP, Center 49 PASS / 1 SKIP, Collector 47 PASS / 0 SKIP. Node client 10 PASS. Browser live denial path kiểm tra 9 views, anonymous rejection và 3 viewports. Ma trận IAM-UI-01–25 có 18 PASS, 7 BLOCKED. Ảnh và TRX lưu nội bộ.
