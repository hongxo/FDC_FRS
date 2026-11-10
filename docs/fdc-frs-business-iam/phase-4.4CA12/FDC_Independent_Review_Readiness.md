# Independent review readiness

INDEPENDENT_REVIEW_PENDING. G1/G2 remain blocked. Source and raw evidence are available for partial technical review.

Review exact source 064a1751c6b0e85c0bf618e1ed27a154cdd51941, dependency versions, commands and hash manifest. A.12 adds no schema migrations. Existing owned-fixture migrations are exercised by the runner. Verify station scope, same-assignment and authority gates using the unchanged regression assertions, and confirm ied.configuration.write is still denied.

Candidate flow is TLS peer → pinned chain → request-scoped CRL/OCSP → ephemeral certificate/Collector binding → SQL station/IED authority → ingestion/outbox. Worker flow is SQL staging commit → OS kill → fresh process and lease → authority/checksum revalidation → release or quarantine → HTTPS download. Browser flow is policy status → CSRF → password challenge → MFA session → logout commit → revoked-cookie denial. The isolated browser flow uses a synthetic factor and explicit test activation.

Threat checks cover forged certificate headers, invalid/unknown/unavailable revocation evidence, wrong station, worker recovery after revoke, held browser responses and stale cookies. Remaining rotation/rebind, in-transaction failures, persisted BFCache, principal separation, multi-node trust and restore need qualifying evidence.

Rollback uses a normal Git revert of A.12 technical commits and clean rebuild. Kill only owned test child processes. Verify generated fixture names and ownership markers before cleanup; verify DB and login counts afterwards. Preserve raw failures and unrelated checkout edits.

The independent reviewer must provide a signed disposition tied to source/evidence hashes, findings and scope. No reviewer assignment or signature has been evidenced.

## Security follow-up source

Review the worker lease race against exact source 2930a32ea2a02fe5579785cbeb8302b37819cf47. The initial regression used the same canonical IAM baseline as Draft #24. Cross-branch IAM V1 testing requires a checked-out IAM V1 ref. See [worker closure](FDC_A12_Security_Closure_Register.md).
