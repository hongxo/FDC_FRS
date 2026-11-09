# Khuyến nghị theo nhóm · PROPOSED

- D01/D07. Tách listener/service Human và Machine. Listener riêng cùng process chỉ xem xét cho Lab có guard. Tách giảm nhầm danh tính và phạm vi sự cố, đổi lại thêm vận hành PKI và triển khai.
- D02/D05. Dùng hasher đã review và calibrate, SQL session authority có revoke và scope theo deployment. Argon2id là phương án cần đánh giá riêng. Chốt timing, rate limits, key lifecycle và consistency trước issuance.
- D03/D06. Ưu tiên WebAuthn UV với privileged users, TOTP chỉ qua ngoại lệ có hạn. Recovery độc lập theo custody để tránh fallback thành đường chiếm tài khoản.
- D04. Ký catalog và operation mapping theo object/station, giữ 7 baseline roles chỉ qua seed release. Chia delegation nhỏ hơn giảm overgrant nhưng tăng số role grants cần quản lý.
- D09. Commit authority cùng transaction, procedure-enforced DML. Tách IAM authority sang DB độc lập cần chứng minh consistency và failover/revoke race.
- D10. Outbox cùng transaction, archive độc lập có kiểm tra integrity/restore. Chỉ lưu audit trong SQL giảm chi phí trước mắt, chưa đủ bằng chứng chống sửa và custody.

Xem toàn bộ phương án, owner, risk và acceptance trong [ma trận](IAM_20_Subdecision_Decision_Matrix.md).
