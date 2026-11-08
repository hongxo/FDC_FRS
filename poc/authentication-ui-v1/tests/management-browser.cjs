// Browser component fixtures only: intercepted HTTPS responses do not prove deployed IAM authentication.
const { chromium } = require('playwright');
const fs = require('node:fs/promises');
const path = require('node:path');
const root = path.resolve(__dirname, '..');
const evidence = path.resolve(root, '../../docs/auth-d12-backend-implementation/phase-3/evidence');
const results = [];
const check = (condition, reason) => { if (!condition) throw new Error(reason); };
(async () => {
  await fs.mkdir(evidence, { recursive: true });
  const browser = await chromium.launch({ headless: true, executablePath: process.env.FDC_UI_BROWSER || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  try {
    const page = await browser.newPage({ viewport: { width: 1440, height: 1000 } });
    const errors = []; const scripts = []; let status = 503; let screenData = { items: [], hasMore: false };
    page.on('pageerror', error => errors.push(error.message));
    await page.route('https://fdc.lab.test/**', async route => {
      const url = new URL(route.request().url());
      if (url.pathname.startsWith('/api/')) {
        const data = url.pathname.includes('/change-requests/') ? { approvalStatus: 'Approved', effectiveStatus: 'NotApplied' } : screenData;
        return route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(data) });
      }
      const local = path.resolve(root, '.' + url.pathname);
      if (!local.startsWith(root + path.sep)) return route.abort();
      if (local.endsWith('.mjs')) scripts.push(local);
      const contentType = local.endsWith('.html') ? 'text/html' : local.endsWith('.mjs') ? 'text/javascript' : local.endsWith('.css') ? 'text/css' : 'image/svg+xml';
      return route.fulfill({ status: 200, contentType, body: await fs.readFile(local) });
    });
    await page.goto('https://fdc.lab.test/management.html');
    await page.getByText('Chưa thể tải dữ liệu quản trị.', { exact: false }).waitFor();
    check(await page.locator('#results tr').count() === 0, '503 must leave no fixture rows'); results.push({ name: '503 fail-closed empty state', status: 'PASS' });
    status = 200; screenData = { items: [{ username: '<img src=x onerror=alert(1)>', active: true, id: 'synthetic' }], hasMore: false };
    await page.locator('#refresh').click(); await page.locator('#results td').first().waitFor();
    check(await page.locator('#results img').count() === 0, 'text-only rendering required');
    check((await page.locator('#results').textContent()).includes('<img src=x'), 'render text, not markup'); results.push({ name: 'API data is text, no HTML injection', status: 'PASS' });
    status = 401; await page.locator('#refresh').click(); await page.getByText('Phiên đăng nhập không hợp lệ.', { exact: false }).waitFor();
    check(await page.locator('#results tr').count() === 0, '401 must clear private rows'); results.push({ name: '401 clears stale private rows', status: 'PASS' });
    status = 200; screenData = { items: [{ code: 'eighth-custom', name: 'Custom role', active: false }], nextCode: null };
    await page.locator('[data-screen=roles]').click(); await page.getByText('eighth-custom', { exact: true }).waitFor(); results.push({ name: 'S06 dynamic role beyond seven', status: 'PASS' });
    await page.locator('[data-screen=grants]').click(); await page.getByText('Vui lòng nhập mã định danh hợp lệ.', { exact: true }).waitFor(); results.push({ name: 'S07 requires explicit subject and assignment filters', status: 'PASS' });
    await page.locator('#request-id').fill('00000000-0000-0000-0000-000000000001'); await page.locator('#request-read').click();
    await page.getByText('Đã phê duyệt — chưa áp dụng quyền.', { exact: true }).waitFor(); results.push({ name: 'approved request not shown as applied', status: 'PASS' });
    await page.setViewportSize({ width: 390, height: 844 });
    check(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), 'mobile body horizontal overflow');
    await page.screenshot({ path: path.join(evidence, 'management-mobile.png'), fullPage: true }); results.push({ name: '390px mobile layout', status: 'PASS' });
    check(errors.length === 0, errors.join('\n')); check(!scripts.some(name => /mock-data|app\.js/.test(name)), 'runtime fixture scripts forbidden');
    results.push({ name: 'no page exceptions or mock runtime scripts', status: 'PASS' });
  } finally { await browser.close(); await fs.writeFile(path.join(evidence, 'browser-results.json'), JSON.stringify(results, null, 2)); }
  process.stdout.write(JSON.stringify(results, null, 2) + '\n');
})().catch(error => { console.error(error); process.exitCode = 1; });
