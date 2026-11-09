# Screen inventory

Ảnh dưới tmp/phase4-4ca1/<run>/iam-*.png là bằng chứng browser loopback, chỉ thể hiện trạng thái chưa mở hoặc từ chối. Số liệu theo [test matrix](IAM_UI_E2E_Test_Matrix.md). Permission chính xác theo [matrix](IAM_UI_Permission_Matrix.md).

| Screen | Route | Endpoint | Evidence | Actual / blocker |
|---|---|---|---|---|
| S01 | /identity/#login | Chưa map login | iam-login.png | Status D02/D05, không form gửi mật khẩu |
| S02 | /identity/#mfa | Chưa map MFA | iam-mfa.png | Status D03 |
| S03 | /identity/#recovery | Chưa map recovery | iam-recovery.png | Status D06 |
| S04 | /identity/#profile | Chưa map profile/logout | iam-profile.png | Status chung, không trả user/session data |
| S05 | /identity/management.html#users | users/detail/roles GET | iam-users.png | Search và detail client, authority trả 503 |
| S06 | /identity/management.html#roles và #permissions | roles/detail/permissions/catalog GET | iam-roles.png, iam-permissions.png | Catalog canonical, 503 |
| S07 | /identity/management.html#grants | station-grants GET | iam-grants.png | Hai ID cùng assignment, 503 |
| S08 | /identity/management.html#audit | security-audit/detail GET | iam-audit.png | Read-only và 503, D10 chưa hoàn tất |

Theo dõi đề nghị sử dụng change-requests GET theo ID khi authority cho phép menu. My Requests, Pending Review, Pending Approval và Apply Queue chờ các endpoint canonical và workflow release.
