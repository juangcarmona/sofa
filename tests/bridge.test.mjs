import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, writeFile, access, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

test('a failed admission cannot upload the preceding job receipt', async () => {
  const dir = await mkdtemp(join(tmpdir(), 'sofa-bridge-'));
  try {
    const receipt = join(dir, 'receipt.json');
    await writeFile(receipt, '{"state":"waiting-human","receipt":"old"}');
    const child = spawnSync(process.execPath, [resolve('src/bridge/sofa.mjs'), 'github-event'], {
      cwd: dir, env: { ...process.env, SOFA_SOCKET: join(dir, 'absent.sock'), LIGHTHOUSE_RECEIPT: receipt }, encoding: 'utf8',
    });
    assert.equal(child.status, 1);
    await assert.rejects(access(receipt), { code: 'ENOENT' });
  } finally { await rm(dir, { recursive: true, force: true }); }
});

test('label entry uses the host router and retries return the same durable receipt', async () => {
  const { createServer } = await import('node:http');
  const { spawn } = await import('node:child_process');
  const { createHash } = await import('node:crypto');
  const { readFile } = await import('node:fs/promises');
  const dir = await mkdtemp(join(tmpdir(), 'sofa-route-'));
  const socket = join(dir, 'bridge.sock');
  const event = join(dir, 'event.json');
  const receipt = join(dir, 'receipt.json');
  let admitted; let submissions = 0; let routes = 0;
  const authorization = { kind: 'label', actor: 'owner', eventId: 42 };
  const server = createServer(async (request, response) => {
    let raw = ''; for await (const chunk of request) raw += chunk;
    const send = (value, status = 200) => { response.writeHead(status, { 'content-type': 'application/json' }); response.end(JSON.stringify(value)); };
    if (request.url === '/policy') return send({ repository: 'owner/repo', owner: 'owner', maxSeconds: 60, packageVersion: '0.10.0', policyDigest: 'policy' });
    if (request.url === '/route') { routes++; return send({ action: 'run', stage: 'refine', route: 'supervised', authorization }); }
    if (request.method === 'GET') return admitted ? send(admitted) : send({}, 404);
    if (request.url === '/runs') { submissions++; const value = JSON.parse(raw); admitted = { id: value.runId, request: raw, state: 'waiting-human', receipt: 'https://github.com/receipt' }; return send(admitted); }
    send({}, 400);
  });
  try {
    await new Promise((resolve) => server.listen(socket, resolve));
    await writeFile(event, JSON.stringify({ action: 'labeled', label: { name: 'refine' }, issue: { number: 1 } }));
    async function execute() {
      const child = spawn(process.execPath, [resolve('src/bridge/sofa.mjs'), 'github-event'], { env: { ...process.env, SOFA_SOCKET: socket, LIGHTHOUSE_RECEIPT: receipt, GITHUB_EVENT_PATH: event, GITHUB_REPOSITORY: 'owner/repo', GITHUB_ACTOR: 'owner', GITHUB_EVENT_NAME: 'issues', GITHUB_RUN_ID: '7', GITHUB_SHA: 'a'.repeat(40), LIGHTHOUSE_WORKFLOW_REVISION: 'a'.repeat(40) }, stdio: ['ignore', 'ignore', 'pipe'] });
      let errors = ''; child.stderr.on('data', (chunk) => errors += chunk);
      assert.equal(await new Promise((resolve) => child.on('exit', resolve)), 0, errors);
    }
    await execute(); await execute();
    assert.equal(routes, 2); assert.equal(submissions, 1);
    const request = JSON.parse(admitted.request);
    assert.equal(request.schemaVersion, 2); assert.equal(request.stage, 'refine');
    assert.equal(request.predecessor, null); assert.equal(request.approvalCommentId, null);
    assert.deepEqual(request.authorization, authorization);
    assert.equal(request.runId, createHash('sha256').update('owner/repo:1:refine:42').digest('hex'));
    assert.equal(JSON.parse(await readFile(receipt, 'utf8')).id, request.runId);
  } finally { await new Promise((resolve) => server.close(resolve)); await rm(dir, { recursive: true, force: true }); }
});
