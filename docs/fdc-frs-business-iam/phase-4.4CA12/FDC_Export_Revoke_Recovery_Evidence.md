# Export/revoke after worker restart

In the revoke case, the parent waited for worker A's committed staging marker and hard-killed the process. It then revoked the auditor's grant before starting worker B. B obtained neither a prepared nor released result. SQL showed Quarantined, no payload and no lease owner. HTTPS download returned 401 and zero bytes. The same test contains a successful control, release replay denial and one audit/outbox pair for each terminal result. See a12-worker-process.json in the [evidence manifest](FDC_A12_Raw_Evidence_Manifest.md).
