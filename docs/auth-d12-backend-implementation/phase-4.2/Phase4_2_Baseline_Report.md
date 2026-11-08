# Phase4.2 baseline

2026-11-08. Development/Lab only. Source d21f3520ecc8634f71e7a679a945439a964d9dc8. Base648c03162a9469bf6de531a9b91146f9f71e81a8.
Environment: Windows, SDK10.0.403, SQL2022 16.0.1210.3, confirmed lpc:. / FdcIam_DevelopmentLab (user confirmation; not FDC_Center_Lab). SQL MachineName guard XOTH-ETC-2; @@SERVERNAME DESKTOP-N62V8AF is instance metadata.
Independent reviewer: NOT_PROVIDED. Owner decisions remain PENDING. Readiness/P0-IAM unchanged. Evidence: Evidence_Index.md and local ignored evidence/. Commands: Security_Regression_Report.md. No credentials/raw cookie/private keys shipped.

PR4 verified OPEN/DRAFT at tip648c031 before branching. Created feature/d12-phase4-runtime-integration from that tip; inherited Phase2/3/4 dependencies. Original root has no .git; implementation in previously authorized separate official checkout tmp/FDC_FRS-phase2-delivery. Original root source not overwritten. Official history lacked product Center/Core; imported27 original source/static/project/lock files excluding config/bin/obj/data. Core dependency source unchanged; no Collector import/change.

Read Phase2/3 reports, Phase4 defects/DR/security reports, Phase4.1 completion/tests/delivery and original AUTH-R2/Dynamic_RBAC/canonical UI material. Hash manifests local; some design inputs exist only in original root. Historical NOT_AUTHORIZED design snapshots do not override subsequent user coding authorization, which does not sign domain policies.

Commands: git status/remote/branch/log/fetch; gh auth status/repo view/pr view; source inventory; local cache restore; SQL postcheck. Final DB24 app tables/0 app rows/33 FK/93 checks/0 invalid. Existing migration202611080001 product10.0.14 unchanged. Lab runner sysadmin=1, not least-privilege acceptance. No new migration or seed.

| Work | Dependencies | Disposition |
|---|---|---|
| Product host/DI/negative HTTPS | Explicit Phase4.2 lab authorization | Implemented, review pending |
| Generic operation resolver | No effective grants needed | Implemented default-deny |
| Unavailable UI + management GETs | Actual HTTPS/SQL session consumer | Implemented negative slice |
| Login/MFA/recovery | Route release and owner policy | Policy + implementation blocked |
| Effective grants/delegations | D04/D09/D10 | Policy + implementation blocked |
| Restore/HA/principals | Reviewed separate disposable targets | Policy/environment blocked |

## Remaining conditions / next action

D01 runtime/route/trust release, D04 explicit permission/scope/delegation/custody, D02/D03/D05/D06 authentication values, D09 all-writer protocol and D10 archive/restore/principals still pending. Missing dependent issuers/providers/writers/UI remain IMPLEMENTATION gaps, not merely approval gaps. Separate restore/HA/multi-host environments absent. Independent review NOT_PROVIDED. Revert isolated feature commits for code rollback; no schema down, stale-authority restore or Human fallback to legacy keys.
