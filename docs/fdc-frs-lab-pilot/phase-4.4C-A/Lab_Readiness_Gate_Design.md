# Gate design

DRAFT → SUBMITTED → UNDER_REVIEW → APPROVED → READY_FOR_EXECUTION → EXECUTED → CLOSED là mô hình PROPOSED. REJECTED, EXPIRED, REVOKED và BLOCKED là nhánh độc lập. Phase này xuất DRAFT và đề xuất xác minh offline. Chưa kích hoạt transition có hiệu lực qua API/SQL.

Evaluator của Center kiểm tra hình thức draft, evidence reference, limit, endpoint và expiry. Chỉ có NOT_READY, EXPIRED và BLOCKED_BY_POLICY trong phase này. READY_FOR_REVIEW, REVOKED và APPROVED_FOR_SCHEDULED_TEST cần authority chính thức. Scope hash là fingerprint toàn draft hiện hành, chưa dùng làm canonical signed token. Chính sách cần định nghĩa canonical encoding, SoD, version pin, revoke/commit ordering và audit theo D09/D10. Không có cấu hình bật approval.
