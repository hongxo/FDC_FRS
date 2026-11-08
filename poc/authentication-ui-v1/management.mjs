const base = '/api/identity/v1/admin';
const idPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const ids = value => { if (typeof value !== 'string' || !idPattern.test(value)) throw new Error('INVALID_ID'); return value; };
export function managementPath(screen, { offset = 0, userId, userRoleId, afterCode } = {}) {
  if (!Number.isInteger(offset) || offset < 0 || offset > 10000) throw new Error('INVALID_PAGE');
  const query = new URLSearchParams({ offset: String(offset), limit: '50' });
  switch (screen) {
    case 'users': return `${base}/users?${query}`;
    case 'roles': {
      const roleQuery = new URLSearchParams({ limit: '50' });
      if (afterCode !== undefined) {
        if (typeof afterCode !== 'string' || afterCode.length > 80 || /[\x00-\x1f]/.test(afterCode)) throw new Error('INVALID_PAGE');
        roleQuery.set('afterCode', afterCode);
      }
      return `${base}/roles?${roleQuery}`;
    }
    case 'grants': query.set('userId', ids(userId)); query.set('userRoleId', ids(userRoleId)); return `${base}/station-grants?${query}`;
    case 'audit': return `${base}/security-audit?${query}`;
    default: throw new Error('UNSUPPORTED_SCREEN');
  }
}
export function changeStatus(value, status = 200) {
  if (status === 202) return 'Đã tiếp nhận — chưa áp dụng quyền.';
  if (value?.effectiveStatus === 'Applied') return 'Đã áp dụng.';
  if (value?.approvalStatus === 'Approved') return 'Đã phê duyệt — chưa áp dụng quyền.';
  if (value?.approvalStatus === 'Rejected') return 'Đã từ chối.';
  if (value?.approvalStatus === 'Pending') return 'Đang chờ xử lý — chưa áp dụng quyền.';
  return 'Chưa xác nhận trạng thái hiệu lực.';
}
export class ManagementApiError extends Error { constructor(status, code) { super(code); this.status = status; } }
export function createManagementClient(fetcher = globalThis.fetch, protocol = globalThis.location?.protocol) {
  async function read(path, signal) {
    if (protocol !== 'https:') throw new ManagementApiError(0, 'HTTPS_REQUIRED');
    const response = await fetcher(path, { method: 'GET', credentials: 'same-origin', mode: 'same-origin', cache: 'no-store', redirect: 'error', headers: { Accept: 'application/json' }, signal });
    if (!response.ok || response.status !== 200) throw new ManagementApiError(response.status, response.status === 401 ? 'SESSION_REQUIRED' : response.status === 403 ? 'ACCESS_DENIED' : 'AUTHORITY_UNAVAILABLE');
    if (!response.headers.get('content-type')?.toLowerCase().includes('application/json')) throw new ManagementApiError(0, 'INVALID_RESPONSE');
    return response.json();
  }
  return Object.freeze({
    async list(screen, options, signal) {
      const data = await read(managementPath(screen, options), signal);
      if (!data || !Array.isArray(data.items) || data.items.length > 100) throw new ManagementApiError(0, 'INVALID_RESPONSE');
      if (screen !== 'roles' && typeof data.hasMore !== 'boolean') throw new ManagementApiError(0, 'INVALID_RESPONSE');
      if (screen === 'roles' && data.nextCode != null && (typeof data.nextCode !== 'string' || data.nextCode.length > 80)) throw new ManagementApiError(0, 'INVALID_RESPONSE');
      return { items: data.items, hasMore: screen === 'roles' ? data.nextCode != null : data.hasMore, nextCode: screen === 'roles' ? data.nextCode : undefined };
    },
    async request(id, signal) { return read(`${base}/change-requests/${ids(id)}`, signal); }
  });
}

