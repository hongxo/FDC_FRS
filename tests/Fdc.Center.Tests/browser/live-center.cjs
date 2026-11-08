// Live product Center HTTPS + SQL session. No route fulfilment, fixture API response or mock principal.
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs/promises');
const path = require('node:path');
(async () => {
 const origin = new URL(process.env.FDC_CENTER_TEST_URL);
 assert.equal(origin.protocol, 'https:'); assert.equal(origin.hostname, '127.0.0.1');
 const browser = await chromium.launch({headless:true, executablePath:process.env.FDC_UI_BROWSER || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
 const results=[]; const traffic=[];
 try {
  const context=await browser.newContext({ignoreHTTPSErrors:true, viewport:{width:1440,height:1000}});
  await context.route('**/*', route => new URL(route.request().url()).origin === origin.origin ? route.continue() : route.abort());
  await context.addCookies([{name:'__Host-FdcSession',value:process.env.FDC_CENTER_TEST_COOKIE,url:origin.origin,httpOnly:true,secure:true,sameSite:'Strict'}]);
  context.on("request",r=>traffic.push({request:new URL(r.url()).pathname})); context.on("response",r=>traffic.push({response:new URL(r.url()).pathname,status:r.status()})); context.on("requestfailed",r=>traffic.push({failed:new URL(r.url()).pathname,error:r.failure()?.errorText}));
  const page=await context.newPage(); const errors=[]; page.on('pageerror',e=>errors.push(e.message));
  for(const screen of ['login','mfa','recovery','profile']) {
   await page.goto(`${origin.origin}/identity/#${screen}`);
   await page.locator('#policy-status').filter({hasText:/chưa được mở/}).waitFor();
   assert.equal(await page.locator('input[type=password]').count(),0);
   results.push({screen,result:'PASS',scope:'policy-unavailable rendering; not completed authentication'});
  }
  for(const screen of ['users','roles','grants','audit']) {
   await page.goto(`${origin.origin}/identity/`);
   await page.goto(`${origin.origin}/identity/management.html#${screen}`, {waitUntil:"load"});
   if(screen!=="grants") await page.locator("#status").filter({hasText:"Chưa thể tải dữ liệu quản trị"}).waitFor();
   if(screen==='grants') {
    await page.locator('#subject-user').fill('11111111-1111-1111-1111-111111111111');
    await page.locator('#subject-assignment').fill('22222222-2222-2222-2222-222222222222');
   }
   const responsePromise=page.waitForResponse(r=>r.url().includes('/api/identity/v1/admin/') && r.request().method()==='GET');
   await page.locator('#refresh').click(); const response=await responsePromise;
   assert.equal(response.status(),503);
   await page.locator('#status').filter({hasText:'Chưa thể tải dữ liệu quản trị'}).waitFor();
   assert.equal(await page.locator('#results tbody tr').count(),0);
   results.push({screen,result:'PASS',scope:'real SQL session + real API policy denial; no synthetic rows'});
  }
  await context.clearCookies();
  const noCookie=page.waitForResponse(r=>r.url().includes('/api/identity/v1/admin/security-audit'));
  await page.locator('#refresh').click(); assert.equal((await noCookie).status(),401);
  await page.locator('#status').filter({hasText:'Phiên đăng nhập không hợp lệ'}).waitFor();
  for(const [name,width,height] of [['desktop',1440,1000],['tablet',768,1024],['mobile',390,844]]) {
   await page.setViewportSize({width,height});
   assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1));
   if(process.env.FDC_UI_EVIDENCE) await page.screenshot({path:path.join(process.env.FDC_UI_EVIDENCE,`center-live-${name}.png`),fullPage:true});
  }
  assert.deepEqual(errors,[]);
  if(process.env.FDC_UI_EVIDENCE) await fs.writeFile(path.join(process.env.FDC_UI_EVIDENCE,'center-live-browser.json'),JSON.stringify({origin:'loopback HTTPS (ephemeral port)',results,anonymous:401,responsive:['desktop','tablet','mobile'],pageErrors:errors,limitations:'Negative/policy-blocked integration only. Full login/MFA/RBAC business E2E BLOCKED.'},null,2));
  console.log('Live Center browser: 8 policy/denial screens, anonymous rejection and 3 viewports PASS. Full authentication E2E remains blocked.');
 } finally {if(process.env.FDC_UI_EVIDENCE) await fs.writeFile(path.join(process.env.FDC_UI_EVIDENCE,"center-browser-traffic.json"), JSON.stringify(traffic,null,2)); await browser.close();}
})().catch(error=>{console.error(String(error.message).replaceAll(process.env.FDC_CENTER_TEST_COOKIE || "__absent__", "[redacted]"));console.error('Live Center browser assertion failed; inspect local test and evidence.');process.exitCode=1;});
