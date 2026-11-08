# SQL boundaries, faults and concurrency

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

Actual boundary: SqlRoleCrudService requires the same DbContext transaction from IAuthorizedCommitBoundary, rechecks actor/state inside it and SaveChanges atomically writes effect/history/audit/outbox. Default PendingD09CommitBoundary still refuses execution. Lab CRUD tests supply a test-only boundary around a rollback transaction; EF SaveChanges/savepoint failure atomicity is measured, not a production commit ordering protocol.

PASS:5 CRUD/fix SQL cases,2 session SQL cases,1 scope SQL case,2 real connection/timeout cases and1 observation-only query profile. No SQL service was stopped, no shared instance failover injected. Database login failure is against a random nonexistent local DB, never creates it. Timeout only on the test connection.

BLOCKED: simultaneous revoke/commit, concurrent grants/approvals, multi-node session issuance, lock fairness/deadlock/failover/unknown outcomes and durable D09 authority ordering. No sleep-based races substituted. Generic actor boundary cannot be assumed to implement required global role PolicyGate Exclusive. No race-free or committed-revoke guarantee accepted. D09 owner selection, least-privilege writer enforcement and deterministic two-node gate implementation are prerequisites.
