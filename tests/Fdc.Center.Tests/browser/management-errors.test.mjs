import test from 'node:test';
import assert from 'node:assert/strict';
import { createManagementClient, managementErrorMessage } from '../../../src/Fdc.Center/wwwroot/identity/management.mjs';

for (const [status, code, message] of [
  [401, 'SESSION_REQUIRED', /đăng nhập lại/], [403, 'ACCESS_DENIED', /Không có quyền/],
  [409, 'CONFLICT', /xung đột/], [412, 'PRECONDITION_FAILED', /đã thay đổi/],
  [428, 'PRECONDITION_REQUIRED', /điều kiện/], [429, 'RATE_LIMITED', /Quá nhiều/],
  [503, 'AUTHORITY_UNAVAILABLE', /Chưa thể tải/]
]) {
  test(`HTTP ${status} has explicit safe state and does not retry or render server body`, async () => {
    let calls = 0, parsed = false;
    const client = createManagementClient(async () => {
      calls++;
      return { ok: false, status, json: async () => { parsed = true; return { items: [{ name: 'MUST-NOT-RENDER' }] }; } };
    }, 'https:');
    await assert.rejects(client.list('users'), error => {
      assert.equal(error.status, status); assert.equal(error.message, code);
      assert.match(managementErrorMessage(error), message); return true;
    });
    assert.equal(calls, 1); assert.equal(parsed, false);
  });
}
test('unknown server exception and credential text are never shown to the user', () => {
  const text = managementErrorMessage({ status: 500, message: 'SENSITIVE-SERVER-DETAIL' });
  assert.doesNotMatch(text, /SENSITIVE/);
});

test('canonical catalog, filtered users and assignment-bound grants use distinct read paths', async () => {
  const requests = [];
  const client = createManagementClient(async (path, options) => {
    requests.push([new URL(path, 'https://fixture.invalid'), options]);
    return { ok: true, status: 200, headers: new Headers({ 'content-type': 'application/json' }),
      json: async () => ({ items: [], hasMore: false }) };
  }, 'https:');
  await client.list('permissions');
  await client.list('users', { search: ' A+B ', active: 'false' });
  const userId = '11111111-1111-1111-1111-111111111111';
  const userRoleId = '22222222-2222-2222-2222-222222222222';
  await client.list('grants', { userId, userRoleId });
  await assert.rejects(client.list('grants', { userId }), /INVALID_ID/);
  assert.deepEqual(requests.map(([url]) => url.pathname.split('/').at(-1)), ['permission-catalog', 'users', 'station-grants']);
  assert.equal(requests[1][0].searchParams.get('search'), 'A+B');
  assert.equal(requests[1][0].searchParams.get('active'), 'false');
  assert.equal(requests[2][0].searchParams.get('userId'), userId);
  assert.equal(requests[2][0].searchParams.get('userRoleId'), userRoleId);
  assert.ok(requests.every(([, options]) => options.method === 'GET' && options.credentials === 'same-origin'));
});

test('menu consumes only server navigation and rejects non-HTTPS before fetching', async () => {
  const client = createManagementClient(async () => assert.fail('Unexpected network request'), 'http:');
  await assert.rejects(client.navigation(), /HTTPS_REQUIRED/);
});
