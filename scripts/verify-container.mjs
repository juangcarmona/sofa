import { spawnSync } from 'node:child_process';

if (!process.env.npm_execpath) throw new Error('Run npm run verify:container');
const result = spawnSync(process.execPath, [process.env.npm_execpath, 'run', 'verify'], {
  stdio: 'inherit', env: { ...process.env, SOFA_RUNTIME_TEST_SCOPE: 'container' },
});
if (result.error) throw result.error;
process.exitCode = result.status ?? 1;
