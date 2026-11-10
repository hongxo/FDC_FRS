# A.12 executed acceptance matrix

Three new test methods contain multiple observations. Original blocker closure is counted by distinct A.11 IDs. Full regression results and failed attempts are in the raw manifest.

| Test / original IDs | Layer and environment | Security invariant | Actual qualifying evidence | Command and source | Status |
|---|---|---|---|---|---|
| Strict_CRL_and_OCSP_are_required_on_real_SQL_bound_machine_ingress / A11-032 | Real HTTPS, signed responder and disposable SQL | Revocation and station binding before machine operation | Ten revocation failures return 401 empty with no records/outbox; wrong station 403; valid bound upload 201; completion one record/one processing outbox | Full runner; source 064a1751c6b0e85c0bf618e1ed27a154cdd51941; a12-mtls-revocation.json and TRX | PASS at isolated candidate layer; rotation and denial audit remain open |
| Worker_process_is_killed_after_staging_and_replacement_rechecks_SQL_authority / A11-039,040,060 | Separate OS workers, disposable SQL, HTTPS download | No duplicate or stale-authority release after crash | A hard-killed after SQL staging. B releases once with matching artifact hash, or quarantines after revoke. Replayed release denied; paired audit/outbox. Real PIDs and timestamps retained | Full runner or filter FullyQualifiedName~Worker_process_is_killed; a12-worker-process.json and TRX | PASS after durable staging barrier. Deterministic clock advances lease 61 seconds. Inside-transaction kill/race remain open |
| Login_form_MFA_and_pending_logout_clear_business_tabs_against_disposable_SQL / A11-067,068,069,070 | Actual Chromium forms, HTTPS, disposable SQL and canonical endpoints | No cookie before MFA, no stale protected content or revoked-cookie reuse | Form 202 then MFA 204; logout 204; pending detail ignored and other tab cleared; Back refused 401; cookie replay 401/zero bytes. Session/challenge and paired audit checked | Full runner or filter FullyQualifiedName~Login_form_MFA; a12-auth-ui.json, a12-auth-ui-sql.json, TRX and screenshots | PASS for observed states. Persisted pageshow not observed |

All three run on the final technical source noted above. Evidence records UTC and source SHA. Earlier focused runs are retained by their distinct SHA. There is no synthetic PASS for an unexecuted layer.

| Mandatory E2E IDs | Status | Evidence or gap |
|---|---|---|
| 01–04 | PASS | Actual isolated machine HTTPS ingress with strict signed evidence |
| 05 | BLOCKED | No certificate rotation |
| 06 | BLOCKED | No kill during staging transaction |
| 07–08 | PASS | Kill before release and actual replacement process |
| 09 | BLOCKED | No simultaneous lease race |
| 10 | PASS | Revocation during downtime prevented bytes |
| 11 | BLOCKED | No persisted BFCache observation |
| 12–13 | PASS | Held responses and cross-tab session invalidation; backend replay denial |
| 14–16 | BLOCKED | Atomic Collector rebind absent |
| 17–18 | BLOCKED | Multi-node revoke/proxy absent |
| 19 | BLOCKED | Restore verification absent |
| 20 | BLOCKED | Activation harness not executed; approvals pending |

Mandatory totals: 9 PASS, 11 BLOCKED, zero FAIL/SKIP. Independent review is pending.
