# Authentication UI V1 integration

Implemented a new management.html entry point using unchanged V1 CSS/brand assets and management.mjs. Original index.html/app.js/mock-data.js/components.js and S01–S04 remain unchanged. No sample users/grants are loaded by the new runtime entry point. It fetches fixed same-origin HTTPS API paths with credentials:same-origin, cache:no-store, redirect:error and textContent rendering.

Read subset: S05 users; S06 dynamic role list/code cursor; S07 explicitly identified subject+UserRole grant list; S08 audit list and request status inspection. API401 clears stale private rows;403/503 show explicit errors without fixture fallback. Tab/page restore/visibility changes invalidate outstanding requests; generation guards discard stale responses. APPROVED and202 are not displayed as effective APPLIED.

NOT integrated: User detail/revoke UI, permission matrix editing, role/grant CRUD forms, request submission/review/apply, real-session login/MFA and live host routing. No backend writer exists to call safely. These remain BLOCKED_BY_POLICY rather than mock-enabled. The management entry point is not served by Center; no listener/static-file/proxy config changed.

Node17 tests PASS. Headless Edge8 browser component checks PASS using INTERCEPTED HTTPS FIXTURES, not real authenticated API data. Includes no-HTML injection,401 clear,503 empty, custom role beyond seven, subject filters, approval/effective distinction and390px layout. Screenshot visually inspected. No claim of live S05–S08 end-to-end completion. Existing V1 prototype regression not rerun; original files untouched and assets copied byte-for-byte.
