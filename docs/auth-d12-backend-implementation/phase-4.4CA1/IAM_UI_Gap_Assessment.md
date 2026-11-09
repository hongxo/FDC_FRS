# Source audit S01–S08

Baseline 363749f, PR #12, nhánh 4.4C-A. POC nằm ở checkout nguồn, tách khỏi Center. Center dùng HTML/CSS/ES modules. Đã đối chiếu Dynamic RBAC architecture, API contracts, permission catalog, StationGrant và SoD, hồ sơ 4.3B và 4.4C-A. D01–D10 chưa có release phê duyệt trong hồ sơ. POC có dữ liệu fixture, runtime chính không dùng.

| Screen | Existing UI | Backend API | Database | Authorization | Runtime Integration | E2E | Gap | Action |
|---|---|---|---|---|---|---|---|---|
| S01 Login | POC form, Center status | Chưa map login | IAM session/user | D02/D05 | BLOCKED_BY_POLICY | Chỉ đường từ chối | Chưa có issuance | Giữ trang trạng thái |
| S02 MFA | POC challenge, Center status | Chưa map | IAM foundation | D03/D05 | BLOCKED_BY_POLICY | Không có live challenge | Chưa có policy | Giữ status |
| S03 Recovery | POC form, Center status | Chưa map | IAM foundation | D06 | BLOCKED_BY_POLICY | Không có live recovery | Chưa có policy | Không nhận identity công khai |
| S04 Profile | POC và Center status | SQL session reader; chưa có profile/logout route | IAM session | D05 | BLOCKED_BY_POLICY | Session negative path | Thiếu API profile/revoke | Chặn profile anonymous |
| S05 Users | Center management | users/details/roles GET | Canonical IAM | D04/D05 pending | RUNTIME_VERIFIED denial | 503/401 | Shell static công khai, thiếu search/detail | Bảo vệ shell, thêm client |
| S06 Roles/Permissions | Center role list | roles/detail/permissions/catalog GET | Canonical IAM | D04 pending | RUNTIME_VERIFIED denial | Roles 503 | Thiếu catalog view | Thêm catalog và detail |
| S07 Station grants | Center assignment filter | station-grants GET | Canonical UserRole scope | D04/D09 | RUNTIME_VERIFIED denial | 503 | Chưa có object policy | Giữ lọc cùng assignment |
| S08 Audit | Center list | security-audit/detail GET | Canonical audit | D04/D10 | RUNTIME_VERIFIED denial | 503 | Thiếu detail, D10 chưa ký | Thêm read detail |

Đường từ chối được kiểm thử 4.3B. Không có live login hoặc dữ liệu business được phép. Lab readiness 4.4C-A chỉ trả policy status. COMTRADE/machine intake tách tuyến riêng.
