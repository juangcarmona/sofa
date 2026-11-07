import { chromium } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { createHmac } from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { resolve } from 'node:path';
import assert from 'node:assert/strict';

const argumentsMap = new Map();
for (let index = 2; index < process.argv.length; index += 2) {
  const key = process.argv[index];
  if (!['--private-directory', '--evidence-directory', '--provider-container'].includes(key) || argumentsMap.has(key) || !process.argv[index + 1]) throw Error('Supply the private setup, safe evidence directory and owned development provider container.');
  argumentsMap.set(key, process.argv[index + 1]);
}
const directory = resolve(argumentsMap.get('--private-directory'));
const evidenceDirectory = resolve(argumentsMap.get('--evidence-directory'));
const container = argumentsMap.get('--provider-container');
if (!container || ! /^[A-Za-z0-9][A-Za-z0-9_.-]{0,127}$/.test(container)) throw Error('Select the owned SoFa development provider container.');
const configuration = JSON.parse(await readFile(resolve(directory, 'development.json'), 'utf8'));
const accounts = JSON.parse(await readFile(resolve(directory, 'test-accounts.json'), 'utf8'));
assert.equal(accounts.length, 2, 'Use isolated synthetic development accounts.');
const origin = new URL(configuration.SoFa.Origin).origin;
const issuer = new URL(configuration.SoFa.Identity.Development.Issuer);
const environment = Object.fromEntries((await readFile(resolve(directory, 'provider.env'), 'utf8')).trim().split('\n').map(line => {
  const separator = line.indexOf('='); return [line.slice(0, separator), line.slice(separator + 1)];
}));
const startedAt = new Date().toISOString();
const proofs = {};
let browser, activePage, paused = false, stage = 'startup';
await mkdir(evidenceDirectory, { recursive: true });
for (const signal of ['SIGINT', 'SIGTERM']) process.once(signal, async () => {
  if (paused) { execFileSync('docker', ['unpause', container], { stdio: 'ignore' }); paused = false; }
  await browser?.close(); process.exit(1);
});
const wait = milliseconds => new Promise(done => setTimeout(done, Math.max(0, milliseconds)));
const nextCounter = () => wait(30_100 - Date.now() % 30_000);

async function accessible(page, name) {
  const result = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa', 'wcag22aa']).analyze();
  assert.deepEqual(result.violations.map(violation => violation.id), [], name + ' accessibility');
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), name + ' horizontal layout');
}

async function accountUi(page) {
  let response;
  await page.route('**/api/connections/github', route => route.fulfill({ json: response }));
  try {
    for (const [name, state] of [
      ['unlinked', { state: 'unlinked' }], ['unavailable', { state: 'unavailable' }],
      ['linked', { state: 'linked', login: 'fixture-user', userId: 71, verifiedAt: new Date().toISOString(), repositories: [{ repository: 'example/repository', availability: 'available', issuesRead: true, actionsRead: true, issuesWrite: true, checkedAt: new Date().toISOString() }] }],
      ['insufficient', { state: 'linked', login: 'fixture-user', userId: 71, verifiedAt: new Date().toISOString(), repositories: [{ repository: 'example/repository', availability: 'denied', issuesRead: false, actionsRead: false, issuesWrite: false, checkedAt: new Date().toISOString() }] }],
    ]) {
      response = state;
      await page.reload();
      await page.waitForFunction(() => !document.getElementById('account-status').textContent.startsWith('Checking'));
      await accessible(page, name);
      await page.screenshot({ path: resolve(evidenceDirectory, 'account-' + name + '.png'), fullPage: true });
    }
    response = { state: 'linked', login: '<img src=x onerror=alert(1)>', userId: 71, verifiedAt: new Date().toISOString(), repositories: [] };
    await page.reload();
    await page.waitForFunction(() => document.getElementById('github-account').textContent.includes('<img'));
    assert.equal(await page.locator('#github-account img').count(), 0);
    await page.keyboard.press('Tab');
    assert.ok(await page.evaluate(() => document.activeElement instanceof HTMLAnchorElement || document.activeElement instanceof HTMLButtonElement));
  } finally { await page.unroute('**/api/connections/github'); await page.reload(); }
  proofs.sourceUiFixtures = proofs.accountAccessibility = proofs.keyboardNavigation = true;
}

function totp(secret, offset = 0) {
  let bits = '';
  for (const character of secret.replace(/[\s=]/g, '').toUpperCase()) {
    const value = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ234567'.indexOf(character);
    assert.ok(value >= 0); bits += value.toString(2).padStart(5, '0');
  }
  const key = Buffer.from(bits.match(/.{8}/g).map(value => Number.parseInt(value, 2)));
  const counter = Buffer.alloc(8);
  counter.writeBigUInt64BE(BigInt(Math.floor(Date.now() / 30_000) + offset));
  const digest = createHmac('sha1', key).update(counter).digest();
  return String((digest.readUInt32BE(digest.at(-1) & 15) & 0x7fffffff) % 1_000_000).padStart(6, '0');
}

