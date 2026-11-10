import { invalidateForLogout, onLogout } from './session-lifecycle.mjs';

const login = document.getElementById('login-form');
const mfa = document.getElementById('mfa-form');
const logout = document.getElementById('logout-button');
const status = document.getElementById('auth-feedback');
let policy, challenge, request, generation = 0;

export function setAuthPolicy(value) {
  policy = value;
  for (const [id, feature] of [['login-controls', 'login'], ['mfa-controls', 'mfa']]) {
    const controls = document.getElementById(id);
    controls.disabled = policy?.[feature] !== 'ACTIVE';
  }
  logout.disabled = policy?.logout !== 'ACTIVE';
}
function clear() {
  generation++;
  request?.abort();
  challenge = undefined;
  login.reset();
  mfa.reset();
  document.getElementById('authenticated-links').hidden = true;
}
onLogout(() => { clear(); status.textContent = 'Đang đăng xuất. Nội dung phiên đã được xóa.'; });
addEventListener('pagehide', clear);

async function post(path, body, signal) {
  const proof = await fetch('/api/identity/v1/auth/csrf', {
    credentials: 'same-origin', cache: 'no-store', redirect: 'error', signal
  });
  if (!proof.ok) throw new Error();
  const { token } = await proof.json();
  const response = await fetch('/api/identity/v1/auth/' + path, {
    method: 'POST', credentials: 'same-origin', cache: 'no-store', redirect: 'error',
    headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': token },
    body: JSON.stringify(body), signal
  });
  if (!response.ok) throw new Error();
  return response;
}
async function submit(form, feature, action) {
  if (policy?.[feature] !== 'ACTIVE') return;
  request?.abort();
  const current = request = new AbortController();
  const revision = ++generation;
  const button = form.querySelector('button');
  button.disabled = true;
  try {
    await action(current.signal, () => !current.signal.aborted && revision === generation);
  } catch (error) {
    if (error.name !== 'AbortError' && revision === generation)
      status.textContent = 'Không hoàn tất xác thực. Kiểm tra thông tin hoặc liên hệ quản trị.';
  } finally {
    button.disabled = false;
  }
}
login.addEventListener('submit', event => {
  event.preventDefault();
  submit(login, 'login', async (signal, current) => {
    const username = login.elements.username.value;
    const password = login.elements.password.value;
    login.elements.password.value = '';
    challenge = undefined;
    const response = await post('login', { username, password }, signal);
    if (response.status !== 202) throw new Error();
    const data = await response.json();
    if (!current()) return;
    if (data.code !== 'MFA_REQUIRED' || typeof data.challenge !== 'string') throw new Error();
    challenge = data.challenge;
    location.hash = 'mfa';
    status.textContent = 'Nhập mã MFA để hoàn tất đăng nhập.';
    mfa.elements.factor.focus();
  });
});
mfa.addEventListener('submit', event => {
  event.preventDefault();
  if (!challenge) { status.textContent = 'Đăng nhập lại để nhận thử thách MFA mới.'; return; }
  submit(mfa, 'mfa', async (signal, current) => {
    const proof = challenge;
    const factorResponse = mfa.elements.factor.value;
    mfa.reset();
    const response = await post('mfa/verify', { challenge: proof, factorResponse }, signal);
    if (!current()) return;
    if (response.status !== 204) throw new Error();
    challenge = undefined;
    login.reset();
    location.hash = 'profile';
    document.getElementById('authenticated-links').hidden = false;
    status.textContent = 'Đăng nhập thành công.';
    dispatchEvent(new Event('fdc-authenticated'));
  });
});
logout.addEventListener('click', async () => {
  if (policy?.logout !== 'ACTIVE') return;
  invalidateForLogout();
  logout.disabled = true;
  try {
    await post('logout', {}, undefined);
    status.textContent = 'Đã đăng xuất.';
    location.hash = 'login';
  } catch {
    status.textContent = 'Chưa xác nhận thu hồi phiên từ máy chủ. Thử đăng xuất lại.';
  } finally {
    logout.disabled = policy?.logout !== 'ACTIVE';
  }
});
