const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const fs = require('node:fs/promises');
const path = require('node:path');

module.exports = async function verifyBusiness(page, call, expect, evidence) {
  const ids = JSON.parse(process.env.FDC_A7_IDS);
  const base = '/api/business/v1/';
  const dashboard = await expect('Dashboard only Station A', await call(base + 'dashboard'), 200);
  assert.equal(dashboard.stations, 1);
  for (const [route, visible, hidden] of [
    ['stations', ids.stationA, ids.stationB], ['ieds', ids.iedA, ids.iedB],
    ['records', ids.recordA, ids.recordB], ['collectors', ids.collectorA, ids.collectorB]
  ]) {
    const rows = await expect(route + ' scoped list', await call(base + route), 200);
    assert.deepEqual(rows.map(row => row.id), [visible]);
    const row = await expect(route + ' detail A', await call(base + route + '/' + visible), 200);
    assert.equal(row.stationId, ids.stationA);
    assert.equal(await expect(route + ' detail B', await call(base + route + '/' + hidden), 404), null);
  }
  const businessPage = await page.context().newPage();
  try {
    await businessPage.goto(new URL('/identity/business.html', page.url()).href);
    await businessPage.getByRole('button', { name: 'Tổng quan' }).click();
    await businessPage.getByRole('status').filter({ hasText: 'Số trạm trong phạm vi: 1' }).waitFor();
    await businessPage.getByRole('button', { name: 'Trạm', exact: true }).click();
    await businessPage.locator('#results li').filter({ hasText: 'A7-Station-A' }).waitFor();
    assert.equal(await businessPage.locator('#results li').count(), 1);
    await businessPage.getByRole('button', { name: 'A7-Station-A', exact: true }).click();
    await businessPage.locator('#object-detail h2').filter({ hasText: 'A7-Station-A' }).waitFor();
    if (evidence) await businessPage.screenshot({ path: path.join(evidence, 'a7-stations.png') });
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

  const signal = process.env.FDC_A7_SIGNAL;
  await fs.writeFile(signal, 'revoke', { flag: 'wx' });
  for (let i = 0; ; i++) {
    try { await fs.access(signal + '.done'); break; }
    catch { if (i >= 100) throw new Error('SQL revoke did not complete'); }
    await new Promise(resolve => setTimeout(resolve, 30));
  }
  const revoked = await file(ids.fileA, false);
  await expect('Previously authorized file denied after SQL revoke', revoked, 404);
  assert.equal(revoked.bytes.length, 0);
  const after = await expect('Dashboard after revoke', await call(base + 'dashboard'), 200);
  assert.equal(after.stations, 0);
  return { digest: ids.digestA, bytes: allowed.bytes.length, deniedBytes: [denied.bytes.length, revoked.bytes.length],
    correlation: allowed.correlation };
};
