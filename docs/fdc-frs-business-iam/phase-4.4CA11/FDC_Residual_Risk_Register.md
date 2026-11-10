# Residual risk register

| Priority | Gap | Impact | Exit evidence |
|---|---|---|---|
| P0 | OCSP ingress and combined policy | Candidate evidence does not prove machine ingress denial | Approved candidate configuration and revoked/unknown/outage mTLS tests |
| P0 | Abrupt process termination | Claims, audit and recovery unproven across processes | Kill/restart tests at claim, staging and release boundaries |
| P1 | BFCache and pending logout | Stale browser view under adverse timing | Real persisted restore plus held detail and logout |
| P1 | Station/cert rebind | Atomic scope and credential cutover unavailable | Implement and test revised SoD workflow |
| P1 | SQL authority and DPAPI deployment profile | Runtime candidate differs from service-account installation | Three principal identities and service account rehearsal |
| P1 | Proxy/HA and restore | Failover, trust and restore-epoch hazards | Isolated two-node and restore drills |
| Gate | Independent acceptance / policy | No activation basis | Signed exact SHA disposition and separate permit |
