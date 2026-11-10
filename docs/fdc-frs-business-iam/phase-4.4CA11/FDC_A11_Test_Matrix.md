# A.11 acceptance matrix

34 PASS / 0 FAIL / 86 BLOCKED / 0 SKIP across 120 rows. PASS is specific to the named layer. OCSP validates a live loopback responder and does not prove the mTLS ingress. Worker tests use two DI scopes in one process.

| ID | Requirement | Layer | Preconditions | Input | Expected | Actual | Environment | Tested SHA | Evidence | Status | Defect / gap | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| A11-001 | SKIP reconciliation | SQL | Runtime SQL | SKIP reconciliation | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-002 | Random owned DB | SQL | Runtime SQL | Random owned DB | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-003 | Migrations | SQL | Runtime SQL | Migrations | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-004 | Non-SA roles | SQL | Runtime SQL | Non-SA roles | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-005 | Least privilege | SQL | Runtime SQL | Least privilege | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-006 | Generated login | SQL | Runtime SQL | Generated login | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-007 | Actual HTTPS startup | SQL | Runtime SQL | Actual HTTPS startup | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-008 | Canonical IAM | SQL | Runtime SQL | Canonical IAM | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-009 | DPAPI wrong entropy | SQL | Runtime SQL | DPAPI wrong entropy | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-010 | Secret read denial | SQL | Runtime SQL | Secret read denial | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-011 | Authority DML denial | SQL | Runtime SQL | Authority DML denial | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-012 | Aggregate denial | SQL | Runtime SQL | Aggregate denial | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-013 | Invalid config fail closed | SQL | Runtime SQL | Invalid config fail closed | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | CenterRuntimeDatabaseTests | PASS | — | D09 |
| A11-014 | DB/login cleanup | SQL | Runtime SQL | DB/login cleanup | Enforce scoped SQL/profile | Enforce scoped SQL/profile | Runtime SQL | c331cad | cleanup logs | PASS | — | D09 |
| A11-015 | Separate admin and migrator | SQL | Runtime SQL | Separate admin and migrator | Enforce scoped SQL/profile | Admin = migrator | Runtime SQL | — | — | BLOCKED | SQL gap | D09 |
| A11-016 | Valid CRL. | PKI | Synthetic CA | Valid CRL. | Accept valid certificate | Accept valid certificate | Synthetic CA | c331cad | A10CertificateTests | PASS | — | D07/D08 |
| A11-017 | Revoked by CRL. | PKI | Synthetic CA | Revoked by CRL. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A10CertificateTests | PASS | — | D07/D08 |
| A11-018 | CRL signature invalid. | PKI | Synthetic CA | CRL signature invalid. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A10CertificateTests | PASS | — | D07/D08 |
| A11-019 | CRL expired. | PKI | Synthetic CA | CRL expired. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A10CertificateTests | PASS | — | D07/D08 |
| A11-020 | CRL unavailable. | PKI | Synthetic CA | CRL unavailable. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A10CertificateTests | PASS | — | D07/D08 |
| A11-021 | Valid OCSP good. | PKI | Synthetic CA | Valid OCSP good. | Accept valid certificate | Accept valid certificate | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-022 | OCSP revoked. | PKI | Synthetic CA | OCSP revoked. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-023 | OCSP unknown. | PKI | Synthetic CA | OCSP unknown. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-024 | OCSP unauthorized responder. | PKI | Synthetic CA | OCSP unauthorized responder. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-025 | OCSP invalid signature. | PKI | Synthetic CA | OCSP invalid signature. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-026 | OCSP stale response. | PKI | Synthetic CA | OCSP stale response. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-027 | OCSP responder timeout. | PKI | Synthetic CA | OCSP responder timeout. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-028 | OCSP responder HTTP 500. | PKI | Synthetic CA | OCSP responder HTTP 500. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-029 | OCSP responder unreachable. | PKI | Synthetic CA | OCSP responder unreachable. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A11OcspTests | PASS | — | D07/D08 |
| A11-030 | Certificate expired. | PKI | Synthetic CA | Certificate expired. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A10CertificateTests | PASS | — | D07/D08 |
| A11-031 | Wrong EKU. | PKI | Synthetic CA | Wrong EKU. | Reject invalid evidence | Reject invalid evidence | Synthetic CA | c331cad | A10CertificateTests | PASS | — | D07/D08 |
| A11-032 | Wrong station binding. | PKI | Synthetic CA | Wrong station binding. | Reject invalid evidence | Qualifying run/evidence absent | Synthetic CA | — | — | BLOCKED | PKI gap | D07/D08 |
| A11-033 | Unknown collector. | PKI | Synthetic CA | Unknown collector. | Reject invalid evidence | Qualifying run/evidence absent | Synthetic CA | — | — | BLOCKED | PKI gap | D07/D08 |
| A11-034 | Certificate rotation overlap. | PKI | Synthetic CA | Certificate rotation overlap. | Reject invalid evidence | Qualifying run/evidence absent | Synthetic CA | — | — | BLOCKED | PKI gap | D07/D08 |
| A11-035 | Revoked old certificate after rotation. | PKI | Synthetic CA | Revoked old certificate after rotation. | Reject invalid evidence | Qualifying run/evidence absent | Synthetic CA | — | — | BLOCKED | PKI gap | D07/D08 |
| A11-036 | Worker crash before claim. | WRK | Owned SQL worker | Worker crash before claim. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-037 | Crash after claim. | WRK | Owned SQL worker | Crash after claim. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-038 | Crash during staging. | WRK | Owned SQL worker | Crash during staging. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-039 | Crash after staging commit. | WRK | Owned SQL worker | Crash after staging commit. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-040 | Crash before release. | WRK | Owned SQL worker | Crash before release. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-041 | Crash after release commit. | WRK | Owned SQL worker | Crash after release commit. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-042 | Duplicate job. | WRK | Owned SQL worker | Duplicate job. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-043 | Duplicate worker. | WRK | Owned SQL worker | Duplicate worker. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-044 | Lease expiration. | WRK | Owned SQL worker | Lease expiration. | Fence stale state; preserve authorized release and audit | Fence stale state; preserve authorized release and audit | Owned SQL worker | c331cad | a11-export-lease.json | PASS | — | D04/D09/D10 |
| A11-045 | Lost lease ownership. | WRK | Owned SQL worker | Lost lease ownership. | Fence stale state; preserve authorized release and audit | Fence stale state; preserve authorized release and audit | Owned SQL worker | c331cad | a11-export-lease.json | PASS | — | D04/D09/D10 |
| A11-046 | SQL outage. | WRK | Owned SQL worker | SQL outage. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-047 | Outbox failure. | WRK | Owned SQL worker | Outbox failure. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-048 | Audit failure. | WRK | Owned SQL worker | Audit failure. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-049 | Revocation before staging. | WRK | Owned SQL worker | Revocation before staging. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-050 | Revocation during staging. | WRK | Owned SQL worker | Revocation during staging. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-051 | Revocation before release. | WRK | Owned SQL worker | Revocation before release. | Fence stale state; preserve authorized release and audit | Fence stale state; preserve authorized release and audit | Owned SQL worker | c331cad | a11-export-quarantine.json | PASS | — | D04/D09/D10 |
| A11-052 | Revocation after release. | WRK | Owned SQL worker | Revocation after release. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-053 | Authority version change. | WRK | Owned SQL worker | Authority version change. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-054 | Expired session. | WRK | Owned SQL worker | Expired session. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-055 | Cross-station artifact access. | WRK | Owned SQL worker | Cross-station artifact access. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-056 | Stale cached permission. | WRK | Owned SQL worker | Stale cached permission. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-057 | Partial artifact cleanup. | WRK | Owned SQL worker | Partial artifact cleanup. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-058 | Checksum mismatch. | WRK | Owned SQL worker | Checksum mismatch. | Fence stale state; preserve authorized release and audit | Fence stale state; preserve authorized release and audit | Owned SQL worker | c331cad | a11-export-quarantine.json | PASS | — | D04/D09/D10 |
| A11-059 | Storage unavailable. | WRK | Owned SQL worker | Storage unavailable. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-060 | Restart recovery. | WRK | Owned SQL worker | Restart recovery. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-061 | Multi-process concurrency. | WRK | Owned SQL worker | Multi-process concurrency. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-062 | Cancellation. | WRK | Owned SQL worker | Cancellation. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-063 | Retry exhaustion. | WRK | Owned SQL worker | Retry exhaustion. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-064 | Idempotent replay. | WRK | Owned SQL worker | Idempotent replay. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-065 | Quarantine retention. | WRK | Owned SQL worker | Quarantine retention. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-066 | BFCache | WEB | Chromium | BFCache | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-067 | Pending logout | WEB | Chromium | Pending logout | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-068 | Logout during detail | WEB | Chromium | Logout during detail | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-069 | Back after logout | WEB | Chromium | Back after logout | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-070 | Multiple tabs | WEB | Chromium | Multiple tabs | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-071 | Cookie switch | WEB | Chromium | Cookie switch | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-072 | SecurityVersion | WEB | Chromium | SecurityVersion | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-073 | Idle timeout | WEB | Chromium | Idle timeout | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-074 | Absolute timeout | WEB | Chromium | Absolute timeout | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-075 | MFA expiry | WEB | Chromium | MFA expiry | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-076 | Refresh | WEB | Chromium | Refresh | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-077 | Browser restore | WEB | Chromium | Browser restore | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-078 | Delayed response | WEB | Chromium | Delayed response | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-079 | Cache revalidation | WEB | Chromium | Cache revalidation | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-080 | History navigation | WEB | Chromium | History navigation | Clear stale DOM; reauthorize | Qualifying run/evidence absent | Chromium | — | — | BLOCKED | WEB gap | D05 |
| A11-081 | Self-approval denied. | COL | Synthetic registry | Self-approval denied. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-082 | Cross-station approval denied. | COL | Synthetic registry | Cross-station approval denied. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-083 | Wrong certificate denied. | COL | Synthetic registry | Wrong certificate denied. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-084 | Revoked certificate denied. | COL | Synthetic registry | Revoked certificate denied. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-085 | Expired certificate denied. | COL | Synthetic registry | Expired certificate denied. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-086 | Stale request denied. | COL | Synthetic registry | Stale request denied. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-087 | Duplicate apply idempotent. | COL | Synthetic registry | Duplicate apply idempotent. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-088 | Concurrent rebind. | COL | Synthetic registry | Concurrent rebind. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-089 | Outbox failure rollback. | COL | Synthetic registry | Outbox failure rollback. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-090 | SQL outage fail closed. | COL | Synthetic registry | SQL outage fail closed. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-091 | Old certificate rejected after cutover. | COL | Synthetic registry | Old certificate rejected after cutover. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-092 | New certificate accepted after approved cutover. | COL | Synthetic registry | New certificate accepted after approved cutover. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-093 | Human cookie rejected at machine ingress. | COL | Synthetic registry | Human cookie rejected at machine ingress. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-094 | Machine cert rejected for Human management. | COL | Synthetic registry | Machine cert rejected for Human management. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-095 | Station B data never accessible from Station A. | COL | Synthetic registry | Station B data never accessible from Station A. | Enforce binding and independent scope | Qualifying run/evidence absent | Synthetic registry | — | — | BLOCKED | COL gap | D04/D07/D08 |
| A11-096 | Proxy identity forwarding. | INF | Isolated topology | Proxy identity forwarding. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-097 | Header spoofing denied. | INF | Isolated topology | Header spoofing denied. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-098 | Session survives permitted failover. | INF | Isolated topology | Session survives permitted failover. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-099 | Revoked session denied across nodes. | INF | Isolated topology | Revoked session denied across nodes. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-100 | Authority version consistency. | INF | Isolated topology | Authority version consistency. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-101 | Multi-node commit gate. | INF | Isolated topology | Multi-node commit gate. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-102 | Audit/outbox consistency. | INF | Isolated topology | Audit/outbox consistency. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-103 | Worker failover. | INF | Isolated topology | Worker failover. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-104 | Backup disposable SQL. | INF | Isolated topology | Backup disposable SQL. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-105 | Restore disposable SQL. | INF | Isolated topology | Restore disposable SQL. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-106 | Migration rollback rehearsal. | INF | Isolated topology | Migration rollback rehearsal. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-107 | Recovery after Center restart. | INF | Isolated topology | Recovery after Center restart. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-108 | Revocation after failover. | INF | Isolated topology | Revocation after failover. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-109 | Certificate rotation. | INF | Isolated topology | Certificate rotation. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-110 | Health/monitoring. | INF | Isolated topology | Health/monitoring. | Preserve security and recovery | Qualifying run/evidence absent | Isolated topology | — | — | BLOCKED | INF gap | D01/D05/D09 |
| A11-111 | Assign reviewer | GATE | Owner review | Assign reviewer | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-112 | Verify independence | GATE | Owner review | Verify independence | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-113 | Review source SHA | GATE | Owner review | Review source SHA | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-114 | Review raw evidence hashes | GATE | Owner review | Review raw evidence hashes | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-115 | Resolve findings | GATE | Owner review | Resolve findings | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-116 | Obtain signed decision | GATE | Owner review | Obtain signed decision | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-117 | Approve policies | GATE | Owner review | Approve policies | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-118 | Validate OT permit | GATE | Owner review | Validate OT permit | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-119 | Approve change and rollback | GATE | Owner review | Approve change and rollback | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |
| A11-120 | Authorize activation | GATE | Owner review | Authorize activation | Signed disposition and exact scope | Qualifying run/evidence absent | Owner review | — | — | BLOCKED | GATE gap | D01/D04/D07/D08/D09/D10 |

