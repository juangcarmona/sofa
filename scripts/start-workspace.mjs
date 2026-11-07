import { spawn, spawnSync } from 'node:child_process';
import { readFile, realpath } from 'node:fs/promises';
import { dirname, isAbsolute, resolve, sep } from 'node:path';

import { prepareAspire } from './prepare-aspire.mjs';

const root = resolve(import.meta.dirname, '..');
const configured = process.env.SOFA_CONFIG_FILE;
if (!configured || !isAbsolute(configured)) throw Error('Set SOFA_CONFIG_FILE to an absolute private configuration file.');
const file = await realpath(configured);
if (file === root || file.startsWith(root + sep)) throw Error('Keep workspace credentials outside the checkout.');
const configuration = JSON.parse(await readFile(file, 'utf8'));
const environment = await prepareAspire({ ...process.env, SOFA_CONFIG_FILE: file });
if (configuration.SoFa?.Identity?.Mode === 'Development') environment.SSL_CERT_FILE = resolve(dirname(file), 'trust-bundle.pem');
for (const args of [
  ['restore', 'SoFa.slnx', '--locked-mode', '--disable-build-servers'],
  ['build', 'src/SoFa.AppHost/SoFa.AppHost.csproj', '--no-restore', '-c', 'Release', '--disable-build-servers'],
]) {
  const result = spawnSync('dotnet', args, { cwd: root, env: environment, stdio: 'inherit' });
  if (result.error) throw result.error;
  if (result.status !== 0) process.exit(result.status ?? 1);
}
// Aspire's dotnet-run hook can select another configuration and start its setup wizard.
// Execute the exact build above; the AppHost's pinned CLI bundle still owns DCP discovery.
const child = spawn('dotnet', ['src/SoFa.AppHost/bin/Release/net10.0/SoFa.AppHost.dll'], { cwd: root, env: environment, stdio: 'inherit' });
for (const signal of ['SIGINT', 'SIGTERM']) process.once(signal, () => child.kill(signal));
child.once('error', () => { process.exitCode = 1; });
child.once('exit', (code, signal) => { process.exitCode = code ?? (signal === 'SIGINT' ? 130 : 1); });
