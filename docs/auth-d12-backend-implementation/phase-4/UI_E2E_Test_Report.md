# UI tests and E2E limits

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

Actual rerun:17 Node client tests PASS and8 headless Edge browser component checks PASS. Browser uses intercepted local HTTPS responses, no real backend/user authentication. It checks503 no fallback,401 private-row clearing, text-only rendering against HTML payload, custom role beyond seven, explicit grant subject/assignment, approval versus applied,390px viewport and no runtime fixture scripts/page errors. Raw node-tests.tap,browser-results.json and management-mobile.png remain local.

S01–S04 remain the original mock-labelled prototype and have no real login/MFA/reset/profile integration; S05–S08 have a separate read entry and unregistered API adapters. Full API/UI E2E for all eight screens and named actor roles/custom role is BLOCKED. No synthetic default admin or role seed inserted as runtime authority. Fixture browser PASS is not live E2E PASS.

Original prototype and S01–S04 were not changed. No E2E test against IED/SCADA/Collector or a production URL. The actor cohort must be approved with real lab session/MFA and supported mappings before acceptance. Browser/client raw artifacts lack per-test exact start timestamps; artifact capture times are identified separately, not invented as execution start.
