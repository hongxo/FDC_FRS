const status = document.getElementById('status');
const results = document.getElementById('results');
const controls = document.getElementById('filters');
const detail = document.getElementById('object-detail');
const search = document.getElementById('business-search');
const previous = document.getElementById('business-prev');
const next = document.getElementById('business-next');
const pageSize = 20;
let request, view = 'dashboard', offset = 0;

function clear() {
  request?.abort();
  results.replaceChildren();
  detail.replaceChildren();
  previous.disabled = next.disabled = true;
}

async function read(path, signal) {
  const response = await fetch('/api/business/v1/' + path, {
    credentials: 'same-origin', mode: 'same-origin', redirect: 'error', cache: 'no-store',
    headers: { Accept: 'application/json' }, signal
  });
  if (!response.ok) throw Object.assign(new Error(), { status: response.status });
  return response.json();
}
const message = error => ({ 401: 'Phiên đăng nhập đã hết hạn.', 403: 'Không có quyền truy cập.',
  404: 'Không tìm thấy dữ liệu trong phạm vi.', 503: 'Chưa thể xác minh chính sách hoặc quyền truy cập.' })[error.status]
  ?? 'Không tải được dữ liệu.';

async function openObject(id) {
  request?.abort();
  detail.replaceChildren();
  const current = request = new AbortController();
  try {
    const row = await read(view + '/' + encodeURIComponent(id), current.signal);
    if (current.signal.aborted) return;
    const heading = document.createElement('h2');
    heading.textContent = row.label;
    detail.append(heading);
    if (view === 'collectors') {
      for (const [endpoint, title, describe] of [
        ['heartbeat', 'Nhịp kết nối', item => item.receivedAtUtc + ' · ' + item.status],
        ['transfers', 'Lịch sử truyền', item => item.createdAtUtc + ' · ' + item.outcome]
      ]) {
        const section = document.createElement('section');
        const label = document.createElement('h3');
        label.textContent = title;
        section.append(label);
        try {
          const history = await read('collectors/' + encodeURIComponent(id) + '/' + endpoint, current.signal);
          if (current.signal.aborted) return;
          for (const entry of history) {
            const row = document.createElement('p');
            row.textContent = describe(entry);
            section.append(row);
          }
          if (!history.length) section.append('Không có dữ liệu trong phạm vi.');
        } catch (error) {
          if (current.signal.aborted) return;
          if (error.status === 401) throw error;
          section.append(message(error));
        }
        detail.append(section);
      }
      return;
    }
    if (view !== 'records') return;
    const files = await read('records/' + encodeURIComponent(id) + '/downloadable-files', current.signal);
    if (current.signal.aborted) return;
    if (!files.length) detail.append('Không có file được phép tải.');
    for (const file of files) {
      const link = document.createElement('a');
      link.href = '/api/business/v1/record-files/' + encodeURIComponent(file.id) + '/download';
      link.textContent = file.name + ' (' + file.sizeBytes + ' byte)';
      const row = document.createElement('p');
      row.append(link);
      detail.append(row);
    }
  } catch (error) {
    if (!current.signal.aborted) {
      if (error.status === 401) results.replaceChildren();
      detail.textContent = message(error);
    }
  }
}

async function load() {
  clear();
  const current = request = new AbortController();
  status.textContent = 'Đang kiểm tra quyền…';
  controls.hidden = view === 'dashboard';
  try {
    const query = new URLSearchParams({ offset: String(offset), limit: String(pageSize), search: search.value.trim() });
    const data = await read(view === 'dashboard' ? view : view + '?' + query, current.signal);
    if (current.signal.aborted) return;
    if (view === 'dashboard' && Number.isSafeInteger(data.stations)) {
      status.textContent = 'Số trạm trong phạm vi: ' + data.stations;
    } else if (Array.isArray(data)) {
      for (const row of data) {
        const item = document.createElement('li');
        const button = document.createElement('button');
        button.type = 'button';
        button.textContent = row.label;
        button.addEventListener('click', () => openObject(row.id));
        item.append(button);
        results.append(item);
      }
      previous.disabled = offset === 0;
      next.disabled = data.length < pageSize || offset >= 10000;
      status.textContent = data.length ? 'Đã tải dữ liệu trong phạm vi.' : 'Không có dữ liệu trong phạm vi.';
    } else status.textContent = 'Phản hồi máy chủ không hợp lệ.';
  } catch (error) {
    if (!current.signal.aborted) status.textContent = message(error);
  }
}

for (const button of document.querySelectorAll('[data-view]')) button.addEventListener('click', () => {
  view = button.dataset.view;
  offset = 0;
  search.value = '';
  load();
});
document.getElementById('business-find').addEventListener('click', () => { offset = 0; load(); });
previous.addEventListener('click', () => { offset = Math.max(0, offset - pageSize); load(); });
next.addEventListener('click', () => { offset += pageSize; load(); });
addEventListener('pagehide', clear);
document.addEventListener('visibilitychange', () => {
  if (document.hidden) { clear(); status.textContent = 'Chọn phân hệ để kiểm tra lại quyền.'; }
});
