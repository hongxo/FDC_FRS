# Live S01–S08 UI slice

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Actual /identity surface hosted by Center, not POC server. S01–S04 explicit unavailable states, no credential/fake MFA success. S05–S08 same-origin canonical real GETs, paging/error handling, no mock rows/fallback. Runtime copy adds deep links; original POC unchanged. Sensitive mutation forms disabled.

Live Edge context uses same actual Center HTTPS+SQL transaction fixture. Session consumer reads real SQL; no API response fulfilment. Route handler only allows same-origin traffic and aborts external. Test cookie supplied by process env, no output. Certificate trust exception confined loopback test context, no trusted-store install. Not actual credential login.

Actual8 screen assertions PASS (4 unavailable render,4 SQL-backed policy503), clear cookie401, no rows/page errors, desktop1440/tablet768/mobile390 overflow checks.3 screenshots, path/status-only traffic, JSON, committed-source TRX. Assertions inside1 C# test, not8 full E2E cases.

Initial failed runs retained: aborted response-body/navigation race; static asset SQL authentication/shared rollback-context contention when auth preceded assets, branch-only auth trial auto-insertion; fixed explicit global auth after assets. Same-document hash reused prior screen in harness; enter fresh deep links through index. Final browser run and committed-d21f352 run4/4 C# PASS.17 Node component checks supplement live run.

Full login/challenge/reset/profile/session mutations/custom-role request-review-apply/effective StationGrant/audit export/CSRF mutation workflow remain BLOCKED. No role-based navigation acceptance from role-less fixture.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
