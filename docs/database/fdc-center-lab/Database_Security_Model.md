# SQL security and authority boundaries

2026-11-08 · Development/Lab only. Source a0a6096dbf2c192cc2c674175c17137f59a17e47; baseline PR5 9cd35889ddbbcc8d88e831419284877f4add5c6a. Official checkout tmp/FDC_FRS-phase2-delivery; original root source unchanged.
Target: local lpc:. / FDC_Center_Lab, SQL2022 16.0.1210.3, MachineName XOTH-ETC-2. Old FdcIam_DevelopmentLab is separate and not migrated/copied. Current user instruction explicitly authorizes new Lab creation/implementation; no owner domain approval inferred.
CURRENT: deployed schema. VERIFIED: only actual checks below. PROPOSED/PENDING: unapproved policies and future workflows. APPROVED: none newly assigned. Reviewer NOT_PROVIDED. No P0-IAM/Lab GO/UAT readiness promotion.
Evidence: Evidence_Index.md and ignored local evidence/. Commands: Database_Deployment_Runbook.md / Database_Test_Plan.md. No password/raw session/private key/DPAPI artifact committed.

| Principal contract | Actual local state | Limit |
|---|---|---|
| Migration operator | Explicit transient operator credential runs guarded tool, not application | Local operator sysadmin, NOT least-privilege migrator acceptance |
| fdc_center_migration | Role with VIEW DEFINITION, no members | D09-controlled DDL/writer procedure rights pending; scaffold not deployable production role |
| fdc_center_runtime | Dedicated random-password SQL login/user, role membership only, non-sa/non-db_owner verified | SELECT User/PersonReference/Session/AuthorityVersion + one table VIEW DEFINITION; no record data grants pending D04 |
| fdc_center_reporting | Role, no members, denies unscoped summary views and writes | Scoped reporting procedure/authority design pending |
| fdc_center_audit_writer | Role, no members, no archive powers | D10 failure/custody/writer policy pending |
| fdc_center_maintenance | Role VIEW DATABASE STATE, no members | Backup/maintenance privileges require operator review |

Actual runtime tests assert SQL229 for password/MFA reads, authority changes, role/audit deletes, record writes and global summaries. Product startup guard rejects sa, sysadmin/db_owner, wrong target or legacy-key mode. Runtime credential generated cryptographically and encrypted Windows DPAPI CurrentUser in workspace tmp outside checkout; filename ignored. Lab convenience only, not production vault/key rotation policy. No default human account/password/admin or StationGrant. Infrastructure SQL user is not an IAM User.

Old IAM is not connected as a second active authority. New DB0 app rows/no configured AuthorityVersion; no actual Human authentication enrollment. Real startup/API test uses nonexisting random cookie to force SQL lookup and401; machine route403. Source application still requires approved D04 authority for reads; fixture-only allow providers live in tests, not runtime. Human/machine service contracts separate; no RoleCode authorization or inherited global admin.

SQL injection review: LINQ and interpolated values parameterized; cleanup table names fixed source allowlist (localized EF1003 annotation), SQL identifiers for new login/restore safely quoted and generated only, review scripts not user request SQL. Least privilege protects IAM/ingest; D09 all-writer/revoke ordering and direct-DML production model still not approved. Archive fields/signing/read principals not automatic acceptance.

## Limits / next action

D01–D10 applicable policy choices remain PENDING. Complete approved Human/Machine routing/PKI, D04 scope providers, D09 all-writer control, D10 archive/custody/restore-epoch acceptance and business analysis workflows before activation. No fake analysis or full DR01–DR29 PASS. Independent review required. No Production/IED/SCADA/firewall/Collector change. Code rollback by reviewed feature revert; database rollback follows guarded runbook, never restore stale effective authority.
