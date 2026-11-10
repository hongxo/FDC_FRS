const status = document.getElementById('binding-status');
const list = document.getElementById('binding-list');
const form = document.getElementById('binding-form');
let controller, generation = 0, expired = false;
let monitor, sessionView;
function acceptSession(response) {
  const stamp = response.headers.get('X-Human-Session-View');
  if (!stamp || (sessionView && sessionView !== stamp)) throw Object.assign(new Error(), { status: 401 });
  sessionView = stamp;
}
function clear() {
  generation++;
  controller?.abort();
  list.replaceChildren();
  form.reset();
}
async function call(path = '', body, etag) {
  const currentGeneration = generation;
  controller?.abort();
  const current = controller = new AbortController();
  const headers = { Accept: 'application/json' };
  if (body) {
    const proof = await fetch('/api/identity/v1/auth/csrf', { cache: 'no-store', signal: current.signal, redirect: 'error' });
    if (!proof.ok) throw Object.assign(new Error(), { status: proof.status });
    headers['X-CSRF-Token'] = (await proof.json()).token;
    headers['Content-Type'] = 'application/json';
    headers['Idempotency-Key'] = body.requestId ?? crypto.randomUUID();
    if (etag) headers['If-Match'] = etag;
  }
  const response = await fetch('/api/business/v1/collector-binding-requests/' + path, {
    method: body ? 'POST' : 'GET', cache: 'no-store', credentials: 'same-origin', redirect: 'error',
    headers, body: body ? JSON.stringify(body) : undefined, signal: current.signal });
  if (!response.ok) throw Object.assign(new Error(), { status: response.status });
  acceptSession(response);
  const data = await response.json();
  if (current.signal.aborted || currentGeneration !== generation) throw new DOMException('Stale response', 'AbortError');
  return data;
}
function failed(error) {
  if (error.name === 'AbortError') return;
  if (error.status === 401 || error.status === 503) { clear(); form.hidden = true; expired = error.status === 401; }
  status.textContent = ({ 401: 'Phiên đã hết hạn.', 403: 'Không có quyền thực hiện bước này.',
    409: 'Yêu cầu đã thay đổi. Tải lại danh sách.', 503: 'Chưa thể xác minh chính sách hoặc dữ liệu.' })[error.status] ?? 'Yêu cầu không hợp lệ.';
}
async function refresh() {
  if (expired) return;
  clear();
  try {
    for (const row of await call()) {
      const item = document.createElement('li');
      item.dataset.request = row.id;
      const text = document.createElement('p');
      text.textContent = row.id + ' · ' + row.state + ' · ' + row.targetStatus;
      item.append(text);
      for (const [action, title] of [['review', 'Review'], ['approve', 'Approve'], ['apply', 'Apply']]) {
        const button = document.createElement('button');
        button.type = 'button'; button.textContent = title;
        button.addEventListener('click', async () => {
          button.disabled = true;
          try {
            const eTag = '"' + row.versionStamp + '"';
            await call(row.id, { action, eTag }, eTag);
            await refresh();
          } catch (error) { failed(error); }
          finally { button.disabled = false; }
        });
        item.append(button);
      }
      list.append(item);
    }
    form.hidden = false;
    status.textContent = 'Đã tải yêu cầu trong phạm vi.';
  } catch (error) { failed(error); }
}
form.addEventListener('submit', async event => {
  event.preventDefault();
  const data = new FormData(form);
  try {
    await call('', { collectorId: data.get('collector'), expectedVersion: Number(data.get('version')),
      targetStatus: data.get('target'), requestId: crypto.randomUUID() });
    await refresh();
  } catch (error) { failed(error); }
});
document.getElementById('binding-refresh').addEventListener('click', refresh);
async function checkSession() {
  if (document.hidden || expired || monitor) return;
  const currentGeneration = generation;
  const current = monitor = new AbortController();
  try {
    const response = await fetch('/api/business/v1/collector-binding-requests/', {
      cache: 'no-store', credentials: 'same-origin', redirect: 'error', signal: current.signal });
    if (!response.ok) throw Object.assign(new Error(), { status: response.status });
    acceptSession(response);
    const rows = await response.json();
    if (current.signal.aborted || currentGeneration !== generation) return;
    const visible = new Set(rows.map(row => row.id));
    if ([...list.children].some(row => !visible.has(row.dataset.request))) {
      clear();
      status.textContent = 'Phạm vi truy cập đã thay đổi.';
    }
  } catch (error) { if (!current.signal.aborted) failed(error); }
  finally { monitor = null; }
}
setInterval(checkSession, 15000);
addEventListener('pagehide', () => { monitor?.abort(); clear(); });
document.addEventListener('visibilitychange', () => { if (document.hidden) clear(); else refresh(); });
refresh();
