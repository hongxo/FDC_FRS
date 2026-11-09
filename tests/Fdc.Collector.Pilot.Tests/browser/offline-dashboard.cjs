// Explicit local OFFLINE_FIXTURE viewer. Never intercept/fabricate API success or connect an IED.
const {chromium} = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs/promises');
const path = require('node:path');
const {pathToFileURL} = require('node:url');
(async()=>{
 const root=process.env.FDC_PILOT_DASHBOARD;
 assert.ok(root&&path.isAbsolute(root));
 const browser=await chromium.launch({headless:true,executablePath:process.env.FDC_UI_BROWSER||'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
 const results=[],requests=[],errors=[];
 try{
  const context=await browser.newContext({viewport:{width:1440,height:1000}});
  await context.route('**/*',route=>new URL(route.request().url()).protocol==='file:'?route.continue():route.abort());
  context.on('request',r=>requests.push(new URL(r.url()).protocol));
  const page=await context.newPage();page.on('pageerror',e=>errors.push(e.message));
  await page.goto(pathToFileURL(path.join(root,'index.html')).href);
  await page.locator('#status').filter({hasText:'1 bản ghi'}).waitFor();
  assert.match(await page.locator('#scope').textContent(),/OFFLINE_FIXTURE/);
  for(let i=0;i<8;i++){
   await page.locator('#screens button').nth(i).click();
   assert.equal(await page.locator('#screens button').nth(i).getAttribute('aria-pressed'),'true');
   assert.ok((await page.locator('#title').textContent()).length>0);
   if(i===6)assert.equal(await page.locator('#plot polyline').count(),1);
   if(i===7)assert.match(await page.locator('#results').textContent(),/BLOCKED.*NOT_TESTED|NOT_TESTED/);
   results.push({screen:`S-LAB0${i+1}`,result:'PASS',scope:'Offline fixture viewer only'});
  }
  await page.locator('#screens button').nth(4).click();
  await page.locator('#date').fill('2099-01-01');assert.equal(await page.locator('#results tbody tr').count(),0);
  await page.locator('#date').fill('');await page.locator('#vendor').selectOption('OFFLINE_FIXTURE');assert.equal(await page.locator('#results tbody tr').count(),1);
  for(const [name,width,height] of [['desktop',1440,1000],['tablet',768,1024],['mobile',390,844]]){
   await page.setViewportSize({width,height});await page.locator('h1').click();assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1));
   if(process.env.FDC_PILOT_EVIDENCE)await page.screenshot({path:path.join(process.env.FDC_PILOT_EVIDENCE,`offline-dashboard-${name}.png`),fullPage:true});
  }
  assert.deepEqual(errors,[]);assert.ok(requests.every(p=>p==='file:'));
  const report={EvidenceKind:'OFFLINE_FIXTURE',results,DateFilter:'PASS',VendorFilter:'PASS',Responsive:['desktop','tablet','mobile'],PageErrors:errors,NetworkProtocols:[...new Set(requests)],Limits:'No authenticated Center data, real IED/vendor/SCADA or full pilot acceptance'};
  if(process.env.FDC_PILOT_EVIDENCE)await fs.writeFile(path.join(process.env.FDC_PILOT_EVIDENCE,'offline-dashboard-results.json'),JSON.stringify(report,null,2));
  console.log('PASS:8 offline screens, waveform SVG, filters,3 viewports,no page errors,no network requests. Not real Lab acceptance.');
 }finally{await browser.close();}
})().catch(error=>{console.error(error.message);process.exitCode=1;});
