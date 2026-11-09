# Feature activation gate

15 key định nghĩa trong FeatureActivationGate.cs. Dependency dùng mã formal từ JSON. Reader authority phải bind feature, exact scope/environment, approval ref, signed release digest, authority version/restore epoch, effective window, revocation, actor và committed audit ID. Revision tăng khi đổi metadata. Gate đối chiếu lại sau xác minh, deny khi thiếu hoặc sai dữ liệu, và loại Production khỏi candidate scope. ACTIVE chỉ có thể xuất hiện với trusted readers/verifier và activation audit; fixture tests chỉ xác minh logic.

Center đăng ký PendingFeatureActivationReader và PendingPolicyReleaseEvidenceVerifier. GET auth-status dùng AUTH_LOGIN/MFA/RECOVERY/SESSION, trả trạng thái, không cấp quyền. Các gate còn lại là candidate contracts, chưa gắn vào mutation handlers. DISABLED/IMPLEMENTED_NOT_AUTHORIZED là trạng thái inventory, APPROVED_NOT_ACTIVATED/ACTIVE/EXPIRED/REVOKED là kết quả khi có snapshot; hiện cả 15 đều BLOCKED_BY_POLICY.

Gate không thay Human session, RBAC, station/object scope, SoD hoặc D09 commit transaction. Đánh giá là point-in-time. Mọi writer tương lai phải kiểm tra lại trong commit boundary. Không có config, route hay UI để ghi activation. Chưa chọn cơ chế custody/authority store.
