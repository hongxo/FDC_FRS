# FDC Export Revoke Race Report

Chưa có export worker và policy artifact lifecycle. BLOCKED. Bounded download chỉ chứng minh yêu cầu mới bị từ chối sau revoke. Không suy ra stream đang chạy bị ngắt. Thiết kế export phải tái kiểm authority trước đọc, publish và tải, quarantine khi revoke, audit outcome theo D09/D10.
