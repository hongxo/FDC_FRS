# FDC SQL Scope Integrity Report

Mỗi A.7 browser fixture tạo DB FdcA7_GUID trên SQL local xác minh host, apply IAM migration có guard, tạo Center tables từ EF model vì migration Center pin tên Lab. FK/trigger IAM giữ nguyên, không sửa shared Lab. DB được drop và kiểm DB_ID sau run. Một transaction riêng rollback sáu role. Chưa phải migration parity/HA test.
