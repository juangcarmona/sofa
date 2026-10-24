import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, writeFile, chmod, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

test('session cleanup accepts exact absence but preserves real failures', async () => {
  const directory = await mkdtemp(join(tmpdir(), 'sofa-cleanup-'));
  try {
    const binary = join(directory, 'opencode');
    for (const [response, code, expected] of [['Deleted', 0, 0], ['Session not found: ses_test', 1, 0], ['Session not found: ses_other', 1, 1], ['Database locked', 1, 1]]) {
      await writeFile(binary, `#!/bin/sh\nprintf '%s\\n' '${response}'\nexit ${code}\n`);
      await chmod(binary, 0o700);
      const run = spawnSync('sh', [resolve('src/execution/entrypoint.sh'), 'cleanup-session', 'ses_test'], { env: { ...process.env, PATH: directory + ':' + process.env.PATH }, encoding: 'utf8' });
      assert.equal(run.status, expected, run.stderr);
    }
  } finally { await rm(directory, { recursive: true, force: true }); }
});
