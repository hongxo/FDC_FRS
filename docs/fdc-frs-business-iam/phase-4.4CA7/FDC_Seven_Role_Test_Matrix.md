# FDC Seven Role Test Matrix

| Test identity | Phạm vi fixture | Kiểm tra |
|---|---|---|
| demo.system-admin | Có Station A, thiếu permission | Dashboard deny |
| demo.security-admin | Có permission, thiếu grant | Scope rỗng |
| demo.operator | A, metadata + download | Browser login/MFA, A allowed, B denied, revoke |
| demo.reviewer | A, metadata | Chỉ A, download permission deny |
| demo.approver | B, metadata | Chỉ B, download permission deny |
| demo.auditor | B, metadata | Chỉ B, download permission deny |
| demo.viewer | A, metadata | Chỉ A, download permission deny |

Sáu identity kiểm qua SQL authority trong giao dịch rollback. Chỉ operator qua browser. Quyền do fixture ghi bằng RolePermission/UserRole/StationGrant/GrantFunction. Tên label không phải policy. Không kiểm approval positive.
