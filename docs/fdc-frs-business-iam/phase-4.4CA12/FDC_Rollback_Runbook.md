# Rollback runbook

The new security branch adds a process-level integration test on top of Draft #24. Revert the follow-up test and documentation commits on that branch if necessary. The follow-up adds no migration or deployed component. Rebuild from a clean archive and run the disposable harness for verification.

On a failed run, preserve TRX and sanitized evidence. The parent terminates its own worker child trees in finally and removes the run-specific temporary marker files. SQL cleanup is controlled by the disposable fixture ownership guard. Check its final count before any rerun. The prior deployment readiness and rollback responsibilities are in [independent review readiness](FDC_Independent_Review_Readiness.md).
