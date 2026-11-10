import { onLogout } from './session-lifecycle.mjs';
import { appendWaveform } from './waveform.mjs';
const status = document.getElementById('status');
const results = document.getElementById('results');
const controls = document.getElementById('filters');
const detail = document.getElementById('object-detail');
const search = document.getElementById('business-search');
const previous = document.getElementById('business-prev');
const next = document.getElementById('business-next');
const first = document.getElementById('business-first');
const last = document.getElementById('business-last');
const count = document.getElementById('business-count');
const navigation = document.getElementById('business-navigation');
const views = [...navigation.querySelectorAll('[data-view]')];
const pageSize = 20;
let request, sessionCheck, view = 'dashboard', offset = 0, lastOffset = 0, expired = false;
let sessionView;

function clear() {
  request?.abort();
  results.replaceChildren();
  detail.replaceChildren();
  count.textContent = '';
  first.disabled = previous.disabled = next.disabled = last.disabled = true;
}

async function fetchBusiness(path, signal) {
  const response = await fetch('/api/business/v1/' + path, {
    credentials: 'same-origin', mode: 'same-origin', redirect: 'error', cache: 'no-store',
    headers: { Accept: 'application/json' }, signal
  });
  if (!response.ok) throw Object.assign(new Error(), { status: response.status });
  const stamp = response.headers.get('X-Human-Session-View');
  if (!stamp || (sessionView && sessionView !== stamp)) {
    invalidate({ status: 401 });
    throw Object.assign(new Error(), { status: 401 });
  }
  sessionView = stamp;
  return response;
}

async function read(path, signal) {
  return (await fetchBusiness(path, signal)).json();
}

function invalidate(error) {
  clear();
  search.value = '';
  status.textContent = message(error);
  navigation.hidden = true;
  if (error.status !== 401 && error.status !== 403) return;
  expired = true;
  controls.hidden = true;
}

async function checkSession() {
  if (expired || document.hidden || sessionCheck) return;
  const current = sessionCheck = new AbortController();
  try {
    const allowed = await read('navigation', current.signal);
    if (current.signal.aborted) return;
    for (const button of views) button.hidden = !allowed.includes(button.dataset.view);
    navigation.hidden = allowed.length === 0;
    if (!allowed.includes(view)) {
      clear();
      search.value = '';
      controls.hidden = true;
      status.textContent = 'Chọn phân hệ được phép.';
    }
  } catch (error) {
    if (!current.signal.aborted) invalidate(error);
  } finally {
    sessionCheck = null;
  }
}
const message = error => ({ 401: 'Phiên đăng nhập đã hết hạn.', 403: 'Không có quyền truy cập.',
  404: 'Không tìm thấy dữ liệu trong phạm vi.', 503: 'Chưa thể xác minh chính sách hoặc quyền truy cập.' })[error.status]
  ?? 'Không tải được dữ liệu.';

async function collectorHistory(section, id, endpoint, describe, current) {
  let historyOffset = 0;
  const body = document.createElement('div');
  const total = document.createElement('p');
  const back = document.createElement('button');
  const forward = document.createElement('button');
  back.type = forward.type = 'button';
  back.textContent = 'Lịch sử trước';
  forward.textContent = 'Lịch sử sau';
  section.append(total, body, back, forward);
  async function page() {
    back.disabled = forward.disabled = true;
    body.replaceChildren();
    try {
      const data = await read('collectors/' + encodeURIComponent(id) + '/' + endpoint +
        '?paged=true&limit=20&offset=' + historyOffset, current.signal);
      if (current.signal.aborted) return;
      total.textContent = 'Tổng lịch sử trong phạm vi: ' + data.total;
      for (const entry of data.items) {
        const row = document.createElement('p');
        row.textContent = describe(entry);
        body.append(row);
      }
      if (!data.items.length) body.append('Không có dữ liệu trong phạm vi.');
      back.disabled = historyOffset === 0;
      forward.disabled = historyOffset + data.items.length >= data.total;
    } catch (error) {
      if (current.signal.aborted) return;
      if (error.status === 401 || error.status === 503) invalidate(error);
      else body.textContent = message(error);
    }
  }
  back.addEventListener('click', () => { historyOffset = Math.max(0, historyOffset - 20); page(); });
  forward.addEventListener('click', () => { historyOffset += 20; page(); });
  await page();
}

async function openObject(id, objectView = view) {
  if (expired) return;
  request?.abort();
  detail.replaceChildren();
  const current = request = new AbortController();
  try {
    const row = await read(objectView + '/' + encodeURIComponent(id), current.signal);
    if (current.signal.aborted) return;
    const heading = document.createElement('h2');
    heading.textContent = row.label;
    detail.append(heading);
    const related = { stations: ['ieds', 'IED thuộc trạm'], ieds: ['collectors', 'Collector được liên kết'],
      collectors: ['records', 'Bản ghi COMTRADE'] }[objectView];
    if (related) {
      const [endpoint, title] = related;
      const section = document.createElement('section');
      const label = document.createElement('h3');
      label.textContent = title;
      section.append(label);
      try {
        const rows = await read(objectView + '/' + encodeURIComponent(id) + '/' + endpoint, current.signal);
        if (current.signal.aborted) return;
        for (const child of rows) {
          const button = document.createElement('button');
          button.type = 'button';
          button.textContent = child.label;
          button.addEventListener('click', () => openObject(child.id, endpoint));
          section.append(button);
        }
        if (!rows.length) section.append('Không có dữ liệu trong phạm vi.');
      } catch (error) {
        if (current.signal.aborted) return;
        if (error.status === 401) throw error;
        section.append(message(error));
      }
      detail.append(section);
    }
    if (objectView === 'collectors') {
      for (const [endpoint, title, describe] of [
        ['heartbeat', 'Nhịp kết nối', item => item.receivedAtUtc + ' · ' + item.status],
        ['transfers', 'Lịch sử truyền', item => item.createdAtUtc + ' · ' + item.outcome + ' · lần ' + item.attempt + (item.errorCode ? ' · ' + item.errorCode : '')]
      ]) {
        const section = document.createElement('section');
        const label = document.createElement('h3');
        label.textContent = title;
        section.append(label);
        await collectorHistory(section, id, endpoint, describe, current);
        if (current.signal.aborted) return;
        detail.append(section);
      }
      return;
    }
    if (objectView !== 'records') return;
    appendWaveform(detail, id, read, current.signal, invalidate);
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
      if (error.status === 401 || error.status === 503) invalidate(error);
      else detail.textContent = message(error);
    }
  }
}