async function signIn(page, account, secretFile, { wrongFactor = false, denied = false } = {}) {
  activePage = page;
  await page.goto(origin + '/');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await page.locator('#password').waitFor();
  if (await page.locator('#username').isVisible()) await page.locator('#username').fill(account.username);
  await page.locator('#password').fill(account.password);
  await page.locator('#kc-login').click();
  await page.locator('#totp, #otp').waitFor();
  assert.equal((await page.request.get(origin + '/api/session')).status(), 401);
  const enrollment = await page.locator('#totp').count() > 0;
  let secret;
  if (enrollment) {
    await page.locator('#mode-manual').click();
    secret = (await page.locator('#kc-totp-secret-key').innerText()).replace(/\s/g, '');
    await writeFile(resolve(directory, secretFile), JSON.stringify({ secret }), { mode: 0o600 });
    await page.locator('#userLabel').fill('isolated-browser-proof');
  } else secret = JSON.parse(await readFile(resolve(directory, secretFile), 'utf8')).secret;
  const input = enrollment ? '#totp' : '#otp', submit = enrollment ? '#saveTOTPBtn' : '#kc-login';
  if (wrongFactor) {
    const permitted = new Set([-1, 0, 1].map(offset => totp(secret, offset)));
    let wrong = '000000'; while (permitted.has(wrong)) wrong = String(Number(wrong) + 1).padStart(6, '0');
    await page.locator(input).fill(wrong);
    await page.locator(submit).click();
    assert.equal((await page.request.get(origin + '/api/session')).status(), 401);
    assert.equal(await page.locator(input).getAttribute('aria-invalid'), 'true');
    proofs.passwordOnlyDenied = proofs.wrongTotpDenied = true;
  }
  await page.locator(input).fill(totp(secret));
  await page.locator(submit).click();
  await page.waitForURL(url => url.origin === origin && url.pathname === (denied ? '/' : '/workspace'));
  assert.equal((await page.request.get(origin + '/api/session')).status(), denied ? 401 : 200);
}

async function session(page) {
  const response = await page.request.get(origin + '/api/session');
  assert.equal(response.status(), 200); return response.json();
}

async function revokeProviderSessions() {
  const response = await fetch(issuer.origin + '/realms/master/protocol/openid-connect/token', {
    method: 'POST', body: new URLSearchParams({ client_id: 'admin-cli', grant_type: 'password',
      username: environment.KC_BOOTSTRAP_ADMIN_USERNAME, password: environment.KC_BOOTSTRAP_ADMIN_PASSWORD }),
  });
  assert.equal(response.status, 200);
  const { access_token: access } = await response.json();
  const revoke = await fetch(issuer.origin + '/admin/realms/sofa-dev/users/' + encodeURIComponent(accounts[0].subject) + '/logout', {
    method: 'POST', headers: { Authorization: 'Bearer ' + access },
  });
  assert.equal(revoke.status, 204);
}

