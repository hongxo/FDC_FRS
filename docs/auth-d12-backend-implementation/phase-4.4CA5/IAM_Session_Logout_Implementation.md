# IAM Session Logout Implementation

Cookie __Host-FdcSession Secure, HttpOnly, SameSite Strict trong fixture. HumanSessionHandler đọc SQL tại protected API. Logout dùng digest từ principal, fixture thu hồi SQL và ghi outbox trước khi clear cookie. Browser thử lại cookie cũ và nhận 401. Chưa có rotation, revoke-all HTTP, key HA hoặc store được duyệt.
