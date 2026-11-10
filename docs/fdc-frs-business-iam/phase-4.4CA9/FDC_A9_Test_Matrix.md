# A.9 Test Matrix

Mỗi dòng là một yêu cầu quan sát riêng. Một browser TRX có thể kiểm tra nhiều assertion. Bảng này và số lượng test .NET không cộng gộp.

| ID | Requirement | Layer | Preconditions | Input | Expected | Actual | Evidence | Status | Defect | Policy dependency |
|---|---|---|---|---|---|---|---|---|---|---|
| A9-01 | Create unique DB | SQL | A.9 fresh SQL | Create unique DB | Owned random name | Owned random name | cleanup.log | PASS | — | D09 |
| A9-02 | Migrate IAM 001 | SQL | A.9 fresh SQL | Migrate IAM 001 | IAM history and canonical tables | IAM history and canonical tables | TRX | PASS | — | D09 |
| A9-03 | Migrate Center 002–006 | SQL | A.9 fresh SQL | Migrate Center 002–006 | Six history rows and three views | Six history rows and three views | TRX | PASS | — | D09 |
| A9-04 | Scoped aggregate | SQL | A.9 fresh SQL | Scoped aggregate | Scope before pagination | Scope before pagination | TRX | PASS | — | D04 |
| A9-05 | IED composite ownership | SQL | A.9 fresh SQL | IED composite ownership | Foreign key rejects cross-station link | Foreign key rejects cross-station link | TRX | PASS | — | D09 |
| A9-06 | Operational event station FK | SQL | A.9 fresh SQL | Operational event station FK | Orphan write rejected | Orphan write rejected | TRX | PASS | — | D09 |
| A9-07 | Record replay | SQL | A.9 fresh SQL | Record replay | Single record after concurrent duplicates | Single record after concurrent duplicates | TRX | PASS | — | D09 |
| A9-08 | Required outbox rollback | SQL | A.9 fresh SQL | Required outbox rollback | No partial record or files | No partial record or files | TRX | PASS | — | D10 |
| A9-09 | Checksum rejection | SQL | A.9 fresh SQL | Checksum rejection | No SQL mutation from bad digest | No SQL mutation from bad digest | TRX | PASS | — | E03 |
| A9-10 | Profile query SQL | SQL | A.9 fresh SQL | Profile query SQL | Five thousand synthetic rows, real plan | Five thousand synthetic rows, real plan | query-profile.json | PASS | — | D09 |
| A9-11 | SOE replay | SQL | A.9 fresh SQL | SOE replay | Conflicting replay denied | Conflicting replay denied | TRX | PASS | — | D09 |
| A9-12 | Fault association | SQL | A.9 fresh SQL | Fault association | Cross-station fault/record link denied | Cross-station fault/record link denied | TRX | PASS | — | D09 |
| A9-13 | Borrowed transaction | SQL | A.9 fresh SQL | Borrowed transaction | Reader sees local revoke without committing fixture | Reader sees local revoke without committing fixture | TRX | PASS | — | D09 |
| A9-14 | Missing session transaction | SQL | A.9 fresh SQL | Missing session transaction | Reader leaves borrowed transaction available | Reader leaves borrowed transaction available | TRX | PASS | — | D05/D09 |
| A9-15 | Session miss profile | SQL | A.9 fresh SQL | Session miss profile | Negative lookups complete at concurrency 1/10 | Negative lookups complete at concurrency 1/10 | authority-miss-profile.json | PASS | — | D09 |
| A9-16 | Create/cleanup after migration error | SQL | A.9 fresh SQL | Create/cleanup after migration error | Marker-owned DB removed after exception | Marker-owned DB removed after exception | run-01/cleanup.log | PASS | — | D09 |
| A9-17 | Legacy lost commit ACK | SQL | A.9 fresh SQL | Legacy lost commit ACK | Unknown outcome and safe retry behavior | Chưa đủ bằng chứng | TRX SKIP | SKIP | A9-D06 | D09 |
| A9-18 | Legacy concurrent revoke gate | SQL | A.9 fresh SQL | Legacy concurrent revoke gate | Mutation/revoke ordered at commit | Chưa đủ bằng chứng | TRX SKIP | SKIP | A9-D06 | D09 |
| A9-19 | Legacy SQL management scope | SQL | A.9 fresh SQL | Legacy SQL management scope | Cross-subject grants hidden | Chưa đủ bằng chứng | TRX SKIP | SKIP | A9-D06 | D04 |
| A9-20 | Legacy role mutation/SoD | SQL | A.9 fresh SQL | Legacy role mutation/SoD | Independent reviewer and apply enforced | Chưa đủ bằng chứng | TRX SKIP | SKIP | A9-D06 | D04/D09 |
| A9-21 | Scoped business navigation | Business UI/API | HTTPS + SQL A/B | Scoped business navigation | Only allowed operation routes returned | Only allowed operation routes returned | a7-browser.json | PASS | — | D04 |
| A9-22 | Hide navigation after revoke | Business UI/API | HTTPS + SQL A/B | Hide navigation after revoke | No authorized business routes remain | No authorized business routes remain | a7-browser.json | PASS | — | D04 |
| A9-23 | Search cross-station | Business UI/API | HTTPS + SQL A/B | Search cross-station | Station B search empty | Station B search empty | a7-browser.json | PASS | — | D04 |
| A9-24 | Paged total | Business UI/API | HTTPS + SQL A/B | Paged total | Count 21 scoped IED rows | Count 21 scoped IED rows | Browser TRX | PASS | — | D04 |
| A9-25 | Next | Business UI/API | HTTPS + SQL A/B | Next | Page two has 1 scoped row | Page two has 1 scoped row | Browser TRX | PASS | — | D04 |
| A9-26 | Previous | Business UI/API | HTTPS + SQL A/B | Previous | Original page restored | Original page restored | Browser TRX | PASS | — | D04 |
| A9-27 | Last | Business UI/API | HTTPS + SQL A/B | Last | Last page without extra results | Last page without extra results | Browser TRX | PASS | — | D04 |
| A9-28 | First | Business UI/API | HTTPS + SQL A/B | First | First page ordering restored | First page ordering restored | Browser TRX | PASS | — | D04 |
| A9-29 | Session expiration | Business UI/API | HTTPS + SQL A/B | Session expiration | Idle/absolute expiry clears rendered data | Chưa đủ bằng chứng | Time-based browser evidence missing | BLOCKED | A9-D05 | D05 |
| A9-30 | Dashboard trends | Business UI/API | HTTPS + SQL A/B | Dashboard trends | Seven UTC days, A only | Seven UTC days, A only | a7-browser.json | PASS | — | D04 |
| A9-31 | Dashboard alerts | Business UI/API | HTTPS + SQL A/B | Dashboard alerts | Only A event | Only A event | a7-browser.json | PASS | — | D04 |
| A9-32 | Station–IED | Business UI/API | HTTPS + SQL A/B | Station–IED | A relation available, B empty | A relation available, B empty | a7-browser.json | PASS | — | D04 |
| A9-33 | IED–Collector | Business UI/API | HTTPS + SQL A/B | IED–Collector | A relation available, B empty | A relation available, B empty | a7-browser.json | PASS | — | D04 |
| A9-34 | Collector–COMTRADE | Business UI/API | HTTPS + SQL A/B | Collector–COMTRADE | A relation available, B empty | A relation available, B empty | a7-browser.json | PASS | — | D04 |
| A9-35 | Mobile responsive | Business UI/API | HTTPS + SQL A/B | Mobile responsive | No overflow at 390px | No overflow at 390px | a8-collector-mobile.png and Browser TRX | PASS | — | D04 |
| A9-36 | Lab test list | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Only scoped station/IED proposals | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-37 | Lab test detail | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Exact station, IED and revision binding | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-38 | Test plan | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Plan matches immutable revision | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-39 | Evidence metadata | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | No cross-station evidence | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-40 | Permit read | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Permit bound to current revision | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-41 | Readiness | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Gate fails closed without verified permit | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-42 | Draft | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | No execution enabled | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-43 | Submit | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Revision and audit committed together | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-44 | Review | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Requester cannot review | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-45 | Approve | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Approver independent from requester/reviewer | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-46 | Ready | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Validity and evidence rechecked | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-47 | Synthetic apply | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | No device networking | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-48 | Missing permit | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Execution denied | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-49 | Expired permit | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Execution denied | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-50 | Same person aliases | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | PersonReference SoD enforced | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-51 | Cross-station approval | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | HTTP deny and SQL unchanged | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-52 | Aborted result | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Result linked to attempt and audit | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-53 | Completed result | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Result linked to permit and revision | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-54 | Export | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Scoped evidence artifact | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-55 | Audit history | Lab workflow | Signed or isolated mapping and permit | Synthetic plan A/B | Ordered redacted revision events | Chưa đủ bằng chứng | Không có runtime evidence | BLOCKED | A9-D03 | D04/D08/D09/D10 |
| A9-56 | CFG integrity | Security | Isolated security fixture | CFG integrity | Digest matches fixture | Digest matches fixture | a7-browser.json | PASS | — | E03 |
| A9-57 | DAT integrity | Security | Isolated security fixture | DAT integrity | Digest matches fixture | Digest matches fixture | a7-browser.json | PASS | — | E03 |
| A9-58 | Cross-station download | Security | Isolated security fixture | Cross-station download | CFG and DAT deny with zero bytes | CFG and DAT deny with zero bytes | a7-browser.json | PASS | — | D04 |
| A9-59 | Download audit atomicity | Security | Isolated security fixture | Download audit atomicity | Two prepared audit/outbox correlations matched | Two prepared audit/outbox correlations matched | a7-sql-verification.json | PASS | — | D10 |
| A9-60 | Swapped path at file source | Security | Isolated security fixture | Swapped path at file source | Opened redirect cannot stage bytes | Opened redirect cannot stage bytes | TRX Path_swapped_after_attributes_check | PASS | — | E03 |
| A9-61 | HTTP audit for swapped path | Security | Isolated security fixture | HTTP audit for swapped path | Audit disposition correlated to deny | Chưa đủ bằng chứng | HTTP swap test missing | BLOCKED | A9-D04 | D10 |
| A9-62 | CRL revoked leaf | Security | Isolated security fixture | CRL revoked leaf | Denied by chain revocation | Chưa đủ bằng chứng | CRL fixture missing | BLOCKED | A9-D04 | D07 |
| A9-63 | OCSP revoked leaf | Security | Isolated security fixture | OCSP revoked leaf | Denied under policy | Chưa đủ bằng chứng | OCSP fixture missing | BLOCKED | A9-D04 | D07 |
| A9-64 | Unavailable revocation | Security | Isolated security fixture | Unavailable revocation | Fail closed with no bypass | Chưa đủ bằng chứng | PKI evidence missing | BLOCKED | A9-D04 | D07 |
| A9-65 | Expired/wrong EKU leaf | Security | Isolated security fixture | Expired/wrong EKU leaf | Chain validator rejects | Chain validator rejects | a8-mtls.json | PASS | — | D07 |
| A9-66 | Export revoke race | Security | Isolated security fixture | Export revoke race | Worker recheck denies artifact release | Chưa đủ bằng chứng | Worker evidence missing | BLOCKED | A9-D03 | D09/D10 |
| A9-67 | Multi-user proxy cache | Security | Isolated security fixture | Multi-user proxy cache | No cross-user/station reuse | Chưa đủ bằng chứng | Proxy E2E missing | BLOCKED | A9-D04 | D04/D05 |
| A9-68 | Human cookie replay | Security | Isolated security fixture | Human cookie replay | Revoked cookie denied | Revoked cookie denied | a7-browser.json | PASS | — | D05 |
| A9-69 | No-store | Security | Isolated security fixture | No-store | Authorized download response no-store | Authorized download response no-store | Browser TRX | PASS | — | D05 |
| A9-70 | Download after station revoke | Security | Isolated security fixture | Download after station revoke | Zero bytes after grant revoked | Zero bytes after grant revoked | a7-browser.json | PASS | — | D04 |
| A9-71 | Heartbeat scope | Collector/IAM | Synthetic A/B + SQL | Heartbeat scope | A shows healthy; B denied | A shows healthy; B denied | Browser TRX | PASS | — | D04 |
| A9-72 | Transfer scope | Collector/IAM | Synthetic A/B + SQL | Transfer scope | A shows accepted; B denied | A shows accepted; B denied | Browser TRX | PASS | — | D04 |
| A9-73 | Binding mutation SoD | Collector/IAM | Synthetic A/B + SQL | Binding mutation SoD | Independent reviewed apply | Chưa đủ bằng chứng | No runtime evidence | BLOCKED | Collector gap | D04/D07 |
| A9-74 | Retry error detail | Collector/IAM | Synthetic A/B + SQL | Retry error detail | Redacted typed detail | Chưa đủ bằng chứng | No runtime evidence | BLOCKED | Collector gap | D04 |
| A9-75 | History UI pagination | Collector/IAM | Synthetic A/B + SQL | History UI pagination | Next/back for independent histories | Chưa đủ bằng chứng | No runtime evidence | BLOCKED | Collector gap | D04 |
| A9-76 | Machine station binding | Collector/IAM | Synthetic A/B + SQL | Machine station binding | Wrong station 403 | Wrong station 403 | a8-mtls.json | PASS | — | D07 |
| A9-77 | Inactive registry binding | Collector/IAM | Synthetic A/B + SQL | Inactive registry binding | Inactive registration 403 | Inactive registration 403 | a8-mtls.json | PASS | — | D07 |
| A9-78 | Registry unavailable | Collector/IAM | Synthetic A/B + SQL | Registry unavailable | No intake, 503 | No intake, 503 | a8-mtls.json | PASS | — | D07 |
| A9-79 | Machine accepted SQL intake | Collector/IAM | Synthetic A/B + SQL | Machine accepted SQL intake | One record, two files, one outbox | One record, two files, one outbox | a8-mtls.json | PASS | — | D07/D09 |
| A9-80 | Human rejected by Machine | Collector/IAM | Synthetic A/B + SQL | Human rejected by Machine | Human and spoofed identity denied | Human and spoofed identity denied | a8-mtls.json | PASS | — | D01 |
| A9-81 | D01 topology approval | Acceptance | Evidence and owner disposition | Owner records | Signed boundary and named approver | Chưa đủ bằng chứng | Decision_Records.json pending | BLOCKED | Review/gate | D01 |
| A9-82 | D04 catalog release | Acceptance | Evidence and owner disposition | Owner records | Signed operation map | Chưa đủ bằng chứng | Decision_Records.json pending | BLOCKED | Review/gate | D04 |
| A9-83 | D07 PKI approval | Acceptance | Evidence and owner disposition | Owner records | Signed lifecycle and revocation | Chưa đủ bằng chứng | Decision_Records.json pending | BLOCKED | Review/gate | D07 |
| A9-84 | D08 permit authority | Acceptance | Evidence and owner disposition | Owner records | Named OT owner and approved window | Chưa đủ bằng chứng | Decision_Records.json pending | BLOCKED | Review/gate | D08 |
| A9-85 | D09 lab rollback | Acceptance | Evidence and owner disposition | Owner records | Owner-reviewed deployment rollback | Chưa đủ bằng chứng | Decision_Records.json pending | BLOCKED | Review/gate | D09 |
| A9-86 | D10 audit policy | Acceptance | Evidence and owner disposition | Owner records | Signed archive/retention | Chưa đủ bằng chứng | Decision_Records.json pending | BLOCKED | Review/gate | D10 |
| A9-87 | Independent review | Acceptance | Evidence and owner disposition | Owner records | Signed review with evidence | Chưa đủ bằng chứng | REVIEW_PENDING | BLOCKED | Review/gate | Independent reviewer |
| A9-88 | Shared Lab activation | Acceptance | Evidence and owner disposition | Owner records | Approved exact scope and permit | Chưa đủ bằng chứng | No signed release | BLOCKED | Review/gate | D01/D04/D07/D08/D09/D10 |
| A9-89 | UAT and HA/restore | Acceptance | Evidence and owner disposition | Owner records | Verified failover and recovery | Chưa đủ bằng chứng | Evidence missing | BLOCKED | Review/gate | E decisions |
| A9-90 | Production release | Acceptance | Evidence and owner disposition | Owner records | Signed operational handover | Chưa đủ bằng chứng | Evidence missing | BLOCKED | Review/gate | E decisions |
