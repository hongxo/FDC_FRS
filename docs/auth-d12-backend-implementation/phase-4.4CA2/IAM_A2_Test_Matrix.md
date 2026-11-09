# A2-01–A2-30

PASS chỉ áp dụng cho phạm vi ghi rõ ở cột Actual. Negative-path PASS không chứng nhận flow thành công. SOURCE_REVIEW bổ sung kiểm tra cấu hình phát hành. Raw TRX và ảnh trong tmp/phase4-4ca2.

| ID | Case | Layer | Actual | Evidence | Result |
|---|---|---|---|---|---|
| A2-01 | Anonymous | LIVE_RUNTIME | 401 shell và API | Center HTTPS TRX | PASS |
| A2-02 | Issuance gate | LIVE_RUNTIME | GET policy BLOCKED, POST absent, không Set-Cookie | Center HTTPS TRX; iam-login.png | PASS |
| A2-03 | Invalid credential | LIVE_RUNTIME | Credential verifier chưa được mở; không nhận password | Center HTTPS TRX | BLOCKED |
| A2-04 | MFA gate | LIVE_RUNTIME | MFA disabled và không route verify | Center HTTPS TRX; iam-mfa.png | PASS |
| A2-05 | Session expiry | LIVE_RUNTIME | SQL idle expired trả 401 | Center HTTPS TRX | PASS |
| A2-06 | Session revocation | LIVE_RUNTIME | Revoked fixture cookie trả 401 | Center HTTPS TRX | PASS |
| A2-07 | Logout | LIVE_RUNTIME | Không có logout route có hiệu lực | Center HTTPS TRX | BLOCKED |
| A2-08 | CSRF | UNIT | Guard từ chối thiếu proof hoặc cross-origin | Phase2SecurityTests | PASS |
| A2-09 | User list | LIVE_RUNTIME | 503 policy denial, không dữ liệu | iam-users.png | PASS |
| A2-10 | User detail | LIVE_RUNTIME | 503 ở request trực tiếp | Center HTTPS TRX | PASS |
| A2-11 | Role catalog | LIVE_RUNTIME | 503 policy denial, không dữ liệu | iam-roles.png | PASS |
| A2-12 | Permission catalog | LIVE_RUNTIME | 503 policy denial, không dữ liệu | iam-permissions.png | PASS |
| A2-13 | Role mutation | LIVE_RUNTIME | POST trả 405, không có mutation | Center HTTPS TRX | PASS |
| A2-14 | StationGrant isolation | INTEGRATION | SQL không trả grant/functions sai assignment; live read 503 | ManagementSqlLabTests; Center TRX | PASS |
| A2-15 | Cross-role mixing | UNIT | Từ chối proposal khác assignment | ManagementProposalTests | PASS |
| A2-16 | Self-approval | UNIT | Independent workflow từ chối cùng PersonReference | IndependentWorkflowTests | PASS |
| A2-17 | Stale authority | UNIT | Stale version hoặc mapping bị từ chối | OperationMappingTests; AuthorizationTests | PASS |
| A2-18 | Concurrent authority | INTEGRATION | SQL commit gate fixture serialize, không bật production writers | CommitGateSqlLabTests | PASS |
| A2-19 | Audit correlation | LIVE_RUNTIME | Read adapter có correlation DTO; chưa có correlation qua live approval/apply/outbox | iam-audit.png | BLOCKED |
| A2-20 | Audit immutability | INTEGRATION | Chưa có archive/restore evidence D10 | D10 ADR | BLOCKED |
| A2-21 | Human on machine | INTEGRATION | Machine boundary và Human lane phủ nhận | PilotMachineBoundaryTests; Center TRX | PASS |
| A2-22 | mTLS on Human | UNIT | Collector certificate với cookie bị chặn trước SQL | Phase2SecurityTests | PASS |
| A2-23 | Direct API access | LIVE_RUNTIME | Anonymous 401, fixture session không vượt pending authority | Center HTTPS TRX | PASS |
| A2-24 | Lab station scope | LIVE_RUNTIME | 401/503; không có authorized station-scoped read | Center HTTPS TRX | BLOCKED |
| A2-25 | Responsive | LIVE_RUNTIME | Browser 3 viewport, ảnh 9 views | center-live-browser.json | PASS |
| A2-26 | Default credentials | SOURCE_REVIEW | Không có account/secret hoặc seed trong diff | Delivery scan | PASS |
| A2-27 | Mock runtime | SOURCE_REVIEW | Code runtime gọi API thật, không có fixture data trong UI | Diff; browser traffic | PASS |
| A2-28 | Full S01–S08 E2E | LIVE_RUNTIME | Chỉ denial/blocked paths | center-live-browser.json | BLOCKED |
| A2-29 | Business-data E2E | LIVE_RUNTIME | Chưa có policy mapping ký | Policy register | BLOCKED |
| A2-30 | Approval/Apply/Audit | LIVE_RUNTIME | Chưa có writer authority và D10 acceptance | Policy register | BLOCKED |
