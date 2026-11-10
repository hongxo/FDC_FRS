const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const fs = require('node:fs/promises');
const path = require('node:path');

module.exports = async function verifyBusiness(page, call, expect, evidence) {
  const ids = JSON.parse(process.env.FDC_A7_IDS);
  const base = '/api/business/v1/';
  const dashboard = await expect('Dashboard only Station A', await call(base + 'dashboard'), 200);
  assert.equal(dashboard.stations, 1);
  const trends = await expect('Scoped seven-day trends', await call(base + 'dashboard/trends'), 200);
  assert.equal(trends.length, 7);
  assert.equal(trends.reduce((total, row) => total + row.records, 0), 1);
  const alerts = await expect('Scoped alerts', await call(base + 'dashboard/alerts'), 200);
  assert.deepEqual(alerts.map(row => row.code), ['FIXTURE-ALERT-A']);
  const nav = await expect('Scoped business navigation', await call(base + 'navigation'), 200);
  assert.deepEqual(nav, ['dashboard', 'stations', 'ieds', 'records', 'collectors']);
  for (const [parent, idA, idB, child, expected] of [
    ['stations', ids.stationA, ids.stationB, 'ieds', ids.iedA],
    ['ieds', ids.iedA, ids.iedB, 'collectors', ids.collectorA],
    ['collectors', ids.collectorA, ids.collectorB, 'records', ids.recordA]
  ]) {
    const rows = await expect(parent + ' relationship A', await call(base + parent + '/' + idA + '/' + child), 200);
    assert.ok(rows.some(row => row.id === expected));
    assert.ok(rows.every(row => row.stationId === ids.stationA));
    assert.deepEqual(await expect(parent + ' relationship B hidden', await call(base + parent + '/' + idB + '/' + child), 200), []);
  }
  for (const [route, visible, hidden] of [
    ['stations', ids.stationA, ids.stationB], ['ieds', ids.iedA, ids.iedB],
    ['records', ids.recordA, ids.recordB], ['collectors', ids.collectorA, ids.collectorB]
  ]) {
    const rows = await expect(route + ' scoped list', await call(base + route + (route === 'ieds' ? '?search=A7-IED' : '')), 200);
    assert.deepEqual(rows.map(row => row.id), [visible]);
    const row = await expect(route + ' detail A', await call(base + route + '/' + visible), 200);
    assert.equal(row.stationId, ids.stationA);
    assert.equal(await expect(route + ' detail B', await call(base + route + '/' + hidden), 404), null);
  }
  const pageOne = await expect('Station paging after SQL scope', await call(base + 'stations?offset=0&limit=1'), 200);
  const pageTwo = await expect('No out-of-scope row on next page', await call(base + 'stations?offset=1&limit=1'), 200);
  assert.deepEqual(pageOne.map(row => row.id), [ids.stationA]);
  assert.deepEqual(pageTwo, []);
  for (const [route, value] of [['heartbeat', 'Healthy'], ['transfers', 'Accepted']]) {
    const allowed = await expect('Collector A ' + route, await call(base + 'collectors/' + ids.collectorA + '/' + route), 200);
    assert.equal(allowed.length, 22);
    assert.equal(allowed[0].status ?? allowed[0].outcome, value);
    if (route === 'transfers') {
      assert.ok(allowed.every(row => row.recordId === ids.recordA && (row.errorCode === null || row.errorCode === 'TIMEOUT')));
      assert.ok(allowed.some(row => row.errorCode === 'TIMEOUT'));
    }
    assert.equal(await expect('Collector B ' + route + ' denied', await call(base + 'collectors/' + ids.collectorB + '/' + route), 404), null);
  }
  const businessPage = await page.context().newPage();
  try {
    await businessPage.goto(new URL('/identity/business.html', page.url()).href);
    await businessPage.getByRole('button', { name: 'Tổng quan' }).click();
    await businessPage.getByRole('status').filter({ hasText: 'Số trạm trong phạm vi: 1' }).waitFor();
    await businessPage.locator('#object-detail').getByText(/FIXTURE-ALERT-A/).waitFor();
    assert.equal(await businessPage.locator('#object-detail').getByText(/FIXTURE-ALERT-B/).count(), 0);
    await businessPage.getByRole('button', { name: 'Trạm', exact: true }).click();
    await businessPage.locator('#results li').filter({ hasText: 'A7-Station-A' }).waitFor();
    assert.equal(await businessPage.locator('#results li').count(), 1);
    await businessPage.getByRole('button', { name: 'A7-Station-A', exact: true }).click();
    await businessPage.locator('#object-detail h2').filter({ hasText: 'A7-Station-A' }).waitFor();
    if (evidence) await businessPage.screenshot({ path: path.join(evidence, 'a7-stations.png') });
    await businessPage.locator('#object-detail').getByRole('button', { name: 'A7-IED-A', exact: true }).click();
    await businessPage.locator('#object-detail').getByRole('button', { name: 'A7-Collector-A', exact: true }).click();
    await businessPage.locator('#object-detail').getByRole('button', { name: /^[a-f0-9]{64}$/ }).click();
    await businessPage.locator('#object-detail').getByRole('link', { name: /fixture\.dat/ }).waitFor();
    await businessPage.getByRole('button', { name: 'Xem dạng sóng COMTRADE', exact: true }).click();
    await businessPage.locator('[aria-label="Dạng sóng COMTRADE"] [role="status"]').getByText('2 mẫu · ASCII · Hiển thị toàn bộ mẫu.', { exact: true }).waitFor();
    assert.equal(await businessPage.locator('[aria-label="Dạng sóng COMTRADE"] polyline').count(), 1);
    assert.match(await businessPage.locator('[data-waveform-summary]').textContent(), /Min 0 · Max 1/);
    if (evidence) await businessPage.screenshot({ path: path.join(evidence, 'business-waveform.png') });
    await expect('Station B waveform hidden', await call(base + 'records/' + ids.recordB + '/waveform'), 404);
    await businessPage.locator('#business-search').fill('A7-Station-B');
    await businessPage.locator('#business-find').click();
    await businessPage.getByRole('status').filter({ hasText: 'Không có dữ liệu trong phạm vi.' }).waitFor();
    assert.equal(await businessPage.locator('#results li').count(), 0);
    await businessPage.getByRole('button', { name: 'IED', exact: true }).click();
    await businessPage.locator('#business-search').fill('A9-PAGE');
    await businessPage.locator('#business-find').click();
    await businessPage.locator('#business-count').getByText('Tổng trong phạm vi: 21').waitFor();
    const labels = () => businessPage.locator('#results li').allTextContents();
    const firstPage = await labels();
    assert.equal(firstPage.length, 20);
    await businessPage.locator('#business-next').click();
    await businessPage.locator('#business-count').getByText('Tổng trong phạm vi: 21').waitFor();
    const lastPage = await labels();
    assert.equal(lastPage.length, 1);
    assert.deepEqual([...firstPage, ...lastPage].sort(), Array.from({length: 21}, (_, i) => 'A9-PAGE-' + String(i).padStart(2, '0')));
    for (const [selector, expected] of [['#business-prev', firstPage], ['#business-last', lastPage], ['#business-first', firstPage]]) {
      await businessPage.locator(selector).click();
      await businessPage.locator('#business-count').getByText('Tổng trong phạm vi: 21').waitFor();
      assert.deepEqual(await labels(), expected);
    }
    await businessPage.getByRole('button', { name: 'Collector', exact: true }).click();
    await businessPage.getByRole('button', { name: 'A7-Collector-A', exact: true }).click();
    await businessPage.locator('#object-detail').getByText(/Healthy/).first().waitFor();
    await businessPage.locator('#object-detail').getByText(/Accepted/).waitFor();
    for (const title of ['Nhịp kết nối', 'Lịch sử truyền']) {
      const section = businessPage.locator('#object-detail section').filter({ has: businessPage.getByRole('heading', { name: title, exact: true }) });
      await section.getByText('Tổng lịch sử trong phạm vi: 22', { exact: true }).waitFor();
      const before = await section.locator('div > p').allTextContents();
      assert.equal(before.length, 20);
      await section.getByRole('button', { name: 'Lịch sử sau', exact: true }).click();
      await section.getByRole('button', { name: 'Lịch sử trước', exact: true }).waitFor({ state: 'visible' });
      await section.page().waitForFunction(title => {
        const section = [...document.querySelectorAll('#object-detail section')].find(section => section.querySelector('h3')?.textContent === title);
        return section?.querySelectorAll('div > p').length === 2;
      }, title);
      const after = await section.locator('div > p').allTextContents();
      assert.equal(new Set([...before, ...after]).size, 22);
      await section.getByRole('button', { name: 'Lịch sử trước', exact: true }).click();
      await section.page().waitForFunction(title => {
        const section = [...document.querySelectorAll('#object-detail section')].find(section => section.querySelector('h3')?.textContent === title);
        return section?.querySelectorAll('div > p').length === 20;
      }, title);
      assert.deepEqual(await section.locator('div > p').allTextContents(), before);
    }
    assert.equal((await businessPage.locator('#object-detail').textContent()).includes('opaque.unreviewed.detail'), false);
    await businessPage.setViewportSize({ width: 390, height: 844 });
    assert.ok(await businessPage.evaluate(() => document.documentElement.scrollWidth <= innerWidth));
    if (evidence) await businessPage.screenshot({ path: path.join(evidence, 'a8-collector-mobile.png') });
    await businessPage.getByRole('button', { name: 'COMTRADE', exact: true }).click();
    await businessPage.locator('#results li button').first().click();
    const link = businessPage.getByRole('link', { name: /fixture\.dat/ });
    await link.waitFor();
    assert.equal(await link.getAttribute('href'), base + 'record-files/' + ids.fileA + '/download');
    if (evidence) await businessPage.screenshot({ path: path.join(evidence, 'a7-download-link.png') });
  } finally { await businessPage.close(); }

  const file = (id, range) => page.evaluate(async ({ id, range }) => {
    const response = await fetch('/api/business/v1/record-files/' + id + '/download', {
      cache: 'no-store', headers: range ? { Range: 'bytes=0-2' } : {}
    });
    return { status: response.status, body: null, bytes: Array.from(new Uint8Array(await response.arrayBuffer())),
      cache: response.headers.get('cache-control'), range: response.headers.get('content-range'),
      correlation: response.headers.get('x-correlation-id') };
  }, { id, range });
  const denied = await file(ids.fileB, false);
  await expect('Station B file denied', denied, 404);
  assert.equal(denied.bytes.length, 0);
  const allowed = await file(ids.fileA, true);
  await expect('Station A file authorized; Range ignored', allowed, 200);
  assert.equal(allowed.range, null);
  assert.match(allowed.cache, /no-store/i);
  assert.equal(crypto.createHash('sha256').update(Buffer.from(allowed.bytes)).digest('hex'), ids.digestA);

  const cfg = await file(ids.cfgA, false);
  await expect('Station A CFG authorized', cfg, 200);
  assert.equal(crypto.createHash('sha256').update(Buffer.from(cfg.bytes)).digest('hex'), ids.cfgDigestA);
  const deniedCfg = await file(ids.cfgB, false);
  await expect('Station B CFG denied', deniedCfg, 404);
  assert.equal(deniedCfg.bytes.length, 0);
  const signal = process.env.FDC_A7_SIGNAL;
  await fs.writeFile(signal, 'revoke', { flag: 'wx' });
  for (let i = 0; ; i++) {
    try { await fs.access(signal + '.done'); break; }
    catch { if (i >= 100) throw new Error('SQL revoke did not complete'); }
    await new Promise(resolve => setTimeout(resolve, 30));
  }
  const revoked = await file(ids.fileA, false);
  await expect('Waveform unavailable after SQL grant revoke', await call(base + 'records/' + ids.recordA + '/waveform'), 404);
  await expect('Previously authorized file denied after SQL revoke', revoked, 404);
  assert.equal(revoked.bytes.length, 0);
  const after = await expect('Dashboard after revoke', await call(base + 'dashboard'), 200);
  assert.equal(after.stations, 0);
  assert.deepEqual(await expect('Navigation hidden after grant revoke', await call(base + 'navigation'), 200), []);
  return { digest: ids.digestA, bytes: allowed.bytes.length, deniedBytes: [denied.bytes.length, revoked.bytes.length],
    correlation: allowed.correlation, cfgCorrelation: cfg.correlation, cfgDigest: ids.cfgDigestA, cfgBytes: cfg.bytes.length };
};
