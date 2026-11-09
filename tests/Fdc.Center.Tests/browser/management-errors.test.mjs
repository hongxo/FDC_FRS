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
