import assert from 'node:assert/strict';
import { createHash, randomBytes } from 'node:crypto';
import { spawnSync } from 'node:child_process';
import { mkdir, mkdtemp, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { test } from 'node:test';
import { zipSync, strToU8 } from 'fflate';
import { extractScanner, installScanner, scannerEnvironment, scanRepository, verifyArchive, version } from '../../src/tooling/security/check-secrets.mjs';

test('scanner verifies archive bytes and rejects a changed download', () => {
  const bytes = Buffer.from('synthetic archive');
  const expected = createHash('sha256').update(bytes).digest('hex');
  verifyArchive(bytes, expected);
  assert.throws(() => verifyArchive(Buffer.from('changed archive'), expected), /checksum mismatch/);
});

test('scanner subprocesses receive OS paths but no credentials or scanner overrides', () => {
  const environment = scannerEnvironment({ Path: '/bin', SystemRoot: '/system', HOME: '/home',
    GITHUB_TOKEN: 'synthetic', CLOUDFLARE_API_TOKEN: 'synthetic', FIREBASE_TOKEN: 'synthetic',
    GITLEAKS_CONFIG: 'untrusted-config' });
  assert.deepEqual(environment, { Path: '/bin', SystemRoot: '/system', HOME: '/home' });
});

test('unborn repository still scans the working tree with redaction', () => {
  const calls = [];
  scanRepository('scanner', process.cwd(), (binary, args) => {
    calls.push({ binary, args });
    return { status: 0, stdout: args[0] === 'rev-list' ? '0' : '' };
  }, () => {});
  assert.equal(calls.length, 3);
  assert.equal(calls[2].args[0], 'dir');
  assert.ok(calls[2].args.includes('--redact=100'));
  assert.ok(calls[2].args.includes('--ignore-gitleaks-allow'));
});

test('all-ref history failure blocks success but still scans the working tree', () => {
  const calls = [];
  assert.throws(() => scanRepository('scanner', process.cwd(), (binary, args) => {
    calls.push(args);
    return { status: args[0] === 'git' ? 1 : 0, stdout: args[0] === 'rev-list' ? '2' : '' };
  }, () => {}), /Secret scanning failed/);
  assert.equal(calls.length, 4);
  assert.ok(calls[1].includes('--log-opts=--all --full-history'));
  assert.equal(calls[3][0], 'dir');
});

test('staged changes are scanned without printing their content', () => {
  const calls = [];
  scanRepository('scanner', process.cwd(), (binary, args, options) => {
    calls.push({ args, options });
    return { status: 0, stdout: args[0] === 'rev-list' ? '0' : args[0] === 'diff' ? 'synthetic diff' : '' };
  }, () => {});
  assert.equal(calls[2].args[0], 'stdin');
  assert.equal(calls[2].options.input, 'synthetic diff');
  assert.ok(calls[2].args.includes('--redact=100'));
});

test('scanner execution errors and unknown history fail closed', () => {
  assert.throws(() => scanRepository('missing', process.cwd(), () => ({ status: null, error: new Error('missing') })), /Cannot inspect/);
  assert.throws(() => scanRepository('scanner', process.cwd(), () => ({ status: 0, stdout: 'unknown' })), /Invalid repository/);
  assert.throws(() => scanRepository('missing', process.cwd(), (binary) => binary === 'git'
    ? { status: 0, stdout: '0' }
    : { status: null, error: new Error('missing') }, () => {}), /Secret scanning failed/);
});

test('real scanner detects an ignored synthetic secret and redacts reports', async () => {
  const directory = await mkdtemp(join(tmpdir(), 'sofa-scan-test-'));
  try {
    const binary = await installScanner();
    const token = ['ghp', randomBytes(18).toString('hex')].join('_');
    const report = join(directory, 'report.json');
    await writeFile(join(directory, '.env.local'), `GITHUB_TOKEN=${token}\n`);
    await writeFile(join(directory, '.gitignore'), '.env.local\n');
    const result = spawnSync(binary, ['dir', directory, '--config', join(process.cwd(), '.gitleaks.toml'),
      '--redact=100', '--no-banner', '--report-format=json', '--report-path', report],
      { encoding: 'utf8', env: scannerEnvironment() });
    assert.equal(result.status, 1);
    const findings = await readFile(report, 'utf8');
    assert.ok(JSON.parse(findings).some((finding) => finding.File.endsWith('.env.local')));
    assert.ok(!findings.includes(token));
    assert.ok(!`${result.stdout}${result.stderr}`.includes(token));
  } finally {
    await rm(directory, { recursive: true, force: true });
  }
});
async function fixture(t, { bytes, checksum } = {}) {
  const directory = await mkdtemp(join(tmpdir(), 'sofa-installer-test-'));
  t.after(() => rm(directory, { recursive: true, force: true }));
  const archive = bytes || Buffer.from(zipSync({ 'nested/gitleaks.exe': strToU8('synthetic executable') }));
  const digest = checksum || createHash('sha256').update(archive).digest('hex');
  const urls = [];
  const options = {
    platform: 'win32', arch: 'x64',
    fetcher: async (url) => {
      urls.push(url);
      return { ok: true, arrayBuffer: async () => url.endsWith('_checksums.txt')
        ? Buffer.from(`${digest}  gitleaks_${version}_windows_x64.zip\n`) : archive };
    },
    run: (binary, args, options) => {
      assert.deepEqual(args, ['version']);
      assert.ok(!('GITHUB_TOKEN' in options.env));
      return { status: 0, stdout: version };
    },
  };
  return { directory, options, urls };
}

test('corrupted archive is rejected with the underlying ZIP extraction error', async (t) => {
  const { directory, options } = await fixture(t, { bytes: Buffer.from('not a ZIP') });
  await assert.rejects(installScanner(directory, options), /Gitleaks extraction failed.*(?:invalid zip|invalid ZIP)/);
  assert.deepEqual(await readdir(join(directory, 'gitleaks', version)), []);
});

test('official checksum mismatch stops installation before extraction', async (t) => {
  const { directory, options, urls } = await fixture(t, { checksum: '0'.repeat(64) });
  let extracted = false;
  options.extract = async () => { extracted = true; };
  await assert.rejects(installScanner(directory, options), /checksum mismatch/);
  assert.equal(extracted, false);
  assert.ok(urls[0].endsWith(`gitleaks_${version}_checksums.txt`));
  assert.deepEqual(await readdir(join(directory, 'gitleaks', version)), []);
});

test('valid cached binary is version-checked and reused without downloading', async (t) => {
  const { directory, options, urls } = await fixture(t);
  const binary = await installScanner(directory, options);
  assert.equal(await readFile(binary, 'utf8'), 'synthetic executable');
  options.fetcher = () => { throw new Error('Cache must not download'); };
  assert.equal(await installScanner(directory, options), binary);
  assert.equal(urls.length, 2);
});

test('incomplete and corrupt previous installs are safely replaced', async (t) => {
  const { directory, options } = await fixture(t);
  const destination = join(directory, 'gitleaks', version, 'win32-x64');
  await mkdir(destination, { recursive: true });
  await writeFile(join(destination, 'gitleaks.exe'), 'partial binary');
  const binary = await installScanner(directory, options);
  assert.equal(await readFile(binary, 'utf8'), 'synthetic executable');
  await writeFile(binary, 'corrupt cached binary');
  assert.equal(await installScanner(directory, options), binary);
  assert.equal(await readFile(binary, 'utf8'), 'synthetic executable');
});

test('cached or downloaded wrong-version binaries cannot be accepted', async (t) => {
  const { directory, options } = await fixture(t);
  const binary = await installScanner(directory, options);
  options.run = () => ({ status: 0, stdout: '0.0.0' });
  await assert.rejects(installScanner(directory, options), /not version/);
  assert.equal(await readFile(binary, 'utf8'), 'synthetic executable');
});

test('simultaneous installers publish only one verified cache', async (t) => {
  const { directory, options, urls } = await fixture(t);
  const [a, b] = await Promise.all([installScanner(directory, options), installScanner(directory, options)]);
  assert.equal(a, b);
  assert.equal(urls.length, 2);
  assert.deepEqual(await readdir(join(directory, 'gitleaks', version)), ['win32-x64']);
});

test('extraction failure retains process exit and stderr diagnostics', async (t) => {
  const { directory } = await fixture(t);
  await assert.rejects(extractScanner('synthetic.tar.gz', join(directory, 'out'), 'linux',
    () => ({ status: 2, stderr: 'tar: Unexpected EOF in archive' })), /tar exit 2: tar: Unexpected EOF in archive/);
  await assert.rejects(extractScanner('synthetic.tar.gz', join(directory, 'out'), 'linux',
    () => ({ status: null, error: new Error('spawn tar ENOENT') })), /spawn tar ENOENT/);
});

test('failed installation never runs or reports a successful security scan', async (t) => {
  const { directory, options } = await fixture(t, { checksum: '0'.repeat(64) });
  const logs = [];
  let scanned = false;
  await assert.rejects((async () => {
    const binary = await installScanner(directory, options);
    scanRepository(binary, directory, () => { scanned = true; return { status: 0, stdout: '0' }; }, (line) => logs.push(line));
  })(), /checksum mismatch/);
  assert.equal(scanned, false);
  assert.deepEqual(logs, []);
});

test('a busy installation lock fails closed without deleting another owner', async (t) => {
  const { directory, options } = await fixture(t);
  const lock = join(directory, 'gitleaks', version, 'win32-x64.lock');
  await mkdir(lock, { recursive: true });
  await assert.rejects(installScanner(directory, { ...options, lockTimeout: 0 }), /install lock timed out/);
  assert.deepEqual(await readdir(join(directory, 'gitleaks', version)), ['win32-x64.lock']);
});