async function load() {
  if (expired) return;
  clear();
  const current = request = new AbortController();
  status.textContent = 'Đang kiểm tra quyền…';
  controls.hidden = view === 'dashboard';
  try {
    const query = new URLSearchParams({ paged: 'true', offset: String(offset), limit: String(pageSize), search: search.value.trim() });
    const data = await read(view === 'dashboard' ? view : view + '?' + query, current.signal);
    if (current.signal.aborted) return;
    if (view === 'dashboard' && Number.isSafeInteger(data.stations)) {
      status.textContent = 'Số trạm trong phạm vi: ' + data.stations;
      const totals = document.createElement('dl');
      totals.setAttribute('aria-label', 'Thống kê nghiệp vụ');
      for (const [key, label] of [['ieds', 'IED được phép xem'], ['collectors', 'Collector được phép xem'], ['records', 'Bản ghi COMTRADE được phép xem']]) {
        if (!Number.isSafeInteger(data[key])) continue;
        const title = document.createElement('dt');
        title.textContent = label;
        const value = document.createElement('dd');
        value.textContent = String(data[key]);
        totals.append(title, value);
      }
      detail.append(totals);
      for (const [endpoint, title, describe] of [
        ['trends', 'Bản ghi theo ngày UTC', item => item.dayUtc.slice(0, 10) + ' · ' + item.records],
        ['alerts', 'Sự kiện vận hành', item => item.createdAtUtc + ' · ' + item.code]
      ]) {
        const section = document.createElement('section');
        const heading = document.createElement('h2');
        heading.textContent = title;
        section.append(heading);
        try {
          const rows = await read('dashboard/' + endpoint, current.signal);
          if (current.signal.aborted) return;
          for (const item of rows) {
            const line = document.createElement('p');
            line.textContent = describe(item);
            section.append(line);
          }
          if (!rows.length) section.append('Không có dữ liệu trong phạm vi.');
        } catch (error) {
          if (current.signal.aborted) return;
          if (error.status === 401) throw error;
          section.append(message(error));
        }
        detail.append(section);
      }
    } else if (Number.isSafeInteger(data.total) && data.total >= 0 && Array.isArray(data.items)) {
      lastOffset = Math.max(0, Math.ceil(data.total / pageSize) - 1) * pageSize;
      if (offset > lastOffset) {
        offset = lastOffset;
        return load();
      }
      for (const row of data.items) {
        const item = document.createElement('li');
        const button = document.createElement('button');
        button.type = 'button';
        button.textContent = row.label;
        button.addEventListener('click', () => openObject(row.id));
        item.append(button);
        results.append(item);
      }
      first.disabled = previous.disabled = offset === 0;
      next.disabled = last.disabled = offset >= lastOffset;
      count.textContent = 'Tổng trong phạm vi: ' + data.total;
      status.textContent = data.items.length ? 'Đã tải dữ liệu trong phạm vi.' : 'Không có dữ liệu trong phạm vi.';
    } else status.textContent = 'Phản hồi máy chủ không hợp lệ.';
  } catch (error) {
    if (!current.signal.aborted) {
      if (error.status === 401 || error.status === 503) invalidate(error);
      else status.textContent = message(error);
    }
  }
}

for (const button of views) button.addEventListener('click', () => {
  if (button.dataset.view === 'lab' || button.dataset.view === 'bindings') {
    location.assign(button.dataset.view === 'lab' ? '/identity/lab.html' : '/identity/collector-bindings.html');
    return;
  }
  view = button.dataset.view;
  offset = 0;
  search.value = '';
  load();
});
document.getElementById('business-find').addEventListener('click', () => { offset = 0; load(); });
previous.addEventListener('click', () => { offset = Math.max(0, offset - pageSize); load(); });
next.addEventListener('click', () => { offset += pageSize; load(); });
first.addEventListener('click', () => { offset = 0; load(); });
last.addEventListener('click', () => { offset = lastOffset; load(); });
onLogout(() => { sessionCheck?.abort(); invalidate({ status: 401 }); });
addEventListener('pagehide', () => { sessionCheck?.abort(); clear(); });
addEventListener('pageshow', checkSession);
checkSession();
setInterval(checkSession, 15000);
document.addEventListener('visibilitychange', () => {
  if (document.hidden) {
    sessionCheck?.abort();
    clear();
    if (!expired) status.textContent = 'Chọn phân hệ để kiểm tra lại quyền.';
  } else checkSession();
});
