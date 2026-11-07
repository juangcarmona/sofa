import { spawnSync } from 'node:child_process';
import { readFile, mkdir } from 'node:fs/promises';
import { homedir } from 'node:os';
import { join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

// Resolve the project-pinned native CLI, including SDKs without the DNX fallback.
// Keep both extraction and validation under the same isolated Aspire home.
export async function prepareAspire(environment = process.env) {
  const root = resolve(import.meta.dirname, '..');
  const manifest = JSON.parse(await readFile(join(root, 'package.json'), 'utf8'));
  const version = manifest.devDependencies['@microsoft/aspire-cli'];
  if (!/^\d+\.\d+\.\d+$/.test(version)) throw Error('Pin the Aspire CLI to the AppHost SDK version.');
  const home = join(environment.XDG_CACHE_HOME || join(homedir(), '.cache'), 'sofa', 'aspire', version);
  const configured = { ...environment, ASPIRE_HOME: home, ASPIRE_NPM_CACHE_DIR: join(home, 'npm'),
    ASPIRE_CLI_TELEMETRY_OPTOUT: '1', AspireCliBundlePath: home };
  await mkdir(home, { recursive: true });
  const entrypoint = join(root, 'node_modules/@microsoft/aspire-cli/bin/aspire.js');
  const selected = spawnSync(process.execPath, [entrypoint, '--version'], { cwd: root, env: configured, encoding: 'utf8', timeout: 30000 });
  if (selected.status !== 0 || !selected.stdout.trim().startsWith(version + '+')) throw Error('The installed native Aspire CLI does not match the pinned project version.');
  const result = spawnSync(process.execPath, [entrypoint, 'setup', '--install-path', home],
    { cwd: root, env: configured, stdio: 'inherit', timeout: 120000 });
  if (result.error || result.status !== 0) throw Error('The pinned Aspire bundle must be prepared before building the workspace.');
  return configured;
}
if (process.argv[1] === fileURLToPath(import.meta.url)) await prepareAspire();
