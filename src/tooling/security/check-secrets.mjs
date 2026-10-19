import { createHash } from 'node:crypto';
import { spawnSync } from 'node:child_process';
import { chmod, copyFile, mkdir, mkdtemp, readFile, readdir, rename, rm, writeFile } from 'node:fs/promises';
import { homedir } from 'node:os';
import { basename, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { setTimeout as delay } from 'node:timers/promises';
import { unzipSync } from 'fflate';

// Avoid 8.30.1's reported detection regression: github.com/gitleaks/gitleaks/issues/2170.
export const version = '8.29.1';
const archives = {
  'linux-x64': `gitleaks_${version}_linux_x64.tar.gz`,
  'win32-x64': `gitleaks_${version}_windows_x64.zip`,
};
export const cacheRoot = process.platform === 'win32'
  ? join(process.env.LOCALAPPDATA || join(homedir(), 'AppData', 'Local'), 'SoFa', 'Cache')
  : join(homedir(), '.cache', 'sofa');

export function verifyArchive(bytes, expected) {
  if (!/^[a-f0-9]{64}$/.test(expected) || createHash('sha256').update(bytes).digest('hex') !== expected) {
    throw new Error('Gitleaks archive checksum mismatch');
  }
}

export function scannerEnvironment(source = process.env) {
  const allowed = new Set(['PATH', 'SYSTEMROOT', 'WINDIR', 'HOME', 'USERPROFILE', 'HOMEDRIVE',
    'HOMEPATH', 'TEMP', 'TMP', 'COMSPEC', 'PATHEXT', 'LANG', 'LC_ALL']);
  return Object.fromEntries(Object.entries(source).filter(([name]) => allowed.has(name.toUpperCase())));
}

async function download(url, fetcher) {
  const response = await fetcher(url, { signal: AbortSignal.timeout(120_000) });
  if (!response.ok) throw new Error(`Gitleaks download failed: HTTP ${response.status} (${url})`);
  return Buffer.from(await response.arrayBuffer());
}

export async function extractScanner(path, directory, platform = process.platform, run = spawnSync) {
  const executable = platform === 'win32' ? 'gitleaks.exe' : 'gitleaks';
  try {
    await mkdir(directory, { recursive: true });
    if (platform === 'win32') {
      // Extract only the executable to a controlled path, never ZIP entry paths.
      const files = unzipSync(await readFile(path), { filter: (entry) => basename(entry.name.replaceAll('\\', '/')) === executable });
      const matches = Object.entries(files);
      if (matches.length !== 1) throw new Error(`Expected one ${executable} in ZIP, found ${matches.length}`);
      const binary = join(directory, executable);
      await writeFile(binary, matches[0][1], { flag: 'wx', mode: 0o700 });
      return binary;
    }
    const result = run('tar', ['-xf', path, '-C', directory], { encoding: 'utf8', env: scannerEnvironment() });
    if (result.error || result.status !== 0) {
      throw new Error(result.error?.message || `tar exit ${result.status}: ${String(result.stderr).trim().slice(0, 4096)}`);
    }
    const found = [];
    async function find(folder) {
      for (const entry of await readdir(folder, { withFileTypes: true })) {
        const child = join(folder, entry.name);
        if (entry.isDirectory()) await find(child);
        else if (entry.isFile() && entry.name === executable) found.push(child);
      }
    }
    await find(directory);
    if (found.length !== 1) throw new Error(`Expected one ${executable} in archive, found ${found.length}`);
    return found[0];
  } catch (error) {
    throw new Error(`Gitleaks extraction failed (${platform}, ${path} -> ${directory}): ${error.message}`, { cause: error });
  }
}

function expectedVersion(binary, run) {
  const result = run(binary, ['version'], { encoding: 'utf8', env: scannerEnvironment(), timeout: 10_000 });
  return !result.error && result.status === 0 && String(result.stdout).trim().replace(/^v/, '') === version;
}

async function validCache(binary, run) {
  try {
    const receipt = JSON.parse(await readFile(join(binary, '..', 'install.json'), 'utf8'));
    verifyArchive(await readFile(binary), receipt.binarySha256);
    return receipt.version === version && expectedVersion(binary, run);
  } catch { return false; }
}

export async function installScanner(base = cacheRoot, {
  platform = process.platform, arch = process.arch, fetcher = fetch, run = spawnSync,
  extract = extractScanner, lockTimeout = 130_000,
} = {}) {
  const key = `${platform}-${arch}`;
  const archive = archives[key];
  if (!archive) throw new Error('Security scanning supports Linux x64 and Windows x64');
  const parent = join(base, 'gitleaks', version);
  const destination = join(parent, key);
  const binary = join(destination, platform === 'win32' ? 'gitleaks.exe' : 'gitleaks');
  const lock = join(parent, `${key}.lock`);
  await mkdir(parent, { recursive: true, mode: 0o700 });
  const deadline = Date.now() + lockTimeout;
  // Exclusive directory ownership serializes publishers across Node processes.
  // A crashed owner's lock fails closed; never delete a possibly live install lock.
  for (;;) {
    try { await mkdir(lock); break; }
    catch (error) {
      if (error.code !== 'EEXIST') throw error;
      if (Date.now() >= deadline) throw new Error(`Gitleaks install lock timed out: ${lock}; remove only after confirming no installer is running`);
      await delay(50);
    }
  }
  let staging;
  try {
    if (await validCache(binary, run)) return binary;
    staging = await mkdtemp(join(parent, `${key}.tmp-`));
    const url = `https://github.com/gitleaks/gitleaks/releases/download/v${version}/`;
    const checksums = (await download(`${url}gitleaks_${version}_checksums.txt`, fetcher)).toString('utf8');
    const matches = checksums.split(/\r?\n/).map((line) => line.trim().split(/\s+/))
      .filter(([, name]) => name === archive);
    if (matches.length !== 1 || !/^[a-f0-9]{64}$/.test(matches[0][0])) throw new Error(`Gitleaks checksum entry missing or ambiguous: ${archive}`);
    const bytes = await download(`${url}${archive}`, fetcher);
    verifyArchive(bytes, matches[0][0]);
    const archivePath = join(staging, archive);
    await writeFile(archivePath, bytes, { flag: 'wx' });
    const extractedDirectory = join(staging, 'extracted');
    const executable = await extract(archivePath, extractedDirectory, platform, run);
    if (!expectedVersion(executable, run)) throw new Error(`Gitleaks installed binary is not version ${version}`);
    const stagedBinary = join(staging, basename(binary));
    await copyFile(executable, stagedBinary);
    if (platform !== 'win32') await chmod(stagedBinary, 0o700);
    await writeFile(join(staging, 'install.json'), JSON.stringify({ version, binarySha256: createHash('sha256').update(await readFile(stagedBinary)).digest('hex') }));
    await rm(extractedDirectory, { recursive: true, force: true });
    await rm(archivePath);
    await rm(destination, { recursive: true, force: true });
    await rename(staging, destination);
    staging = undefined;
    return binary;
  } finally {
    try { if (staging) await rm(staging, { recursive: true, force: true }); }
    finally { await rm(lock, { recursive: true, force: true }); }
  }
}

export function scanRepository(binary, root, run = spawnSync, log = console.log) {
  const options = { cwd: root, env: scannerEnvironment(), stdio: 'inherit' };
  const common = ['--config', join(root, '.gitleaks.toml'), '--redact=100', '--no-banner', '--ignore-gitleaks-allow'];
  const history = run('git', ['rev-list', '--all', '--count'], { ...options, stdio: 'pipe' });
  if (history.error || history.status !== 0) throw new Error('Cannot inspect repository history');
  const count = String(history.stdout).trim();
  if (!/^\d+$/.test(count)) throw new Error('Invalid repository history count');
  let failed = false;
  if (Number(count) > 0) {
    const result = run(binary, ['git', root, ...common, '--log-opts=--all --full-history'], options);
    if (result.error || result.status !== 0) failed = true;
  } else {
    log('History scan: N/A (repository has no commits)');
  }
  const staged = run('git', ['diff', '--cached', '--no-ext-diff', '--no-textconv', '--unified=0'],
    { ...options, stdio: 'pipe', maxBuffer: 32 * 1024 * 1024 });
  if (staged.error || staged.status !== 0) throw new Error('Cannot inspect staged changes');
  if (staged.stdout.length > 0) {
    const result = run(binary, ['stdin', ...common], { ...options, input: staged.stdout });
    if (result.error || result.status !== 0) failed = true;
  }
  const result = run(binary, ['dir', root, ...common], options);
  if (result.error || result.status !== 0) failed = true;
  if (failed) throw new Error('Secret scanning failed; review redacted diagnostics');
  log('Index and working-tree scans passed (including untracked and ignored environment/log files)');
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    const root = fileURLToPath(new URL('../../../', import.meta.url));
    scanRepository(await installScanner(), root);
  } catch (error) {
    console.error(error.message);
    process.exitCode = 1;
  }
}
