# FDC/FRS business feature delivery

Tested technical source `f3ebd2177f818330b7faff81d5b47236346b29de`. Feature branch `feature/fdc-frs-comtrade-waveform`, based on the security baseline at `d414db6`. This delivery implements business functions in the existing Center, API and COMTRADE reader.

## Delivered functions

- FRS waveform preview in the existing COMTRADE record detail. Open a record in `/identity/business.html` and select **Xem dạng sóng COMTRADE**. The API is `GET /api/business/v1/records/{id}/waveform`. Select an analog or digital channel and view points against milliseconds, min/max and original CFG timestamps.
- FDC/FRS dashboard shows IED, Collector and COMTRADE record totals. Each total uses the corresponding existing read operation and SQL station scope. Unmapped or unauthorized totals are omitted. Existing station totals, trends, alerts and paging are preserved.

The waveform path uses the existing RecordDownload authority, checksum-verified private file staging and audit/outbox transaction. One verified CFG/DAT pair with matching basename is required. Path and extension checks are retained. CFG is limited to 1 MiB, DAT to 16 MiB, channels to 64, rate segments to 1000, samples to 200000 and output points to 2000. Input encoding is UTF-8. Larger/unsupported/malformed pairs return structured 422. A decimated preview is labeled because it can miss brief peaks. Channel values use the parser's a*x+b scaling and declared units. The response preserves primary/secondary metadata and raw source timestamps.

## Actual verification

- Full solution build succeeded with 0 warnings and 0 errors.
- Full xUnit results on the tested source were 420 PASS / 0 FAIL / 1 SKIP. Center 90 PASS / 1 child-only worker-entry SKIP, Identity 279 PASS, Collector Pilot 51 PASS. Four parser tests verify the stream overload against the file parser for ASCII, BINARY, BINARY32 and FLOAT32, including caller stream ownership.
- Focused SQL and Chromium replay completed 2 PASS / 0 FAIL / 0 SKIP. It exercises dashboard scope/counts, omitted unauthorized aggregates, waveform rendering, cross-station denial and denial after SQL grant revoke. The paired audit query requires the waveform prepared event.
- Node regression completed 11 PASS / 0 FAIL / 0 SKIP.

The full runner's global cleanup guard returned 1 because a matching database already existed from 2026-11-10 21:54:50, before the full run started at 23:21. The focused replay recorded exactly the same pre-existing DB name before and after and zero generated runtime logins in both snapshots. The targeted fixtures created by that replay were removed. This observation does not establish ownership or cleanup of the pre-existing DB.

## Evidence and reproduction

Evidence relative to this worktree is `tmp/phase4-4ca10/business-completion` for the full run and `tmp/business-verification` for focused replay. TRX, logs, sanitized browser steps, SQL verification and screenshots are retained locally. The earlier waveform-only run is under `tmp/phase4-4ca10/business-waveform`.

Build with `dotnet build Fdc.Identity.slnx --no-restore` after package restore. The full runner command is `tools/Invoke-A10DisposableRegression.ps1 -ConfirmDisposable -RunId <new-id> -NativeLibrary <pinned-dll> -Simulator <pinned-exe>` with the owned runtime opt-in. Focused replay filters `Browser_business_authority_and_download` and `Business_reads_scope_aggregate`, using the existing local Shared Memory disposable fixture profiles and Chromium dependency path. Node command is `node --test tests/Fdc.Center.Tests/browser/management-errors.test.mjs`.

SHA-256 of focused evidence:

```text
7cfaa1cf8f9fb8e12a08a7e3eb37a6702e47a0dab31b40a2f97a0627809cd162  tests.log
5e1d51222d7eaa665cc80470bed4d81c2b4fed9d59ab0a76d52831fcd34b6f6c  before.log
5e1d51222d7eaa665cc80470bed4d81c2b4fed9d59ab0a76d52831fcd34b6f6c  after.log
d94284b67adb8474e9fa1829e8a34ffcf27095e1f6e95d544555b5eb46f9792f  node.log
bf03278658b21e3ba0694ccc78563359e3ad0730fcfdb741438b77710a05fe22  a7-browser.json
7ab89922825e0630ad32a11f5c51996e325928738113c246670c1505c6bd210c  a7-sql-verification.json
aeaa7bdc196491eadb70e1f0c60337e304d0c522f1b4a465eb1999afa0ff83cb  business-waveform.png
02dc2b54eb04241bbcd21317e9d9630a76d52e7bec2ddd02711d77ab585faa2f  focused TRX
```

## Integration and rollback

The read adapters retain their existing caller contracts. No new permission catalog entry, IAM Administration screen or migration is introduced. Tests run against synthetic data on disposable local SQL and loopback HTTPS. Main runtime deployment mappings and readiness gates still require their existing approvals. Integrating this feature branch into the application's active build remains a release step.

Rollback uses normal Git revert of the two technical feature commits and a clean rebuild. Existing file download and business read routes remain the baseline behavior.
