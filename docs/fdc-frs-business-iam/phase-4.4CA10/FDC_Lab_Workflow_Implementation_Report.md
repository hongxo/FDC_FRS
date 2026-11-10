# FDC Lab Workflow Implementation Report

Lab workflow mở rộng chính bảng labprep.PermitDraft của candidate 007. Migration 008 mở rộng DraftStatus, thêm Station/IED FK, PersonReference, permit/result digest, history và idempotency result. Các row proposal cũ không có IsSynthetic=true được loại khỏi runtime candidate. Migration 009 tạo queue export SQL. Migration 010 bổ sung request/history cho synthetic Collector registry.

Luồng mô phỏng gồm DRAFT → SUBMITTED → REVIEWED → APPROVED → READY → SYNTHETIC_EXECUTION → COMPLETED/FAILED/ABORTED. Kế hoạch và evidence manifest bất biến trong revision. ETag bảo vệ chuyển trạng thái. Retry cùng actor/request/digest trả kết quả history đã lưu. Kịch bản FAILED/ABORTED là kết quả mô phỏng định trước trong plan, không phải sự kiện từ IED.

LabWorkflowService dùng canonical SQL session, UserRole, GrantFunction, same-assignment và StationGrant qua BusinessAuthorization. SqlAuthorityCommitBoundary phải có plan serializable trước khi đọc authority và mutation. Permit scope, IED synthetic, validity, actor, PersonReference và revision được kiểm tra. Effect, workflow history, audit và outbox cùng transaction. Denied mutation ghi audit/outbox trong gate transaction riêng sau rollback. Tracker của Center được xóa khi callback thất bại để ngăn resave trạng thái đã rollback.

API /api/lab/v1/tests có list/detail/history, tạo draft, transition, sync JSON export và durable export request/download. Mutation kiểm tra HTTPS Human, exact approved origin, CSRF, Idempotency-Key và If-Match. UI lab.html có plan/evidence metadata, permit, readiness, submit/review/approve, synthetic apply, result và history.

Startup giữ PendingBusinessOperationPolicy, PendingSqlCommitGatePlanProvider và PendingLabHttpOrigin. Các mapping a10.fixture.* và actor sessions chỉ có trong test assembly/owned DB. Mã execution chỉ hash plan, không nhận IED adapter hoặc network client. Cần D04 duyệt mã và operation mapping trước runtime release. Lab evidence hiện là manifest digest trong plan cùng JSON export, chưa phải kho upload arbitrary evidence có retention nghiệp vụ.
