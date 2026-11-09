# FDC COMTRADE CFG DAT Security Report

Browser nhận CFG và DAT, kiểm SHA-256 mỗi file, chứng minh B trả 404/zero bytes, Range vẫn full DAT và no-store, grant revoke từ chối lần đọc tiếp. SQL xác minh hai correlation HTTP khớp outbox. Adapter kiểm tra đường dẫn thực từ file handle trước khi đọc, rồi tạo snapshot staging với buffer giới hạn. Windows dùng GetFinalPathNameByHandle, Linux dùng /proc/self/fd. Platform khác fail closed. ACL root/staging vẫn bắt buộc. Chưa chạy race đổi symlink có kiểm soát hoặc kiểm transport completed/aborted. Audit chỉ ghi prepared.
