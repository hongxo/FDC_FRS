# Export worker recovery

`LabExportCoordinator` checks staged payload SHA-256 before release. Reclaim proceeds through the existing SQL lease and authorization gate. An expired job or abandoned claim can be quarantined while the live lease of another worker remains protected. Quarantine clears staged bytes and uses the existing transactional audit/outbox writer.

Existing SQL serialization and authority rechecks must fence stale owners on stage/release. Follow-up requires two processes, abrupt termination at each durable boundary, a bounded retry scheduler, externally stored artifact reconciliation and retention. Current tests exercise JSON export staging in SQL.
