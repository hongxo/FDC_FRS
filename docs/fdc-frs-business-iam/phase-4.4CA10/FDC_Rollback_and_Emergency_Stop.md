# FDC Rollback and Emergency Stop

Rollback candidate 010 → 009 → 008 → 007 yêu cầu owned A.10 marker và transaction. Migration 007 từ chối khi còn proposal/evidence. Test chỉ xóa đúng synthetic inventory của fixture trống rồi chứng minh schema tùy chọn được đảo. Có raw a10-migration-rollback.json.

Khi audit/outbox hoặc authority không khả dụng, không release artifact hay trả thành công mutation. Candidate queue giữ payload private cho tới release recheck. Khi stopped, không có device adapter cần ngắt trong slice hash-plan này. Emergency stop của shared Lab cần OT owner đặt tên và quy trình riêng.

Không áp down migration lên dữ liệu đã có evidence để đổi readiness. Với shared deployment tương lai, owner phải chọn rollback binary hoặc forward fix, backup/restore point, durable queue reconciliation và preserve audit theo D09/D10.
