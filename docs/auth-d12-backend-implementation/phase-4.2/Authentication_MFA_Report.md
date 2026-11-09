# Authentication MFA session

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Actual product SQL session consumer validates opaque cookie digest, current User/Session/security/grant/policy/catalog/hierarchy/RestoreEpoch and idle/absolute validity. Disabled/revoked rejects401, unavailable SQL503. Raw proof never SQL/logged. Test session synthetic rollback-only, no role/grant/admin seed; not credential-login issuance.

Existing own-session revoke SQL service registered with pending policy;3 SQL tests rerun PASS for current/all scope, version/challenge/audit/outbox atomicity. Does not prove complete logout/current-user/rotation/reset HTTP flows.

Missing: real SQL login issuer/shared throttle/lockout provider, approved password cost/provider binding, WebAuthn/TOTP enrollment/assertion verifier and recovery issuer/consume. RP ID/origin/UV/TOTP exceptions/custody/channel/session durations/SameSite remain owner choices. No fake OTP or client mfa=true path. Exact route release needed: UI proposal /api/identity/v1/login versus earlier /api/identity/v1/auth/login; neither alias silently added.

Actual credential login/MFA/reset/HTTP logout: BLOCKED_BY_POLICY plus IMPLEMENTATION. Secure/HttpOnly issuance/rotation cannot be inferred from browser manually supplied cookie. CSRF service registered and primitives tested; live sensitive mutations not enabled, so CSRF end-to-end acceptance not claimed.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
