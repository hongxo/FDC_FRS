# Authentication security tests

Date: 2026-11-08 (Asia/Bangkok). Baseline `b8bd4c0b1d6731b0945b3a6bd12a7b0ea50465e4`; validated source `f60f8e2cdebc25416bbce5d4ab19b7bf6594b960`. Development/Lab only. Formal approvals, P0-IAM and production readiness unchanged.

PASS at component level: PasswordHasher salted hashes, wrong/malformed proofs, rehash and input bounds; opaque token format/randomness/purpose binding; disabled/revoked/expiry/idle/absolute/security/grant/policy/restore-epoch rejection; secure host cookie settings; forged identity header and duplicate-cookie rejection; ASP.NET antiforgery real token-pair validation, missing proof/Origin checks. Existing synthetic reader tests are not SQL integration evidence.

Real SQL session reads and current-version/disable/revoke fixtures PASS (2 cases). Real missing-database and1-second command-timeout tests PASS (2 cases) after P4-D003 correction. SQL error wrapping no longer escapes as generic InvalidOperationException on these paths. Timeout is a test-only WAITFOR prefix, not an outage of other users' SQL service.

BLOCKED: actual HTTP login valid/invalid/unknown accounts, shared rate limiting/lockout, enumeration timing, logout/rotation/concurrent issuance, password change/reset/recovery/single-use and recovery session invalidation. No live issuer or approved D02/D03/D05/D06 policy. No plaintext password/raw token/connection string in committed evidence. No claim of session fixation or full browser cookie semantics acceptance.
