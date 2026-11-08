# Bounded performance observation

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

P4-PERF-01 actual:100 sequential scoped SQL user-list reads after10 warmup,3 synthetic users, one connection/rollback transaction. No HTTP/login/current-authority-provider/MFA cost, no380-station concurrency or WAN. Source commit recorded in raw profile; exact100 latency samples retained.

| Metric | Observed |
|---|---|
| P50 | 0.4761 ms |
| P95 | 0.6708 ms |
| P99 | 0.8994 ms |
| Throughput | 1955.35 query calls/s in this tiny warm local loop |
| Error rate | 0 |
| Client CPU | 62.50 ms total process delta |
| Client working set | 113238016 bytes |

SQL CPU,network,disk,pool/deadlocks/statement-only duration NOT_MEASURED. Stopwatch includes client EF/query materialization, not standalone SQL server duration. Percentiles nearest-rank on100 samples; not sufficient for capacity/SLA claims. Acceptance threshold PENDING. No hardware sizing or acceptance pass inferred.

Login latency/concurrent sessions/multi-instance/failover/recovery performance BLOCKED by missing runtime/policies. Actual connection failure and timeout rejection tested separately for correctness. No stress/load test on production or operating Collector. Raw scoped-query-profile.json includes UTC timestamps and samples.
