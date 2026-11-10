# A.12 final assessment

1. Eight original A.11 blockers closed in isolated scope. 78 remain BLOCKED. Closed IDs are 032,039,040,060,067,068,069,070.
2. Three new acceptance test methods actually executed. Full regression counts are reported in the raw manifest without adding historical phase runs.
3. All three new methods PASS after fixes. Mandatory E2E has 9 PASS and 11 BLOCKED, zero FAIL or SKIP in that requirement matrix. The child-only worker entry is SKIP in the parent discovery and is run explicitly in child processes.
4. OCSP is integrated into real HTTPS in the isolated Pilot candidate. Main startup retains its policy boundaries.
5. STRICT_COMBINED tested with signed CRL and OCSP. Other compositions remain open.
6. Certificate rotation remains BLOCKED.
7. Actual hard kill after committed staging and before release PASS. Kill inside staging remains BLOCKED.
8. Simultaneous multi-process lease race remains BLOCKED.
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
