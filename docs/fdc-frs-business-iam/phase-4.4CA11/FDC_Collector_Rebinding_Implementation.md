# Collector rebinding implementation gap

Existing binding-request UI/API operates on binding status. A.11 has no atomic station or certificate rebinding implementation. Proposed design carries old/new station and certificate fingerprint, expected binding revision, reason, request ID and approvals. Apply serializes the binding, validates current old and new station scopes and credential eligibility, and records cutover plus audit/outbox atomically. Runtime rechecks active binding/version for each ingestion commit. Policy owner must decide treatment of buffered data from the old station.
