# RBAC and StationGrant security tests

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

Component tests PASS for inactive/expired role/assignment/grant, same-UserRole permission+scope join, exact supported catalog, global delegation separation, cross-station/hierarchy/version/MFA/SoD and mandatory ied.configuration.write deny. No role-name/code-based permission bypass added.

Real SQL scoped management query test PASS: visibility predicates precede paging/search; wrong user+assignment pair denied even when multiple IDs are visible. CRUD SQL tests PASS for clone isolation/deactivate/version updates and precise target binding. SQL current-authority reader populated end-to-end scope joins remain unaccepted; query fixtures/test authority are not real policy providers.

P4-D001 reproduced an insert-only inconsistent request where payload targeted role A and metadata targeted role B, causing a wrong-role mutation in the isolated service with test authority. High severity for activation if such a record reaches apply; no exposed-runtime exploit demonstrated. Fix checks kind/target/base/catalog and immutable requester/digest binding before review authorization or effect. Actual SQL regression retested PASS. Full DR scenarios remain BLOCKED.
