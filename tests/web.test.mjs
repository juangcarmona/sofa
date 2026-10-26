import test from 'node:test';
import assert from 'node:assert/strict';
import { once } from 'node:events';
import { get } from 'node:http';
import { createObservationServer } from '../src/web/server.mjs';

async function start(t, options = {}) {
  const server = createObservationServer({ repositories: ['owner/repo'], ...options });
  server.listen(0, '127.0.0.1'); await once(server, 'listening');
  t.after(() => new Promise(resolve => { server.close(resolve); server.closeAllConnections(); }));
  return `http://127.0.0.1:${server.address().port}`;
}

test('web surface bounds collection, coalesces requests and refreshes expired evidence', async t => {
  let calls = 0, clock = 0;
  const root = await start(t, { now: () => clock, ttl: 15, observe: async repos => { calls++; assert.deepEqual(repos, ['owner/repo']); await new Promise(r => setTimeout(r, 15)); return [{ repository: repos[0], availability: 'unavailable' }]; } });
  const responses = await Promise.all(Array.from({ length: 5 }, () => fetch(`${root}/api/observations`).then(r => r.json())));
  assert.equal(calls, 1); assert.equal(responses[0].repositories[0].availability, 'unavailable');
  await fetch(`${root}/api/observations`); assert.equal(calls, 1);
  clock = 16; await fetch(`${root}/api/observations`); assert.equal(calls, 2);
});

test('web cannot dispatch, select arbitrary repos, traverse files or serve cross-site reads', async t => {
  let calls = 0;
  const root = await start(t, { observe: async () => { calls++; return []; } });
  for (const method of ['POST', 'PUT', 'DELETE', 'PATCH']) assert.equal((await fetch(`${root}/api/observations`, { method })).status, 405);
  for (const path of ['/api/dispatch', '/engine.json', '/api/observations?repository=other/repo', '/%2e%2e/package.json']) assert.equal((await fetch(root + path)).status, 404);
  const rawStatus = headers => new Promise((resolve, reject) => get(`${root}/api/observations`, { headers }, res => { res.resume(); resolve(res.statusCode); }).on('error', reject));
  assert.equal(await rawStatus({ host: 'evil.example' }), 403);
  assert.equal(await rawStatus({ 'sec-fetch-site': 'cross-site' }), 403);
  assert.equal(calls, 0);
  const page = await fetch(root); assert.equal(page.status, 200);
  assert.match(page.headers.get('content-security-policy'), /frame-ancestors 'none'/);
  assert.equal(page.headers.get('access-control-allow-origin'), null);
  assert.equal((await fetch(`${root}/tokens.css`)).status, 200);
});

test('collection failure is generic, retriable and does not expose credentials', async t => {
  let fail = true;
  const root = await start(t, { observe: async () => { if (fail) throw Error('private credential'); return []; } });
  const response = await fetch(`${root}/api/observations`); assert.equal(response.status, 503); assert.doesNotMatch(await response.text(), /credential/);
  fail = false; assert.equal((await fetch(`${root}/api/observations`)).status, 200);
  assert.throws(() => createObservationServer({ repositories: ['../secrets'] }), /owner\/repository/);
  assert.throws(() => createObservationServer({ repositories: [] }), /owner\/repository/);
});
