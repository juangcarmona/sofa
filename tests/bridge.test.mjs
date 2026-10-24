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
