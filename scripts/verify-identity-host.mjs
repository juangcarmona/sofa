import { mkdtemp, readFile, writeFile, mkdir, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { resolve, join } from 'node:path';
import { createServer } from 'node:net';
import { spawn, spawnSync } from 'node:child_process';
import https from 'node:https';
import assert from 'node:assert/strict';
import { prepareAspire } from './prepare-aspire.mjs';

const root = resolve(import.meta.dirname, '..');
const parent = await mkdtemp(join(tmpdir(), 'sofa-provider-host-'));
const directory = join(parent, 'identity');
const evidence = resolve(root, 'test-results/identity');
let application, providerContainer, proof;
let stage = 'private-setup';
const wait = milliseconds => new Promise(done => setTimeout(done, milliseconds));
let environment = Object.fromEntries(Object.entries(process.env).filter(([name]) => !/^sofa(?:__|_config_file$)/i.test(name)));
function command(executable, args, options = {}) {
  const result = spawnSync(executable, args, { cwd: root, env: environment, encoding: 'utf8', ...options });
  if (result.error || result.status !== 0) throw Error('A required identity host setup command failed.');
  return result.stdout?.trim();
}
async function availablePort() {
  const server = createServer();
  await new Promise((ready, reject) => { server.once('error', reject); server.listen(0, '127.0.0.1', ready); });
  const port = server.address().port;
  await new Promise(done => server.close(done));
  return port;
}
function ready(origin, ca) {
  return new Promise(done => {
    const request = https.get(origin + '/health/live', { ca, timeout: 2000 }, response => { response.resume(); done(response.statusCode === 200); });
    request.on('error', () => done(false)); request.on('timeout', () => { request.destroy(); done(false); });
  });
}
async function stop() {
  if (proof && proof.exitCode === null) {
    proof.kill('SIGTERM');
    for (let attempt = 0; attempt < 50 && proof.exitCode === null; attempt++) await wait(100);
    if (proof.exitCode === null) proof.kill('SIGKILL');
  }
  if (!application || application.exitCode !== null) return;
  application.kill('SIGTERM');
  for (let attempt = 0; attempt < 100 && application.exitCode === null; attempt++) await wait(100);
  if (application.exitCode === null) application.kill('SIGKILL');
}
for (const signal of ['SIGINT', 'SIGTERM']) process.once(signal, async () => { await stop(); process.exit(1); });
try {
  const appPort = await availablePort(), issuerPort = await availablePort();
  assert.notEqual(appPort, issuerPort);
  const origin = 'https://127.0.0.1:' + appPort;
  command(process.execPath, ['scripts/setup-development-identity.mjs', '--directory', directory,
    '--origin', origin, '--issuer', 'https://127.0.0.1:' + issuerPort + '/realms/sofa-dev', '--test-accounts']);
  const configurationPath = join(directory, 'development.json');
  const configuration = JSON.parse(await readFile(configurationPath, 'utf8'));
  // This registered-client-shaped fixture enables the account controls, but no GitHub
  // credential or actual user consent is supplied; source transport tests own that lane.
  configuration.SoFa.GitHub = { ClientId: 'development-ui-fixture', ClientSecret: 'isolated-client-fixture-value' };
  await writeFile(configurationPath, JSON.stringify(configuration), { mode: 0o600 });
  stage = 'isolated-browser-trust';
  const browserHome = join(directory, 'browser-home');
  const nss = join(browserHome, '.pki/nssdb');
  await mkdir(nss, { recursive: true, mode: 0o700 });
  const certutil = process.env.SOFA_TEST_CERTUTIL || 'certutil';
  command(certutil, ['-N', '-d', 'sql:' + nss, '--empty-password']);
  command(certutil, ['-A', '-d', 'sql:' + nss, '-n', 'SoFa isolated identity test CA', '-t', 'C,,', '-i', join(directory, 'ca.crt')]);
  environment.SOFA_CONFIG_FILE = configurationPath;
  environment.DOTNET_PROCESSOR_COUNT ||= '2';
  environment.MSBUILDDISABLENODEREUSE = '1';
  environment.DOTNET_ENVIRONMENT = 'Development';
  environment.SSL_CERT_FILE = join(directory, 'trust-bundle.pem');
  // Build before launch so resource discovery uses the exact Release assembly.
  stage = 'pinned-aspire-bundle';
  environment = await prepareAspire(environment);
  stage = 'release-build';
  command('dotnet', ['build', 'src/SoFa.AppHost/SoFa.AppHost.csproj', '-c', 'Release', '--no-restore', '--disable-build-servers']);
  const { open } = await import('node:fs/promises');
  const log = await open(join(directory, 'apphost.log'), 'wx', 0o600);
  stage = 'application-start';
  application = spawn('dotnet', ['src/SoFa.AppHost/bin/Release/net10.0/SoFa.AppHost.dll'], { cwd: root, env: environment, stdio: ['ignore', log.fd, log.fd] });
  await log.close();
  const ca = await readFile(join(directory, 'ca.crt'));
  stage = 'workspace-and-provider-readiness';
  let healthy = false;
  for (let attempt = 0; attempt < 180; attempt++) {
    if (application.exitCode !== null) break;
    if (await ready(origin, ca)) {
      // The liveness endpoint does not attest provider readiness; check discovery separately.
      try {
        const result = command('curl', ['--silent', '--fail', '--cacert', join(directory, 'ca.crt'), '--max-time', '2',
          'https://127.0.0.1:' + issuerPort + '/realms/sofa-dev/.well-known/openid-configuration']);
        healthy = JSON.parse(result).issuer === configuration.SoFa.Identity.Development.Issuer;
      } catch { }
      if (healthy) break;
    }
    await wait(500);
  }
  assert.ok(healthy, 'The real workspace and pinned provider must start with trusted TLS.');
  const containers = command('docker', ['ps', '--filter', 'label=sofa.scope=issue-49-development', '--format', '{{.Names}}']).split('\n').filter(Boolean);
  for (const name of containers) {
    const mounts = JSON.parse(command('docker', ['inspect', name, '--format', '{{json .Mounts}}']));
    if (mounts.some(mount => mount.Source === join(directory, 'import'))) providerContainer = name;
  }
  assert.ok(providerContainer, 'Find only the provider mounted from this test setup.');
  await mkdir(evidence, { recursive: true });
  stage = 'real-provider-browser-proof';
  proof = spawn(process.execPath, ['scripts/verify-identity-provider.mjs', '--private-directory', directory,
    '--evidence-directory', evidence, '--provider-container', providerContainer], {
    cwd: root, env: { ...environment, NODE_EXTRA_CA_CERTS: join(directory, 'ca.crt') }, stdio: 'inherit',
  });
  const exit = await new Promise((done, reject) => { proof.once('error', reject); proof.once('exit', code => done(code)); });
  assert.equal(exit, 0, 'Real provider verification must pass.');
  console.log('Isolated Aspire identity verification passed; physical phone and real GitHub consent remain separate.');
} catch (error) {
  await mkdir(evidence, { recursive: true });
  await writeFile(join(evidence, 'host-failure.json'), JSON.stringify({
    stage, errorType: error?.constructor?.name ?? 'Error',
    applicationExited: application ? application.exitCode !== null : null,
    privateLogsRetained: false,
  }, null, 2) + '\n');
  console.error('Identity host verification failed. No private setup or raw browser/host logs are retained as public artifacts.');
  process.exitCode = 1;
} finally {
  await stop();
  if (providerContainer) {
    // DCP normally removes its provider. A bounded fallback targets this exact test mount only.
    const inspected = spawnSync('docker', ['inspect', providerContainer, '--format', '{{json .Mounts}}'], { encoding: 'utf8' });
    if (inspected.status === 0 && JSON.parse(inspected.stdout).some(mount => mount.Source === join(directory, 'import')))
      spawnSync('docker', ['rm', '-f', providerContainer], { stdio: 'ignore' });
  }
  await rm(parent, { recursive: true, force: true });
}
