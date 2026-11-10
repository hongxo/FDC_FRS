# FDC IAM Security Regression

Canonical evaluator và SQL readers tiếp tục kiểm tra UserRole, RolePermission, StationGrant, GrantFunction, session/user versions và resource ownership. Không có RoleName-based authorization. Tên demo.* chỉ là nhãn fixture. Lab tests bổ sung same-assignment negative, thiếu grant, khác station, stale authority, PersonReference SoD, concurrency và audit rollback. Mandatory deny ied.configuration.write giữ nguyên.

Bảy role fixtures có regression SQL scoped read và các vai trò chính có HTTP/browser workflow. Không suy ra mọi role × operation × expiry combination đều đã chạy browser. Chưa có human independent security sign-off.

Toàn bộ 23 IAM legacy tests được bật bằng disposable opt-in và chạy thật. Trong đó có gate/lost ACK/unknown outcome, SQL timeout/missing DB, role lifecycle, management scope, session revoke và outbox rollback. Không dùng named shared Lab để giảm SKIP. Hai runtime non-SA/DPAPI bài vẫn được giữ SKIP.
