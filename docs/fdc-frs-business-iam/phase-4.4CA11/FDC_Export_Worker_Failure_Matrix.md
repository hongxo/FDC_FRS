# Export worker failure matrix

| Failure | Expected | Evidence |
|---|---|---|
| Expired claim, replacement worker | Replacement releases once, original fenced | `Expired_export_claim_can_be_recovered_and_previous_owner_is_fenced` |
| Authority revoked before resumed release | Quarantine, no bytes, paired audit/outbox | `Recovered_revoked_export_and_corrupt_staging_are_quarantined` |
| Corrupt staged bytes | Quarantine, payload cleared | Same SQL test with injected payload mutation |
| OS process termination / storage outage / retries | Fence old owner and recover once | BLOCKED pending process/fault harness |

See [matrix](FDC_A11_Test_Matrix.md) for individual scenarios.
