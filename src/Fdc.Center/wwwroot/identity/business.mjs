const status = document.getElementById('status');
const results = document.getElementById('results');
let request;

function clear() {
  request?.abort();
  results.replaceChildren();
}

for (const button of document.querySelectorAll('[data-view]')) button.addEventListener('click', async () => {
  clear();
  const current = request = new AbortController();
  status.textContent = 'Đang kiểm tra quyền…';
  try {
    const response = await fetch('/api/business/v1/' + button.dataset.view, {
      credentials: 'same-origin', mode: 'same-origin', redirect: 'error', cache: 'no-store',
      headers: { Accept: 'application/json' }, signal: current.signal
    });
    if (current.signal.aborted) return;
    if (!response.ok) {
      status.textContent = ({ 401: 'Phiên đăng nhập đã hết hạn.', 403: 'Không có quyền truy cập.',
        503: 'Chưa thể xác minh chính sách hoặc quyền truy cập.' })[response.status] ?? 'Không tải được dữ liệu.';
      return;
    }
    const data = await response.json();
    if (current.signal.aborted) return;
    if (button.dataset.view === 'dashboard' && Number.isSafeInteger(data.stations)) {
      status.textContent = 'Số trạm trong phạm vi: ' + data.stations;
    } else if (Array.isArray(data)) {
      for (const row of data) {
        const item = document.createElement('li');
        item.textContent = row.label;
        results.append(item);
      }
      status.textContent = data.length ? 'Đã tải dữ liệu trong phạm vi.' : 'Không có dữ liệu trong phạm vi.';
    }
  } catch {
    if (!current.signal.aborted) status.textContent = 'Không tải được dữ liệu.';
  }
});
addEventListener('pagehide', clear);
document.addEventListener('visibilitychange', () => {
  if (document.hidden) { clear(); status.textContent = 'Chọn phân hệ để kiểm tra lại quyền.'; }
});
