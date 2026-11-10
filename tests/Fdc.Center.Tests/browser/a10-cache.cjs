const assert = require('node:assert/strict');
const { chromium } = require('playwright');
const fs = require('node:fs/promises');
const path = require('node:path');
(async () => {
  const origin = new URL(process.env.FDC_A10_ORIGIN);
  assert.equal(origin.protocol, 'https:');
  assert.equal(origin.hostname, '127.0.0.1');
  const idA = process.env.FDC_A10_CACHE_A, idB = process.env.FDC_A10_CACHE_B;
  const jobA = process.env.FDC_A10_JOB_A, jobB = process.env.FDC_A10_JOB_B;
  const browser = await chromium.launch({ executablePath: process.env.FDC_UI_BROWSER ||
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  try {
    async function context(token) {
      const context = await browser.newContext({ ignoreHTTPSErrors: true });
      await context.route('**/*', route => new URL(route.request().url()).origin === origin.origin ? route.continue() : route.abort());
      await context.addCookies([{ name: '__Host-FdcSession', value: token, url: origin.origin, httpOnly: true, secure: true, sameSite: 'Strict' }]);
      return context;
    }
    const a = await context(process.env.FDC_A10_TOKEN_A), b = await context(process.env.FDC_A10_TOKEN_B);
    const pageA = await a.newPage(), pageB = await b.newPage();
    await pageA.clock.install();
    const route = new URL('/identity/lab.html', origin).href;
    for (const [page, id, hidden, ownedJob, hiddenJob] of [[pageA, idA, idB, jobA, jobB], [pageB, idB, idA, jobB, jobA]]) {
      await page.goto(route);
      await page.locator('#lab-list button').filter({ hasText: id }).click();
      await page.locator('#lab-status').getByText('Đã tải chi tiết trong phạm vi.').waitFor();
      assert.equal(await page.locator('#lab-detail h2').textContent(), id);
      assert.equal(await page.locator('#lab-list').getByText(hidden, { exact: false }).count(), 0);
      const response = await page.request.get(new URL('/api/lab/v1/tests/', origin).href);
      assert.match(response.headers()['cache-control'], /no-store/i);
      assert.deepEqual((await response.json()).map(row => row.permitId), [id]);
      const denied = await page.request.get(new URL('/api/lab/v1/tests/' + hidden + '/1', origin).href);
      assert.equal(denied.status(), 404);
      const stations = await page.request.get(new URL('/api/business/v1/stations', origin).href);
      assert.equal(stations.status(), 200);
      assert.deepEqual((await stations.json()).map(row => row.label), [id === idA ? 'A10-A' : 'A10-B']);
      const dashboard = await page.request.get(new URL('/api/business/v1/dashboard', origin).href);
      assert.equal(dashboard.status(), 200);
      assert.equal((await dashboard.json()).stations, 1);
      const exported = await page.request.get(new URL('/api/lab/v1/tests/exports/' + ownedJob + '/download', origin).href);
      assert.equal(exported.status(), 200);
      assert.match(exported.headers()['cache-control'], /no-store/i);
      assert.equal((await exported.json()).PermitId, id);
      const foreignExport = await page.request.get(new URL('/api/lab/v1/tests/exports/' + hiddenJob + '/download', origin).href);
      assert.equal(foreignExport.status(), 404);
      assert.equal((await foreignExport.body()).length, 0);
    }
    const signal = process.env.FDC_A10_REVOKE;
    await fs.writeFile(signal + '.publishing', 'revoke', { flag: 'wx' });
    await fs.rename(signal + '.publishing', signal);
    for (let i = 0;; i++) {
      try { await fs.access(signal + '.done'); break; }
      catch { if (i > 200) throw new Error('SQL revoke did not complete'); }
      await new Promise(resolve => setTimeout(resolve, 30));
    }
    await pageA.clock.runFor(15050);
    await pageA.locator('#lab-status').getByText('Phiên đã hết hạn.').waitFor();
    assert.equal(await pageA.locator('#lab-detail').textContent(), '');
    assert.equal((await pageA.request.get(new URL('/api/lab/v1/tests/', origin).href)).status(), 401);
    const revokedExport = await pageA.request.get(new URL('/api/lab/v1/tests/exports/' + jobA + '/download', origin).href);
    assert.equal(revokedExport.status(), 401);
    assert.equal((await revokedExport.body()).length, 0);
    assert.equal((await pageB.request.get(new URL('/api/lab/v1/tests/', origin).href)).status(), 200);
    await a.clearCookies();
    await a.addCookies([{ name: '__Host-FdcSession', value: process.env.FDC_A10_TOKEN_B, url: origin.origin,
      httpOnly: true, secure: true, sameSite: 'Strict' }]);
    await pageA.reload();
    await pageA.locator('#lab-list button').filter({ hasText: idB }).click();
    await pageA.locator('#lab-status').getByText('Đã tải chi tiết trong phạm vi.').waitFor();
    assert.equal(await pageA.locator('#lab-detail h2').textContent(), idB);
    assert.equal((await pageA.locator('#lab-detail').textContent()).includes(idA), false);
    if (process.env.FDC_UI_EVIDENCE) {
      await pageA.screenshot({ path: path.join(process.env.FDC_UI_EVIDENCE, 'a10-cache-session-switch.png'), fullPage: true });
      await fs.writeFile(path.join(process.env.FDC_UI_EVIDENCE, 'a10-cache-browser.json'), JSON.stringify({
        run: path.basename(process.env.FDC_UI_EVIDENCE), utc: new Date().toISOString(), commit: process.env.FDC_A5_COMMIT,
        reverseProxy: 'HTTPS loopback with shared URL cache', stationIsolation: true, revokedA: 401, unaffectedB: 200,
        sensitiveDomCleared: true, scopedAggregates: true, privateExportArtifacts: true,
        foreignExportBytes: 0, revokedExportBytes: 0, browserSessionSwitch: 'B only', proofMaterial: 'omitted' }, null, 2));
    }
  } finally { await browser.close(); }
})().catch(error => {
  const location = String(error.stack).split('\n').find(line => /a10-cache\.cjs:\d+/.test(line));
  console.error('A.10 cache browser assertion failed.', location ?? '');
  process.exitCode = 1;
});
