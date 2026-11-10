# FDC Security Defect Register

| ID | Finding | State | Evidence / owner |
|---|---|---|---|
| A10-F01 | Fixture thử tạo alias vi phạm unique PersonReference | FIXED | run-02 FAIL, đổi thành SQL denial assertion, IAM/QA |
| A10-F02 | Lab test host thiếu storage DI | FIXED | run-03 FAIL, pending adapter thêm vào fixture |
| A10-F03 | Injected outbox trigger dùng sai schema | FIXED | run-04 FAIL, đúng ops.AuditOutbox |
| A10-F04 | Center tracker có thể giữ entity sau rollback | FIXED / regression | a10-binding-rollback.json, retry cùng scope |
| A10-F05 | Private view cần nhận biết đổi phiên và scope | FIXED / regression | fingerprint, no-store/Vary, browser cache/expiry evidence |
| A10-F06 | Windows reader đụng publishing handle của Node | FIXED / regression | b0acb06 clean run FAIL, atomic rename + bounded sharing retry |
| A10-E01 | Restore trong sandbox dùng cache khác Windows user | ENVIRONMENT_FIXED | NETSDK1064, restore local cache cùng user |
| A10-G01 | OCSP, intermediate/delta CRL và responder outage | OPEN | PKI owner, candidate supports pinned full CRL only |
| A10-G02 | Non-SA runtime/DPAPI disposable profile | OPEN / SKIP | DBA + Security, named guard và target disposition |
| A10-G03 | Multi-process queue recovery/cancel/failure matrix | TEST_GAP | DBA/QA, chưa có crash/failover evidence |
| A10-G04 | Certificate/station rebinding và bulk COMTRADE export | IMPLEMENTATION_GAP | IAM/PKI/Center owner |
| A10-G05 | Independent review và signed policy release | BLOCKED_BY_POLICY | PM/OT/Security |
| A10-G06 | Back-forward cache và pending request tại logout riêng | TEST_GAP | UI/QA, chưa có browser case độc lập |

Không phát hiện P0 mới bằng các bài đã chạy. Nhận định này cần independent review xác nhận. Readiness P0-IAM không được tự đóng.
