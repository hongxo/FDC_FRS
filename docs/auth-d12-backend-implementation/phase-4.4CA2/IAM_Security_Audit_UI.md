# S08 Security Audit

/identity/management.html#audit dùng SecurityAudit và SecurityAuditDetail. Lọc mã thao tác chính xác qua query hiện hữu. List hiển thị event ID/correlation ID, action, outcome, timestamp, detail chỉ projection các trường cho phép. Không render raw payload hoặc thêm write endpoint. Target/source/evidence metadata mới cần mapping D10, quyền đọc và retention. Đọc correlation khác với chứng minh audit immutability.
