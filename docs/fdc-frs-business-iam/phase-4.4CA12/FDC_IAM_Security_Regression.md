# IAM regression

A.12 adds a tested Login/MFA/logout UI connected to canonical HTTP contracts. It uses feature ACTIVE only to expose controls. Authentication and SQL authority remain server responsibilities. Business, Lab and Collector binding views clear local state on logout signals; a cached page compares the non-secret logout marker on pageshow. Login inputs, MFA challenge and pending requests are cleared on pagehide. Controls revalidate policy after restore.

The isolated browser test uses random temporary credentials, test factor and fresh SQL data. Normal Center authentication flows retain pending policy and stores. This phase does not activate a shared deployment or configure real MFA enrollment. Canonical tests for authority, scope, mutation gates and audit/outbox are included in the full regression. Existing user edits to management.mjs and its test remain excluded from clean-source verification.
