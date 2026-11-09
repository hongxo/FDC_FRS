# A4-01–A4-45

PASS chỉ áp dụng cho lớp và hành vi ghi trong bảng. Unit success của login/session không phải isolated full E2E. Báo cáo credential SQL reader riêng ở integration.

| ID | Case | Layer | Actual | Evidence | Status |
|---|---|---|---|---|---|
| A4-01 | Approval validation | UNIT | ECDSA fixture valid only for exact signed context | SignedPolicyReleaseEvidenceTests | PASS |
| A4-02 | Fail closed | UNIT | 15 pending features deny | FeatureActivationGateTests | PASS |
| A4-03 | Wrong scope | UNIT | Wrong scope/ref/SHA/environment rejected | SignedPolicyReleaseEvidenceTests | PASS |
| A4-04 | Expired approval | UNIT | Expiry rejected | FeatureActivationGateTests | PASS |
| A4-05 | Revoked approval | UNIT | Revoked key/snapshot rejected | SignedPolicyReleaseEvidenceTests; FeatureActivationGateTests | PASS |
| A4-06 | Invalid signature | UNIT | Modified signature and payload rejected | SignedPolicyReleaseEvidenceTests | PASS |
| A4-07 | Isolated login | INTEGRATION | AuthenticationService + canonical credential SQL + fixture session/outbox; transaction rolled back | SessionRevocationSqlLabTests | PASS |
| A4-08 | Invalid password | UNIT | Reject before completion | AuthenticationCompletionTests | PASS |
| A4-09 | Locked account | ISOLATED_E2E | Reviewed lockout SQL provider pending | Login report | BLOCKED |
| A4-10 | Rate limiting | ISOLATED_E2E | Policy, distributed limits pending | Login report | BLOCKED |
| A4-11 | MFA challenge | UNIT | Opaque challenge distinct from session proof | AuthenticationCompletionTests | PASS |
| A4-12 | MFA replay | ISOLATED_E2E | No active verifier/factor/replay store | Login report | BLOCKED |
| A4-13 | Session issuance | INTEGRATION | SQL fixture digest matches proof; HTTP cookie issuance still blocked | SessionRevocationSqlLabTests; AuthenticationCompletionTests | PASS |
| A4-14 | Session rotation | ISOLATED_E2E | Issuer and rotation policy pending | Login report | BLOCKED |
| A4-15 | Logout | ISOLATED_E2E | No production or Lab logout route | Center HTTPS TRX | BLOCKED |
| A4-16 | Session revoke | INTEGRATION | SQL revoke plus expired/revoked session denials | SessionRevocationSqlLabTests; Center TRX | PASS |
| A4-17 | CSRF | UNIT | Cross-origin and missing proof denied | Phase2SecurityTests | PASS |
| A4-18 | Secure cookie | ISOLATED_E2E | Injected secure cookie is not issuance validation | Center HTTPS TRX | BLOCKED |
| A4-19 | User list | INTEGRATION | SQL scoped fixture lists only visible user | ManagementSqlLabTests | PASS |
| A4-20 | User detail | INTEGRATION | Scoped detail hides other users, live mapping pending | ManagementSqlLabTests; Center TRX | PASS |
| A4-21 | Role list | ISOLATED_E2E | Live authority 503; no authorized role list browser proof | Center HTTPS TRX | BLOCKED |
| A4-22 | Permission catalog | INTEGRATION | Canonical scoped catalog SQL fixture | ManagementSqlLabTests | PASS |
| A4-23 | Role-permission matrix | ISOLATED_E2E | Mutation fixture exists; authorized effective matrix pending | RoleCrudSqlLabTests | BLOCKED |
| A4-24 | Unauthorized role change | INTEGRATION | Center rejects POST without route | Center HTTPS TRX | PASS |
| A4-25 | StationGrant isolation | INTEGRATION | SQL wrong subject and assignment return no functions | ManagementSqlLabTests | PASS |
| A4-26 | Cross-role mixing | UNIT | Same-assignment validator denies | ManagementProposalTests | PASS |
| A4-27 | Self-approval | UNIT | PersonReference independence checked | IndependentWorkflowTests | PASS |
| A4-28 | Stale authority | UNIT | Version and scope stale denied | FeatureActivationGateTests; OperationMappingTests | PASS |
| A4-29 | Concurrent apply/revoke | ISOLATED_E2E | Commit-lock fixture only; all writers and multi-node pending | CommitGateSqlLabTests | BLOCKED |
| A4-30 | Audit/outbox atomic | INTEGRATION | Outbox failure rolls back revoke and role changes | SessionRevocationSqlLabTests; RoleCrudSqlLabTests | PASS |
| A4-31 | Audit tamper | PRODUCTION_READINESS | Archive custody and D10 validation pending | Audit report | BLOCKED |
| A4-32 | Archive restore | PRODUCTION_READINESS | Restore exercise not executed | Rollback runbook | BLOCKED |
| A4-33 | Human/Machine | INTEGRATION | Machine lane rejects human proof, Human lane denies station API | PilotMachineBoundaryTests; Center TRX | PASS |
| A4-34 | Proxy spoofing | INTEGRATION | Forged headers do not create Human session | Center HTTPS TRX | PASS |
| A4-35 | Certificate mismatch | UNIT | Certificate + Human cookie denied before SQL | Phase2SecurityTests | PASS |
| A4-36 | Multi-node session | PRODUCTION_READINESS | No multi-node approved topology or drill | Checklist | BLOCKED |
| A4-37 | SQL outage | INTEGRATION | Unavailable authority returns 503 | Center HTTPS TRX | PASS |
| A4-38 | DP rotation | PRODUCTION_READINESS | Key custody and rotation tests pending | Checklist | BLOCKED |
| A4-39 | Responsive | ISOLATED_E2E | 9 denial views, 3 viewports | center-live-browser.json | PASS |
| A4-40 | Default admin | PRODUCTION_READINESS | No new admin/seed in diff | Delivery scan and diff | PASS |
| A4-41 | Runtime mock data | PRODUCTION_READINESS | Fixture keys/accounts only in tests | Diff and browser traffic | PASS |
| A4-42 | Lab login E2E | LAB_LIVE_E2E | G2/G3 blocked | Lab readiness | BLOCKED |
| A4-43 | Lab RBAC E2E | LAB_LIVE_E2E | G2/G3 blocked | Lab readiness | BLOCKED |
| A4-44 | Lab approval E2E | LAB_LIVE_E2E | G2/G3 blocked | Lab readiness | BLOCKED |
| A4-45 | Production review | PRODUCTION_READINESS | No UAT/release/sign-off/restore drill | Checklist | BLOCKED |
