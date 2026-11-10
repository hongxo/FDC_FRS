# Runtime SQL and DPAPI

`CenterRuntimeDatabaseTests` preserves the assertions in A10-019/020. The A.11 fixture provisions a randomly named owned database via Windows Shared Memory, migrates IAM and Center schemas, and creates a disposable non-SA login with explicit SELECT grants. Center verifies local server, ownership marker, login shape, schema and privileged-role exclusions before using the profile. The opt-in is internal DI and restricted to Testing.

The connection is DPAPI CurrentUser-protected with per-run entropy. Unprotect with invalid entropy is rejected and temporary buffers are cleared. The actual Center starts and reads the fixture through the runtime identity. SQL access denies sensitive reads, authority writes and unscoped aggregates covered by the original tests. The local SQL configuration was preserved.

Migration and test administration currently share the integrated Windows identity. Runtime is separate. Dedicated deployment identity, service-account DPAPI rehearsal and Windows cross-account unprotect remain open. See [raw evidence](FDC_A11_Raw_Evidence_Manifest.md).
