# SQL backup and restore

BLOCKED for this acceptance phase. Use only random ownership-marked SQL databases and a unique lab backup artifact. Verify checksum, inspect logical files, restore under a different marked name with unique file paths, verify schema/version and audit/outbox, then confirm prior sessions cannot be resurrected under an explicit restore epoch procedure. Verify backup copy cleanup and no shared DB connections. A.11 migration/fixture cleanup is separate evidence.
