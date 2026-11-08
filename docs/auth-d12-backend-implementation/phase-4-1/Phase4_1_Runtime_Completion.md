# Phase 4.1 — code runtime đã bổ sung

2026-11-08 (Asia/Bangkok). Baseline `415cca4cb032528928518701e5606c47d37cd363`; source/test content `8848dd9ad716ba12a6445f8549ffbe5522c0a9cb`. Development/Lab only. APPROVAL/readiness không thay đổi.

## Group1: SQL transaction gate thực tế

Application/SqlCommitGatePolicy.cs và Infrastructure/SqlAuthorityCommitBoundary.cs triển khai IAuthorizedCommitBoundary cũ, không tạo IAM thứ hai. Plan bắt buộc từ trusted server provider, không có policy defaults cho runtime. Plan validate actor, prefix, bounded lock timeout/isolation; merge duplicate user lock sang mode mạnh nhất và sort GUID canonical trước acquire. Cấm transaction lồng/ambient và pending entity changes từ ngoài gate.

Gate kiểm tra writable primary, tự BEGIN transaction, lấy Policy gate trước user gates bằng parameterized sys.sp_getapplock với LockOwner=Transaction. Negative/invalid return denies; callback phải recheck current session/MFA/permission/scope/SoD và ghi required effect/audit trong cùng DbContext. Lock giữ qua commit. Failure rollback + clear tracker; lỗi khi commit trả CommitOutcomeUnknownException, không tự retry/khẳng định rollback đã undo commit.

7 SQL tests trên non-authoritative unique AuthorityVersion probe keys kiểm chứng hai thứ tự mutation/revoke, global Policy Exclusive chặn actor khác, lock timeout không callback, rollback, reject nesting và unknown ACK sau actual commit. SQL DMV WAIT + TaskCompletionSource barriers là ordering evidence; polling delay không là proof. Probe PolicyConfigured=false, key khác Center, namespace FdcTest unique; tạo/update/commit probe rồi xóa đúng key trong finally. Không tạo user/role/grant cho gate tests.

Cơ chế tham chiếu [Microsoft sp_getapplock](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql): transaction owner và kiểm mã trả về là yêu cầu API. Tài liệu này không phê duyệt D09. Unknown-ACK dùng test interceptor sau actual commit, không là chứng minh SQL failover/network partition.

## Group2: SQL own-session revocation

Application/SessionRevocationPolicy.cs và Infrastructure/SqlSessionRevocationService.cs thêm revoke-current/everywhere. Không nhận arbitrary target UserId: subject lấy từ current SQL session digest, valid versions/epoch/time và kiểm lại trong gate. Cấm unknown operation. Provided PendingSessionRevocationPolicy chặn trước connect; real operation-specific assurance/MFA policy vẫn phải owner chọn.

Current revoke không revoke các session khác hoặc tăng SecurityVersion. Everywhere tăng User.SecurityVersion, revoke session cùng subject và consume pending challenges; không ảnh hưởng người khác, không cấp role/grant. Session/security/challenge changes + sanitized SecurityEvent/AuditOutbox trong một SaveChanges ở transaction gate. Stale/unknown/revoked proof không chọn user khác; lỗi outbox fail-closed và SQL savepoint rollback được kiểm chứng. No raw token/password/OTP ghi vào audit.

3 SQL tests dùng rollback-only account/session fixtures và test-only authority/boundary để kiểm chứng DML/atomicity. Chúng chưa chứng minh service được wire với candidate gate và actual approved MFA/session policy trong Center. Không thêm logout endpoint/cookie-clear result giả khi SQL không thành công.

## Group3/4: HTTP/SQL integration và reliability

HumanHttpSqlLabTests dùng actual Kestrel HTTPS loopback, actual HumanSessionHandler, actual SqlSessionAuthorityReader và canonical role GET adapter. Không mock authentication/SQL rows. Authenticated fixture tới mandatory D04 gate và503; không cookie/forged header/invalid/disabled/revoked proof401; actual SQL connection failure503. Đây là negative integration slice; không phải Login/MFA issuance hay full UI E2E.

Test host/certificate chỉ trong test project: loopback random port; test peer pin theo đúng cert; không install trust store hoặc export key/cert file. Windows TLS harness cần disposable user-key-context import + server-auth EKU; retest PASS. Không thay Fdc.Center listener, reverse proxy, firewall hoặc PKI production.

Unknown-ACK test hook ném lỗi sau actual SQL COMMIT; fresh DB read thấy probe effect tồn tại, caller nhận UnknownOutcome. Đây là điều kiện cần xử lý idempotency thay vì auto-retry, không đóng HA/recovery gate.

## Scope chưa hoàn tất

Không host registration, real LoginTransactionStore/MFA/recovery, D04 mapping provider, full User/UserRole/StationGrant/delegation workflow, D10 schema/archive hay live S01–S08 activation. Existing RoleCrud/read/evaluator được tái sử dụng; không hardcode role names/codes, seed user/password/grants hoặc wildcard admin. Migration created/applied:0. PendingD09CommitBoundary và mọi policy pending vẫn còn. Runtime integration không được suy từ build library hoặc test fixtures.

New source/test files: Application/SqlCommitGatePolicy.cs, Application/SessionRevocationPolicy.cs; Infrastructure/SqlAuthorityCommitBoundary.cs, SqlSessionRevocationService.cs; tests CommitGatePlanTests.cs, CommitGateSqlLabTests.cs, SessionRevocationTests.cs, SessionRevocationSqlLabTests.cs, HumanHttpSqlLabTests.cs. .gitignore thay đổi trong delivery checkout để giữ raw evidence local. Center/Collector/IED/SCADA và schema001 không đổi.
