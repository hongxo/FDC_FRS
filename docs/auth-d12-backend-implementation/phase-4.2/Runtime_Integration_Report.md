# Actual Fdc.Center integration

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

Program.cs calls product CenterApplication.Build; factory selects opt-in CenterIdentityRuntime before operational Store/InboxWorker construction. This is the product executable, not a separate IAM demo. IdentityRuntime:Enabled defaults false. Enabled mode requires external ConnectionStrings:FdcIdentity, writable-primary intent and Development/Testing/Lab. Production rejects activation; requests require loopback HTTPS. Test cert ephemeral in memory, no trusted-store install. No operating listener/config/firewall changed.

Actual DI: DbContextFactory, SqlSessionAuthorityReader, Fdc.Human authentication/policy, RoleManagementService+SqlRoleQueries, ManagementQueryService+SqlManagementQueries, SqlCurrentAuthorityReader+PermissionEvaluator. Pending policies remain explicit. No Store/BundleReceiver/InboxWorker in this profile. No forwarded principal trust.

Pipeline: HTTPS/loopback/machine-path deny + no-store/CSP/security headers; public /identity assets; explicit global authentication; authorization; real admin GET endpoints. One branch-only authentication trial allowed minimal-host automatic insertion earlier; fixed by explicit registration after assets and retested. Public UI carries no account/business data.

Actual /api/identity/v1/admin GETs: role list/detail/permissions; users/detail/roles; station-grants; permission-catalog; audit/detail; change-request detail. Policies return503. Status/events/commands/download/waveform preserve route identities but fence503 after auth without executing data handlers. Machine /api/stations rejects403. No canonical login aliases invented. Old dashboard not served in Human profile.

Legacy startup remains for compatibility, including Demo/key/mTLS behavior; NOT claimed IAM-secure. Do not deploy shared business backend reachable through legacy keys and infer isolation. D01 topology approval and bypass testing still required.

Actual product factory tests:3 startup PASS +1 HTTPS/SQL PASS (browser inside same test), no auth mocks. Test DB factory shares real SQL transaction for rollback-only fixture, not production-concurrency validation. No complete deployment PKI/proxy or all-endpoint station enforcement proof.

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
