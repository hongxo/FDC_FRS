# A3-01–A3-40

PASS chỉ áp dụng cho lớp và hành vi được ghi. SOURCE_REVIEW là kiểm tra phát hành bổ sung. Fixture session hoặc activation không được nâng lên nghiệm thu runtime. Raw evidence giữ local.

| ID | Case | Layer | Actual | Evidence | Status |
|---|---|---|---|---|---|
| A3-01 | Decision extraction | UNIT | 20 bản ghi PENDING được đối chiếu | decision-validation.json | PASS |
| A3-02 | Decision ID integrity | UNIT | D01–D12/E01–E08 theo thứ tự nguồn, không duplicate | decision-validation.json | PASS |
| A3-03 | Missing approval | UNIT | Pending reader và thiếu decisions deny | FeatureActivationGateTests | PASS |
| A3-04 | Invalid evidence | UNIT | Sai digest/scope/feature và self-asserted evidence bị chặn | FeatureActivationGateTests | PASS |
| A3-05 | Expired approval | UNIT | Hết hạn tại mốc expiry | FeatureActivationGateTests | PASS |
| A3-06 | Revoked approval | UNIT | Phát hiện revoke trước/sau verification | FeatureActivationGateTests | PASS |
| A3-07 | Default disabled | UNIT | Cả 15 gate BLOCKED_BY_POLICY | FeatureActivationGateTests | PASS |
| A3-08 | Unauthorized activation | UNIT | Không có audit/actor, sai environment hoặc version đều chặn | FeatureActivationGateTests | PASS |
| A3-09 | Anonymous access | LIVE_RUNTIME | 401 IAM page, navigation và data API | Center HTTPS TRX | PASS |
| A3-10 | Invalid credentials | UNIT | Sai password không gọi completion, không có proof | AuthenticationCompletionTests | PASS |
| A3-11 | Authorized login | LIVE_RUNTIME | Không có signed issuance policy hoặc live route | iam-login.png | BLOCKED |
| A3-12 | Session issuance | UNIT | Fixture commit khớp digest, failed commit không trả handle | AuthenticationCompletionTests | PASS |
| A3-13 | Secure cookie | LIVE_RUNTIME | Fixture cookie được inject, chưa có runtime Set-Cookie issuance | Center HTTPS TRX | BLOCKED |
| A3-14 | Session rotation | LIVE_RUNTIME | Chưa có issuer/rotation integration | Runtime report | BLOCKED |
| A3-15 | Logout | LIVE_RUNTIME | Chưa map route theo policy | Center HTTPS TRX | BLOCKED |
| A3-16 | Session revocation | LIVE_RUNTIME | Fixture bị revoke trả 401 | Center HTTPS TRX | PASS |
| A3-17 | MFA enforcement | LIVE_RUNTIME | Gate/UI deny verified; positive factor enforcement chưa có | iam-mfa.png | BLOCKED |
| A3-18 | Recovery expiry | LIVE_RUNTIME | Không phát hành recovery token | MFA/recovery report | BLOCKED |
| A3-19 | User list | LIVE_RUNTIME | 503 khi D04 pending, không rows | iam-users.png | PASS |
| A3-20 | User detail | LIVE_RUNTIME | 503 direct read | Center HTTPS TRX | PASS |
| A3-21 | Role catalog | LIVE_RUNTIME | 503 khi D04 pending | iam-roles.png | PASS |
| A3-22 | Permission catalog | LIVE_RUNTIME | 503 khi D04 pending | iam-permissions.png | PASS |
| A3-23 | Role mutation approval | LIVE_RUNTIME | POST blocked, chưa có approved workflow | Center HTTPS TRX | BLOCKED |
| A3-24 | StationGrant isolation | INTEGRATION | SQL join từ chối sai subject/assignment/grant | ManagementSqlLabTests | PASS |
| A3-25 | Cross-role mixing | UNIT | Assignment validator từ chối | ManagementProposalTests | PASS |
| A3-26 | Self-approval | UNIT | PersonReference independence | IndependentWorkflowTests | PASS |
| A3-27 | Stale authority | UNIT | Version cũ hoặc mapping stale bị chặn | FeatureActivationGateTests; OperationMappingTests | PASS |
| A3-28 | Concurrent apply/revoke | INTEGRATION | SQL commit-lock regression đạt; chưa có live apply/revoke writers | CommitGateSqlLabTests | BLOCKED |
| A3-29 | Audit correlation | INTEGRATION | Revocation audit/outbox fixture kiểm tra correlation | SessionRevocationSqlLabTests | PASS |
| A3-30 | Audit immutability | INTEGRATION | Chưa có archive/restore evidence | D10 ADR | BLOCKED |
| A3-31 | Human on machine | INTEGRATION | Boundary fixture và station route deny | PilotMachineBoundaryTests; Center HTTPS TRX | PASS |
| A3-32 | Machine on Human | UNIT | Client cert + cookie bị chặn trước SQL | Phase2SecurityTests | PASS |
| A3-33 | Direct API auth | LIVE_RUNTIME | Direct anonymous 401 và pending session 503 | Center HTTPS TRX | PASS |
| A3-34 | CSRF | UNIT | Reject cross-origin/missing proof trong primitive | Phase2SecurityTests | PASS |
| A3-35 | Default credentials | SOURCE_REVIEW | Không seed, account hoặc known secret trong phát hành | Delivery scan; diff | PASS |
| A3-36 | Production mock | SOURCE_REVIEW | Fixture providers chỉ nằm trong tests | Diff; source review | PASS |
| A3-37 | Responsive | LIVE_RUNTIME | 3 viewports và 9 views | center-live-browser.json | PASS |
| A3-38 | Full S01–S08 E2E | LIVE_RUNTIME | Chỉ policy/denial paths | center-live-browser.json | BLOCKED |
| A3-39 | Authorized data E2E | LIVE_RUNTIME | D04 signed mapping pending | Policy package | BLOCKED |
| A3-40 | Approval/Apply/Audit E2E | LIVE_RUNTIME | D04/D09/D10 activation pending | Policy package | BLOCKED |
