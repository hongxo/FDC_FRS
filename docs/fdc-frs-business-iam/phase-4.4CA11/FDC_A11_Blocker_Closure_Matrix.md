# A.10 → A.11 blocker reconciliation

| Area | Prior gap | A.11 evidence | Disposition |
|---|---|---|---|
| Runtime / DPAPI | A10-019/020 skipped | Original test methods executed with owned SQL fixture | PASS in isolated test |
| PKI | OCSP and rotation policy unverified | Signed responses through loopback validator | Partial, ingress and rotation open |
| Export | Worker recovery, fences, checksum | Reclaimed lease and tampered staging against SQL | Partial, hard crash and separate processes open |
| Browser | A10-055 BFCache and pending logout | Baseline navigation/session controls | BLOCKED, combined case lacks evidence |
| Collector | Station/certificate rebind | Existing status workflow | BLOCKED, atomic cutover missing |
| HA/DR | Multi-node and restore | No accepted topology or restore evidence | BLOCKED |
| Governance | Decisions, signatures and permits | See decision and gate packages | PENDING |

The two closed rows were A.10 SKIPs. No prior BLOCKED item is counted as closed.
