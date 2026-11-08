# S05–S08 read integration candidate

Open `management.html` through an independently reviewed same-origin HTTPS Human host. It is not served by Fdc.Center and does not create login/session functionality. Do not present this as a completed live IAM deployment.

The V1 styles/brand asset are copied unchanged. Original V1 `index.html`, `app.js`, `mock-data.js` and S01–S04 remain untouched in the original workspace and are not imported as runtime code. This entry uses only real same-origin GET calls to fixed canonical API candidates; no fallback fixtures or frontend permission authority.

S05 user list, S06 dynamic role list/code cursor, S07 explicit user+assignment grant list, S08 security-event list and request status inspection are implemented. CRUD forms, permission matrix editing, request submission/review/apply, user details/session revoke and live login are not integrated. All new backend read adapters are unregistered and pending D04/current-session policy. Browser tests intercept HTTPS responses as test fixtures only, not proof of live API/security acceptance.

Run client tests: `node --test poc/authentication-ui-v1/tests/management-client.test.mjs`.
Browser component runner: `node poc/authentication-ui-v1/tests/management-browser.cjs`, with an externally supplied Playwright module and optional `FDC_UI_BROWSER` executable. It does not install dependencies or contact a production service. Evidence stays local under phase-3/evidence and must not be mistaken for real authenticated integration.
