# User management API

Implemented opt-in GET /api/identity/v1/admin/users, /users/{id}, /users/{id}/roles. Search/status filtering and bounded offset/limit execute after explicit server visibility in SQL; DTOs contain only ID/username/status/version and safe assignment fields. No password hash/MFA secret/raw session/payload projection. UserRole queries enforce subject plus allowed assignment and Role IDs.

PendingManagementReadAuthority throws BLOCKED_BY_D04 before querying. New adapters require Fdc.Human and HTTPS and are not registered in any running Center. SQL unavailable is mapped to safe503, invalid bounds400, denied403, missing/invisible detail404, missing Human identity401. HTTP runtime acceptance not executed.

Create/disable/account-state changes, security history expansion, session revoke and change-request submission remain BLOCKED_BY_POLICY (D04/D05/D09/D10), not implemented or exposed. No physical user deletion, default password, bootstrap user or self-recovery bypass. Existing Phase2 login/MFA gap is not bypassed.

Actual SQL fixture test verifies hidden user sorts before visible user yet scope-before-paging returns only visible row, search cannot find hidden username, and wrong subject/assignment combinations return empty. Fixtures rollback; not a live account-management acceptance test.
