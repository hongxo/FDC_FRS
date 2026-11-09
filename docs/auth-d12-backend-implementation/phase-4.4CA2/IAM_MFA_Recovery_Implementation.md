# S02/S03 MFA và Recovery

S02 /identity/#mfa hiển thị control mã xác minh vô hiệu hóa. S03 /identity/#recovery hiển thị thông báo chung và không có control nhận thông tin tài khoản. Không có endpoint challenge/recovery thực thi. D03/D06 phải duyệt factor, giới hạn retry, expiry, recovery single-use, rate limiting và bảo vệ chống enumeration. PendingMfaPolicy giữ nguyên.
