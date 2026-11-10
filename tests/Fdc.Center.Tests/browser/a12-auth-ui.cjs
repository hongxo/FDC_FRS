const assert = require('node:assert/strict');
const { chromium } = require('playwright');
const fs = require('node:fs/promises');
const path = require('node:path');

async function run() {
  const origin = new URL(process.env.FDC_A5_ORIGIN);
  assert.equal(origin.protocol, 'https:');
  assert.equal(origin.hostname, '127.0.0.1');
  const ids = JSON.parse(process.env.FDC_A7_IDS);
  const evidence = process.env.FDC_UI_EVIDENCE;
  const browser = await chromium.launch({
    executablePath: process.env.FDC_UI_BROWSER || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
    ignoreDefaultArgs: ['--disable-back-forward-cache']
  });
  try {
    const context = await browser.newContext({ ignoreHTTPSErrors: true });
    await context.addInitScript(() => {
      addEventListener('pageshow', event => { window.a12Persisted = event.persisted; });
    });
    const page = await context.newPage();
    await page.goto(new URL('/identity/', origin).href);
    await page.locator('#login-controls:not([disabled])').waitFor();
    await page.locator('[name=username]').fill(process.env.FDC_A5_USER);
    await page.locator('[name=password]').fill(process.env.FDC_A5_PASSWORD);
    const loginResponse = page.waitForResponse(r => r.url().endsWith('/auth/login') && r.request().method() === 'POST');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    assert.equal((await loginResponse).status(), 202);
    await page.getByText('Nhập mã MFA để hoàn tất đăng nhập.', { exact: true }).waitFor();
    assert.equal((await context.cookies()).some(c => c.name === '__Host-FdcSession'), false);
    await page.locator('[name=factor]').fill(process.env.FDC_A5_FACTOR);
    const factorResponse = page.waitForResponse(r => r.url().endsWith('/auth/mfa/verify'));
    await page.getByRole('button', { name: 'Xác minh', exact: true }).click();
    assert.equal((await factorResponse).status(), 204);
    await page.getByText('Đăng nhập thành công.', { exact: true }).waitFor();
    const session = (await context.cookies()).find(c => c.name === '__Host-FdcSession');
    assert.ok(session?.secure && session.httpOnly && session.sameSite === 'Strict');
    assert.equal(await page.evaluate(() => document.cookie.includes('__Host-FdcSession')), false);
    if (evidence) await page.screenshot({ path: path.join(evidence, 'a12-login-complete.png') });

    const pending = await context.newPage();
    await pending.goto(new URL('/identity/business.html', origin).href);
    await pending.getByRole('button', { name: 'Trạm', exact: true }).click();
    await pending.getByRole('button', { name: 'A7-Station-A', exact: true }).waitFor();
    let receivedDetail, resumeDetail;
    const detailHeld = new Promise(resolve => { receivedDetail = resolve; });
    const releaseDetail = new Promise(resolve => { resumeDetail = resolve; });
    await pending.route('**/api/business/v1/stations/' + ids.stationA, async route => {
      const response = await route.fetch();
      assert.equal(response.status(), 200);
      receivedDetail();
      await releaseDetail;
      try { await route.fulfill({ response }); } catch { /* Logout can abort the held request. */ }
    });
    await pending.getByRole('button', { name: 'A7-Station-A', exact: true }).click();
    await detailHeld;

    const history = await context.newPage();
    await history.goto(new URL('/identity/business.html', origin).href);
    await history.getByRole('button', { name: 'Trạm', exact: true }).click();
    await history.getByRole('button', { name: 'A7-Station-A', exact: true }).click();
    await history.locator('#object-detail h2').getByText('A7-Station-A', { exact: true }).waitFor();
    await history.goto(origin.href);

    let receivedLogout, resumeLogout;
    const logoutHeld = new Promise(resolve => { receivedLogout = resolve; });
    const releaseLogout = new Promise(resolve => { resumeLogout = resolve; });
    await page.route('**/api/identity/v1/auth/logout', async route => {
      const response = await route.fetch();
      assert.equal(response.status(), 204);
      receivedLogout();
      await releaseLogout;
      await route.fulfill({ response });
    });
    await page.getByRole('button', { name: 'Đăng xuất', exact: true }).click();
    await logoutHeld;
    await pending.waitForFunction(() => !document.querySelector('#results').childElementCount &&
      !document.querySelector('#object-detail').childElementCount &&
      document.querySelector('#business-navigation').hidden);
    assert.equal(await page.locator('#authenticated-links').isVisible(), false);
    resumeDetail();
    await pending.waitForFunction(() => !document.querySelector('#object-detail').childElementCount);
    if (evidence) await pending.screenshot({ path: path.join(evidence, 'a12-pending-logout-cleared.png') });
    resumeLogout();
    await page.getByText('Đã đăng xuất.', { exact: true }).waitFor();

    await history.goBack();
    await history.waitForFunction(() => !document.querySelector('#object-detail').childElementCount &&
      !document.querySelector('#results').childElementCount);
    const persisted = await history.evaluate(() => window.a12Persisted === true);
    const noStaleContent = await history.locator('body').evaluate(body => !body.textContent.includes('A7-Station-A'));
    assert.equal(noStaleContent, true);
    await context.addCookies([session]);
    const replay = await page.evaluate(async () => {
      const response = await fetch('/fixture/profile', { cache: 'no-store' });
      return { status: response.status, bytes: (await response.arrayBuffer()).byteLength };
    });
    assert.deepEqual(replay, { status: 401, bytes: 0 });
    if (evidence) {
      await history.screenshot({ path: path.join(evidence, 'a12-history-after-logout.png') });
      await fs.writeFile(path.join(evidence, 'a12-auth-ui.json'), JSON.stringify({
        utc: new Date().toISOString(), commit: process.env.FDC_A5_COMMIT,
        login: 202, mfa: 204, logout: 204, pendingResponseIgnored: true,
        multiTabCleared: true, historyNoStaleContent: noStaleContent,
        backNavigation, persistedBFCacheObserved: persisted, persistedBFCacheAcceptance: persisted ? 'PASS' : 'BLOCKED',
        revokedCookieReplay: replay, proofMaterial: 'omitted'
      }, null, 2));
    }
  } finally { await browser.close(); }
}
run().catch(error => {
  console.error('A12 UI verification failed.', String(error.stack).split(String.fromCharCode(10)).find(line => line.includes('a12-auth-ui.cjs:')) ?? '');
  process.exitCode = 1;
});
