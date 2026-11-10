# FDC Disposable SQL Harness Design

Harness test-only yêu cầu FDC_RUN_A9_DISPOSABLE=1 và FDC_A9_SQL_MASTER là profile Windows auth qua lpc:. vào master local allowlisted. Mỗi test tạo tên FDC_A9_TEST_<GUID>, ghi Fdc.A9.Run và Fdc.Disposable. IAM 001 chạy EF nguyên bản. Center 002–006 lấy từng migration operation/batch, thay đúng predicate tên catalog trong bộ nhớ, giữ guard host/session, DDL và transaction theo migration. Guard source và migration Lab 007 giữ nguyên.

Tài khoản Windows quản lý DB. EF queries chạy trong database dưới EXECUTE AS USER fdc_a9_runtime, user WITHOUT LOGIN có quyền CRUD theo schema fixture và SHOWPLAN, không dùng principal này để chứng minh least privilege production. Cleanup yêu cầu flag tạo trong tiến trình, host và marker Run ID khớp. Không cưỡng bức đóng kết nối. Lỗi trước khi ghi marker hoặc tiến trình bị kill cần DBA kiểm tra thủ công.