try {
  const labels = JSON.parse(execFileSync('docker', ['inspect', container, '--format', '{{json .Config.Labels}}'], { encoding: 'utf8' }));
  assert.equal(labels['sofa.scope'], 'issue-49-development', 'Only pause the owned isolated development provider.');
  const image = execFileSync('docker', ['inspect', container, '--format', '{{.Config.Image}}'], { encoding: 'utf8' }).trim();
  assert.ok(image.endsWith('@sha256:a78d6b79572168cb25886b80a8b5b8d4ed4390743e3eecb810538683e209dd6b'));
  browser = await chromium.launch({ headless: true, env: { ...process.env, HOME: resolve(directory, 'browser-home') } });
  const desktop = await browser.newContext({ viewport: { width: 1365, height: 900 } });
  const page = await desktop.newPage();
  await page.goto(origin + '/');
  await accessible(page, 'signed-out');
  await page.screenshot({ path: resolve(evidenceDirectory, 'signed-out.png'), fullPage: true });
  await nextCounter(); // Do not reuse a code consumed by a previous operator smoke test.
  stage = 'owner-mfa';
  await signIn(page, accounts[0], 'owner-totp.json', { wrongFactor: true });
  assert.equal((await page.request.post(origin + '/auth/logout')).status(), 400);
  assert.equal((await page.request.get(origin + '/api/session')).status(), 200);
  proofs.csrfRejected = true;
  await page.getByRole('button', { name: 'Sign out', exact: true }).click();
  assert.equal((await page.request.get(origin + '/api/session')).status(), 401);
  proofs.localLogout = true;
  await nextCounter();
  stage = 'fresh-ceremony-with-provider-sso';
  // The same browser retains provider SSO. Password plus TOTP must still be requested.
  await signIn(page, accounts[0], 'owner-totp.json');
  proofs.freshMfaDespiteProviderSso = true;
  await accountUi(page);
  const original = await session(page);
  const ownerCookies = await desktop.cookies(origin + '/workspace');
  assert.ok(ownerCookies.some(cookie => cookie.name === '__Host-SoFa.Owner' && cookie.secure && cookie.httpOnly && cookie.sameSite === 'Lax'));
  assert.equal(await page.evaluate(() => localStorage.length + sessionStorage.length), 0);
  await page.screenshot({ path: resolve(evidenceDirectory, 'owner-desktop.png'), fullPage: true });
  proofs.opaqueSecureCookie = proofs.browserStorageEmpty = proofs.trustedHttps = true;

  const phoneViewport = await browser.newContext({ viewport: { width: 390, height: 844 } });
  const phonePage = await phoneViewport.newPage();
  await nextCounter();
  stage = 'second-owner-session';
  await signIn(phonePage, accounts[0], 'owner-totp.json');
  await accessible(phonePage, 'phone viewport');
  const phoneOriginal = await session(phonePage);
  await phonePage.screenshot({ path: resolve(evidenceDirectory, 'owner-phone-viewport.png'), fullPage: true });
  console.log(JSON.stringify({ stage: 'owner-sessions-established', realProvider: true, physicalPhone: false }));

  const wrongBrowser = await browser.newContext();
  const wrongPage = await wrongBrowser.newPage();
  stage = 'wrong-owner';
  await signIn(wrongPage, accounts[1], 'other-totp.json', { denied: true });
  assert.equal((await wrongPage.request.get(origin + '/workspace')).status(), 401);
  await accessible(wrongPage, 'wrong owner');
  await wrongPage.screenshot({ path: resolve(evidenceDirectory, 'wrong-owner.png'), fullPage: true });
  proofs.wrongOwnerDenied = true;
  await wrongBrowser.close();

  stage = 'online-provider-refresh';
  await wait(Date.parse(original.providerValidatedAt) + 300_100 - Date.now());
  const refreshed = await session(page);
  assert.ok(Date.parse(refreshed.providerValidatedAt) > Date.parse(original.providerValidatedAt));
  assert.equal(refreshed.idleDeadline, original.idleDeadline);
  assert.equal(refreshed.absoluteDeadline, original.absoluteDeadline);
  await wait(Date.parse(phoneOriginal.providerValidatedAt) + 300_100 - Date.now());
  const phoneRefreshed = await session(phonePage);
  assert.ok(Date.parse(phoneRefreshed.providerValidatedAt) > Date.parse(phoneOriginal.providerValidatedAt));
  assert.equal(phoneRefreshed.idleDeadline, phoneOriginal.idleDeadline);
  assert.equal(phoneRefreshed.absoluteDeadline, phoneOriginal.absoluteDeadline);
  proofs.onlineRefresh = proofs.pollingDoesNotRenew = true;
  console.log(JSON.stringify({ stage: 'real-online-refresh-verified', originalDeadlinesPreserved: true }));

  await revokeProviderSessions();
  stage = 'recognized-provider-revocation';
  await wait(Date.parse(refreshed.providerValidatedAt) + 300_100 - Date.now());
  assert.equal((await page.request.get(origin + '/api/session')).status(), 401);
  proofs.providerRevocationDenied = true;
  execFileSync('docker', ['pause', container], { stdio: 'ignore' }); paused = true;
  stage = 'provider-outage';
  await wait(Date.parse(phoneRefreshed.providerValidatedAt) + 300_100 - Date.now());
  assert.equal((await phonePage.request.get(origin + '/api/session')).status(), 503);
  await phonePage.goto(origin + '/');
  await phonePage.getByRole('button', { name: 'Sign out locally', exact: true }).click();
  assert.equal((await phonePage.request.get(origin + '/api/session')).status(), 401);
  proofs.outageDenied = proofs.localLogoutDuringOutage = true;
  const receipt = { status: 'passed', startedAt, completedAt: new Date().toISOString(), proofs,
    provider: 'Keycloak 26.8.1', syntheticAccounts: true, physicalPhone: false, githubUserConsentVerified: false };
  await writeFile(resolve(evidenceDirectory, 'provider-verification.json'), JSON.stringify(receipt, null, 2) + '\n');
  console.log(JSON.stringify(receipt));
} catch (error) {
  // Browser error call logs can include live cookies and callback queries. Never publish them.
  await writeFile(resolve(directory, 'provider-verification-error.txt'), JSON.stringify({ errorType: error.name, stage, assertion: error.code === 'ERR_ASSERTION', sourceLocations: String(error.stack).split('\n').filter(line => /verify-identity-provider\.mjs:\d+:\d+/.test(line)).map(line => line.match(/verify-identity-provider\.mjs:\d+:\d+/)[0]) }), { mode: 0o600 });
  console.log(JSON.stringify({ status: 'failed', stage, location: activePage ? new URL(activePage.url()).origin + new URL(activePage.url()).pathname : null, fields: activePage ? await activePage.locator('input').evaluateAll(elements => elements.map(element => ({ id: element.id, name: element.name, type: element.type }))) : [], diagnostic: 'Private provider-verification-error.txt; redact credentials before inspection.' }));
  process.exitCode = 1;
} finally {
  if (paused) execFileSync('docker', ['unpause', container], { stdio: 'ignore' });
  await browser?.close();
}