## Mandatory E2E

| Case | Scenario | Disposition |
|---|---|---|
| E2E-01 | Runtime Center starts under non-SA SQL. | PASS — actual Center with owned profile |
| E2E-02 | Runtime denies secret reads and authority DML. | PASS — actual Center with owned profile |
| E2E-03 | DPAPI profile protection. | PASS — actual Center with owned profile |
| E2E-04 | OCSP revoked cert denied. | BLOCKED at ingress — validator PASS |
| E2E-05 | OCSP unknown/unavailable denied. | BLOCKED at ingress — validator PASS |
| E2E-06 | CRL revoked cert denied. | PASS — synthetic CRL listener |
| E2E-07 | Certificate rotation and old cert revoke. | BLOCKED — qualifying run absent |
| E2E-08 | Worker crash during staging. | BLOCKED — qualifying run absent |
| E2E-09 | Worker crash before release. | BLOCKED — qualifying run absent |
| E2E-10 | Multi-process worker recovery. | BLOCKED — qualifying run absent |
| E2E-11 | Export/revoke race after restart. | BLOCKED — qualifying run absent |
| E2E-12 | Browser BFCache after logout. | BLOCKED — qualifying run absent |
| E2E-13 | Pending logout response. | BLOCKED — qualifying run absent |
| E2E-14 | Collector station rebind SoD. | BLOCKED — qualifying run absent |
| E2E-15 | Collector certificate rebind and mTLS. | BLOCKED — qualifying run absent |
| E2E-16 | Cross-station denial after rebind. | BLOCKED — qualifying run absent |
| E2E-17 | Multi-node session revoke. | BLOCKED — qualifying run absent |
| E2E-18 | Proxy spoofing denied. | BLOCKED — qualifying run absent |
| E2E-19 | Disposable SQL backup/restore. | BLOCKED — qualifying run absent |
| E2E-20 | Shared Lab activation denied when policy pending. | BLOCKED — qualifying run absent |

INF-16: test traffic was local SQL and loopback. No accidental OT routing in the proposed HA topology remains to be independently verified.
