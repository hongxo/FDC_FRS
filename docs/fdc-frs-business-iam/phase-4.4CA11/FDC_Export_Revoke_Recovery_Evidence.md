# Revoke and recovery

A deterministic SQL test claims and stages under an initial scope, updates authorization, expires the lease with the test clock and reclaims from a fresh DI scope. The test also injects corrupted staging separately. Both paths must quarantine and clear payload with paired audit/outbox. The test models recovery from abandoned leases inside one process. Crash/restart and bulk COMTRADE release remain separate obligations.
