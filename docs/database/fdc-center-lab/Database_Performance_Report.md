# Measured query profile and limits

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

Actual workload: SYNTHETIC two stations,one assigned IED,5000 metadata-only DisturbanceRecord rows,5 warmups,50 sequential scoped queries returning50 IDs. One local process/SQL instance,not380 Collector ingest/file transfers. Metadata fixtures removed afterward.

| Metric | Actual |
|---|---|
| P50 |3.8609 ms |
| P95 |4.2921 ms |
| P99 |4.9425 ms |
| Query throughput |254.59311437646582 queries/s |
| Unexpected query errors |0 |
| Test-process CPU delta |125 ms |
| Managed memory before/after |49532120 / 51899256 bytes |

Raw sorted samples in query-profile.json; SQL execution plan captured in scoped-query-plan.sqlplan. CPU/memory are .NET test-process observations,not SQL host resource metrics. SQL host CPU/RAM/disk/network and storage throughput NOT_COLLECTED. No approved SLA/sample acceptance or fleet projection. Allocated MDF/log72/136MiB observed after fixtures,not recommended sizing.

Indexes follow actual station/time/state/record/job access patterns and uniqueness: StationId+EventAtUtc+Id; StationId+State+CreatedAtUtc; Station+IED+ManifestId unique; SHA256 file lookup; record+filename unique; pending queue State+AvailableAtUtc; Collector/time; SOE station/time and source identity. No indiscriminate materialized reporting tables or NOLOCK. Fixture plan must be reviewed for sort/lookup/cardinality; no unconditional index-seek claim. Production cardinality,station skew,WAN/reconnect/file/offline windows and realistic data distribution require separate benchmark. No hardware configuration finalized.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
