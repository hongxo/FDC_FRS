const assert = require('node:assert/strict');
const { chromium } = require('playwright');
const fs = require('node:fs/promises');
const path = require('node:path');
(async () => {
  const origin = new URL(process.env.FDC_A10_ORIGIN);
  assert.equal(origin.protocol, 'https:'); assert.equal(origin.hostname, '127.0.0.1');
  const actors = JSON.parse(process.env.FDC_A10_ACTORS);
  const browser = await chromium.launch({ executablePath: process.env.FDC_UI_BROWSER ||
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  try {
    async function open(name) {
      const context = await browser.newContext({ ignoreHTTPSErrors: true });
      await context.route('**/*', route => new URL(route.request().url()).origin === origin.origin ? route.continue() : route.abort());
      await context.addCookies([{ name: '__Host-FdcSession', value: actors[name], url: origin.origin,
        httpOnly: true, secure: true, sameSite: 'Strict' }]);
      const page = await context.newPage();
      await page.goto(new URL('/identity/collector-bindings.html', origin).href);
      await page.locator('#binding-status').getByText('Đã tải yêu cầu trong phạm vi.').waitFor();
      return page;
    }
    const op = await open('demo.operator');
    await op.locator('[name=collector]').fill(process.env.FDC_A10_COLLECTOR);
    await op.locator('[name=version]').fill('1');
    await op.locator('[name=target]').selectOption('Revoked');
    await op.getByRole('button', { name: 'Tạo yêu cầu binding', exact: true }).click();
    await op.locator('#binding-list li').filter({ hasText: 'Pending' }).waitFor();
    const text = await op.locator('#binding-list li p').textContent();
    const request = text.split(' · ')[0];
    assert.match(request, /^[a-f0-9-]{36}$/i);
    await op.getByRole('button', { name: 'Review', exact: true }).click();
    await op.locator('#binding-status').getByText('Không có quyền thực hiện bước này.').waitFor();
    const reviewer = await open('demo.reviewer');
    await reviewer.getByRole('button', { name: 'Review', exact: true }).click();
    await reviewer.locator('#binding-list li').filter({ hasText: 'Reviewed' }).waitFor();
    await reviewer.getByRole('button', { name: 'Approve', exact: true }).click();
    await reviewer.locator('#binding-status').getByText('Không có quyền thực hiện bước này.').waitFor();
    const approver = await open('demo.approver');
    await approver.getByRole('button', { name: 'Approve', exact: true }).click();
    await approver.locator('#binding-list li').filter({ hasText: 'Approved' }).waitFor();
    await approver.getByRole('button', { name: 'Apply', exact: true }).click();
    await approver.locator('#binding-status').getByText('Không có quyền thực hiện bước này.').waitFor();
    const executor = await open('demo.system-admin');
    await executor.getByRole('button', { name: 'Apply', exact: true }).click();
    await executor.locator('#binding-list li').filter({ hasText: 'Applied' }).waitFor();
    const other = await open('approver.other-station');
    assert.equal(await other.locator('#binding-list li').count(), 0);
    if (process.env.FDC_UI_EVIDENCE) {
      await executor.screenshot({ path: path.join(process.env.FDC_UI_EVIDENCE, 'a10-binding-applied.png'), fullPage: true });
      await fs.writeFile(path.join(process.env.FDC_UI_EVIDENCE, 'a10-binding-browser.json'), JSON.stringify({
        request, utc: new Date().toISOString(), commit: process.env.FDC_A5_COMMIT,
        workflow: 'Pending→Reviewed→Approved→Applied', independentPeople: 4,
        selfReview: 403, reviewerApproval: 403, approverApply: 403, otherStationList: [] }, null, 2));
    }
  } finally { await browser.close(); }
})().catch(error => {
  const location = String(error.stack).split('\n').find(line => /a10-binding\.cjs:\d+/.test(line));
  console.error('A.10 binding browser assertion failed.', location ?? '');
  process.exitCode = 1;
});
