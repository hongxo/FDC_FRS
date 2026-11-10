# FDC Lab Workflow Implementation

Hiện có LabReadinessGate đánh giá permit proposal và luôn trả BLOCKED_BY_POLICY khi các điều kiện hình thức hợp lệ. SQL candidate 007 có InventoryProposal, PermitDraft (DRAFT/SUBMITTED) và ProposalAudit. Endpoint readiness mặc định trả 503 vì thiếu operation map đã duyệt. Chưa có Lab list/detail, synthetic transition adapter, approve/apply, result hoặc export browser-to-SQL. Cần mở rộng domain 007 bằng revision, PersonReference, station/IED FK và gate permit trong một mô hình duy nhất.
