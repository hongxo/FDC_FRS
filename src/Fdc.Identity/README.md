# Fdc.Identity — isolated development foundation

.NET 10 IAM library. It is NOT registered in Fdc.Center or Fdc.Collector.

- Domain/Security: dynamic role and same-UserRole scope evaluator, mandatory OT deny, person/revision SoD, version/expiry checks.
- Application: current-authority contracts, guarded role queries, immutable request/digest/approval primitives. Pending D04/D09 implementations deny/throw before persistence, never grant a default administrator.
- Infrastructure: EF SQL Server mappings, scoped read repository, transactional request/audit/outbox store requiring an externally supplied D09 transaction; offline generated SQL/migration class.
- Api: three canonical role query adapter routes requiring a future `Fdc.Human` policy and real session authority. Not mapped to any running host; no login/cookie/MFA implementation or force-apply endpoint.
- Seed manifest is a PENDING template. Missing IDs/codes/catalog sign-off are intentional, not an executable seed. Validator requires independent approval verification; no users, passwords or station grants are seeded.

Build/test the new project independently. The existing solution file and Center/Collector sources are intentionally unchanged while D01/D04/D05/D09 integration gates remain pending. See development reports under `docs/auth-d12-backend-implementation/` for exact completed/blocked scope.

Do NOT run migrations against an unconfirmed database. The offline generator only produces review artifacts; SQL integration, model snapshot/upgrade validation and rollback execution remain blocked on lab/owner confirmation. `DbContext.Database.Migrate()` is never called at startup.
