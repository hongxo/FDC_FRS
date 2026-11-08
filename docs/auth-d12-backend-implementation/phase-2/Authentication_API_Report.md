# Authentication API report

Status: BLOCKED_BY_POLICY for runtime activation. Phase2 adds reusable HumanSessionHandler and HumanRequestSecurity, not new registered HTTP routes. Existing three role GET adapters remain unregistered in Center.

POST /api/identity/v1/auth/login, GET /auth/me and POST /auth/logout are candidate routes from the instruction, not declared approved A01–A21 contracts. No aliases added. No actual HTTP login/logout/reset API success is claimed.

Implemented: secure host-prefix cookie writer; explicit SameSite input; no Domain; Secure/HttpOnly/Path=/; opaque digest-only session lookup; no stale authority cache; client cert rejected on Human handler; untrusted forwarded headers do not authenticate. Login orchestration checks supplied policy before any store call; unknown/disabled/locked accounts share rejected result. Shared throttle and atomic audit remain transaction-store responsibilities, with pending default implementation refusing execution.

CSRF checks use actual ASP.NET antiforgery plus exact Origin/Host. Real-provider token pair acceptance and missing-token rejection were executed as component tests, not a deployed reverse-proxy test. No CORS/listener/certificate/firewall changed.

Missing: host DI registration, protected me DTO, logout/revoke SQL writer, password lifecycle, SQL login lockout/rate limiter, HTTP error/ETag/idempotency contracts, bounded hashing concurrency, trusted source normalization, mandatory security audit. These need policy disposition and implementation before exposure. A handler alone must never be treated as endpoint authorization.
