const listeners = new Set();
const key = 'fdc.logout.intent';
const channel = typeof BroadcastChannel === 'function' ? new BroadcastChannel(key) : null;
let observed = readStamp();

function readStamp() {
  try { return localStorage.getItem(key); } catch { return null; }
}
function notify() {
  for (const listener of listeners) listener();
}
channel?.addEventListener('message', event => {
  if (event.data === 'logout') { observed = readStamp(); notify(); }
});
addEventListener('storage', event => {
  if (event.key === key) { observed = event.newValue; notify(); }
});
addEventListener('pageshow', () => {
  const current = readStamp();
  if (current !== observed) { observed = current; notify(); }
});

export function onLogout(listener) {
  listeners.add(listener);
  return () => listeners.delete(listener);
}
export function invalidateForLogout() {
  observed = crypto.randomUUID();
  try { localStorage.setItem(key, observed); } catch { /* Channel still clears live tabs. */ }
  channel?.postMessage('logout');
  notify();
}
