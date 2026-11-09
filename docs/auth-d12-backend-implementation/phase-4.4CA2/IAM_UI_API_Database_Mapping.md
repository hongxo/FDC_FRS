# UI, API và SQL

| Screen | Route | API / permission operation | Store | Evidence | Runtime |
|---|---|---|---|---|---|
| S01 | /identity/#login | GET auth-status public | Không | iam-login.png | Disabled |
| S02 | /identity/#mfa | GET auth-status public | Không | iam-mfa.png | Disabled |
| S03 | /identity/#recovery | GET auth-status public | Không | iam-recovery.png | Status only |
| S04 | /identity/#profile | GET auth-status public | Không có profile data | iam-profile.png | Status only |
| S05 | /identity/management.html#users | users/detail/roles GET, D04 user read | iam | iam-users.png | 503 pending |
| S06 | /identity/management.html#roles, #permissions | roles/detail/permissions/catalog GET, D04 role/catalog read | iam | iam-roles.png và iam-permissions.png | 503 pending |
| S07 | /identity/management.html#grants | grants/functions GET, StationGrants | iam | iam-grants.png | 503 pending |
| S08 | /identity/management.html#audit | security-audit/detail GET, D04/D10 | iam | iam-audit.png | 503 pending |

API dưới /api/identity/v1. Hình browser lưu local. Existing business schemas catalog/ops/ingest/audit giữ nguyên. No SQL lookup cho auth-status.
