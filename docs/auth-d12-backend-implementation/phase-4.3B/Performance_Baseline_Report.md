# Measured Lab baseline

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.

## Hypothesis / environment / dataset

Measure negative SQL session-consumer lookup and scoped metadata-query cost locally, without signed SLA thresholds. Session dataset0 users/sessions/configured authority; nonexistent digest, no writes. Query dataset2 synthetic stations/1IED/5000 metadata records, no waveform ingestion. One testhost/local SQL; pooled connections. Raw samples shipped.

| Workload | Concurrency | Samples | P50ms | P95ms | P99ms | ops/s | Unexpected errors |
|---|---:|---:|---:|---:|---:|---:|---:|
| SQL session miss |1|100|1.5763|2.0842|5.7981|562.17|0|
| SQL session miss |10|100|2.0255|18.8215|21.0603|2241.24|0|
| Scoped50-record query |1|50|3.5826|5.0189|5.6378|267.23|0|

Actual expected assertions:session null, query50 rows; PASS. Warmup5, session100 samples per concurrency, query50 samples; nearest-rank percentiles. Unexpected errors0/200 and0/50. Process CPU for session workloads 437.5ms; working set 151429120->155860992bytes. Query CPU 31.25ms, managed memory 45091320->47466872bytes. Process figures include testhost overhead, not SQL server utilization.

SQL CPU/RAM/disk/network NOT_COLLECTED. Small samples/local warm-cache bias; no owner thresholds, SLA or hardware/capacity acceptance. Actual query plan local and hashed.

## Outstanding

Successful login/MFA, representative permission/grant authority snapshot, protected API and sensitive SQL-transaction latency BLOCKED/no numbers. Concurrency50/100/380/WAN outage/storm/failover/storage-full/power/cert lifecycle NOT_RUN/BLOCKED. Miss/query numbers cannot substitute those tests. No fabricated benchmark or performance pass threshold.

## Commands / evidence / reviewer

Build:`dotnet build Fdc.Identity.slnx --no-restore -v minimal`. Lab:`./tools/Invoke-IamLabVerification.ps1 -EvidenceDirectory <ignored-local-path>` with transient confirmed-Lab/secret environment. Final IAM:`dotnet test tests/Fdc.Identity.Tests/Fdc.Identity.Tests.csproj --no-build --no-restore --logger trx --results-directory <ignored-local-path>`. Node:`node --test poc/authentication-ui-v1/tests/management-client.test.mjs tests/Fdc.Center.Tests/browser/management-errors.test.mjs`. SCA:`dotnet list Fdc.Identity.slnx package --vulnerable --include-transitive`.

[Evidence](Evidence_Index.md), [actual test outcomes](evidence/Test_Execution.json), Source_Manifest.json and Input_Manifest.json. Raw logs/TRX retained in tmp/phase4-3b-evidence of official checkout, not GitHub attachments. Test PASS is bounded to named components, not full DR. Independent QA/Security review remains pending. Rollback:reviewed source reverts; no schema down or stale-authority restore.
