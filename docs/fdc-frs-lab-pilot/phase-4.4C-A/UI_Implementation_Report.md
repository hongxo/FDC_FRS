# UI offline

CLI `readiness-inventory <xls> IP 2 LAB <new-private.html>` tạo trang nội bộ CA01–CA10. Chọn dòng nguồn, xem field issues, xuất proposal chấp nhận/từ chối, lập draft và mở kết quả preflight. Dùng `readiness-evaluate <draft.json> <new-result.json>`. Form chỉ nhập tham chiếu evidence, không thu SCADA hay giữ file nhạy cảm trên server. Không phục vụ HTML trong Center và không có nút chạy IED.

Lưu các file bên dưới Git ignored tmp hoặc kho Lab riêng. Chưa bật màn hình có đăng nhập/ghi SQL vì IAM mapping và review policy đang chờ.

Lần chạy browser đầu thiếu Chromium của Playwright. Run-02 dùng Edge cài sẵn theo convention 4.4B và đã đạt. Cả 10 phần UI được tải trong 3 viewport; test xuất proposal và draft, đọc kết quả giả và ghi nhận không có network request ngoài file.
