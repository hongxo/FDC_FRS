# Ma trận 50 ca

SQL scope = BusinessAuthorizationSqlTests. Unit = canonical IAM regression. Integration = existing Center/IAM TRX trong cùng lượt regression. PASS chỉ trong lớp ghi rõ. BLOCKED gồm ca chưa tích hợp hoặc chưa chạy. Không tính fixture là Lab live.

| ID | Scenario | Layer | Workload | Expected | Actual | Evidence | Status |
|---|---|---|---|---|---|---|---|
| CA6-01 | Dashboard scoped count | SQL | Two stations, grant one | 1 station | 1 station | SQL scope TRX | PASS |
| CA6-02 | Dashboard after revoke | SQL | Deactivate grant, repeat | 0 station | 0 station | SQL scope TRX | PASS |
| CA6-03 | Dashboard trends | Browser | Open trend with cross-station data | Scoped only | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-04 | Dashboard alerts | Browser | Open alerts with cross-station data | Scoped only | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-05 | Station list | SQL | Read scoped station list | Only Station A | Only Station A | SQL scope TRX | PASS |
| CA6-06 | Station A detail | SQL | Read known allowed ID | Row A | Row A | SQL scope TRX | PASS |
| CA6-07 | Station B detail | SQL | Read known hidden ID | No row | No row | SQL scope TRX | PASS |
| CA6-08 | Station search | HTTP | Search hidden station | No row | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-09 | IED page after scope | SQL | Limit 1 across two stations | Only IED A | Only IED A | SQL scope TRX | PASS |
| CA6-10 | IED A detail | SQL | Read allowed IED ID | Row A | Row A | SQL scope TRX | PASS |
| CA6-11 | IED B detail | SQL | Replace IED ID | No row | No row | SQL scope TRX | PASS |
| CA6-12 | Modified station filter | SQL | Set filter to Station B | Empty list | Empty list | SQL scope TRX | PASS |
| CA6-13 | IED mapping missing | SQL | Attempt orphan IED or unmapped legacy object | No exposure | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-14 | IED configuration write | Unit | Evaluate mandatory-deny permission | Denied | Denied | IAM TRX | PASS |
| CA6-15 | IED discovery without permit | HTTP | Call discovery without permit | Denied | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-16 | Record metadata allowed | SQL | Read record list for Station A | One scoped record | One scoped record | SQL scope TRX | PASS |
| CA6-17 | Record metadata after revoke | SQL | Repeat record detail | No row | No row | SQL scope TRX | PASS |
| CA6-18 | Record B file ID | HTTP | Swap file ID | 404 no bytes | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-19 | Direct download anonymous | HTTP | Open download URL without session | 401 | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-20 | CFG/DAT valid download | Browser | Read verified file with download permission | Bytes + audit | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-21 | COMTRADE permission separation | HTTP | Metadata scope but no download permission | Denied | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-22 | Path traversal | Integration | Malformed storage path | Rejected | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-23 | Digest mismatch | Integration | Mutate stored bytes | No bytes released | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-24 | Range response | HTTP | Request Range on candidate file | No partial content | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-25 | Download audit | SQL | Commit prepared audit/outbox before response | Linked event | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-26 | Collector list | SQL | Read scoped Collector metadata | Only Collector A | Only Collector A | SQL scope TRX | PASS |
| CA6-27 | Collector B object | HTTP | Read Collector B by ID | 404 | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-28 | Collector heartbeat history | Browser | Read station heartbeat timeline | Scoped only | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-29 | Collector certificate wrong station | Integration | Present fixture cert for wrong station | Denied | Chưa xác minh mismatch riêng trong run này | A6 gap register | BLOCKED |
| CA6-30 | Human cookie on machine | Integration | Call machine route with Human cookie | Denied | Denied | Center TRX | PASS |
| CA6-31 | Machine cert on Human | Unit | Call Human session handler with cert | Denied | Denied | IAM TRX | PASS |
| CA6-32 | Machine mTLS intake | Integration | Upload synthetic package with fixture cert | Accepted via machine boundary | Accepted via machine boundary | Center TRX | PASS |
| CA6-33 | Lab readiness blocked | Integration | Read readiness with SQL fixture session | 503 pending | 503 pending | Center TRX | PASS |
| CA6-34 | Lab view scoped | Browser | Read permitted Lab result | Scoped result | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-35 | Lab execute | Browser | Attempt without permit | Denied | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-36 | Lab evidence export | Browser | Export without permission | Denied | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-37 | Permission without grant | Unit | Evaluate explicit missing grant | Denied | Không chạy ca grant rỗng riêng | A6 gap register | BLOCKED |
| CA6-38 | Grant without permission | Unit | Evaluate missing operation permission | Denied | Denied | IAM TRX | PASS |
| CA6-39 | Cross-role mixing | Unit | Split permission and grant across assignments | Denied | Denied | IAM TRX | PASS |
| CA6-40 | Expired grant | Unit | Evaluate past validity window | Denied | Denied | IAM TRX | PASS |
| CA6-41 | Revoked grant | SQL | Set grant inactive and repeat next read | No data | No data | SQL scope TRX | PASS |
| CA6-42 | Authority stale | Unit | Use stale version | Denied | Denied | IAM TRX | PASS |
| CA6-43 | SQL unavailable | Integration | Force SQL session authority unavailable | 503 | 503 | Center TRX | PASS |
| CA6-44 | Policy pending | SQL | Use pending business mapping | Policy exception | Policy exception | SQL scope TRX | PASS |
| CA6-45 | Cache partition | Integration | Replay cached data across users | No cross-user data | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-46 | Export revoke race | Integration | Revoke during export | Abort or deny | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-47 | Background station scope | Integration | Queue cross-station job | Deny | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-48 | Browser login + scoped dashboard | Browser | A.5 login plus A.6 APIs in same flow | Only allowed station | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-49 | Lab authorization | Readiness | Verify signed D01/D04/D07 | Approved activation | Chưa xác minh | A6 gap register | BLOCKED |
| CA6-50 | Independent acceptance | Readiness | Review raw evidence and security scope | Signed result | Chưa xác minh | A6 gap register | BLOCKED |
