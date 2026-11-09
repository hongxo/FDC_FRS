# S01/S04 Login và Session

S01 /identity/#login có trường username/password vô hiệu hóa. GET /api/identity/v1/ui/auth-status chỉ trả trạng thái BLOCKED_BY_POLICY, không có body request, lookup hay Set-Cookie. Không có POST phát hành phiên. S04 /identity/#profile có trạng thái chung và mô tả session/logout pending, không trả identity payload.

AuthenticationService, password proof, session reader và revocation primitives được tái sử dụng làm baseline. D02/D05/D09/D10 phải chốt credential verification, account status, throttle, rotation, revocation, MFA continuation, audit và issuance transaction. Browser không lưu token. Test expiry/revoke dùng fixture rollback-only, không dùng tài khoản thật.
