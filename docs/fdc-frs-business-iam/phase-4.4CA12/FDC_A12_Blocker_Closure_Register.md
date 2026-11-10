# A.12 blocker closure register

| Origin and IDs | Root cause and source | Proposed fix / owner | Required reproduction and invariant |
|---|---|---|---|
| A.11 A11-015 | A11RuntimeProfile uses one Windows identity for fixture and migrations | Separate ephemeral migration principal / DBA | Isolated SQL apply/rollback and permissions; runtime cannot migrate |
| A.11 A11-032–035; ingress requirement | PinnedOcspCertificateValidator is disconnected from PilotIngestionEndpoints | Attach synthetic revocation policy to certificate scheme / Center and PKI | Real TLS requests, pinned chain and signed OCSP/CRL, registry and station enforcement, predecessor denied on cutover |
| A.11 worker BLOCKED rows 036–065 | A10LabWorkflowTests tests only DI scopes | Real worker child processes / export | Barrier, kill, replacement, SQL lease/fence and audit/outbox; no duplicate or unauthorized bytes |
| A.11 A11-066–080 | Existing pageshow/session handling has no qualifying persisted BFCache logout test | Chromium real navigation and delayed network / UI | Persisted pageshow after logout, no protected DOM or request replay |
| A.11 A11-081–095 | Lab workflow updates binding status only | Extend canonical workflow / IAM and Collector registry | Real HTTP, SQL and mTLS before/after cutover, SoD and atomic scope |
| A.11 A11-096–110 | No two-node and restore harness | Isolated candidate topology / infrastructure | Network spoofing, node revoke, SQL restore integrity; no OT |
| A.11 A11-111–120 | No signed disposition | Independent and owner review | Signed evidence tied to exact SHA; activation gate remains pending |

Initial actual evidence, raw path and tested commit for these new scenarios are absent. Each original row remains BLOCKED until a qualifying result is linked.

## Executed closure evidence

- A11-032: PASS, HTTPS + SQL on 9df0f045799d925bd55602c8a543454fae44a772. Actual wrong-station rejection 403. Raw a12-mtls-revocation.json and TRX.
- A11-039, A11-040 and A11-060: PASS, real OS workers + SQL on 65dc0fcdef94e3a67d9bb51841a3da50d70ae538. Actual hard kill and replacement after SQL staging; success and revoked controls. Raw a12-worker-process.json and TRX.
- Other IDs remain BLOCKED. See the [executed tests](FDC_A12_Executed_Test_Matrix.md) and [raw manifest](FDC_A12_Raw_Evidence_Manifest.md) for commands and environment.

- A11-067/068/069/070: PASS at actual browser/SQL scope on d65b6931e792e390f52f9317963f1d6f2fb63f9a. Canonical form login/MFA, delayed logout and station detail, tab propagation, 401 Back and revoked-cookie replay were executed. Raw a12-auth-ui.json and screenshots. A11-066 persisted BFCache remains BLOCKED.
