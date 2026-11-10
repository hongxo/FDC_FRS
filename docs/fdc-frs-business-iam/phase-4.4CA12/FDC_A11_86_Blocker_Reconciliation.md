# A.11 blocker import

Original rows imported verbatim from the A.11 matrix. Closure updates require a linked qualifying A.12 run.

| ID | Requirement | Layer | Preconditions | Input | Expected | Actual | Environment | Tested SHA | Evidence | Status | Defect / gap | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| A11-015 | Separate admin and migrator | SQL | Runtime SQL | Separate admin and migrator | Enforce scoped SQL/profile | Admin = migrator | Runtime SQL | — | — | BLOCKED | SQL gap | D09 |
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
| A11-046 | SQL outage. | WRK | Owned SQL worker | SQL outage. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-047 | Outbox failure. | WRK | Owned SQL worker | Outbox failure. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-048 | Audit failure. | WRK | Owned SQL worker | Audit failure. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-049 | Revocation before staging. | WRK | Owned SQL worker | Revocation before staging. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-050 | Revocation during staging. | WRK | Owned SQL worker | Revocation during staging. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-052 | Revocation after release. | WRK | Owned SQL worker | Revocation after release. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-053 | Authority version change. | WRK | Owned SQL worker | Authority version change. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-054 | Expired session. | WRK | Owned SQL worker | Expired session. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-055 | Cross-station artifact access. | WRK | Owned SQL worker | Cross-station artifact access. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-056 | Stale cached permission. | WRK | Owned SQL worker | Stale cached permission. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
| A11-057 | Partial artifact cleanup. | WRK | Owned SQL worker | Partial artifact cleanup. | Fence stale state; preserve authorized release and audit | Qualifying run/evidence absent | Owned SQL worker | — | — | BLOCKED | WRK gap | D04/D09/D10 |
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

## A.12 disposition

Original rows above preserve the A.11 baseline. The following four IDs are now PASS for the listed isolated layer. All other 82 remain BLOCKED.

| Original ID | Evidence | Actual outcome | Tested source |
|---|---|---|---|
| A11-032 | a12-mtls-revocation.json and TRX | Wrong station 403 through the SQL-bound machine route | 9df0f045799d925bd55602c8a543454fae44a772 |
| A11-039 | a12-worker-process.json and TRX | Actual worker killed after SQL staging commit | 65dc0fcdef94e3a67d9bb51841a3da50d70ae538 |
| A11-040 | Same worker run | Hard kill at the pre-release barrier; one authorized release | 65dc0fcdef94e3a67d9bb51841a3da50d70ae538 |
| A11-060 | Same worker run | Separate process recovery with current authority, checksum and paired events | 65dc0fcdef94e3a67d9bb51841a3da50d70ae538 |

## UI closure update

A11-067,068,069,070 are additionally PASS on source d65b6931e792e390f52f9317963f1d6f2fb63f9a, raw tmp/a12ui/tmp/focused-03/a12-auth-ui.json and TRX. Actual Chromium observed committed logout held at the response, no stale Business DOM across tabs or delayed detail, and protected Back refused with 401. SQL verified revoked session, consumed challenge and paired audit events. Total closure is now 8 original IDs; 78 remain BLOCKED. A11-066 remains BLOCKED because persisted pageshow was not observed.

## Security integration follow-up

A11-043 and A11-061 close using the same qualifying two-process race execution. Two real PIDs crossed the claim barrier, SQL retained one owner, and the losing process returned false on release. The winner released once and replay returned false. Source 2930a32ea2a02fe5579785cbeb8302b37819cf47, UTC 2026-11-10 13:44:55. Raw a12-worker-lease-race.json and Center TRX. Closure is now 10 of 86 original IDs. The other 76 remain BLOCKED.
