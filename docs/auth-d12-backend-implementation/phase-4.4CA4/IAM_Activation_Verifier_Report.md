# Activation verifier

Candidate ECDSA P-256 / SHA-256 kiểm từng attestation với key được lấy từ IApprovalTrustReader. So sánh exact decision/option, approver, key authority, validity, revocation, signed repository/ref/SHA/scope/environment, payload digest, feature, activation actor/audit ID, phiên bản, authority versions và thời hạn. Từ chối người ký trùng người kích hoạt trong candidate. Gate đối chiếu signed binding với snapshot và đọc lại revision.

Chưa có trust store/provider thực, release có chữ ký hoặc adoption của wire format. Center vẫn đăng ký pending reader/verifier. Test key sinh trong bộ nhớ không đại diện owner. Khi tích hợp, scope authority và revocation phải được tái kiểm tại D09 commit boundary.
