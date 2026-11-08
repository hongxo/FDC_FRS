# Defects and retest

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

| ID | Severity | Component/root cause | Safe reproduction | Fix/status | Evidence/retest |
|---|---|---|---|---|---|
| P4-D001 | High (conditional service activation risk) | Role apply used metadata target while preview used payload target | Rollback SQL insert-only mismatched request; fixture-only independent authority | Metadata target/base/catalog/kind+requester digest checked before effect; FIXED_IN_CODE | defects-before.trx FAIL; sql-crud-final.trx PASS |
| P4-D002 | Medium | Applied replay returned current role.Version | Apply create, apply later rename, replay first request | Matching RoleHistory original version; missing history deny; FIXED_IN_CODE | defects-before.trx FAIL; sql-crud-final.trx PASS |
| P4-D003 | Medium | EF wraps SQL error in InvalidOperationException; DbException catch missed it | Local nonexistent DB connection, no DB creation | Narrow inner SQL/timeout normalization in readers/adapters; FIXED_IN_CODE | defect-D003-before.trx FAIL; sql-failure-final.trx2 PASS |

No full exposed-runtime exploit or independent sign-off claimed. No Critical found in bounded tests; absence outside scope unproven. High conditional P4-D001 remediation does not close integration readiness.

| Blocker | Priority | Required action/status |
|---|---|---|
| P4-B01 | High acceptance blocker | Real session/login/MFA/recovery and approved D03/D05/D06 policy absent; BLOCKED |
| P4-B02 | High acceptance blocker | D04 operation/delegation/seed/custody mapping missing; no runtime authority provider; BLOCKED |
| P4-B03 | High acceptance blocker | D09 serialized commit/revoke/multi-node path absent; BLOCKED |
| P4-B04 | High acceptance blocker | D01 Human/Machine host not integrated; original Center legacy paths not remediated in this scope; BLOCKED |
| P4-B05 | High acceptance blocker | Full D10 target/request/reason audit schema, archive/restore/least-privilege evidence absent; BLOCKED |

Low hardening: role GET transport guard consistency improved; no prior live bypass reproduced. No approved catalog/permission expansion, relaxed MFA, removed failing test or forged approval. Review/change requests for B01–B05 must name accountable owners; no signatures supplied.
