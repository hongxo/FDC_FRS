# Residual risk and defect register

| Priority / ID | Gap | Disposition / required evidence |
|---|---|---|
| P0 | Registry cutover and revocation policy | Atomic old/new cert binding and actual mTLS cutover |
| P0 | Worker failures inside transactions | Hard kill before SQL staging commits and evidence of rollback |
| P0 | Persisted BFCache | Actual persisted pageshow after logout on supported browser |
| P1 | Proxy, HA and DR | Two-node revoke/spoof/bypass and owned restore |
| P1 | SQL identities | Distinct migrator/runtime/fixture administration with rollback |
| P1 | Main Login activation | Feature authority, authentication flow and real MFA policy remain pending in normal Center startup |
| A12-DEF-01 | Browser test expected Back to complete | Resolved by asserting observed server 401 and no protected content; BFCache still BLOCKED |
| A12-DEF-02 | Form markup appeared while policy pending | Resolved by creating forms only on server ACTIVE; existing zero-form/disabled assertions retained |
| A12-DEF-03 | Windows file lock during COMTRADE fixture cleanup | Observed once in a full run. Original test passed three isolated reruns. Lock owner/root cause unproven; retain failed evidence and investigate recurrence |
| Gate | Unsigned policy and acceptance | Signed disposition with SHA, scope and separate activation permit |
