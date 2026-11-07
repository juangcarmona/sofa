import { readFile, writeFile, mkdir, realpath } from 'node:fs/promises';
import { execFileSync } from 'node:child_process';
import { randomBytes, randomUUID } from 'node:crypto';
import { isIP } from 'node:net';
import { resolve, dirname, basename, sep, isAbsolute } from 'node:path';
import { fileURLToPath } from 'node:url';

process.umask(0o077);
const values = new Map();
for (let index = 2; index < process.argv.length; index++) {
  const key = process.argv[index];
  if (!['--directory', '--origin', '--issuer', '--owner-subject', '--test-accounts'].includes(key) || values.has(key)) throw Error('Unknown or duplicate setup option.');
  values.set(key, key === '--test-accounts' ? true : process.argv[++index]);
}
function address(key, issuer = false) {
  const url = new URL(values.get(key));
  if (url.protocol !== 'https:' || url.username || url.password || url.search || url.hash
    || url.pathname !== (issuer ? '/realms/sofa-dev' : '/')) throw Error('Use exact HTTPS application and sofa-dev issuer addresses.');
  return url;
}
const origin = address('--origin');
const issuer = address('--issuer', true);
if (!values.get('--directory') || !isAbsolute(values.get('--directory'))) throw Error('Select an absolute private directory outside the repository.');
const directory = resolve(await realpath(dirname(resolve(values.get('--directory')))), basename(values.get('--directory')));
const repository = await realpath(fileURLToPath(new URL('..', import.meta.url)));
if (directory === repository || directory.startsWith(repository + sep)) throw Error('Keep development secrets outside the checkout.');
const testAccounts = values.get('--test-accounts') === true;
if (typeof process.getuid !== 'function' || process.getuid() === 0) throw Error('Create the private development setup as a non-root POSIX user.');
if (!testAccounts && !values.get('--owner-subject')) throw Error('Supply the exact provider owner subject, or request isolated synthetic test accounts.');
await mkdir(directory, { mode: 0o700 }); // Refuse to overwrite an existing private setup.
const tls = resolve(directory, 'tls');
await mkdir(tls, { mode: 0o700 });
const privateWrite = (path, data) => writeFile(path, data, { mode: 0o600, flag: 'wx' });
const secret = () => randomBytes(32).toString('base64url');
const ownerSubject = testAccounts ? randomUUID() : values.get('--owner-subject');
const clientSecret = secret();
const realm = JSON.parse(await readFile(new URL('../src/identity/development/realm.template.json', import.meta.url), 'utf8'));
realm.clients[0].secret = clientSecret;
realm.clients[0].redirectUris = [new URL('/signin-oidc', origin).href];
realm.clients[0].attributes['post.logout.redirect.uris'] = origin.href;
const accounts = testAccounts ? [
  { subject: ownerSubject, username: 'development-owner', password: secret() },
  { subject: randomUUID(), username: 'development-other', password: secret() },
] : [];
if (testAccounts) realm.users = accounts.map(account => ({
  id: account.subject, username: account.username, enabled: true,
  firstName: 'Development', lastName: 'Account', email: `${account.username}@example.invalid`,
  credentials: [{ type: 'password', value: account.password, temporary: false }], requiredActions: ['CONFIGURE_TOTP'],
}));
await mkdir(resolve(directory, 'import'), { mode: 0o700 });
await privateWrite(resolve(directory, 'import', 'sofa-dev-realm.json'), JSON.stringify(realm));
await privateWrite(resolve(directory, 'test-accounts.json'), JSON.stringify(accounts));
const openssl = args => execFileSync('openssl', args, { stdio: 'ignore' });
const caKey = resolve(directory, 'ca.key');
const ca = resolve(directory, 'ca.crt');
openssl(['req', '-x509', '-newkey', 'rsa:3072', '-nodes', '-days', '90', '-keyout', caKey, '-out', ca,
  '-subj', '/CN=SoFa isolated development CA', '-addext', 'basicConstraints=critical,CA:TRUE', '-addext', 'keyUsage=critical,keyCertSign,cRLSign']);
for (const [name, url] of [['app', origin], ['identity', issuer]]) {
  const key = resolve(tls, name + '.key');
  const csr = resolve(tls, name + '.csr');
  const cert = resolve(tls, name + '.crt');
  const extensions = resolve(tls, name + '.extensions');
  await privateWrite(extensions, `basicConstraints=critical,CA:FALSE\nkeyUsage=critical,digitalSignature,keyEncipherment\nextendedKeyUsage=serverAuth\nsubjectAltName=${isIP(url.hostname) ? 'IP' : 'DNS'}:${url.hostname}\n`);
  openssl(['req', '-new', '-newkey', 'rsa:3072', '-nodes', '-keyout', key, '-out', csr, '-subj', '/CN=SoFa development ' + name]);
  openssl(['x509', '-req', '-in', csr, '-CA', ca, '-CAkey', caKey, '-set_serial', '0x' + randomBytes(16).toString('hex'),
    '-days', '30', '-out', cert, '-extfile', extensions]);
}
await privateWrite(resolve(directory, 'trust-bundle.pem'), await readFile('/etc/ssl/certs/ca-certificates.crt', 'utf8') + '\n' + await readFile(ca, 'utf8'));
await privateWrite(resolve(directory, 'development.json'), JSON.stringify({
  Kestrel: { Endpoints: { Https: { Url: origin.origin, Certificate: { Path: resolve(tls, 'app.crt'), KeyPath: resolve(tls, 'app.key') } } } },
  SoFa: { Origin: origin.origin, Identity: { Mode: 'Development', Development: {
    ProviderUser: `${process.getuid()}:0`, Issuer: issuer.href, ClientId: 'sofa-development', ClientSecret: clientSecret, OwnerSubject: ownerSubject,
  } }, Repositories: ['juangcarmona/lighthouse', 'juangcarmona/sofa'] },
}, null, 2));
await privateWrite(resolve(directory, 'provider.env'), [
  'KC_BOOTSTRAP_ADMIN_USERNAME=development-admin', 'KC_BOOTSTRAP_ADMIN_PASSWORD=' + secret(),
  'KC_HOSTNAME=' + issuer.origin, 'KC_HTTP_ENABLED=false',
  'KC_HTTPS_CERTIFICATE_FILE=/run/sofa-tls/identity.crt', 'KC_HTTPS_CERTIFICATE_KEY_FILE=/run/sofa-tls/identity.key',
  'KC_LOG_LEVEL=warn',
].join('\n') + '\n');
console.log(JSON.stringify({ directory, origin: origin.origin, issuer: issuer.href, testAccounts,
  instructions: 'Private setup created. Trust ca.crt only in the designated development browser/device. Credentials remain in private files; no global trust was changed.' }));
