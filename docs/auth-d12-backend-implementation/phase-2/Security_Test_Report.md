# Security test report

Actual final .NET10 run:102 PASS,0 FAIL,2 SKIPPED (SQL tests require explicit lab environment). Separate SQL run:2 PASS,0 FAIL,0 SKIPPED.104 distinct cases passed across runs;42 new component/unit cases and2 new SQL cases beyond60 foundation cases. Raw evidence: evidence/phase2-identity.trx and phase2-sql-lab.trx.

PASS scope: salted hash/correct and wrong password/malformed hash/rehash/length; opaque proof randomness/format/purpose binding; session expiry/revoke/disable/security/grant/policy/restore versions; cookie constraints; Origin/HTTPS checks; real ASP.NET antiforgery missing/valid token pair; current-authority handler calls; outage503; forged header/duplicate cookie/certificate confusion rejection; pending policy fail-closed. Test doubles are test-only; runtime mock authentication was not added.

SQL PASS scope: exact host/database/disposable guard; primary current session joins against synthetic rollback-only fixtures; subsequent grant change/disable/revoke/epoch mismatch observed; empty-session authority read denies; rollback leaves zero users/sessions/authority rows. No real accounts, credentials or factor secrets inserted.

NOT_RUN/BLOCKED: actual HTTP login/logout and account lockout/shared source limiting; MFA verification/replay/enrollment; concurrent session issuance/rotation; full populated SQL scope joins; revoked-authority sensitive commit races; audit/outbox rollback; restore/failover/PKI/proxy deployment. Password login orchestration is not a deployed SQL identity provider.

## DR01–DR29 mapping

All DR scenarios retain NOT_RUN as full end-to-end acceptance. Unit/SQL subcase evidence is recorded below without marking the entire DR case PASS.

| Cases | Ownership and partial evidence |
|---|---|
| DR01–DR03 | Phase3 create/clone/assignment workflow; domain primitives only |
| DR04 | Phase2 same-assignment evaluator unit evidence; populated SQL grant acceptance pending |
| DR05–DR06 | Phase3 effective permission/deactivation writes; Phase2 evaluator deny subcases |
| DR07–DR09 | Phase2 role/assignment/grant expiry evaluator unit subcases |
| DR10 | Phase2 evaluator cross-station unit subcases; HTTP/list/download disclosure acceptance pending |
| DR11 | Phase2 same-assignment non-combination evaluator unit subcase |
| DR12–DR13 | Phase3 grant/review/apply impact; current domain SoD subcases only |
| DR14–DR15 | Phase2 evaluator SoD and mandatory OT deny unit subcases |
| DR16 | Phase3 role mutation concurrency; not executed |
| DR17 | Phase2/3 D09 commit race; blocked |
| DR18 | Phase2 current-session SQL reads and handler freshness subcases; multi-node acceptance pending |
| DR19 | Phase2 version/catalog default-deny unit subcases |
| DR20 | Phase2 synthetic authority outage/503 and readonly-intent denial; actual SQL outage/failover not run |
| DR21–DR22 | Phase3 history/seed lifecycle; offline metadata/domain evidence only |
| DR23–DR24 | Phase2 evaluator hierarchy/delegation subcases; SQL business acceptance pending |
| DR25 | Phase3 workflow/idempotency; not accepted |
| DR26 | Phase2/3 audit failure and restore; not executed |
| DR27 | Phase2 real antiforgery component + cookie/cert/header negative subcases; deployed boundary pending |
| DR28 | Phase3 administration/recovery custody policy; blocked |
| DR29 | Frontend integration separate scope; UI unchanged |

Findings: source directory has no Git history; remote is README-only initial repository; no approved security-policy artifact was found. New handlers are unregistered and missing policy providers deny, preventing accidental activation. Existing original runtime security posture is unchanged; this work does not remediate every earlier Center issue.

Initial build discovered SDK11 preview; stable10 build/test passed later. One test compilation ambiguity was corrected before final PASS. Build binlog is local-only and excluded from Git because it may capture environment; no log of passwords/tokens/connection strings is intended. Default dotnet first-use installed a sandbox development HTTPS certificate; not trusted/exported/used as production PKI and no production certificate changed.

Raw TRX files remain local-only; delivery-identity.trx and delivery-sql-lab.trx verify the separate Git checkout. No generated test/build artifacts are committed.
