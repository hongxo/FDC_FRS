# Triển khai

Center chuyển quản trị từ static file sang embedded endpoint yêu cầu Fdc.Human. Menu dùng authority server. Giữ nguyên đường client S05–S08, bổ sung tìm người dùng, catalog, user/role/audit detail, breadcrumb và thông báo trạng thái. Không tạo framework frontend mới.

S01–S04 là trạng thái chính sách. S05–S08 có client đọc thật nhưng quyền đọc vẫn fail-closed. Có theo dõi request ID. Chưa có issuance, MFA/recovery/logout hay approval mutation. Lab 4.4C-A và machine ingestion giữ boundary cũ.
