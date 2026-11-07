import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, stat, rm } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import { spawnSync } from 'node:child_process';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { randomUUID } from 'node:crypto';

const root = resolve(import.meta.dirname, '..');
function setup(directory) {
  return spawnSync(process.execPath, ['scripts/setup-development-identity.mjs', '--directory', directory,
    '--origin', 'https://127.0.0.1:5443', '--issuer', 'https://127.0.0.1:8443/realms/sofa-dev', '--test-accounts'], { cwd: root, encoding: 'utf8' });
}

test('development setup keeps credentials private, emits trusted certificates and refuses overwriting an existing setup', async () => {
  const parent = await mkdtemp(join(tmpdir(), 'sofa-private-identity-'));
  const directory = join(parent, 'identity');
  try {
    const result = setup(directory);
    assert.equal(result.status, 0);
    const original = await readFile(join(directory, 'development.json'), 'utf8');
    const configuration = JSON.parse(original);
    const accounts = JSON.parse(await readFile(join(directory, 'test-accounts.json'), 'utf8'));
    assert.equal(accounts.length, 2);
    assert.equal(configuration.SoFa.Identity.Development.ProviderUser, `${process.getuid()}:0`);
    assert.equal(configuration.SoFa.Identity.Development.OwnerSubject, accounts[0].subject);
    assert.ok(!result.stdout.includes(configuration.SoFa.Identity.Development.ClientSecret));
    for (const account of accounts) {
      assert.ok(account.password.length >= 32);
      assert.ok(!result.stdout.includes(account.password));
    }
    assert.equal((await stat(directory)).mode & 0o777, 0o700);
    for (const file of ['development.json', 'provider.env', 'test-accounts.json', 'ca.key', 'tls/app.key', 'tls/identity.key'])
      assert.equal((await stat(join(directory, file))).mode & 0o777, 0o600);
    const chain = spawnSync('openssl', ['verify', '-CAfile', join(directory, 'ca.crt'), join(directory, 'tls/app.crt'), join(directory, 'tls/identity.crt')], { encoding: 'utf8' });
    assert.equal(chain.status, 0);
    assert.equal(setup(directory).status === 0, false);
    assert.equal(await readFile(join(directory, 'development.json'), 'utf8'), original);
  } finally { await rm(parent, { recursive: true, force: true }); }
});

test('development setup refuses a checkout or relative destination before creating credentials', async () => {
  const name = '.sofa-identity-safety-' + randomUUID();
  const target = join(root, name);
  try {
    assert.notEqual(setup(target).status, 0);
    assert.equal(existsSync(target), false);
    assert.notEqual(setup(name).status, 0);
    assert.equal(existsSync(target), false);
  } finally { await rm(target, { recursive: true, force: true }); }
});
