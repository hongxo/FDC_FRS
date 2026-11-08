# Domain model and implementation boundary

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

| Domain | CURRENT deployed | Runtime / limit |
|---|---|---|
| Master data | Canonical Corporation/ManagementUnit/Station/Scope plus VoltageLevel,StationProfile/UnitHistory,Manufacturer,DeviceModel/Firmware,CommunicationProfile,Bay,Feeder,IedDevice | GUID/station lineage; no actual380-station import. IED serves relay/protection device; taxonomy pending. History transition/overlap writer not activated |
| Collector | Registration,CollectorIed,Heartbeat,CollectionJob/Attempt,OperationalEvent | Unique identity/config version, tuple FKs; BindingStatus is not certificate proof; default machine policy denies. Schedule via NextRunAtUtc only, recurring policy pending |
| Ingest | DisturbanceRecord,RecordFile,IngestionAttempt,RecordStatusHistory,ProcessingOutbox | Actual file verifier/SQL candidate writer/replay lock/atomic outbox tested. Not a deployed machine HTTP ingress; no raw binary in SQL |
| Fault/SOE | FaultEvent,FaultEventRecord,StationSoeEvent | Actual Core SOE adapter, duplicate/conflict checks. Fault linkage tested; no fabricated fault classification |
| Analysis | AnalysisAlgorithm,AnalysisJob,AnalysisResult | Schema only for execution; NotImplemented fixtures have NULL result; no algorithm run. Review/approval PENDING_BUSINESS_DECISION |
| Operations | Existing IAM workflow plus separate processing metadata/outbox | No duplicate IAM approval engine; record/analysis approval workflow proposed only. Worker/lease/retry fairness not activated |
| IAM | Original24 entity compatibility, same-assignment UserRole-bound StationGrant/delegation/SoD/versions | No effective seed or changes; all Phase4.2 pending activation preserved |
| Security audit | Existing SecurityEvent/AuditOutbox + AuditDelivery/AuditCheckpoint | Independent safe metadata only; no archive/immutable custody acceptance |
| Dashboard |3 owning-schema views + SqlStationRecords authority-before-filter/paging | Default authority pending; no broad view grants or station count leak endpoint |

Source mapping: Core.Ied→IedDevice and catalog details; Manifest→DisturbanceRecord+RecordFile (source metadata/time raw preserved); SoeEvent→StationSoeEvent including quality/RCB/entry/sequence/confRev/segment flags. No private key/credential fields added to Collector schema. External string identity retained beside internal GUID; mapping must be reviewed before real fleet import. File bounds based existing100MiB/known COMTRADE extensions; oversized source fields reject, never silently truncate. SQL strings mostly200, remote/object keys512; verify real field lengths before migration/cutover.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
