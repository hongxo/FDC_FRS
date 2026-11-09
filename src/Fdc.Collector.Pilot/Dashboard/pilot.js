'use strict';
(() => {
 const data = JSON.parse(document.getElementById('pilot-data').textContent);
 if (data.EvidenceKind !== 'OFFLINE_FIXTURE') throw new Error('OFFLINE_SCOPE_REQUIRED');
 const byId = id => document.getElementById(id);
 const names = ['IED Capability Matrix','Collector Status','File Discovery','Download History','COMTRADE Records','Record Details','Waveform Preview','Pilot Test Results'];
 let screen = 4, selected = data.Records[0];
 const value = x => x == null || x === '' ? 'UNKNOWN' : String(x);
 for (const [index,label] of names.entries()) { const button = document.createElement('button'); button.textContent = `S-LAB0${index+1} ${label}`; button.addEventListener('click',()=>{screen=index;render();}); byId('screens').append(button); }
 for (const [id,field] of [['vendor','Manufacturer'],['station','StationId'],['ied','IedId']]) {
  const all = document.createElement('option'); all.value='';all.textContent='Tất cả';byId(id).append(all);
  for (const item of [...new Set(data.Records.map(r=>value(r.Manifest[field])))].sort()) { const option=document.createElement('option');option.value=item;option.textContent=item;byId(id).append(option); }
  byId(id).addEventListener('change',render);
 }
 byId('date').addEventListener('change',render);
 function filtered() { return data.Records.filter(r=>[['vendor','Manufacturer'],['station','StationId'],['ied','IedId']].every(([id,key])=>!byId(id).value||value(r.Manifest[key])===byId(id).value)&&(!byId('date').value||r.Manifest.CollectedAt.startsWith(byId('date').value))); }
 function table(headers,rows,onSelect) {
  const t=document.createElement('table'),caption=document.createElement('caption');t.className=headers.length>3?'wide':'';caption.textContent=names[screen];t.append(caption);
  const head=document.createElement('thead'),tr=document.createElement('tr');for(const h of headers){const th=document.createElement('th');th.scope='col';th.textContent=h;tr.append(th);}head.append(tr);t.append(head);
  const body=document.createElement('tbody');for(const row of rows){const tr=document.createElement('tr');for(const cell of row.values){const td=document.createElement('td');td.textContent=value(cell);tr.append(td);}if(onSelect){const td=document.createElement('td'),button=document.createElement('button');button.textContent='Chi tiết';button.addEventListener('click',()=>{selected=row.record;screen=5;render();});td.append(button);tr.append(td);}body.append(tr);}t.append(body);byId('results').replaceChildren(t);
 }
 function render() {
  const records=filtered();if(!records.includes(selected))selected=records[0];
  byId('title').textContent=names[screen];byId('status').textContent=`${records.length} bản ghi trong bộ lọc. Nguồn: OFFLINE_FIXTURE.`;
  [...byId('screens').children].forEach((b,i)=>b.setAttribute('aria-pressed',String(i===screen)));byId('wave').hidden=true;
  const rows=values=>values.map(values=>({values}));
  if(screen===0)table(['Vendor claim','Model claim','Firmware','MMS','File Service','COMTRADE'],rows(records.map(r=>[r.Manifest.Manufacturer,r.Manifest.Model,r.Manifest.Firmware,'NOT_TESTED','NOT_TESTED',`OFFLINE_VALIDATED ${r.Preview.Format}`])));
  else if(screen===1)table(['Metric','Actual offline value'],rows([['Downloads',data.DownloadHistory.length],['Upload records',data.UploadStatus.length],['Free bytes at snapshot',data.FreeBytes],['Center mTLS','NOT_TESTED'],['Generated UTC',data.GeneratedAt]]));
  else if(screen===2)table(['Remote path','Size bytes','Modified'],rows(data.Discovery.map(f=>[f.RemotePath,f.Size,f.Modified])));
  else if(screen===3)table(['Job','State','Attempts'],rows(data.DownloadHistory.map(j=>[j.Id,j.State,j.Attempts])));
  else if(screen===4)table(['Record','Vendor','Station','IED','Format','UTC collected','Action'],records.map(r=>({record:r,values:[r.Manifest.Id,r.Manifest.Manufacturer,r.Manifest.StationId,r.Manifest.IedId,r.Preview.Format,r.Manifest.CollectedAt]})),true);
  else if(screen===5)table(['File','Bytes','SHA256','Verification scope'],rows(selected?selected.Manifest.Files.map(f=>[f.Name,f.Size,f.Sha256,selected.ChecksumScope]):[]));
  else if(screen===6){table(['Record','Samples','DAT','Decimated preview','Time reference'],rows(selected?[[selected.Manifest.Id,selected.Preview.Samples,selected.Preview.Format,selected.Preview.PreviewDecimated,selected.Preview.StartTimeRaw]]:[]));if(selected){byId('wave').hidden=false;const s=byId('channel');s.replaceChildren();selected.Preview.Analog.forEach((c,i)=>{const o=document.createElement('option');o.value=String(i);o.textContent=c.Name+' ('+c.Unit+')';s.append(o);});plot();}}
  else table(['Check','Observed scope/result'],rows([['Offline parse/size/hash','Validated for displayed files; no independent IED checksum'],['Hardware ABB/Siemens/Schneider','BLOCKED / NOT_TESTED'],['SCADA coexistence','NOT_TESTED'],['Center authenticated dashboard','BLOCKED_BY_POLICY'],['Lab pilot acceptance','NOT_GRANTED']]));
 }
 function plot(){const index=Number(byId('channel').value||0),points=selected?.Preview.Points||[],svg=byId('plot');svg.replaceChildren();if(!points.length||!selected.Preview.Analog[index])return;let min=Infinity,max=-Infinity;for(const p of points){min=Math.min(min,p.Analog[index]);max=Math.max(max,p.Analog[index]);}const start=points[0].TimeMs,end=points.at(-1).TimeMs;
  const line=document.createElementNS('http://www.w3.org/2000/svg','polyline');line.setAttribute('points',points.map(p=>`${30+840*(p.TimeMs-start)/(end-start||1)},${280-240*(p.Analog[index]-min)/(max-min||1)}`).join(' '));line.setAttribute('fill','none');line.setAttribute('stroke','#007b92');line.setAttribute('stroke-width','2');svg.append(line);
  const label=document.createElementNS('http://www.w3.org/2000/svg','text');label.setAttribute('x','30');label.setAttribute('y','310');label.textContent=`${start}–${end} ms; min ${min}, max ${max}`;svg.append(label);byId('scaling').textContent=selected.Preview.Scaling+'; preview is not a fault classification.';
 }
 byId('channel').addEventListener('change',plot);render();
})();
