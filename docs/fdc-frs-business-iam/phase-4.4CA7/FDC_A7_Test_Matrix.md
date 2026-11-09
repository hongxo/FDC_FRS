# Ma trận A7-01–A7-60

Preconditions cho Browser/SQL là HTTPS loopback, fresh disposable SQL và synthetic data. Unit chạy local với fixture hoặc canonical evaluator. Các ca chưa có endpoint, chưa chạy riêng hoặc cần phê duyệt ghi BLOCKED, không tính PASS.

| ID | Scenario | Layer | Input / steps | Expected | Actual | Evidence | Status / blocker |
|---|---|---|---|---|---|---|---|
| A7-01 | Dashboard tab | Browser | Click Tổng quan | 1 authorized station | 1 authorized station | a7-browser.json / a7-sql-verification.json | PASS |
| A7-02 | Stations tab | Browser | Click Trạm | Only Station A | Only Station A | a7-browser.json / a7-sql-verification.json | PASS |
| A7-03 | Station detail | Browser | Click Station A | Scoped detail heading | Scoped detail heading | a7-browser.json / a7-sql-verification.json | PASS |
| A7-04 | COMTRADE detail | Browser | Click record row | Scoped file link | Scoped file link | a7-browser.json / a7-sql-verification.json | PASS |
| A7-05 | Visible link target | Browser | Inspect DAT href | Exact file ID | Exact file ID | a7-browser.json / a7-sql-verification.json | PASS |
| A7-06 | Permission-aware global navigation | Browser | Open main Center navigation | Only allowed modules | Chưa xác minh | A7 defect register | BLOCKED |
| A7-07 | Search by name | Browser | Submit Station B search | Empty scope | Chưa xác minh | A7 defect register | BLOCKED |
| A7-08 | Paging UI | Browser | Navigate multiple pages | No leak | Chưa xác minh | A7 defect register | BLOCKED |
| A7-09 | Responsive review | Browser | Run mobile A.7 view | Usable view | Chưa xác minh | A7 defect register | BLOCKED |
| A7-10 | Session expired UI | Browser | Click after expiry | Clear results + 401 state | Chưa xác minh | A7 defect register | BLOCKED |
| A7-11 | Scoped count | Browser/SQL | GET Dashboard | Count A only | Count A only | a7-browser.json / a7-sql-verification.json | PASS |
| A7-12 | Station A direct | Browser/SQL | GET A ID | 200 scoped object | 200 scoped object | a7-browser.json / a7-sql-verification.json | PASS |
| A7-13 | Station B direct | Browser/SQL | GET B ID | 404 no object | 404 no object | a7-browser.json / a7-sql-verification.json | PASS |
| A7-14 | IED A direct | Browser/SQL | GET A IED | 200 scoped object | 200 scoped object | a7-browser.json / a7-sql-verification.json | PASS |
| A7-15 | IED B direct | Browser/SQL | GET B IED | 404 no object | 404 no object | a7-browser.json / a7-sql-verification.json | PASS |
| A7-16 | IED list | Browser/SQL | GET list | A only | A only | a7-browser.json / a7-sql-verification.json | PASS |
| A7-17 | Station list | Browser/SQL | GET list | A only | A only | a7-browser.json / a7-sql-verification.json | PASS |
| A7-18 | Trends | Browser/SQL | GET scoped trend | A only | Chưa xác minh | A7 defect register | BLOCKED |
| A7-19 | Alerts | Browser/SQL | GET alerts | A only | Chưa xác minh | A7 defect register | BLOCKED |
| A7-20 | Station relationships | Browser/SQL | Open joined station relationships | Only owned objects | Chưa xác minh | A7 defect register | BLOCKED |
| A7-21 | Record A metadata | Browser/SQL | GET record A | 200 | 200 | a7-browser.json / a7-sql-verification.json | PASS |
| A7-22 | Record B metadata | Browser/SQL | GET record B | 404 | 404 | a7-browser.json / a7-sql-verification.json | PASS |
| A7-23 | Record list | Browser/SQL | GET records | A only | A only | a7-browser.json / a7-sql-verification.json | PASS |
| A7-24 | DAT download | Browser/SQL | GET file A | 200 with verified bytes | 200 with verified bytes | a7-browser.json / a7-sql-verification.json | PASS |
| A7-25 | CFG download | Browser/SQL | GET CFG fixture | 200 verified bytes | Chưa xác minh | A7 defect register | BLOCKED |
| A7-26 | B file ID | Browser/SQL | GET file B | 404 and zero bytes | 404 and zero bytes | a7-browser.json / a7-sql-verification.json | PASS |
| A7-27 | Digest match | Browser/SQL | Hash response bytes | Matches SHA-256 | Matches SHA-256 | a7-browser.json / a7-sql-verification.json | PASS |
| A7-28 | Wrong digest | Unit | Provide mismatched digest | No stream | No stream | TRX | PASS |
| A7-29 | Wrong size | Unit | Provide mismatched size | No stream | No stream | TRX | PASS |
| A7-30 | Cap | Unit | Request file beyond cap | No stream | No stream | TRX | PASS |
| A7-31 | Traversal | Unit | Use parent segment | No stream | No stream | TRX | PASS |
| A7-32 | Alternate stream | Unit | Use ADS suffix | No stream | No stream | TRX | PASS |
| A7-33 | Symlink race | Integration | Swap link during access | No escape | Chưa xác minh | A7 defect register | BLOCKED |
| A7-34 | Range/no-store | Browser/SQL | Send Range | Full 200, no Content-Range and no-store | Full 200, no Content-Range and no-store | a7-browser.json / a7-sql-verification.json | PASS |
| A7-35 | Download outbox | Browser/SQL | Join prepared event and outbox | Correlation matches HTTP | Correlation matches HTTP | a7-browser.json / a7-sql-verification.json | PASS |
| A7-36 | Collector list | Browser/SQL | GET collectors | A only | A only | a7-browser.json / a7-sql-verification.json | PASS |
| A7-37 | Collector A detail | Browser/SQL | GET Collector A | 200 | 200 | a7-browser.json / a7-sql-verification.json | PASS |
| A7-38 | Collector B detail | Browser/SQL | GET Collector B | 404 | 404 | a7-browser.json / a7-sql-verification.json | PASS |
| A7-39 | Heartbeat history | Browser/SQL | Read timeline | Scoped only | Chưa xác minh | A7 defect register | BLOCKED |
| A7-40 | Transfer history | Browser/SQL | Read transfers | Scoped only | Chưa xác minh | A7 defect register | BLOCKED |
| A7-41 | Machine mTLS ingest | Integration | Run SQL-backed mTLS intake | Scoped acceptance | Chưa xác minh | A7 defect register | BLOCKED |
| A7-42 | Human cookie on machine | Integration | Use Human cookie on machine route | Denied | Chưa xác minh | A7 defect register | BLOCKED |
| A7-43 | Machine certificate on Human | Unit | Supply client cert and Human cookie | Denied before SQL | Denied before SQL | TRX | PASS |
| A7-44 | Lab list | Browser | Read allowed result | A only | Chưa xác minh | A7 defect register | BLOCKED |
| A7-45 | Lab details | Browser | Read B result | 404 | Chưa xác minh | A7 defect register | BLOCKED |
| A7-46 | Lab evidence | Browser | Read out-of-scope evidence | Denied | Chưa xác minh | A7 defect register | BLOCKED |
| A7-47 | Lab permit | Browser | Review readiness | Permit scoped | Chưa xác minh | A7 defect register | BLOCKED |
| A7-48 | Lab execute | Browser | Attempt without permit | Denied | Chưa xác minh | A7 defect register | BLOCKED |
| A7-49 | Lab approval | Browser | Review own request | Denied | Chưa xác minh | A7 defect register | BLOCKED |
| A7-50 | Lab export | Browser | Export absent permission | Denied | Chưa xác minh | A7 defect register | BLOCKED |
| A7-51 | Missing permission | SQL | System Admin fixture with StationGrant only | Denied | Denied | a7-role-sql.json | PASS |
| A7-52 | Missing grant | SQL | Security Admin fixture with permission only | Empty | Empty | a7-role-sql.json | PASS |
| A7-53 | No download permission | SQL | Viewer fixture metadata access | Download discovery denied | Download discovery denied | a7-role-sql.json | PASS |
| A7-54 | Cross-role mix | Unit | Separate permission and grant into assignments | Denied | Denied | TRX | PASS |
| A7-55 | Expired grant | Unit | Grant at exact expiry | Denied | Denied | TRX | PASS |
| A7-56 | Revoked grant | Browser/SQL | Repeat file and Dashboard after SQL revoke | 404 no bytes and count 0 | 404 no bytes and count 0 | a7-browser.json / a7-sql-verification.json | PASS |
| A7-57 | Stale authority | Unit | Session has old policy/catalog | Denied | Denied | TRX | PASS |
| A7-58 | Logout replay | Browser/SQL | Restore revoked cookie | 401 | 401 | a7-browser.json / a7-sql-verification.json | PASS |
| A7-59 | Export revoke race | Integration | Revoke while export runs | No unauthorized result | Chưa xác minh | A7 defect register | BLOCKED |
| A7-60 | Independent Lab approval | Readiness | Verify signed release | Scope and approval evidence | Chưa xác minh | A7 defect register | BLOCKED |
