import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { readFile } from 'node:fs/promises';

const origin = `http://127.0.0.1:${process.env.SOFA_ISSUE_FIXTURE_PORT || '5171'}`;
async function fixture(page) {
  let ideas = [], deny = false, lost = false;
  const operations = [];
  await page.route('**/*', async route => {
    const url = new URL(route.request().url());
    const asset = new Map([['/workspace/ideas', 'ideas.html'], ['/ideas.js', 'ideas.js'], ['/ideas.css', 'ideas.css']]).get(url.pathname);
    if (asset) return route.fulfill({ body: await readFile(new URL(`../../src/web/public/${asset}`, import.meta.url)), contentType: asset.endsWith('html') ? 'text/html' : asset.endsWith('js') ? 'text/javascript' : 'text/css' });
    if (!url.pathname.startsWith('/api/')) return route.continue();
    if (url.pathname === '/api/session') return route.fulfill({ json: { authenticated: true, displayName: 'Fixture operator' } });
    if (url.pathname === '/api/connections/github') return route.fulfill({ json: { state: 'linked', login: 'verified-user', userId: 71, dataScope: 'idea-fixture', repositories: [] } });
    if (url.pathname === '/api/controls/csrf') return route.fulfill({ json: { token: 'fixture-csrf' } });
    const response = { availability: 'available', ideas, repositories: ['example/repository'] };
    if (route.request().method() === 'POST') {
      const input = route.request().postDataJSON(); operations.push(input.operation);
      expect(route.request().headers()['x-sofa-csrf']).toBe('fixture-csrf');
      let draft = ideas.find(value => value.id === input.id);
      if (input.operation === 'save') {
        draft = { ...input, id: input.id ?? 'a'.repeat(32), revision: (draft?.revision ?? 0) + 1, state: 'draft' };
        ideas = [draft];
      } else if (input.operation === 'review') {
        draft.state = 'reviewed'; draft.revision++;
        response.related = [{ number: 9, title: '<img src=x onerror=alert(1)> related', equivalent: false }];
        response.preview = `## Intended outcome\n\n${draft.outcome}\n\n## Scope\n\n${draft.scope}`; response.actingUser = 'verified-user';
        response.reason = 'Nothing has been sent. Inspect candidates and exact content before confirming creation.';
      } else if (input.operation === 'create' && deny) {
        response.availability = 'denied'; response.reason = 'Current GitHub user and App issue-write permission is required.';
        return route.fulfill({ status: 403, json: { ...response, ideas } });
      } else if (input.operation === 'create' && lost) {
        draft.state = 'uncertain'; draft.revision++; return route.abort();
      } else if (['create', 'reconcile', 'link'].includes(input.operation)) {
        draft.state = input.operation === 'link' ? 'linked' : 'created'; draft.revision++;
        draft.source = { repositoryId: 22, issueId: 70, number: input.number ?? 7, url: 'https://github.com/example/repository/issues/7' };
      } else if (input.operation === 'abandon') { draft.state = 'abandoned'; draft.revision++; }
    }
    return route.fulfill({ json: { ...response, ideas } });
  });
  return { operations, deny() { deny = true; }, lose() { lost = true; } };
}
async function draft(page) {
  await page.getByLabel('Title', { exact: true }).fill('Useful idea');
  await page.getByLabel('Repository', { exact: true }).selectOption('example/repository');
  await page.getByLabel('Intended outcome', { exact: true }).fill('Retain <script>literal text</script>');
  await page.getByLabel('Scope', { exact: true }).fill('A bounded change');
  await page.getByRole('button', { name: 'Save draft', exact: true }).click();
  await expect(page.locator('#idea-list')).toContainText('Useful idea');
}
for (const width of [1280, 390]) for (const theme of ['light', 'dark']) {
  test(`ideas visual contract ${width} ${theme}: explicit review create and retained receipt`, async ({ page }, info) => {
    await page.setViewportSize({ width, height: 900 }); const source = await fixture(page);
    await page.goto(origin + '/workspace/ideas');
    await page.getByRole('button', { name: `${theme[0].toUpperCase() + theme.slice(1)} appearance`, exact: true }).click();
    await draft(page);
    await page.getByRole('button', { name: 'Review creation', exact: true }).click();
    await expect(page.locator('#idea-review-panel')).toBeVisible();
    await expect(page.locator('#idea-preview')).toContainText('<script>literal text</script>');
    await expect(page.locator('#idea-related img, #idea-preview script')).toHaveCount(0);
    await expect(page.getByRole('button', { name: 'Confirm creation', exact: true })).toBeDisabled();
    expect(source.operations).toEqual(['save', 'review']);
    await page.screenshot({ path: info.outputPath(`ideas-review-${width}-${theme}.png`), fullPage: true });
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
    await page.locator('#idea-checked').focus(); await page.keyboard.press('Space');
    await page.getByRole('button', { name: 'Confirm creation', exact: true }).click();
    await expect(page.getByRole('link', { name: /Open confirmed issue/ })).toBeVisible();
    await page.reload(); await expect(page.getByRole('button', { name: /Useful idea.*created/ })).toBeVisible();
    expect(source.operations).toEqual(['save', 'review', 'create']);
  });
}
test('read-only users inspect candidates before linking; linking sends no write', async ({ page }) => {
  const source = await fixture(page); source.deny(); await page.goto(origin + '/workspace/ideas'); await draft(page);
  const link = page.getByRole('button', { name: 'Link existing issue', exact: true });
  await expect(link).toBeDisabled();
  await page.getByRole('button', { name: 'Review creation', exact: true }).click();
  await expect(page.locator('#idea-review-panel')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Link #9', exact: true })).toBeVisible();
  await expect(link).toBeEnabled();
  await page.getByRole('button', { name: 'Link #9', exact: true }).click();
  await expect(page.getByRole('link', { name: /Open confirmed issue.*#9/ })).toBeVisible();
  expect(source.operations).toEqual(['save', 'review', 'link']);
});
test('ideas lost response refresh reconciles without sending another create', async ({ page }) => {
  const source = await fixture(page); source.lose(); await page.goto(origin + '/workspace/ideas'); await draft(page);
  await page.getByRole('button', { name: 'Review creation', exact: true }).click(); await page.locator('#idea-checked').check();
  await page.getByRole('button', { name: 'Confirm creation', exact: true }).click();
  await expect(page.locator('#idea-status')).toContainText('Creation response was lost');
  await page.getByRole('button', { name: 'Refresh drafts', exact: true }).click();
  await page.getByRole('button', { name: 'Reconcile uncertain creation', exact: true }).click();
  await expect(page.getByRole('link', { name: /Open confirmed issue/ })).toBeVisible();
  expect(source.operations.filter(value => value === 'create')).toHaveLength(1);
});
