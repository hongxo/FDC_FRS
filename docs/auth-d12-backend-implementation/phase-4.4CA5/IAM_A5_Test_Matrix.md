# Ma trận A5-01–A5-50

Browser/SQL chỉ chạy trên HTTPS loopback, fixture rollback. Integration và Unit dùng regression theo phạm vi. Readiness yêu cầu hồ sơ độc lập. Browser = a5-browser.json, SQL = a5-sql-verification.json. Raw TRX giữ local. BLOCKED gồm các ca chưa đủ thành phần hoặc chưa được thực thi trong phạm vi đợt này.

| ID | Scenario | Layer | Preconditions / steps | Expected | Actual | Evidence | Status | Blocker |
|---|---|---|---|---|---|---|---|---|
| A5-01 | Login HTTPS | Browser/SQL | POST login from loopback | 202 | 202 MFA_REQUIRED | browser | PASS | — |
| A5-02 | Anonymous profile | Browser/SQL | GET profile without session | 401 | 401 | browser | PASS | — |
| A5-03 | Missing CSRF | Browser/SQL | POST login without header | 403 | 403 | browser | PASS | — |
| A5-04 | Valid CSRF | Browser/SQL | GET token and submit same-origin | Request accepted | 202 | browser | PASS | — |
| A5-05 | Wrong password | Browser/SQL | POST invalid password | Generic 401 | 401 | browser | PASS | — |
| A5-06 | Unknown account | Browser/SQL | POST unknown subject | Same generic 401 | Matches wrong password | browser | PASS | — |
| A5-07 | Locked account | Isolated E2E | Attempt login locked account | Generic deny | Fixture has no lockout store | policy | BLOCKED | policy |
| A5-08 | Distributed throttle | Isolated E2E | Exceed source/account limit on two nodes | Deny | Provider pending | policy | BLOCKED | policy |
| A5-09 | Malformed JSON | Browser/SQL | POST malformed JSON | 400 | Not executed as separate browser case | contract | BLOCKED | contract |
| A5-10 | Oversize request | Browser/SQL | POST body above limit | Deny | Not executed as separate browser case | contract | BLOCKED | contract |
| A5-11 | Spoofed proxy | Integration | Send forged forwarded identity | 401 | Denied by Center | Center TRX | PASS | — |
| A5-12 | HTTP cookie protection | Browser/SQL | Inspect issued cookie flags | Secure HttpOnly Strict | Flags verified | browser | PASS | — |
| A5-13 | MFA required | Browser/SQL | Complete password only | No session cookie | No cookie | browser | PASS | — |
| A5-14 | MFA wrong factor | Browser/SQL | Submit invalid test factor | 401 | 401 | browser | PASS | — |
| A5-15 | MFA correct | Browser/SQL | Submit generated test factor | 204 and cookie | 204 | browser | PASS | — |
| A5-16 | MFA challenge replay | Browser/SQL | Repeat consumed challenge after logout | 401 | 401 | browser | PASS | — |
| A5-17 | Challenge expiry | Browser/SQL | Advance past expiry | 401 | No separate expiry browser case | contract | BLOCKED | contract |
| A5-18 | Direct API bypass | Browser/SQL | GET profile before MFA | 401 | 401 | browser | PASS | — |
| A5-19 | Session validation | Browser/SQL | GET profile after MFA | 200 via SQL | 200 and fixture subject | browser + SQL | PASS | — |
| A5-20 | Logout | Browser/SQL | POST logout with authenticated CSRF | 204 and clear cookie | 204 + cleared | browser + SQL | PASS | — |
| A5-21 | Cookie replay | Browser/SQL | Restore revoked cookie | 401 | 401 | browser + SQL | PASS | — |
| A5-22 | Session expiry | Integration | Expire fixture SQL idle deadline | 401 | 401 | Center TRX | PASS | — |
| A5-23 | User list | Browser/SQL | GET users using scoped fixture | Only visible user | 200, expected ID | browser | PASS | — |
| A5-24 | User detail | Browser/SQL | GET authorized subject | 200 | 200 | browser | PASS | — |
| A5-25 | Cross-user access | Browser/SQL | GET out-of-scope user ID | 404 | 404 | browser | PASS | — |
| A5-26 | User search | Integration | Search hidden subject under visible scope | No results | Empty | IAM TRX | PASS | — |
| A5-27 | User filter | Browser/SQL | Apply status filter through UI | Filtered rows | Not exercised in A.5 browser | UI | BLOCKED | UI |
| A5-28 | User paging | Integration | Page with limited visibility | Scope precedes paging | Verified | IAM TRX | PASS | — |
| A5-29 | Role catalog | Browser/SQL | GET roles | Scoped role | 200, one role | browser | PASS | — |
| A5-30 | Permission catalog | Browser/SQL | GET catalog | Scoped permission | 200, expected code | browser | PASS | — |
| A5-31 | Role permission matrix | Browser/SQL | GET role permissions | Mapped permission | 200, expected code | browser | PASS | — |
| A5-32 | Effective preview | Isolated E2E | Evaluate effective object preview in UI | Correct version/scope | No approved UI/API | policy | BLOCKED | policy |
| A5-33 | Station assignment binding | Integration | Query with wrong assignment | Empty | Empty | IAM TRX | PASS | — |
| A5-34 | Cross-role mixing | Unit | Evaluate mixed assignment | Deny | Denied | IAM TRX | PASS | — |
| A5-35 | Cross-station object deny | Unit | Check mismatching trusted object station | Deny | Denied | IAM TRX | PASS | — |
| A5-36 | Expired grant | Browser/SQL | Use expired StationGrant | Deny | Not run in browser | station E2E | BLOCKED | station E2E |
| A5-37 | Revoked grant | Browser/SQL | Revoke and retry StationGrant | Deny | Not run in browser | station E2E | BLOCKED | station E2E |
| A5-38 | Independent approver | Unit | Approve by same person | Deny | Denied | IAM TRX | PASS | — |
| A5-39 | Stale approval | Integration | Apply with stale revision | Deny | Denied in fixture | IAM TRX | PASS | — |
| A5-40 | Multi-node apply/revoke | Isolated E2E | Race writers across nodes | Serialized outcome | All-writer deployment pending | D09 | BLOCKED | D09 |
| A5-41 | Audit outbox | Browser/SQL | Inspect login/logout event joins | Two events with outbox | Verified | SQL | PASS | — |
| A5-42 | Outbox failure | Integration | Force outbox failure | Roll back effect | Verified | IAM TRX | PASS | — |
| A5-43 | SQL outage | Integration | Read protected API with unavailable authority | 503 | 503 | Center TRX | PASS | — |
| A5-44 | Human/Machine boundary | Integration | Use Human cookie on machine ingress | Deny | Denied | Center TRX | PASS | — |
| A5-45 | Archive tamper | Integration | Alter archive and verify digest | Detect | No trusted archive adapter | D10 | BLOCKED | D10 |
| A5-46 | Restore audit | Integration | Restore archive and reconcile | Verified evidence | No run | D10 | BLOCKED | D10 |
| A5-47 | Signed Lab activation | Readiness | Verify real release and activate | Approved scope and audit | No signed release | Lab report | BLOCKED | Lab report |
| A5-48 | Authorized Lab E2E | Readiness | Run S01–S08 with real roles | Independent acceptance | Not authorized | Lab report | BLOCKED | Lab report |
| A5-49 | Production restore | Readiness | Run SQL/DP restore drill | Verified RPO/RTO | No run | Production report | BLOCKED | Production report |
| A5-50 | Production release | Readiness | Verify UAT and explicit change authorization | GO if approved | NO-GO | Production report | BLOCKED | Production report |