if (typeof document !== 'undefined') {
  const byId = id => document.getElementById(id);
  const client = createManagementClient();
  const fields = { users: [['Username', 'username'], ['Trạng thái', 'active'], ['ID', 'id']], roles: [['Mã vai trò', 'code'], ['Tên vai trò', 'name'], ['Trạng thái', 'active']], grants: [['UserRole ID', 'userRoleId'], ['Phạm vi', 'scopeKind'], ['Bắt đầu', 'validFrom'], ['Kết thúc', 'validUntil']], audit: [['Thao tác', 'action'], ['Kết quả', 'result'], ['Thời gian', 'timestamp']] };
  const titles = { users: 'Người dùng', roles: 'Vai trò và quyền', grants: 'Quyền truy cập trạm', audit: 'Nhật ký bảo mật' };
  let screen = 'users', offset = 0, generation = 0, pending, requestPending, requestGeneration = 0, roleCursors = [undefined], nextRoleCode;
  function clear() { byId('results').replaceChildren(); byId('previous').disabled = true; byId('next').disabled = true; }
  function message(error) {
    if (error.status === 401) { suspend(); return 'Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.'; }
    if (error.status === 403) return 'Không có quyền xem dữ liệu này.';
    if (error.message === 'INVALID_ID') return 'Vui lòng nhập mã định danh hợp lệ.';
    return 'Chưa thể tải dữ liệu quản trị. Vui lòng thử lại hoặc liên hệ quản trị.';
  }
  async function load() {
    pending?.abort(); pending = new AbortController(); const current = ++generation; clear(); byId('status').textContent = 'Đang tải…';
    try {
      const data = await client.list(screen, { offset, afterCode: roleCursors[offset / 50], userId: byId('subject-user').value, userRoleId: byId('subject-assignment').value }, pending.signal);
      if (current !== generation) return;
      const wrapper = document.createElement('div'); wrapper.className = 'table-scroll'; wrapper.tabIndex = 0;
      const table = document.createElement('table'), caption = document.createElement('caption'); caption.textContent = titles[screen]; table.append(caption);
      const head = document.createElement('tr'); for (const [label] of fields[screen]) { const th = document.createElement('th'); th.scope = 'col'; th.textContent = label; head.append(th); }
      const thead = document.createElement('thead'); thead.append(head); table.append(thead);
      const tbody = document.createElement('tbody');
      for (const row of data.items) { const tr = document.createElement('tr'); for (const [, key] of fields[screen]) { const td = document.createElement('td'); td.textContent = typeof row[key] === 'boolean' ? (row[key] ? 'Hoạt động' : 'Không hoạt động') : String(row[key] ?? ''); tr.append(td); } tbody.append(tr); }
      table.append(tbody); wrapper.append(table); byId('results').append(wrapper);
      byId('status').textContent = data.items.length ? `Đã tải ${data.items.length} mục.` : 'Không có dữ liệu trong phạm vi được phép.';
      nextRoleCode = data.nextCode;
      byId('previous').disabled = offset === 0; byId('next').disabled = !data.hasMore || offset >= 10000;
    } catch (error) { if (current === generation && error.name !== 'AbortError') { clear(); byId('status').textContent = message(error); } }
  }
  for (const button of document.querySelectorAll('[data-screen]')) button.addEventListener('click', () => {
    screen = button.dataset.screen; offset = 0; roleCursors = [undefined];
    for (const nav of document.querySelectorAll('[data-screen]')) { nav.setAttribute('aria-pressed', String(nav === button)); nav.classList.toggle('secondary', nav !== button); }
    byId('screen-title').textContent = titles[screen]; byId('subject-fields').hidden = screen !== 'grants'; load();
  });
  byId('refresh').addEventListener('click', load);
  byId('previous').addEventListener('click', () => { offset = Math.max(0, offset - 50); load(); });
  byId('next').addEventListener('click', () => { if (screen === 'roles') roleCursors[offset / 50 + 1] = nextRoleCode; offset += 50; load(); });
  byId('request-read').addEventListener('click', async () => {
    requestPending?.abort(); requestPending = new AbortController(); const current = ++requestGeneration;
    byId('request-status').textContent = 'Đang tải…';
    try { const value = await client.request(byId('request-id').value, requestPending.signal); if (current === requestGeneration) byId('request-status').textContent = changeStatus(value); }
    catch (error) { if (current === requestGeneration && error.name !== 'AbortError') byId('request-status').textContent = message(error); }
  });
  function suspend() { pending?.abort(); requestPending?.abort(); generation++; requestGeneration++; clear(); byId('request-status').textContent = 'Cần kiểm tra lại trạng thái.'; }
  addEventListener('pagehide', suspend);
  addEventListener('pageshow', () => { suspend(); load(); });
  document.addEventListener('visibilitychange', () => { if (document.hidden) suspend(); else load(); });
}
