const assert = require('node:assert/strict');
const { chromium } = require('playwright');
const fs = require('node:fs/promises');
const path = require('node:path');

(async () => {
  const origin = new URL(process.env.FDC_A10_ORIGIN);
  assert.equal(origin.protocol, 'https:');
  assert.equal(origin.hostname, '127.0.0.1');
  const actors = JSON.parse(process.env.FDC_A10_ACTORS);
  const draft = JSON.parse(process.env.FDC_A10_DRAFT);
  const evidence = process.env.FDC_UI_EVIDENCE;
  const scenario = process.env.FDC_A10_SESSION_SCENARIO || 'idle';
  const browser = await chromium.launch({ executablePath: process.env.FDC_UI_BROWSER ||
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const contexts = [];
  const pages = new Map();
  const trace = [];
  try {
    for (const [name, token] of Object.entries(actors)) {
      const context = await browser.newContext({ ignoreHTTPSErrors: true });
      contexts.push(context);
      await context.route('**/*', route => new URL(route.request().url()).origin === origin.origin ? route.continue() : route.abort());
      await context.addCookies([{ name: '__Host-FdcSession', value: token, url: origin.origin,
        httpOnly: true, secure: true, sameSite: 'Strict' }]);
      const page = await context.newPage();
      await page.clock.install();
      page.on('response', response => {
        const url = new URL(response.url());
        if (url.pathname.startsWith('/api/lab/')) trace.push({ actor: name, path: url.pathname, status: response.status() });
      });
      pages.set(name, page);
    }
    const op = pages.get('demo.operator');
    await op.goto(new URL('/identity/lab.html', origin).href);
    await op.locator('#lab-status').getByText('Không có kế hoạch trong phạm vi.').waitFor();
    for (const [field, value] of Object.entries({ inventory: draft.InventoryId, station: draft.StationId,
      ied: draft.IedId, plan: draft.Plan, evidence: draft.EvidenceSha256[0],
      start: draft.Start.slice(0, 16), end: draft.End.slice(0, 16) }))
      await op.locator('[name=' + field + ']').fill(value);
    await op.getByRole('button', { name: 'Tạo bản nháp' }).click();
    await op.locator('#lab-status').getByText('Đã tải chi tiết trong phạm vi.').waitFor();
    const id = await op.locator('#lab-detail h2').textContent();
    const action = async (page, title, state) => {
      await page.getByRole('button', { name: title, exact: true }).click();
      await page.locator('#lab-detail').getByText('Trạng thái · ' + state, { exact: true }).waitFor();
      await page.waitForFunction(title => [...document.querySelectorAll('#lab-detail button')]
        .some(button => button.textContent === title && !button.disabled), title);
      await page.locator('#lab-status').getByText('Đã tải chi tiết trong phạm vi.').waitFor();
    };
    const open = async name => {
      const page = pages.get(name);
      await page.goto(new URL('/identity/lab.html', origin).href);
      await page.locator('#lab-list button').filter({ hasText: id }).click();
      await page.locator('#lab-status').getByText('Đã tải chi tiết trong phạm vi.').waitFor();
      return page;
    };
    await action(op, 'Gửi yêu cầu', 'SUBMITTED');
    await op.getByRole('button', { name: 'Review', exact: true }).click();
    await op.locator('#lab-status').getByText('Không có quyền thực hiện bước này.').waitFor();
    const reviewer = await open('demo.reviewer');
    await action(reviewer, 'Review', 'REVIEWED');
    await reviewer.getByRole('button', { name: 'Approve', exact: true }).click();
    await reviewer.locator('#lab-status').getByText('Không có quyền thực hiện bước này.').waitFor();
    const approver = await open('demo.approver');
    await action(approver, 'Approve', 'APPROVED');
    await action(approver, 'Gắn permit mô phỏng', 'APPROVED');
    const executor = await open('demo.system-admin');
    await action(executor, 'Kiểm tra sẵn sàng', 'READY');
    await action(executor, 'Chạy mô phỏng', 'COMPLETED');
    const auditor = await open('demo.auditor');
    const exportResponse = await auditor.request.get(new URL('/api/lab/v1/tests/' + id + '/1/export', origin).href);
    assert.equal(exportResponse.status(), 200);
    assert.equal((await exportResponse.json()).DraftStatus, 'COMPLETED');
    const other = pages.get('approver.other-station');
    await other.goto(new URL('/identity/lab.html', origin).href);
    await other.locator('#lab-status').getByText('Không có kế hoạch trong phạm vi.').waitFor();
    assert.equal(await other.locator('#lab-list li').count(), 0);
    const viewer = await open('demo.viewer');
    const second = await viewer.context().newPage();
    await second.clock.install();
    await second.goto(new URL('/identity/lab.html', origin).href);
    await second.locator('#lab-list button').filter({ hasText: id }).click();
    await second.locator('#lab-detail').getByText('Trạng thái · COMPLETED', { exact: true }).waitFor();
    let resume, held;
    const resumeResponse = new Promise(resolve => { resume = resolve; });
    const responseHeld = new Promise(resolve => { held = resolve; });
    await viewer.route('**/api/lab/v1/tests/' + id + '/1', async route => {
      const response = await route.fetch();
      held();
      await resumeResponse;
      try { await route.fulfill({ response }); } catch { /* Request may have been aborted by expiry. */ }
    });
    await viewer.locator('#lab-list button').filter({ hasText: id }).click();
    await responseHeld;
    if (evidence) await viewer.screenshot({ path: path.join(evidence, 'a10-lab-completed.png'), fullPage: true });
    const signal = process.env.FDC_A10_EXPIRE;
    await fs.writeFile(signal, scenario, { flag: 'wx' });
    for (let i = 0;; i++) {
      try { await fs.access(signal + '.done'); break; }
      catch { if (i > 200) throw new Error('Test clock did not advance'); }
      await new Promise(resolve => setTimeout(resolve, 30));
    }
    for (const page of [viewer, second]) {
      await page.clock.runFor(15050);
      await page.locator('#lab-status').getByText(scenario === 'mfa' ? 'Phạm vi truy cập đã thay đổi.' : 'Phiên đã hết hạn.').waitFor();
      assert.equal(await page.locator('#lab-detail').textContent(), '');
      assert.equal(await page.locator('#lab-list li').count(), 0);
      if (scenario !== 'mfa') assert.equal(await page.locator('#lab-create').isVisible(), false);
      const backend = await page.request.get(new URL('/api/lab/v1/tests/', origin).href);
      assert.equal(backend.status(), scenario === 'mfa' ? 200 : 401);
      if (scenario === 'mfa') assert.deepEqual(await backend.json(), []);
    }
    resume();
    await viewer.clock.runFor(50);
    assert.equal(await viewer.locator('#lab-detail').textContent(), '');
    if (evidence) {
      await viewer.screenshot({ path: path.join(evidence, 'a10-' + scenario + '-cleared.png'), fullPage: true });
      await fs.writeFile(path.join(evidence, 'a10-lab-browser-' + scenario + '.json'), JSON.stringify({ run: path.basename(evidence),
        utc: new Date().toISOString(), commit: process.env.FDC_A5_COMMIT, identity: 'synthetic SQL session fixtures',
        workflow: 'DRAFT→SUBMITTED→REVIEWED→APPROVED→READY→COMPLETED', idleExpiryTabs: 2,
        sensitiveDomCleared: true, delayedResponseIgnored: true, scenario,
        backendAfterExpiry: scenario === 'mfa' ? '200 empty scope' : 401, trace }, null, 2));
    }
  } finally { await browser.close(); }
})().catch(error => {
  const location = String(error.stack).split('\n').find(line => /a10-lab\.cjs:\d+/.test(line));
  console.error('A.10 Lab browser assertion failed.', location ?? '');
  process.exitCode = 1;
});
