# Phase4 draft PR

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

## Dependency

Stacked draft on feature/d12-management-workflow; depends on unmerged draft#2 (and#1). No merge or production approval requested.

## Changes

Adds actual SQL regression repros and fixes3 defects: inconsistent role metadata/payload target; idempotent replay returns later version; EF-wrapped authority error escapes typed unavailable contract. Aligns role GET HTTPS/certificate/error guards. Adds bounded real SQL timeout/connection-failure and scoped-query profile tests; retains all failure/retest evidence locally.

## Results and limits

Build passed;166 unit/component cases passed;11 SQL lab cases passed in explicit separate runs;17 Node client and8 intercepted-HTTPS browser component checks passed. Synthetic SQL fixtures rolled back; no migration or persistent seed. NuGet feed returned0 vulnerable package entries for3 projects at scan time.

All29 DR scenarios remain BLOCKED as full acceptance because live Login/MFA/current authority/workflow, D09 ordering and real S01–S08 integration are incomplete. Not E2E acceptance or race-free proof. Tiny3-user query profile is observation only, threshold pending. Technical recommendation NO-GO for complete Lab/UAT. Independent sign-off absent; P0/readiness unchanged.

Raw generated test/build evidence not committed. No source upload to external scanner, operating OT/Collector tests, production changes or automatic merge.
