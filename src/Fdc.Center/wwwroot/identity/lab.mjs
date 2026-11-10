import { onLogout } from './session-lifecycle.mjs';
const status = document.getElementById('lab-status');
const list = document.getElementById('lab-list');
const detail = document.getElementById('lab-detail');
const form = document.getElementById('lab-create');
let request, monitor, selected, epoch = 0, expired = false;
let sessionView;
function acceptSession(response) {
  const stamp = response.headers.get('X-Human-Session-View');
  if (!stamp || (sessionView && stamp !== sessionView)) throw Object.assign(new Error(), { status: 401 });
  sessionView = stamp;
}

function clear() {
  epoch++;
  request?.abort();
  selected = null;
  detail.replaceChildren();
  list.replaceChildren();
  form.reset();
}

async function call(path, body, version) {
  const currentEpoch = epoch;
  request?.abort();
  const current = request = new AbortController();
  const headers = { Accept: 'application/json' };
  if (body) {
    const proof = await fetch('/api/identity/v1/auth/csrf', { cache: 'no-store', signal: current.signal, redirect: 'error' });
    if (!proof.ok) throw Object.assign(new Error(), { status: proof.status });
    headers['X-CSRF-Token'] = (await proof.json()).token;
    headers['Content-Type'] = 'application/json';
    headers['Idempotency-Key'] = body.requestId ?? crypto.randomUUID();
    if (version) headers['If-Match'] = version;
  }
  const response = await fetch('/api/lab/v1/tests/' + path, { method: body ? 'POST' : 'GET',
    credentials: 'same-origin', cache: 'no-store', redirect: 'error', signal: current.signal,
    headers, body: body ? JSON.stringify(body) : undefined });
  if (!response.ok) throw Object.assign(new Error(), { status: response.status });
  acceptSession(response);
  const result = await response.json();
  if (epoch !== currentEpoch || current.signal.aborted) throw new DOMException('Stale response', 'AbortError');
  return result;
}

function failed(error) {
  if (error.name === 'AbortError') return;
  if (error.status === 401 || error.status === 503) {
    clear();
    form.hidden = true;
    expired = error.status === 401;
  }
  status.textContent = ({ 401: 'Phiên đã hết hạn.', 403: 'Không có quyền thực hiện bước này.',
    409: 'Dữ liệu đã thay đổi. Tải lại chi tiết.', 503: 'Chưa thể xác minh chính sách hoặc dữ liệu.' })[error.status] ?? 'Yêu cầu không hợp lệ.';
}

async function checkSession() {
  if (expired || document.hidden || monitor) return;
  const current = monitor = new AbortController();
  const generation = epoch;
  try {
    const response = await fetch('/api/lab/v1/tests/', { credentials: 'same-origin', cache: 'no-store',
      redirect: 'error', signal: current.signal });
    if (!response.ok) throw Object.assign(new Error(), { status: response.status });
    acceptSession(response);
    const rows = await response.json();
    if (current.signal.aborted || generation !== epoch) return;
    const visible = new Set(rows.map(row => row.permitId + '/' + row.permitVersion));
    const listed = [...list.querySelectorAll('[data-permit]')].map(row => row.dataset.permit);
    if ((selected && !visible.has(selected.permitId + '/' + selected.permitVersion)) ||
        listed.some(id => !visible.has(id))) {
      clear();
      status.textContent = 'Phạm vi truy cập đã thay đổi.';
    }
  } catch (error) {
    if (!current.signal.aborted) failed(error);
  } finally { monitor = null; }
}

function paragraph(title, value) {
  const line = document.createElement('p');
  line.textContent = title + ' · ' + value;
  detail.append(line);
}

