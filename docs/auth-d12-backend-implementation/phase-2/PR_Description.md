## Scope

Adds the standalone .NET10 IAM foundation and a partial D12 Phase2 authentication/authorization implementation to this previously README-only repository. The original source directory has no Git history; this is an explicit initial IAM import, not a merge of verified remote Phase1.

Adds SQL current-session/current-authority readers, opaque proof/password primitives, opt-in Human cookie handler and Origin/antiforgery guards. Existing same-UserRole dynamic permission evaluator remains role-name independent. No Center/Collector host is changed or exposed.

## Verification

Stable .NET10 build passed.102 unit/component tests passed;2 explicit SQL lab tests passed separately. SQL fixture rows were rolled back. Full HTTP login/logout, MFA, password recovery, shared lockout/session issuance, audit atomicity and commit/revoke race are not implemented/accepted. Their policies remain blocking and default adapters deny.

## Review constraints

PARTIALLY_COMPLETED/BLOCKED_BY_POLICY. Do not activate handlers or claim Phase2 complete/production ready. No runtime accounts/passwords/grants/catalog seed or startup migration. Foundation migration is included as source, not applied by CI. Formal approvals and P0-IAM status are unchanged. Phase3 role CRUD/workflow is excluded. No automatic merge.

Raw execution evidence is retained locally; generated test/build artifacts are excluded from this PR. This is a draft for review, not a completed-feature merge request.
