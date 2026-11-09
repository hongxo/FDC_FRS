# Evidence index

Ngày:2026-11-09 Asia/Bangkok. Source commit:`e1a3f3ea9d396a9c8ee65cf8c8219f4ba047c06d`; baseline:`1961bebc23300d9f1b3e2ce43afac0550bd7a083` (PR7, ancestry PR6/Phase4.2).

Environment:Windows; .NET SDK10.0.403, Node22.12.0, PowerShell7.6.9; SQL2022 16.0.1210.3, local lpc:., MachineName XOTH-ETC-2. Target FDC_Center_Lab; previous regression Lab FdcIam_DevelopmentLab. Formal decisions:0 APPROVED/20 PENDING; user Development/Lab coding/test authorization remains valid, not policy approval. Reviewer:NOT_PROVIDED; readiness/P0-IAM unchanged.


## Portable

- evidence/Test_Execution.json:all attempts, actual names/outcomes/durations/counters/failure messages, raw TRX hashes/paths.
- authority-miss-profile.json/query-profile.json:actual raw samples/datasets/process metrics/limits.
- center-live-browser.json:8 negative screens/401/3 viewports, not successful auth.
- dependency-scan.log, secret-scan.json, build-final.log, environment.txt:actual bounded checks.
- *before.txt/*after.txt:local Lab schema/migration/zero-row metadata, no row payload.
- Source_Manifest/Input_Manifest:exact source/reference hashes.

## Raw local only

Official checkout tmp/phase4-3b-evidence retains default-tests, initial lab-tests, intermediate sequential, final-lab, final-iam TRX/logs, screenshots/traffic, SQL plan and diagnostic files. Evidence_Manifest binds lengths/hashes. Raw files are not in GitHub; reviewer can inspect locally/reproduce with authorized secrets. No raw cookie/private-key/DPAPI artifact copied. No fake actor approval accepted as runtime evidence.

Initial deadlock diagnostic quoting/QUOTED_IDENTIFIER failures retained; corrected ring-buffer query empty, no exact lock graph. Optional old IAM query profile SKIP; Center metadata profile ran. No restore/failover/fleet acceptance this phase. 4.3A source-unchanged QA was valid for its historical docs-only snapshot; do not edit historical reports or run that assertion as a4.3B source acceptance test.

## Reproduction / final source alignment

Confirm FDC_IAM_CONFIRMED_LAB=FdcIam_DevelopmentLab and FDC_CENTER_CONFIRMED_LAB=FDC_Center_Lab. Supply FDC_IAM_LAB_CONNECTION/FDC_CENTER_LAB_CONNECTION through transient authorized secret environment only; tests guard local lpc:., MachineName and disposable markers. FDC_CENTER_RUNTIME_SECRET_FILE references existing protected runtime credential; never commit/decrypt it into logs. Opt-ins:FDC_RUN_CENTER_BROWSER=1,FDC_RUN_AUTHORITY_PROFILE=1; FDC_UI_EVIDENCE/FDC_CENTER_EVIDENCE ignored local path; NODE_PATH existing Playwright; FDC_UI_BROWSER installed Edge. Missing inputs SKIP, not PASS.

Final IAM run includes last2 release tests. Center run preceded those isolated validator changes and final UI409/428 wording; exercised C# authority/host paths and503 behavior unchanged. Final Node25 and build were rerun after UI wording correction e1a3f3e; evidence/node-tests-final.log included. HTTP428 does not assume version or step-up semantics before contract approval. No additional SQL run is claimed for that text-only follow-up.

Portable SQLCMD metadata .txt removes display padding/trailing spaces; original raw bytes remain unchanged locally and are hashed in Evidence_Manifest.
