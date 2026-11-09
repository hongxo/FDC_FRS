# Ma trận 70 ca A.8

Preconditions là fresh disposable SQL và loopback đối với browser/mTLS, fixture local đối với unit. Không tính shared opt-in SKIP vào PASS.

| ID | Input / scenario | Layer | Preconditions | Expected | Actual | Evidence | Status / blocker |
|---|---|---|---|---|---|---|---|
| A8-01 | Login/MFA dashboard | Browser / SQL | Cô lập | Login/MFA dashboard | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-02 | Station search B empty | Browser / SQL | Cô lập | Station search B empty | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-03 | SQL scoped first page | Browser / SQL | Cô lập | SQL scoped first page | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-04 | SQL scoped second page | Browser / SQL | Cô lập | SQL scoped second page | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-05 | Station A detail | Browser / SQL | Cô lập | Station A detail | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-06 | Station B detail denied | Browser / SQL | Cô lập | Station B detail denied | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-07 | IED A detail | Browser / SQL | Cô lập | IED A detail | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-08 | IED B detail denied | Browser / SQL | Cô lập | IED B detail denied | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-09 | 390px Collector view | Browser / SQL | Cô lập | 390px Collector view | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-10 | Global navigation | Browser / SQL | Cô lập | Global navigation | Chưa xác minh | A8 defect register | BLOCKED |
| A8-11 | Session expiry clears DOM | Browser / SQL | Cô lập | Session expiry clears DOM | Chưa xác minh | A8 defect register | BLOCKED |
| A8-12 | Trend scope | Browser / SQL | Cô lập | Trend scope | Chưa xác minh | A8 defect register | BLOCKED |
| A8-13 | Alert scope | Browser / SQL | Cô lập | Alert scope | Chưa xác minh | A8 defect register | BLOCKED |
| A8-14 | Relationship view | Browser / SQL | Cô lập | Relationship view | Chưa xác minh | A8 defect register | BLOCKED |
| A8-15 | Paging controls multiple visible pages | Browser / SQL | Cô lập | Paging controls multiple visible pages | Chưa xác minh | A8 defect register | BLOCKED |
| A8-16 | CFG A bytes | Browser / SQL | Cô lập | CFG A bytes | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-17 | DAT A bytes | Browser / SQL | Cô lập | DAT A bytes | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-18 | CFG SHA-256 | Browser / SQL | Cô lập | CFG SHA-256 | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-19 | DAT SHA-256 | Browser / SQL | Cô lập | DAT SHA-256 | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-20 | CFG B zero bytes | Browser / SQL | Cô lập | CFG B zero bytes | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-21 | DAT B zero bytes | Browser / SQL | Cô lập | DAT B zero bytes | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-22 | Revoke then DAT zero bytes | Browser / SQL | Cô lập | Revoke then DAT zero bytes | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-23 | Range ignored | Browser / SQL | Cô lập | Range ignored | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-24 | No-store | Browser / SQL | Cô lập | No-store | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-25 | Outbox correlation CFG and DAT | Browser / SQL | Cô lập | Outbox correlation CFG and DAT | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-26 | Wrong digest denied | Unit | Cô lập | Wrong digest denied | Đã xác minh | TRX | PASS |
| A8-27 | Wrong size denied | Unit | Cô lập | Wrong size denied | Đã xác minh | TRX | PASS |
| A8-28 | Traversal and ADS denied | Unit | Cô lập | Traversal and ADS denied | Đã xác minh | TRX | PASS |
| A8-29 | Source mutation after staging | Unit | Cô lập | Source mutation after staging | Đã xác minh | TRX | PASS |
| A8-30 | Deterministic symlink race | Browser / SQL | Cô lập | Deterministic symlink race | Chưa xác minh | A8 defect register | BLOCKED |
| A8-31 | Collector A list | Browser / SQL | Cô lập | Collector A list | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-32 | Collector B detail denied | Browser / SQL | Cô lập | Collector B detail denied | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-33 | Heartbeat A SQL response | Browser / SQL | Cô lập | Heartbeat A SQL response | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-34 | Heartbeat B denied | Browser / SQL | Cô lập | Heartbeat B denied | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-35 | Transfers A SQL response | Browser / SQL | Cô lập | Transfers A SQL response | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-36 | Transfers B denied | Browser / SQL | Cô lập | Transfers B denied | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-37 | UI heartbeat and transfers | Browser / SQL | Cô lập | UI heartbeat and transfers | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-38 | Binding edit SoD | Browser / SQL | Cô lập | Binding edit SoD | Chưa xác minh | A8 defect register | BLOCKED |
| A8-39 | Retry/error detail | Browser / SQL | Cô lập | Retry/error detail | Chưa xác minh | A8 defect register | BLOCKED |
| A8-40 | History UI paging controls | Browser / SQL | Cô lập | History UI paging controls | Chưa xác minh | A8 defect register | BLOCKED |
| A8-41 | Test CA chain | mTLS / unit | Cô lập | Test CA chain | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-42 | Wrong EKU | mTLS / unit | Cô lập | Wrong EKU | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-43 | Expired certificate | mTLS / unit | Cô lập | Expired certificate | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-44 | Bound machine upload | mTLS / unit | Cô lập | Bound machine upload | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-45 | Wrong station | mTLS / unit | Cô lập | Wrong station | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-46 | Inactive collector | mTLS / unit | Cô lập | Inactive collector | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-47 | Human cookie and spoof | mTLS / unit | Cô lập | Human cookie and spoof | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-48 | Registry unavailable | mTLS / unit | Cô lập | Registry unavailable | Đã xác minh | a8-mtls.json / TRX | PASS |
| A8-49 | CRL/OCSP revoked leaf | mTLS / unit | Cô lập | CRL/OCSP revoked leaf | Chưa xác minh | A8 defect register | BLOCKED |
| A8-50 | Machine cert on Human handler | mTLS / unit | Cô lập | Machine cert on Human handler | Đã xác minh | TRX | PASS |
| A8-51 | Lab list | Browser / SQL | Cô lập | Lab list | Chưa xác minh | A8 defect register | BLOCKED |
| A8-52 | Lab detail | Browser / SQL | Cô lập | Lab detail | Chưa xác minh | A8 defect register | BLOCKED |
| A8-53 | Lab evidence | Browser / SQL | Cô lập | Lab evidence | Chưa xác minh | A8 defect register | BLOCKED |
| A8-54 | Lab permit | Browser / SQL | Cô lập | Lab permit | Chưa xác minh | A8 defect register | BLOCKED |
| A8-55 | Lab readiness browser | Browser / SQL | Cô lập | Lab readiness browser | Chưa xác minh | A8 defect register | BLOCKED |
| A8-56 | Lab review | Browser / SQL | Cô lập | Lab review | Chưa xác minh | A8 defect register | BLOCKED |
| A8-57 | Lab approve | Browser / SQL | Cô lập | Lab approve | Chưa xác minh | A8 defect register | BLOCKED |
| A8-58 | Lab execute | Browser / SQL | Cô lập | Lab execute | Chưa xác minh | A8 defect register | BLOCKED |
| A8-59 | Lab export | Browser / SQL | Cô lập | Lab export | Chưa xác minh | A8 defect register | BLOCKED |
| A8-60 | Lab SoD E2E | Browser / SQL | Cô lập | Lab SoD E2E | Chưa xác minh | A8 defect register | BLOCKED |
| A8-61 | Scope empty after revoke | Browser / SQL | Cô lập | Scope empty after revoke | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-62 | Cookie replay denied | Browser / SQL | Cô lập | Cookie replay denied | Đã xác minh | a7-browser.json / a7-sql-verification.json | PASS |
| A8-63 | Same-assignment regression | Regression / readiness | Cô lập | Same-assignment regression | Đã xác minh | TRX | PASS |
| A8-64 | Expired grant evaluator | Regression / readiness | Cô lập | Expired grant evaluator | Đã xác minh | TRX | PASS |
| A8-65 | Authority version regression | Regression / readiness | Cô lập | Authority version regression | Đã xác minh | TRX | PASS |
| A8-66 | Export revoke race | Regression / readiness | Cô lập | Export revoke race | Chưa xác minh | A8 defect register | BLOCKED |
| A8-67 | Multi-user cache isolation | Regression / readiness | Cô lập | Multi-user cache isolation | Chưa xác minh | A8 defect register | BLOCKED |
| A8-68 | Independent signed Lab review | Regression / readiness | Cô lập | Independent signed Lab review | Chưa xác minh | A8 defect register | BLOCKED |
| A8-69 | Native simulator loopback | Regression / readiness | Cô lập | Native simulator loopback | Đã xác minh | native-simulated.json | PASS |
| A8-70 | Production acceptance | Regression / readiness | Cô lập | Production acceptance | Chưa xác minh | A8 defect register | BLOCKED |
