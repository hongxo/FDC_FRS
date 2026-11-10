# Rollback and recovery runbook

1. Keep active environments untouched. Disable the disposable runner after capture. Preserve source SHA, test logs and sanitized correlations.
2. Verify local Shared Memory, expected ownership marker and generated fixture names before cleanup. Drop only marked fixture DBs and owned test logins. Verify both counts are zero. Preserve unrelated databases and logins.
3. If a Git rollback is requested, revert the feature commits on a new commit and rerun regression. Preserve existing user changes. There are no A.11 deployment migrations.
4. For a future isolated worker drill, wait for lease expiration, use a new lease owner and verify old ownership fails. Re-authorize on release, quarantine staged bytes and reconcile transactional audit/outbox. Never recover by releasing unchecked staging.
5. For a future restore drill, follow the ownership guard and restore only to a different disposable target. Reconcile the restore epoch before admitting sessions.
