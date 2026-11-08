# Explicit remaining gaps and decisions

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

| Gap | State | Owner / dependency | Required next evidence |
|---|---|---|---|
| Human login/MFA/recovery/session issuer | BLOCKED_BY_POLICY + IMPLEMENTATION | D01/D02/D03/D05/D06 + exact route release | Real credential/factor/recovery E2E |
| Current scoped admin/data authority providers | BLOCKED_BY_POLICY + IMPLEMENTATION | D04 mapping/delegation/SoD/custody | Same-assignment/object/station runtime acceptance; no grant inference |
| All sensitive writers/DML model | BLOCKED_BY_POLICY + IMPLEMENTATION | D09 DBA/Security/Architecture | Real product multi-node revoke/commit,least-privilege migrator/writer paths |
| Collector enrollment/cert binding | BLOCKED_BY_POLICY + IMPLEMENTATION | D07/OT/PKI/E04 | Trusted mTLS binding/revocation test; DB Bound flag alone not identity |
| Operating SQLite Center cutover/import | NOT_IMPLEMENTED | E02/E03/E04 + app owner | Reconciled stable string→GUID mapping,operating data/manifest lengths,backfill plan; old source untouched |
| File/object durability/retention/reconciliation | PENDING | Storage/SRE/E03/E06 | Real immutable publication,coordinated DB+file restore; not just local checksums |
| Generic recurring schedules/retry/queue workers | PENDING_BUSINESS_DECISION + IMPLEMENTATION | OT/SRE/Business | Approved job states/backpressure/claim/recovery/fairness; schema is not a running queue |
| Station hierarchy move intervals | PENDING | Business/D04/D09 | Transactional current unit/history/hierarchy-version writer,overlap protection; unique open interval insufficient for all closed overlaps |
| Device relocation/identity semantics | PENDING | OT/master-data owner | Preserve record lineage via reviewed reassignment/new identity,do not update station FK through historical records |
| Relay taxonomy/config/device field lengths | PENDING | OT/business | Real catalog profile; source fields>bounds reject,never truncate; no EVNSPC inventory import claimed |
| Fault correlation/analysis algorithms/review | NOT_IMPLEMENTED / PENDING_BUSINESS_DECISION | Protection engineering/Business | Validated algorithms/provenance/results and independent review; no fake output |
| D10 complete archive/history schema/dispatcher | BLOCKED_BY_POLICY + IMPLEMENTATION | Audit/Security/DBA | Keys/retention/checkpoints/immutability/failure policy,data-bearing restore |
| Runtime/report/audit/migration privileges | PARTIAL | D09/D10 | Non-sa runtime verified; migration uses explicit lab operator; unused role contracts are not least-privilege production acceptance |
| RestoreEpoch/session authority recovery | BLOCKED full restore scenario | D05/D09/D10 | Old active sessions denied after real restore epoch reset; empty-schema restore cannot prove it |
|380-station scale/WAN/offline/HA | NOT_RUN | E01/E02/E05/E06/E08 | Representative full benchmark and SLO/hardware/license/cost decisions |
| Independent review/acceptance | NOT_PROVIDED | QA/Security/Architecture owners | Review exact commits/raw evidence/signatures; no readiness self-promotion |

These are concrete code/policy/environment limits; database object count does not imply full system completion. Reference data not approved or imported; no human admin/custody shortcut. SQL role contracts with no members are intentionally inert,not completed operational principals.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