async function show(id, revision) {
  if (expired) return;
  try {
    const row = await call(encodeURIComponent(id) + '/' + revision);
    selected = row;
    detail.replaceChildren();
    const heading = document.createElement('h2');
    heading.textContent = row.permitId;
    detail.append(heading);
    paragraph('Trạng thái', row.draftStatus);
    paragraph('Phiên bản', row.permitVersion);
    paragraph('Kế hoạch', row.draftJson);
    paragraph('Permit', row.permitDigest ?? 'Chưa có');
    paragraph('Cửa sổ UTC', row.startsUtc + ' – ' + row.endsUtc);
    paragraph('Readiness mô phỏng', row.draftStatus === 'READY' ? 'Sẵn sàng theo revision hiện tại' : 'Chưa sẵn sàng');
    paragraph('Kết quả', row.resultDigest ?? 'Chưa có');
    const exportLink = document.createElement('a');
    exportLink.href = '/api/lab/v1/tests/' + row.permitId + '/' + row.permitVersion + '/export';
    exportLink.textContent = 'Xuất bằng chứng mô phỏng';
    detail.append(exportLink);
    for (const [action, title] of [['submit', 'Gửi yêu cầu'], ['review', 'Review'], ['approve', 'Approve'],
      ['permit', 'Gắn permit mô phỏng'], ['ready', 'Kiểm tra sẵn sàng'], ['apply', 'Chạy mô phỏng'], ['abort', 'Hủy']]) {
      const button = document.createElement('button');
      button.type = 'button';
      button.textContent = title;
      button.addEventListener('click', async () => {
        if (!selected || selected !== row || expired) return;
        button.disabled = true;
        try {
          const version = '"' + row.versionStamp + '"';
          await call(row.permitId + '/' + row.permitVersion + '/transition',
            { action, eTag: version, requestId: crypto.randomUUID(), permitIed: row.iedId, testType: row.testType }, version);
          await show(row.permitId, row.permitVersion);
        } catch (error) { failed(error); }
        finally { button.disabled = false; }
      });
      detail.append(button);
    }
    const history = await call(row.permitId + '/' + row.permitVersion + '/history');
    if (selected !== row) return;
    for (const entry of history) paragraph('Lịch sử', entry.recordedUtc + ' · ' + entry.action + ' · ' + entry.state);
    status.textContent = 'Đã tải chi tiết trong phạm vi.';
  } catch (error) { failed(error); }
}

async function refresh() {
  if (expired) return;
  clear();
  try {
    const rows = await call('');
    for (const row of rows) {
      const item = document.createElement('li');
      const button = document.createElement('button');
      button.type = 'button';
      button.textContent = row.permitId + ' · ' + row.draftStatus;
      button.dataset.permit = row.permitId + '/' + row.permitVersion;
      button.addEventListener('click', () => show(row.permitId, row.permitVersion));
      item.append(button);
      list.append(item);
    }
    form.hidden = false;
    status.textContent = rows.length ? 'Đã tải danh sách trong phạm vi.' : 'Không có kế hoạch trong phạm vi.';
  } catch (error) { failed(error); }
}

form.addEventListener('submit', async event => {
  event.preventDefault();
  if (expired) return;
  const fields = new FormData(form);
  try {
    const row = await call('', { inventoryId: fields.get('inventory'), stationId: fields.get('station'),
      iedId: fields.get('ied'), plan: fields.get('plan'), evidenceSha256: [fields.get('evidence')],
      syntheticOutcome: fields.get('outcome'),
      start: fields.get('start') + ':00Z', end: fields.get('end') + ':00Z' });
    await show(row.id, row.revision);
  } catch (error) { failed(error); }
});
document.getElementById('lab-refresh').addEventListener('click', refresh);
addEventListener('pagehide', () => { monitor?.abort(); clear(); });
addEventListener('pageshow', checkSession);
onLogout(() => { monitor?.abort(); failed({ status: 401 }); });
setInterval(checkSession, 15000);
document.addEventListener('visibilitychange', () => {
  if (document.hidden) clear();
  else refresh();
});
refresh();
