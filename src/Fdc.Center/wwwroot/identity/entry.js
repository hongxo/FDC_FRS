import { setAuthPolicy } from './auth-ui.mjs';
'use strict';
const screens = new Map([
  ['login', ['Đăng nhập', 'Đăng nhập chưa được mở theo chính sách hiện tại.']],
  ['mfa', ['Xác minh MFA', 'MFA chưa được mở theo chính sách hiện tại.']],
  ['recovery', ['Khôi phục mật khẩu', 'Khôi phục tài khoản chưa được mở theo chính sách hiện tại.']],
  ['profile', ['Hồ sơ và bảo mật', 'Hồ sơ và thao tác bảo mật chưa được mở theo chính sách hiện tại.']]
]);
const labels = new Map([
  ['users', 'Quản trị người dùng'], ['roles', 'Vai trò'], ['permissions', 'Danh mục quyền'],
  ['grants', 'Phân quyền trạm'], ['audit', 'Nhật ký hệ thống'], ['requests', 'Theo dõi đề nghị']
]);
const navigation = document.getElementById('effective-navigation');
const blockedMessages = new Map([
  ['login', 'Máy chủ xác nhận phát hành phiên đang chờ phê duyệt.'],
  ['mfa', 'Máy chủ xác nhận thử thách MFA đang chờ phê duyệt.'],
  ['recovery', 'Máy chủ xác nhận quy trình khôi phục đang chờ phê duyệt.'],
  ['profile', 'Máy chủ xác nhận quản lý phiên đang chờ phê duyệt.']
]);
let pending, policyStatus;
function render() {
  const hash = location.hash.slice(1);
  const key = screens.has(hash) ? hash : 'login';
  const screen = screens.get(key);
  for (const panel of ['login', 'mfa', 'profile'])
    document.getElementById(panel + '-controls').hidden = panel !== key;
  document.getElementById('screen-title').textContent = screen[0];
  document.getElementById('policy-status').textContent = policyStatus?.[key] === 'ACTIVE' ? screen[0] : screen[1];
  setAuthPolicy(policyStatus);
  document.getElementById('runtime-status').textContent = policyStatus?.[key] === 'ACTIVE' ? 'Chức năng đã được máy chủ mở.' : policyStatus?.[key] === 'BLOCKED_BY_POLICY'
    ? blockedMessages.get(key) : 'Không xác nhận được chính sách từ máy chủ.';
}
function clearNavigation() {
  pending?.abort();
  navigation.hidden = true;
  navigation.replaceChildren();
}
async function loadNavigation() {
  clearNavigation();
  const request = pending = new AbortController();
  try {
    const response = await fetch('/api/identity/v1/ui/navigation', {
      credentials: 'same-origin', mode: 'same-origin', redirect: 'error', cache: 'no-store',
      headers: { Accept: 'application/json' }, signal: request.signal
    });
    if (!response.ok) return;
    const { screens: available } = await response.json();
    if (request.signal.aborted || !Array.isArray(available)) return;
    for (const screen of new Set(available)) {
      if (!labels.has(screen)) continue;
      const link = document.createElement('a');
      link.className = 'btn secondary';
      link.href = 'management.html#' + screen;
      link.textContent = labels.get(screen);
      navigation.append(link);
    }
    navigation.hidden = !navigation.childElementCount;
  } catch { /* No capability response means no navigation. */ }
}
addEventListener('hashchange', render);
addEventListener('fdc-authenticated', loadNavigation);
addEventListener('pageshow', async () => {
  try {
    const response = await fetch('/api/identity/v1/ui/auth-status', {
      credentials: 'omit', mode: 'same-origin', redirect: 'error', cache: 'no-store',
      headers: { Accept: 'application/json' }
    });
    policyStatus = response.ok ? await response.json() : undefined;
  } catch { policyStatus = undefined; }
  render();
  await loadNavigation();
});
addEventListener('pagehide', clearNavigation);
document.addEventListener('visibilitychange', () => { if (document.hidden) clearNavigation(); else loadNavigation(); });
render();
