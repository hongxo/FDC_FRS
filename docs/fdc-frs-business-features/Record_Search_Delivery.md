# COMTRADE record search delivery

Technical source: `b852456421a5902d7f8a0540d5c4ab7cf20f250c`. Branch `feature/fdc-frs-record-search` builds on open Draft PR #26 at `b82c92af8afb51f8746f2a4e354122437763acb1`.

## Business functions

Open an IED detail and choose **Xem bản ghi của IED**. The COMTRADE list keeps that IED filter while paging. The record view also provides UTC collection start/end inputs, a reset action for IED/time conditions, and the existing keyword search. Record rows and details display the stored collection timestamp.

The filter applies to `CollectedAtUtc`. The start is inclusive and the end is exclusive, expressed as `[collectedFromUtc, collectedToUtc)`. These values use UTC. They describe collection timing, while the waveform feature preserves the CFG's source timestamps separately.

## API and compatibility

Both paged and list variants of `GET /api/business/v1/records` accept optional `iedId`, `collectedFromUtc` and `collectedToUtc`, together with the existing station, search, offset and limit inputs. Invalid intervals, a zero IED identifier, non-UTC offsets and record filters on other object types return HTTP 400. Existing calls without the new fields preserve their query behavior.

IED/time conditions are applied in SQL with the existing station scope before Count, Skip and Take. The new record-only nullable `collectedAtUtc` response field is omitted on other object types. Existing read authority, waveform, download and audit paths are reused. This feature adds no migration or IAM Administration behavior.

## Executed evidence

The focused build completed with 0 warnings and 0 errors. Two existing integration methods were expanded and executed against disposable local SQL and real Chromium. The corrected run completed 2 PASS, 0 FAIL, 0 SKIP. SQL cases cover collection start/end boundaries, two filtered pages, alternate IEDs, an out-of-scope station, UTC validation and grant revocation. The browser exercised IED-to-record navigation, native date entry, an empty time window, invalid range, reset and waveform regression.

Initial raw evidence is under `tmp/record-search-check`: both methods failed. The SQL fixture omitted two required CollectorIed links and the Edge datetime-local control rejected the automated seconds-zero input. The corrected fixture includes the required links and the test uses the control's normalized minute representation for the same instants. The API boundary assertions retain full DateTimeOffset precision. A standalone control diagnostic is retained as `tmp/check-date-control.cjs`.

Corrected evidence is under `tmp/record-search-check-02`: build.log, tests.log, TRX, browser steps and full-page business-record-search.png. The before/after fixture snapshots contain the same pre-existing database and zero generated runtime logins. The new test fixtures were removed. Source and assertions are available for review against the technical SHA above.

## Reproduction and integration

Build `Fdc.Identity.slnx` and use the disposable test flags for local Shared Memory, Chromium and the existing fixture factory. Focused filter: `FullyQualifiedName~Browser_business_authority_and_download|FullyQualifiedName~Business_reads_scope_aggregate`.

The feature is delivered as source for review and integration into the active application build. Read mappings and deployment activation retain their existing approval requirements. Rollback is a normal revert of the two technical commits on this branch and a clean rebuild.

## Final regression and evidence hashes

The full solution run on the technical SHA above recorded 420 PASS / 0 FAIL / 1 SKIP. The existing child-only worker entry is the skipped item. Node regression recorded 11 PASS. These are separate totals from the two focused methods.

The full runner exited at its global cleanup guard with two matching fixture databases. Subsequent metadata inspection recorded creation times of 2026-11-10 21:54:50 and 2026-11-11 14:22:17. No ownership inference or manual deletion was performed. The focused before/after snapshots identify exactly the same pre-existing database and zero generated runtime logins at that replay's observation points. Full-run cleanup remains unverified under the runner's global-zero condition.

Full-run evidence: `tmp/phase4-4ca10/record-search-regression`. Node output: `tmp/record-search-node.log`. Cleanup inspection: `tmp/record-search-global-after.log`. Runner exception: `tmp/record-search-runner-error.log`.

SHA-256 for the corrected focused run, followed by full-run logs:

```text
10c665ebe5e23d57d4fa788d5c5514b2c2c10aab9edd27a5e5f1da1c42821311  build.log
4d6fa0a48e45a2ac03e9a0d8cbf1d80e536fdeed6a8af8e328f749c0d64293c5  tests.log
5e1d51222d7eaa665cc80470bed4d81c2b4fed9d59ab0a76d52831fcd34b6f6c  before.log
5e1d51222d7eaa665cc80470bed4d81c2b4fed9d59ab0a76d52831fcd34b6f6c  after.log
0352e9f56123d009f9cdd6efabad54ce23d2d218faa42c6d4a4b6676af1367c2  a7-browser.json
a0cb9ce45a2f8575d1cf6be8abc3e3aaec4736d4a77fc8c691a2e9634f688a7c  a7-sql-verification.json
624ef724e77d90fb970bb6919021e7378fcfa86ba54f146d17b71f32966c3950  business-record-search.png
40a20d69be42cfb9884bbfb0fb9029b194f63b1a79c15863dd66a1fadb310cbc  focused TRX
a6a059f7ffaf644a909d23670cb4d8ae8ce07699e2d4697365307d166ad5884b  build.log
2975a2271a13bec696bc7eaccb7c83d4ed8093a022fcf2496ac1231f60d1e9c7  tests.log
df4e26a04a444901b95afef44e4a96cfae34690fff2ad2c66389c70079cdff2b  cleanup.log
39d39b064157c836095af4f9c11ea043e7107bbba43a5156f18946a0be7ccd4c  record-search-node.log
```
