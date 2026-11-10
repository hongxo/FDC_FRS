# Worker process recovery

The parent starts worker A as a distinct vstest process tree and waits for an atomically published marker after SQL staging commits. It verifies the Preparing state, lease owner and artifact digest, hard-kills the owned process tree and verifies the worker PID exited. Worker B runs in a new process against the same ownership-marked SQL DB, advances the fixture clock 61 seconds, and attempts recovery. This uses a deterministic fixture clock for lease expiry and an actual OS kill. The barrier is after the durable staging commit and before release.

Focused run passed both normal release and revoke during downtime. Replay release was denied. The authorized HTTP download matched the SQL artifact hash. Quarantine cleared bytes and returned 401 with zero bytes. Exactly one paired release event and one paired quarantine event were observed. Kill inside staging, simultaneous lease contention, external storage faults and scheduler behavior remain BLOCKED.
