# Security audit

Implemented scoped SQL audit list/detail DTO and GET /api/identity/v1/admin/security-audit[/{id}]. Fields:EventId,ActorId,Action,Result,Timestamp,CorrelationId. Explicit EventIds from current authority constrain rows before filters/paging, no assumption that station management allows global audit.

Current SecurityEvent schema does NOT contain all required management fields (TargetId,RequestId,Before/After,Reason); those are not fabricated in responses. No audit-write/independent immutable archive or integrity verification is added. Management mutations are disabled, so no fake audit success accompanies an effective permission change. A future versioned expansion migration and reviewed structured redaction/failure policy are needed before writes.

SQL test inserts synthetic audit fixtures inside the same rollback-only fixture transaction to check read isolation. This is not an audit/outbox atomic mutation proof. Sensitive mutation+audit failure rollback, archive tamper/restore/checkpoint verification and retention controls:NOT_RUN/BLOCKED_BY_D09_D10. No tamper-proof claim.
