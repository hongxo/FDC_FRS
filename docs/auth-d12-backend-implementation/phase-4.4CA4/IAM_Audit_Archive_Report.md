# S08 Audit

Giữ read-only projection, filter và correlation. Hồi quy SQL kiểm tra session revoke/audit/outbox atomic và rollback khi lỗi outbox. Archive/restore, retention, tamper detection và reviewer còn chờ D10/E06. Không chạy archive hoặc restore trên DB shared.
