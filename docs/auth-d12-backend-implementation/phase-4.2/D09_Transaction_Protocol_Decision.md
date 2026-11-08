# D09 transaction protocol

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Version0.1 PROPOSED. Approval:PENDING_OWNER_DECISION. Owner:DBA/Architecture/Security. Signature/signed-at/evidence:NOT_PROVIDED.

## Decision required / current

Candidate applock gate7 SQL protocol tests; runtime pending; all writers/direct DML/failover missing.

## Options A/B/C

A transaction-owned Policy/User gates plus controlled principal writer path; B reviewed SQL procedures/row-lock equivalent; C read-only authority check without commit serialization (insufficient).

## Recommendation / security impact

Review A against every writer before activation; B valid alternative if operations model supports it.

Sign namespace/modes/order/actor-target enumeration/isolation/timeouts, inside-gate recheck, audit/version atomicity, unknown-commit durable idempotency lookup and AG semantics. Direct authority DML must not bypass. Need actual product2-node race and least-privilege/failover evidence.

## Compatibility / implementation dependency

Preserve current contracts/source until explicit approved release. Pending-sensitive paths fail-closed. Recommendation/config is not approval. Owner returns exact option/version/hash, rationale, bounded scope, residual risk and signed evidence, then dependent implementation/full tests. Production readiness separate.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
