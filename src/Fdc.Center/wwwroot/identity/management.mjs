const base = '/api/identity/v1/admin';
const idPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const ids = value => { if (typeof value !== 'string' || !idPattern.test(value)) throw new Error('INVALID_ID'); return value; };
export function managementPath(screen, { offset = 0, userId, userRoleId, afterCode, search, active, action } = {}) {
  if (!Number.isInteger(offset) || offset < 0 || offset > 10000) throw new Error('INVALID_PAGE');
  const query = new URLSearchParams({ offset: String(offset), limit: '50' });
  switch (screen) {
    case 'users':
      if (typeof search === 'string' && search.trim()) query.set('search', search.trim());
      if (active === 'true' || active === 'false') query.set('active', active);
      return `${base}/users?${query}`;
    case 'roles': {
      const roleQuery = new URLSearchParams({ limit: '50' });
      if (afterCode !== undefined) {
        if (typeof afterCode !== 'string' || afterCode.length > 80 || /[\x00-\x1f]/.test(afterCode)) throw new Error('INVALID_PAGE');
        roleQuery.set('afterCode', afterCode);
      }
      return `${base}/roles?${roleQuery}`;
    }
    case 'grants': query.set('userId', ids(userId)); query.set('userRoleId', ids(userRoleId)); return `${base}/station-grants?${query}`;
    case 'permissions': return base + '/permission-catalog?' + query;
    case 'audit':
      if (typeof action === 'string' && action.trim()) query.set('action', action.trim());
      return `${base}/security-audit?${query}`;
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
const statusCodes = Object.freeze({ 401: 'SESSION_REQUIRED', 403: 'ACCESS_DENIED', 409: 'CONFLICT', 412: 'PRECONDITION_FAILED', 428: 'PRECONDITION_REQUIRED', 429: 'RATE_LIMITED', 503: 'AUTHORITY_UNAVAILABLE' });
export function managementErrorMessage(error) {
  switch (error?.status) {
    case 401: return 'Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.';
    case 403: return 'Không có quyền xem dữ liệu này.';
    case 409: return 'Yêu cầu xung đột với trạng thái hiện tại. Vui lòng tải lại trước khi tiếp tục.';
    case 412: return 'Dữ liệu đã thay đổi. Vui lòng tải lại trước khi tiếp tục.';
    case 428: return 'Chưa đủ điều kiện để tiếp tục. Vui lòng kiểm tra lại yêu cầu.';
    case 429: return 'Quá nhiều yêu cầu. Vui lòng chờ trước khi thử lại.';
    default: return error?.message === 'INVALID_ID' ? 'Vui lòng nhập mã định danh hợp lệ.' : 'Chưa thể tải dữ liệu quản trị. Vui lòng thử lại hoặc liên hệ quản trị.';
  }
}
export function createManagementClient(fetcher = globalThis.fetch, protocol = globalThis.location?.protocol) {
  async function read(path, signal) {
    if (protocol !== 'https:') throw new ManagementApiError(0, 'HTTPS_REQUIRED');
    const response = await fetcher(path, { method: 'GET', credentials: 'same-origin', mode: 'same-origin', cache: 'no-store', redirect: 'error', headers: { Accept: 'application/json' }, signal });
    if (!response.ok || response.status !== 200) throw new ManagementApiError(response.status, statusCodes[response.status] ?? 'REQUEST_FAILED');
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
    async grantFunctions(userId, userRoleId, grantId, signal) {
      const query = new URLSearchParams({ userId: ids(userId), userRoleId: ids(userRoleId) });
      const rows = await read(base + '/station-grants/' + ids(grantId) + '/functions?' + query, signal);
      if (!Array.isArray(rows)) throw new ManagementApiError(0, 'INVALID_RESPONSE');
      return rows;
    },
    async navigation(signal) {
      const data = await read('/api/identity/v1/ui/navigation', signal);
      if (!data || !Array.isArray(data.screens)) throw new ManagementApiError(0, 'INVALID_RESPONSE');
      return data.screens;
    },
    async detail(screen, id, signal) {
      const key = ids(id);
      switch (screen) {
        case 'users': return [await read(base + '/users/' + key, signal), (await read(base + '/users/' + key + '/roles?limit=50', signal)).items];
        case 'roles': return [await read(base + '/roles/' + key, signal), await read(base + '/roles/' + key + '/permissions', signal)];
        case 'audit': return [await read(base + '/security-audit/' + key, signal)];
        default: throw new Error('UNSUPPORTED_SCREEN');
      }
    },
    async request(id, signal) { return read(`${base}/change-requests/${ids(id)}`, signal); }
  });
}

if (typeof document !== 'undefined') {
  const byId = id => document.getElementById(id);
  const client = createManagementClient();
  const fields = { users: [['Username', 'username'], ['Trạng thái', 'active'], ['ID', 'id']], permissions: [['Mã quyền', 'code'], ['Phân hệ', 'module'], ['Thao tác', 'action'], ['Phạm vi', 'scope'], ['Mức rủi ro', 'risk'], ['Phiên bản', 'catalogVersion']], roles: [['Mã vai trò', 'code'], ['Tên vai trò', 'name'], ['Trạng thái', 'active']], grants: [['Grant ID', 'id'], ['Trạm ID', 'stationId'], ['UserRole ID', 'userRoleId'], ['Phạm vi', 'scopeKind'], ['Bắt đầu', 'validFrom'], ['Kết thúc', 'validUntil']], audit: [['Mã sự kiện', 'id'], ['Mã đối chiếu', 'correlationId'], ['Thao tác', 'action'], ['Kết quả', 'result'], ['Thời gian', 'timestamp']] };
  const titles = { users: 'Người dùng', roles: 'Vai trò và quyền', permissions: 'Danh mục quyền', grants: 'Quyền truy cập trạm', audit: 'Nhật ký bảo mật' };
  let screen = Object.hasOwn(fields, location.hash.slice(1)) ? location.hash.slice(1) : 'users', offset = 0, generation = 0, pending, requestPending, requestGeneration = 0, roleCursors = [undefined], nextRoleCode;
  let navigationPending, detailPending;
  const detailFields = {
    users: ['id', 'username', 'active', 'version'],
    roles: ['id', 'code', 'name', 'description', 'active', 'version'],
    audit: ['id', 'timestamp', 'actorId', 'action', 'result', 'correlationId']
  };
  const pick = (row, keys) => Object.fromEntries(keys.map(key => [key, row[key]]));
  async function navigation() {
    navigationPending?.abort();
    const request = navigationPending = new AbortController();
    try {
      const screens = await client.navigation(request.signal);
      if (navigationPending !== request || request.signal.aborted) return;
      for (const button of document.querySelectorAll('[data-screen]')) button.hidden = !screens.includes(button.dataset.screen);
      byId('request-panel').hidden = !screens.includes('requests');
      byId('navigation-status').textContent = screens.length ? '' : 'Chưa có chức năng quản trị được máy chủ cho phép.';
      return screens.includes(screen);
    } catch {
      if (!request.signal.aborted) byId('navigation-status').textContent = 'Không xác nhận được chức năng quản trị.';
    }
  }
  function configureScreen() {
    detailPending?.abort();
    byId('detail-result').textContent = '';
    byId('detail-fields').hidden = !Object.hasOwn(detailFields, screen);
    byId('user-filters').hidden = screen !== 'users';
    byId('audit-filters').hidden = screen !== 'audit';
    byId('grant-result').textContent = '';
  }
  function clear() { byId('results').replaceChildren(); byId('previous').disabled = true; byId('next').disabled = true; }
  function message(error) {
    if (error.status === 401) suspend();
    return managementErrorMessage(error);
  }
  async function load() {
    pending?.abort(); pending = new AbortController(); const current = ++generation; clear(); byId('status').textContent = 'Đang tải…';
    try {
      const data = await client.list(screen, { offset, afterCode: roleCursors[offset / 50], search: byId('user-search').value, active: byId('user-active').value, action: byId('audit-action').value, userId: byId('subject-user').value, userRoleId: byId('subject-assignment').value }, pending.signal);
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
    byId('screen-title').textContent = titles[screen]; byId('subject-fields').hidden = screen !== 'grants'; configureScreen(); load();
  });
  byId('screen-title').textContent = titles[screen]; byId('subject-fields').hidden = screen !== 'grants';
  for (const nav of document.querySelectorAll('[data-screen]')) { nav.setAttribute('aria-pressed', String(nav.dataset.screen === screen)); nav.classList.toggle('secondary', nav.dataset.screen !== screen); }
  configureScreen();
  for (const id of ['subject-user', 'subject-assignment', 'grant-id']) {
    byId(id).addEventListener('input', () => {
      detailPending?.abort();
      byId('grant-result').textContent = '';
      pending?.abort();
      generation++;
      clear();
    });
  }
  byId('refresh').addEventListener('click', () => { offset = 0; roleCursors = [undefined]; configureScreen(); load(); });
  byId('detail-read').addEventListener('click', async () => {
    detailPending?.abort();
    const request = detailPending = new AbortController();
    const selected = screen;
    byId('detail-result').textContent = 'Đang tải…';
    try {
      const [row, related] = await client.detail(selected, byId('detail-id').value.trim(), request.signal);
      if (request.signal.aborted || detailPending !== request) return;
      const result = { ...pick(row, detailFields[selected]) };
      if (selected === 'users') result.userRoles = related.map(role => pick(role, ['id', 'roleName', 'active', 'validFrom', 'validUntil']));
      if (selected === 'roles') result.permissions = related.filter(code => typeof code === 'string');
      byId('detail-result').textContent = JSON.stringify(result, null, 2);
    } catch (error) {
      if (!request.signal.aborted && detailPending === request) byId('detail-result').textContent = message(error);
    }
  });
  byId('grant-read').addEventListener('click', async () => {
    detailPending?.abort();
    const request = detailPending = new AbortController();
    const target = byId('grant-result');
    target.textContent = 'Đang tải…';
    try {
      const rows = await client.grantFunctions(byId('subject-user').value.trim(),
        byId('subject-assignment').value.trim(), byId('grant-id').value.trim(), request.signal);
      if (!request.signal.aborted && detailPending === request)
        target.textContent = rows.map(row => row.permissionCode).join(', ') || 'Không có chức năng hiển thị.';
    } catch (error) {
      if (!request.signal.aborted && detailPending === request) target.textContent = message(error);
    }
  });
  byId('previous').addEventListener('click', () => { offset = Math.max(0, offset - 50); load(); });
  byId('next').addEventListener('click', () => { if (screen === 'roles') roleCursors[offset / 50 + 1] = nextRoleCode; offset += 50; load(); });
  byId('request-read').addEventListener('click', async () => {
    requestPending?.abort(); requestPending = new AbortController(); const current = ++requestGeneration;
    byId('request-status').textContent = 'Đang tải…';
    try { const value = await client.request(byId('request-id').value, requestPending.signal); if (current === requestGeneration) byId('request-status').textContent = changeStatus(value); }
    catch (error) { if (current === requestGeneration && error.name !== 'AbortError') byId('request-status').textContent = message(error); }
  });
  function suspend() { navigationPending?.abort(); configureScreen(); for (const button of document.querySelectorAll('[data-screen]')) button.hidden = true; byId('request-panel').hidden = true; pending?.abort(); requestPending?.abort(); generation++; requestGeneration++; clear(); byId('request-status').textContent = 'Cần kiểm tra lại trạng thái.'; }
  addEventListener('pagehide', suspend);
  async function restore() {
    suspend();
    if (await navigation()) load();
  }
  addEventListener('pageshow', restore);
  document.addEventListener('visibilitychange', () => { if (document.hidden) suspend(); else restore(); });
}
