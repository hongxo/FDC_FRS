## Dependency

Stacked draft PR on feature/d12-authentication-authorization; depends on#1, which is not merged into main. Review Phase2 first; do not merge out of order.

## Change

Adds scoped SQL user/assignment/grant/catalog/audit reads and8 opt-in canonical Human GET adapters. Adds dynamic role/grant proposal previews and a candidate independent role-review lifecycle composed over the existing revision primitive. Default management authority/apply/conflict/delegation providers remain fail-closed while D04/D09 and other security policies are pending.

Adds a separate V1-styled S05–S08 read entry using same-origin HTTPS APIs without runtime fixtures. Original S01–S04/prototype logic remains untouched. No Center host is activated.

## Verification

Stable .NET10 build passed.159 unit/component tests passed;3 real SQL lab cases passed in explicit separate runs with rollback-only synthetic fixtures.17 Node client tests and8 intercepted-HTTPS browser component checks passed. Raw generated evidence remains local, not in this PR. No migration or persistent seed.

## Limitations

PARTIALLY_COMPLETED/BLOCKED_BY_POLICY. No live login/MFA host, full user/role CRUD, durable request/review/apply SQL, effective assignment/grant/delegation mutation, audit atomicity or D09 race proof. The lifecycle is role-domain only and not an approved canonical status mapping. Live authenticated UI integration is not accepted. Do not activate handlers, declare P0-IAM READY or merge automatically.

## CRUD continuation
Adds real SQL role request/CRUD apply service (create, rename, activate/deactivate, clone, permission diffs) behind pending current-authority and commit gates. Default suite164 PASS;3 new actual SQL rollback-only cases PASS, including stale-version rejection and outbox-failure rollback. These use test-only authority/gate fixtures and do not establish live policy/MFA or D09 ordering. No new HTTP/UI write surface is activated; see CRUD_Progress_Report.md.
