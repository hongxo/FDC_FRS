# 23 blocker A.7

Closure chỉ áp dụng trong phạm vi test đã chạy. Shared policy không đổi.

| ID | Root cause | Missing component | Required code | Test / evidence | Dependency | Owner đề xuất | A.8 |
|---|---|---|---|---|---|---|---|
| A7-06 | CODE_GAP | Global navigation | Effective server menu map | Browser menu by permission | D04 | Center/IAM | BLOCKED |
| A7-07 | TEST_GAP | Station search | Reuse SQL scoped search | Browser search B returns empty | D04 release for shared | Center/QA | PASS trong fixture |
| A7-08 | TEST_GAP | Paging UI | Wire full page UX across multiple visible pages | Browser API offset 0/1 verified; UI next-page not exercised | D04 | Center/QA | BLOCKED |
| A7-09 | TEST_GAP | Responsive | Use existing responsive shell | 390px browser no overflow and screenshot | — | QA | PASS trong fixture |
| A7-10 | TEST_GAP | Session expiry UI | Add expiry with active rendered data | Browser expiry clears DOM | D05 | Center/QA | BLOCKED |
| A7-18 | CODE_GAP | Dashboard trends | Scope SQL trend buckets | Cross-station trend browser | D04 | Center | BLOCKED |
| A7-19 | CODE_GAP | Dashboard alerts | Scope SQL alerts DTO | Cross-station alert browser | D04 | Center | BLOCKED |
| A7-20 | CODE_GAP | Station relationships | Joined station-owned relationships | Detail browser drill-down | D04 | Center | BLOCKED |
| A7-25 | TEST_GAP | CFG download | Add valid CFG fixture and digest | Browser bytes and SQL audit | D10 shared only | QA | PASS trong fixture |
| A7-33 | TEST_GAP | Symlink race | Opened-handle path validation delivered | Deterministic path swap race and no escaped bytes | Storage ACL/D10 | Security/QA | BLOCKED |
| A7-39 | CODE_GAP | Heartbeat history | Scoped timeline API/UI | Browser A only, B 404 | D04 shared only | Center/QA | PASS trong fixture |
| A7-40 | CODE_GAP | Transfer history | Scoped transfer API/UI | Browser A only, B 404 | D04 shared only | Center/QA | PASS trong fixture |
| A7-41 | ENVIRONMENT_GAP | mTLS SQL integration | Fresh SQL CA/registry fixture | Ingest/complete and SQL rows | D01/D07 shared only | PKI/QA | PASS trong fixture |
| A7-42 | ENVIRONMENT_GAP | Human cookie on machine | Same isolated machine host | Cookie/spoof rejected before intake | D01 shared only | Security/QA | PASS trong fixture |
| A7-44 | POLICY_BLOCKED | Lab list | Reviewed catalog and scoped Lab persistence | Permit/object/SoD browser fixture | D04/D08/D09 | Lab/OT/Security | BLOCKED |
| A7-45 | POLICY_BLOCKED | Lab details | Reviewed catalog and scoped Lab persistence | Permit/object/SoD browser fixture | D04/D08/D09 | Lab/OT/Security | BLOCKED |
| A7-46 | POLICY_BLOCKED | Lab evidence | Reviewed catalog and scoped Lab persistence | Permit/object/SoD browser fixture | D04/D08/D09 | Lab/OT/Security | BLOCKED |
| A7-47 | POLICY_BLOCKED | Lab permit | Reviewed catalog and scoped Lab persistence | Permit/object/SoD browser fixture | D04/D08/D09 | Lab/OT/Security | BLOCKED |
| A7-48 | POLICY_BLOCKED | Lab execute | Reviewed catalog and scoped Lab persistence | Permit/object/SoD browser fixture | D04/D08/D09 | Lab/OT/Security | BLOCKED |
| A7-49 | POLICY_BLOCKED | Lab approval | Reviewed catalog and scoped Lab persistence | Permit/object/SoD browser fixture | D04/D08/D09 | Lab/OT/Security | BLOCKED |
| A7-50 | POLICY_BLOCKED | Lab export | Reviewed catalog and scoped Lab persistence | Permit/object/SoD browser fixture | D04/D08/D09 | Lab/OT/Security | BLOCKED |
| A7-59 | CODE_GAP | Export revoke race | Worker with recheck and quarantine | Concurrent revoke and no release | D09/D10 | Center/DBA | BLOCKED |
| A7-60 | INDEPENDENT_REVIEW_REQUIRED | Lab acceptance | Obtain signed release | Independent review and named scope | D01/D04/D07/D09/D10 | Owners/QA | BLOCKED |
