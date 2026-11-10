# A.12 final assessment

1. Ten original A.11 blockers closed in isolated scope. 76 remain BLOCKED. Closed IDs are 032,039,040,043,060,061,067,068,069,070.
2. Four A.12 acceptance test methods actually executed. Full regression counts are reported in the raw manifest without adding historical phase runs.
3. All four A.12 methods PASS. Mandatory E2E has 10 PASS and 10 BLOCKED, zero FAIL or SKIP in that requirement matrix. The child-only worker entry is SKIP in the parent discovery and is run explicitly in child processes.
4. OCSP is integrated into real HTTPS in the isolated Pilot candidate. Main startup retains its policy boundaries.
5. STRICT_COMBINED tested with signed CRL and OCSP. Other compositions remain open.
6. Certificate rotation remains BLOCKED.
7. Actual hard kill after committed staging and before release PASS. Kill inside staging remains BLOCKED.
8. Multi-process lease race PASS with two OS processes and explicit SQL lease barriers.
9. Export/revoke after restart PASS with SQL quarantine and HTTPS 401/zero bytes.
10. Pending logout, delayed detail and multi-tab clearing PASS. Persisted BFCache was not observed and remains BLOCKED.
11. Atomic Collector rebind remains BLOCKED.
12. Multi-node session revoke remains BLOCKED. Browser tabs in one context are separate evidence.
13. Spoofed cert header rejected at direct Pilot ingress. Trusted proxy topology spoofing remains BLOCKED.
14. SQL backup/restore remains BLOCKED.
15. Admin/migrator identity separation remains BLOCKED. Runtime least privilege remains tested.
16. P0/P1 gaps remain in the risk register.
17. G1/G2 BLOCKED.
18. G3/G4 lack signed dispositions.
19. Shared Lab activation remains unauthorized.
20. Production remains NO-GO.

S01/S02/S04 now have real UI event handlers and isolated HTTP/SQL verification. The main Center still has pending feature authority and does not map candidate Login/MFA issuance. Independent review, policy approval, Lab authorization, Lab activation and production readiness remain distinct states.

IAM V1 branch compatibility remains unverified. Existing canonical IAM regression was run on the checked-out baseline. See [the compatibility report](FDC_IAM_V1_Compatibility_Report.md).
