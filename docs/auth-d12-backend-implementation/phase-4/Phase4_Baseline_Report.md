# Phase4 baseline and preconditions

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

Repository hongxo/FDC_FRS, exact origin https://github.com/hongxo/FDC_FRS.git. Clean starting Phase3 branch at b8bd4c0. Created feature/d12-security-validation from origin/feature/d12-management-workflow. PR#2 remains OPEN/DRAFT and depends on PR#1; neither is assumed merged into main. Main remains README-only initial history. CLI/GCM account hongxo and default main verified with read-only GitHub metadata.

Baseline build PASS,0 warnings/errors;164 component/unit PASS,6 SQL cases skipped without lab configuration. Source review confirms real isolated IAM library/SQL role CRUD, but no integrated Login/MFA host or effective management endpoints. Reports alone were not treated as proof of completion.

Lab: prior explicit user confirmation selects FdcIam_DevelopmentLab on local shared-memory default instance, overriding generic attachment name FDC_Center_Lab. Labels/host checked before rollback fixtures. No FDC_Center_Lab creation, production connection, Collector/IED/SCADA/firewall/PKI changes or server outage. Command timeout is injected only into the test connection; missing-database connection never creates a DB.

Migration001 remains installed;24 application tables. D03/D04/D05/D06/D09/D10 policy dispositions remain pending. No signed operation mapping, deployed Human/Machine topology, real factor/recovery authority, shared login issuer or complete durable administrative workflow. Independent reviewer sign-off NOT_PROVIDED. Environment READY only for bounded component/rollback SQL testing, BLOCKED for complete IAM E2E/UAT acceptance.

Raw baseline-build.txt,baseline.trx,baseline-run.json and baseline-sha.txt are local-only. Latest full unit run166 PASS/11 skips and explicit SQL11 PASS are separate from baseline.
