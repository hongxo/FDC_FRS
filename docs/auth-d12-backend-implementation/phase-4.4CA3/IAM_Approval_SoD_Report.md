# Approval và SoD

Giữ canonical request GET và phân biệt pending/approved/applied. Kiểm thử độc lập PersonReference, stale revision và SQL commit lock dùng fixture. Không map apply/revoke authority khi D09 pending. D04/D08/D09/D10 phải ký writer coverage, review order, CSRF/idempotency và outbox.
