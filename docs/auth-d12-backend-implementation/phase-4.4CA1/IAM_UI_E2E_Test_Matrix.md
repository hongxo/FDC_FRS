# IAM-UI-01–25

PASS giới hạn theo lớp test và kết quả quan sát. Policy-denial PASS không phải authorization success. UNIT/INTEGRATION giữ nguyên lớp bằng chứng, không đổi thành live UI. Raw logs và ảnh lưu riêng dưới tmp/phase4-4ca1.

| ID | Case | Layer | Actual | Evidence | Result |
|---|---|---|---|---|---|
| IAM-UI-01 | Anonymous protected pages | LIVE_RUNTIME | 401 management, navigation, Lab | Center HTTPS TRX | PASS |
| IAM-UI-02 | Login success | LIVE_RUNTIME | No login issuance; fixture session is not login | S01 screenshot | BLOCKED |
| IAM-UI-03 | Invalid credentials | LIVE_RUNTIME | Invalid session denied; password login absent | Center HTTPS TRX | BLOCKED |
| IAM-UI-04 | MFA enforcement | LIVE_RUNTIME | Challenge not mapped | S02 screenshot | BLOCKED |
| IAM-UI-05 | Session expiry | LIVE_RUNTIME | SQL idle expiration denies protected HTML | Center HTTPS TRX | PASS |
| IAM-UI-06 | Logout/revoke | LIVE_RUNTIME | Revoked cookie denied; logout endpoint absent | Center HTTPS TRX | BLOCKED |
| IAM-UI-07 | CSRF | UNIT | Existing guard denies missing or cross-origin; no active mutation route | IAM TRX Phase2SecurityTests | PASS |
| IAM-UI-08 | Users authorization | LIVE_RUNTIME | Real API 503 with policy pending; no rows | iam-users.png | PASS |
| IAM-UI-09 | Roles authorization | LIVE_RUNTIME | Real API 503 with policy pending; no rows | iam-roles.png | PASS |
| IAM-UI-10 | Permission catalog | LIVE_RUNTIME | Real API 503 with policy pending; no rows | iam-permissions.png | PASS |
| IAM-UI-11 | Role modification | LIVE_RUNTIME | Authenticated POST denied 405; no mutation endpoint | Center HTTPS TRX | PASS |
| IAM-UI-12 | StationGrant isolation | INTEGRATION | Scoped SQL hides cross-subject fixture grants | ManagementSqlLabTests | PASS |
| IAM-UI-13 | Cross-role mixing | UNIT | Assignment mismatch denied by validator | ManagementProposalTests | PASS |
| IAM-UI-14 | Self-approval | UNIT | Independent workflow rejects same person; UI workflow absent | IndependentWorkflowTests | PASS |
| IAM-UI-15 | Stale authority version | UNIT | Stale policy/version denied by evaluator | OperationMappingTests; AuthorizationTests | PASS |
| IAM-UI-16 | Revoked session | LIVE_RUNTIME | Revoked SQL fixture returns 401 for API and page | Center HTTPS TRX | PASS |
| IAM-UI-17 | Audit evidence | LIVE_RUNTIME | Audit read denied by policy; independent archive/real audit E2E pending | iam-audit.png | BLOCKED |
| IAM-UI-18 | Human cookie on Machine | INTEGRATION | Isolated machine HTTPS contract and Human lane station-route reject | PilotMachineBoundaryTests; Center HTTPS TRX | PASS |
| IAM-UI-19 | Collector cert on Human | UNIT | Machine cert plus Human cookie rejected before SQL | Phase2SecurityTests | PASS |
| IAM-UI-20 | Direct access | LIVE_RUNTIME | Anonymous management/API denied and valid session receives policy denial | Center HTTPS TRX | PASS |
| IAM-UI-21 | Lab station scope | LIVE_RUNTIME | Anonymous 401, Human 503. No approved station-scoped Lab query | Center HTTPS TRX | BLOCKED |
| IAM-UI-22 | Default admin | SOURCE_REVIEW | No new seed, credentials, handler or default account | Diff and delivery scan | PASS |
| IAM-UI-23 | API-side auth | LIVE_RUNTIME | Direct calls still enforce checks independent of hidden menu | Center HTTPS TRX | PASS |
| IAM-UI-24 | Responsive UI | LIVE_RUNTIME | Browser at 3 viewports, no horizontal overflow | center-live-*.png | PASS |
| IAM-UI-25 | Full S01–S08 E2E | LIVE_RUNTIME | Nine views observed on denial paths only | center-live-browser.json | BLOCKED |
