const assert = require('node:assert/strict');
const { chromium } = require('playwright');
const fs = require('node:fs/promises');
const path = require('node:path');

async function run() {
  const origin = new URL(process.env.FDC_A5_ORIGIN);
  assert.equal(origin.protocol, 'https:');
  assert.equal(origin.hostname, '127.0.0.1');
  const browser = await chromium.launch({ executablePath: process.env.FDC_UI_BROWSER ||
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const evidence = process.env.FDC_UI_EVIDENCE;
  const steps = [];
  try {
    const context = await browser.newContext({ ignoreHTTPSErrors: true });
    await context.route('**/*', route => new URL(route.request().url()).origin === origin.origin ? route.continue() : route.abort());
    const page = await context.newPage();
    await page.goto(origin.href);
    const call = (url, body, csrf) => page.evaluate(async ({ url, body, csrf }) => {
      const response = await fetch(url, body === undefined ? { cache: 'no-store' } : {
        method: 'POST', cache: 'no-store', headers: { 'Content-Type': 'application/json',
          ...(csrf ? { 'X-CSRF-Token': csrf } : {}) }, body: JSON.stringify(body)
      });
      return { status: response.status, body: response.headers.get('content-type')?.includes('application/json')
        ? await response.json() : null };
    }, { url, body, csrf });
    const base = '/api/identity/v1/auth';
    async function expect(label, result, status) {
      assert.equal(result.status, status, label);
      steps.push({ label, status });
      await page.locator('#status').evaluate((el, label) => { el.textContent = label; }, label);
      return result.body;
    }
    const cookie = async () => (await context.cookies()).find(c => c.name === '__Host-FdcSession');
    const credentials = { username: process.env.FDC_A5_USER, password: process.env.FDC_A5_PASSWORD };
    const anonymousCsrf = (await expect('Antiforgery issued', await call(base + '/csrf'), 200)).token;
    await expect('Anonymous profile denied', await call('/fixture/profile'), 401);
    await expect('Missing CSRF denied', await call(base + '/login', credentials), 403);
    const wrong = await expect('Wrong password denied', await call(base + '/login', { ...credentials, password: 'invalid-fixture' }, anonymousCsrf), 401);
    const unknown = await expect('Unknown user denied', await call(base + '/login', { ...credentials, username: 'unknown-fixture' }, anonymousCsrf), 401);
    assert.deepEqual(unknown, wrong);
    const mfa = await expect('Password verified · MFA required', await call(base + '/login', credentials, anonymousCsrf), 202);
    assert.equal(mfa.code, 'MFA_REQUIRED');
    assert.equal(await cookie(), undefined);
    await expect('Profile denied before MFA', await call('/fixture/profile'), 401);
    await expect('Incorrect factor denied', await call(base + '/mfa/verify', { challenge: mfa.challenge, factorResponse: 'invalid-fixture' }, anonymousCsrf), 401);
    await expect('MFA complete · cookie issued', await call(base + '/mfa/verify',
      { challenge: mfa.challenge, factorResponse: process.env.FDC_A5_FACTOR }, anonymousCsrf), 204);
    const session = await cookie();
    assert.ok(session?.secure && session.httpOnly && session.sameSite === 'Strict' && session.path === '/');
    assert.equal(await page.evaluate(() => document.cookie.includes('__Host-FdcSession')), false);
    const profile = await expect('Protected profile read using SQL session', await call('/fixture/profile'), 200);
    assert.match(profile.id, /^[0-9a-f-]{36}$/i);
    assert.equal(profile.username, credentials.username);
    const admin = '/api/identity/v1/admin';
    const users = await expect('Scoped user list', await call(admin + '/users'), 200);
    assert.deepEqual(users.items.map(u => u.id), [profile.id]);
    const detail = await expect('Scoped user detail', await call(admin + '/users/' + profile.id), 200);
    assert.equal(detail.id, profile.id);
    await expect('Out-of-scope user hidden', await call(admin + '/users/11111111-1111-1111-1111-111111111111'), 404);
    const roles = await expect('Scoped role catalog', await call(admin + '/roles/'), 200);
    assert.equal(roles.items.length, 1);
    const permission = await expect('Scoped permission catalog', await call(admin + '/permission-catalog'), 200);
    assert.deepEqual(permission.items.map(p => p.code), ['a5.fixture.read']);
    const rolePermissions = await expect('Scoped role permissions', await call(admin + '/roles/' + roles.items[0].id + '/permissions'), 200);
    assert.deepEqual(rolePermissions, process.env.FDC_A7_IDS ? ['a5.fixture.read', 'a7.fixture.download'] : ['a5.fixture.read']);
    if (evidence) await page.screenshot({ path: path.join(evidence, 'a5-isolated-profile.png') });
    const business = process.env.FDC_A7_IDS ? await require('./isolated-business.cjs')(page, call, expect, evidence) : null;
    const authenticatedCsrf = (await expect('Authenticated antiforgery issued', await call(base + '/csrf'), 200)).token;
    await expect('Logout revoked SQL session', await call(base + '/logout', {}, authenticatedCsrf), 204);
    assert.equal(await cookie(), undefined);
    const afterLogoutCsrf = (await expect('Anonymous antiforgery after logout', await call(base + '/csrf'), 200)).token;
    await expect('Consumed MFA challenge denied', await call(base + '/mfa/verify',
      { challenge: mfa.challenge, factorResponse: process.env.FDC_A5_FACTOR }, afterLogoutCsrf), 401);
    await context.addCookies([session]);
    await expect('Revoked cookie replay denied', await call('/fixture/profile'), 401);
    if (evidence) {
      await page.screenshot({ path: path.join(evidence, 'a5-isolated-replay-denied.png') });
      await fs.writeFile(path.join(evidence, business ? 'a7-browser.json' : 'a5-browser.json'), JSON.stringify({
        run: path.basename(evidence), commit: process.env.FDC_A5_COMMIT || 'unspecified', timestampUtc: new Date().toISOString(),
        environment: 'TEST_ONLY HTTPS loopback + disposable SQL rollback fixture', steps,
        cookieAttributes: { secure: true, httpOnly: true, sameSite: 'Strict', path: '/' },
        business, identity: business ? credentials.username : 'fixture', correlation: 'a5-isolated', proofMaterial: 'omitted'
      }, null, 2));
    }
    console.log('A5 isolated browser: login, MFA, cookie, SQL profile, logout and replay denial PASS.');
  } finally { await browser.close(); }
}
run().catch(error => {
  const location = String(error.stack).split('\n').find(line => /isolated-(iam|business)\.cjs:\d+/.test(line));
  console.error('Isolated browser assertion failed.', location ?? '');
  process.exitCode = 1;
});
