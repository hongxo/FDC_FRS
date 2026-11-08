import { test } from 'node:test';
import assert from 'node:assert/strict';
import { managementPath, createManagementClient, changeStatus } from '../management.mjs';
const id = '00000000-0000-0000-0000-000000000001';
const response = (status, data, type = 'application/json') => ({ status, ok: status >= 200 && status < 300, headers: new Headers({ 'content-type': type }), json: async () => data });
test('canonical same-origin routes are fixed server operations', () => {
  assert.match(managementPath('users'), /^\/api\/identity\/v1\/admin\/users\?/);
  assert.match(managementPath('grants', { userId: id, userRoleId: id }), /userRoleId=/);
  assert.throws(() => managementPath('https://evil.test'), /UNSUPPORTED/);
});
test('station IDs are validated intent, not permission selection', () => assert.throws(() => managementPath('grants', { userId: '../admin', userRoleId: id }), /INVALID_ID/));
test('unbounded pages are rejected', () => { assert.throws(() => managementPath('users', { offset: 10001 })); assert.throws(() => managementPath('users', { offset: -1 })); });
test('role cursor is encoded as data', () => assert.match(managementPath('roles', { afterCode: 'role&x=1' }), /afterCode=role%26x%3D1/));
test('202 is pending, never effective', () => assert.match(changeStatus({ effectiveStatus: 'Applied' }, 202), /chưa áp dụng/));
test('approval alone is not apply', () => assert.match(changeStatus({ approvalStatus: 'Approved', effectiveStatus: 'NotApplied' }), /chưa áp dụng/));
test('only explicit applied status is labelled applied', () => { assert.equal(changeStatus({ effectiveStatus: 'Applied' }), 'Đã áp dụng.'); assert.match(changeStatus({}), /Chưa xác nhận/); });
test('HTTPS is required before a fetch', async () => {
  let called = false; const client = createManagementClient(async () => { called = true; }, 'http:');
  await assert.rejects(client.list('users'), /HTTPS_REQUIRED/); assert.equal(called, false);
});
test('cookies are same-origin and requests do not cache or redirect', async () => {
  let captured; const client = createManagementClient(async (path, options) => { captured = options; return response(200, { items: [], hasMore: false }); }, 'https:');
  await client.list('users'); assert.equal(captured.credentials, 'same-origin'); assert.equal(captured.cache, 'no-store'); assert.equal(captured.redirect, 'error'); assert.equal(captured.method, 'GET');
  assert.equal(captured.headers.Authorization, undefined);
});
for (const status of [401, 403, 409, 503]) test(`HTTP ${status} returns error without fixture fallback`, async () => {
  const client = createManagementClient(async () => response(status, { items: [{ username: 'must-not-render' }], hasMore: false }), 'https:');
  await assert.rejects(client.list('users'), error => error.status === status);
});
test('unexpected content type rejected', async () => await assert.rejects(createManagementClient(async () => response(200, {}, 'text/html'), 'https:').list('users'), /INVALID_RESPONSE/));
test('malformed or oversized response rejected', async () => {
  for (const data of [{}, { items: [], hasMore: 'yes' }, { items: Array(101).fill({}), hasMore: false }])
    await assert.rejects(createManagementClient(async () => response(200, data), 'https:').list('users'), /INVALID_RESPONSE/);
});
test('role cursor and dynamic custom roles are retained', async () => {
  const result = await createManagementClient(async () => response(200, { items: [{ code: 'eighth-custom-role' }], nextCode: 'eighth-custom-role' }), 'https:').list('roles');
  assert.equal(result.hasMore, true); assert.equal(result.nextCode, 'eighth-custom-role');
});
test('request detail uses fixed canonical route with UUID', async () => {
  let path; const client = createManagementClient(async value => { path = value; return response(200, { approvalStatus: 'Pending', effectiveStatus: 'NotApplied' }); }, 'https:');
  await client.request(id); assert.equal(path, '/api/identity/v1/admin/change-requests/' + id);
  await assert.rejects(client.request('../force-apply'), /INVALID_ID/);
});
