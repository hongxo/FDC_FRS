# A.12 security closure register

The [original-ID reconciliation](FDC_A11_86_Blocker_Reconciliation.md) preserves all 86 A.11 rows. The [closure register](FDC_A12_Blocker_Closure_Register.md) records baseline causes and evidence for prior A.12 changes.

A11-043 and A11-061 close at isolated E2E scope. Two distinct OS processes start from a shared claim barrier, contend for the same SQL export job, then attempt release. The SQL row names the successful claimant as the lease owner. Exactly one release succeeds and it belongs to that claimant. Replay returns false, the completed job clears its lease, and one release audit/outbox pair exists. Test method `Two_worker_processes_contend_for_one_lease_and_only_owner_releases`, UTC 2026-11-10 13:44:55, source `2930a32ea2a02fe5579785cbeb8302b37819cf47`. See [execution and raw evidence](FDC_A12_Executed_Test_Matrix.md).

Kill during staging, Collector rebind and certificate rotation, persisted BFCache, distinct migration identity, multi-node proxy/revoke and restore integrity remain BLOCKED.
