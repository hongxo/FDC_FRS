const assert = require('node:assert/strict');
const { pathToFileURL } = require('node:url');
const { chromium } = require('playwright');

async function main() {
  const html = process.env.FDC_LAB_READINESS_HTML;
  assert.ok(html, 'Explicit private offline readiness fixture required');
  const browser = await chromium.launch({ headless: true, executablePath: process.env.FDC_UI_BROWSER || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  try {
    const page = await browser.newPage({ viewport: { width: 390, height: 844 }, acceptDownloads: true });
    const errors = [];
    const external = [];
    page.on('pageerror', error => errors.push(error));
    page.on('request', request => { if (!request.url().startsWith('file:')) external.push(request.url()); });
    await page.route('**/*', route => route.request().url().startsWith('file:') ? route.continue() : route.abort());
    await page.goto(pathToFileURL(html).href);
    assert.equal(await page.locator('h2').count(), 10);
    assert.equal(await page.locator('#device option').count() > 1, true);
    assert.equal(await page.locator('#issue option').count() > 1, true);
    await page.locator('#device').selectOption({ index: 1 });
    assert.match(await page.locator('#details').textContent(), /PENDING_VERIFICATION/);
    assert.equal(await page.locator('[name="Firmware"]').inputValue(), '');
    await page.locator('#issue').selectOption({ index: 1 });
    for (const field of ['reviewer', 'reviewEvidence', 'correction']) await page.locator('#' + field).fill('fixture');
    const event = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Xuất đề xuất xác minh' }).click();
    const download = await event;
    const proposal = JSON.parse(require('node:fs').readFileSync(await download.path(), 'utf8'));
    assert.equal(proposal.Status, 'PENDING_VERIFICATION');
    assert.equal(proposal.Disposition, 'PROPOSE_ACCEPT');
    assert.ok(proposal.SourceRow > 2);
    assert.equal(proposal.SourceSheet, 'IP');
    await page.locator('#permit input[name]').evaluateAll(inputs => {
      for (const input of inputs) {
        input.value = input.type === 'number' ? '1' : input.name.endsWith('Hash') ? 'a'.repeat(64) : 'fixture';
      }
    });
    for (const [name, value] of Object.entries({ Start: '2099-01-01T01:00:00Z', End: '2099-01-01T01:01:00Z',
      TimeZone: 'UTC', Operations: 'ASSOCIATE, LIST, READ', Directories: '/records', FileNames: 'sample.cfg, sample.dat',
      IpAddress: '192.0.2.99', Port: '102' })) await page.locator('[name="' + name + '"]').fill(value);
    const draftEvent = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Xuất phiếu nháp' }).click();
    const draftDownload = await draftEvent;
    const draft = JSON.parse(require('node:fs').readFileSync(await draftDownload.path(), 'utf8'));
    assert.deepEqual(draft.Operations, ['ASSOCIATE', 'LIST', 'READ']);
    assert.equal(draft.Version, 1);
    assert.equal(draft.Evidence.length, 5);
    assert.equal(draft.ApprovedBy, undefined);
    if (process.env.FDC_LAB_DRAFT_FIXTURE) require('node:fs').copyFileSync(await draftDownload.path(), process.env.FDC_LAB_DRAFT_FIXTURE);
    await page.locator('#result').setInputFiles({ name: 'fake.json', mimeType: 'application/json',
      buffer: Buffer.from(JSON.stringify({ Status: 'APPROVED_FOR_SCHEDULED_TEST', Findings: [] })) });
    await page.waitForFunction(() => document.querySelector('#status').textContent.includes('không hợp lệ'));
    assert.equal(await page.locator('button').count(), 2);
    for (const width of [390, 820, 1366]) {
      await page.setViewportSize({ width, height: 844 });
      assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth));
    }
    assert.deepEqual(errors, []);
    assert.deepEqual(external, []);
    console.log('PASS readiness offline UI: 10 sections, traceable proposal, fabricated result rejected, 3 viewports, no network.');
  } finally {
    await browser.close();
  }
}

main().catch(error => { console.error(error.message); process.exitCode = 1; });
