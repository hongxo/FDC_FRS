# API mapping

| UI | Endpoint | Hành vi |
|---|---|---|
| Menu | GET /api/identity/v1/ui/navigation | Phiên Human hiện hành, trả screen keys từ cùng read authority |
| Users | GET /api/identity/v1/admin/users và /users/{id}, /users/{id}/roles | Search/active/paging và read detail |
| Roles | GET /api/identity/v1/admin/roles, /roles/{id}, /roles/{id}/permissions | Cursor và permission codes từ backend |
| Catalog | GET /api/identity/v1/admin/permission-catalog | Page canonical DTO |
| Station | GET /api/identity/v1/admin/station-grants | userId và userRoleId, server resolve scope |
| Audit | GET /api/identity/v1/admin/security-audit, /security-audit/{id} | List/detail chỉ đọc |
| Requests | GET /api/identity/v1/admin/change-requests/{id} | Phân biệt approved và applied |
| Lab | GET /api/lab/v1/readiness | 503 station policy pending |

Login/MFA/recovery/profile/logout chưa có endpoint hiệu lực. Không thêm mutation/approval hay mock API. Client đọc same-origin qua HTTPS, no-store, không lưu proof trong web storage. 401/403/503 không đọc hoặc render server error body.
