# Kết quả CA-01–CA-30

Run-02 trừ SQL PARSEONLY và hậu kiểm schema ở run-01. Raw evidence dưới thư mục Git-ignored tmp/phase4-4ca. PASS chỉ áp dụng cho hành vi và môi trường ghi ở từng dòng. 24 test opt-in bị skip trong suite chung không được tính PASS.

| ID | Phép thử | Expected | Actual | Evidence | Result |
|---|---|---|---|---|---|
| CA-01 | Import | Đọc sheet thật | 17 dòng hợp lệ và 93 mục | inventory-counts.json | PASS |
| CA-02 | Thiếu model | Từ chối preflight | INVENTORY_FIELDS_REQUIRED | Center TRX | PASS |
| CA-03 | Thiếu firmware | Từ chối preflight | INVENTORY_FIELDS_REQUIRED | Center TRX | PASS |
| CA-04 | Trùng IP | Gắn lỗi cả hai dòng fixture | DUPLICATE_IP trên fixture | Collector TRX | PASS |
| CA-05 | Truy vết dòng | Proposal có dòng, sheet và hash | Preview và proposal giữ nguồn | browser.log; Collector TRX | PASS |
| CA-06 | IED chưa xác minh | Không cấp permit hiệu lực | Gate luôn chặn và 4.4A không mở adapter | Center và Collector TRX | PASS |
| CA-07 | Lập nháp | Xuất JSON draft | Browser export, CLI đọc và đánh giá | browser.log; fixture-result.json | PASS |
| CA-08 | Scope sai | Từ chối write/traversal/wildcard file | NOT_READY | Center TRX | PASS |
| CA-09 | IP wildcard | Từ chối target | NOT_READY | Center TRX | PASS |
| CA-10 | Cửa sổ hết hạn | Từ chối ngay tại end | EXPIRED | Center TRX | PASS |
| CA-11 | Thiếu stop owner | Không đạt readiness | NOT_READY | Center TRX | PASS |
| CA-12 | Thiếu SCADA baseline | Không đạt readiness | SCADA_REFERENCE_REQUIRED | Center TRX | PASS |
| CA-13 | Thiếu time baseline | Không đạt readiness | TIME_REFERENCE_REQUIRED | Center TRX | PASS |
| CA-14 | Thiếu security | Không đạt readiness | SECURITY_REFERENCE_REQUIRED | Center TRX | PASS |
| CA-15 | Thiếu network | Không đạt readiness | NETWORK_REFERENCE_REQUIRED | Center TRX | PASS |
| CA-16 | Scope thay đổi | Digest đổi và draft không thể có hiệu lực | Fingerprint khác, policy vẫn khóa | Center TRX | PASS |
| CA-17 | Phiên bản cũ | Từ chối stale revision bằng authority | Đã chứng minh hash đổi, chưa có store/authority để thực thi stale check | Center TRX; API report | BLOCKED |
| CA-18 | Revoke | Không thể dùng permit đã thu hồi | Workflow chưa mở | Gate design | BLOCKED |
| CA-19 | Self-approval | Từ chối theo danh tính có thẩm quyền | Approval route không tồn tại, chính sách SoD chưa duyệt | API report | BLOCKED |
| CA-20 | Cross-station | Không truy cập dữ liệu trạm khác | Không có route dữ liệu Lab. Chưa test object-level station scope | CenterStartupTests; API report | BLOCKED |
| CA-21 | Thiếu rollback | Không đạt readiness | ROLLBACK_REQUIRED | Center TRX | PASS |
| CA-22 | Evidence manifest | Từ chối digest sai và thiếu loại bắt buộc | NOT_READY | Center TRX | PASS |
| CA-23 | Audit trail | Mutation và audit commit đồng thời | Chưa có mutation runtime; proposal offline chỉ để review | Migration report | BLOCKED |
| CA-24 | SQL concurrency | Replay và rowversion race trên 007 | PARSEONLY đạt, 007 chưa được apply | sql-parse.log; sql-postcheck.txt | BLOCKED |
| CA-25 | Không gọi IED | UI và evaluator không tạo network traffic | Browser 0 yêu cầu ngoài file, evaluator thuần | browser.log; Center TRX | PASS |
| CA-26 | Không auto-start | Không có đường kích hoạt Collector | Route metadata GET duy nhất và UI chỉ có export | Center TRX; browser.log | PASS |
| CA-27 | Fail-closed | Draft đầy đủ vẫn bị chặn | BLOCKED_BY_POLICY | fixture-result.json; Center TRX | PASS |
| CA-28 | Approval giả | Từ chối result và trường approval tự chèn | Browser từ chối status giả; CLI từ chối field lạ | browser.log; fake-approval.log | PASS |
| CA-29 | Expiry | Bỏ hiệu lực tại mốc end | Ngay trước end vẫn policy-blocked, tại end EXPIRED | Center TRX | PASS |
| CA-30 | Sanitized reports | Không thấy workbook/IP/secret/raw trong phạm vi scan | Bộ dò đã chạy trên các path phát hành | GitHub delivery report | PASS |
