## A.12 isolated ingress, worker recovery and Login UI

Machine uploads now have an isolated Testing scheme requiring both fresh signed CRL and OCSP before SQL station/IED ingestion. Revoked, unknown, stale and unavailable evidence denies the request. A process harness hard-kills a worker after SQL staging and verifies replacement release or quarantine after grant revocation.

The Login/MFA/logout forms now call canonical CSRF and authentication endpoints when the server reports the feature ACTIVE. Logout clears Business tabs and fences delayed responses. Actual browser verification covers form submission, MFA cookie issuance, held logout/detail responses, and cookie replay denial.

Eight original A.11 blocker IDs are closed at their tested isolated layers. Persisted BFCache, atomic rebind, multi-node HA/DR, principal separation and owner approvals remain open. G1/G2 blocked, G3 pending, G4 unauthorized and production no-go. Draft stacked on A.11.
