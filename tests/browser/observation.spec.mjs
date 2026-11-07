import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { mkdir } from 'node:fs/promises';
import { join } from 'node:path';

const payload = { repositories: [
  { repository: 'owner/lighthouse', observedAt: '2026-10-26T07:00:00Z', availability: 'available', runs: [{ id: 1, name: 'refine #45', status: 'in_progress', conclusion: null, sourceRevision: 'abc1234', updatedAt: '2026-10-26T07:00:00Z', url: 'https://github.com/owner/lighthouse/actions/runs/1' }], attention: [{ number: 45, title: 'Initialize the factory view', labels: ['agent-working'], url: 'https://github.com/owner/lighthouse/issues/45' }, { number: 46, title: '<img src=x onerror=alert(1)>', labels: ['waiting-review'], url: 'javascript:alert(1)' }] },
  { repository: 'owner/sofa', observedAt: '2026-10-26T07:00:00Z', availability: 'unavailable' },
] };
for (const theme of ['dark', 'light']) test(`${theme}: evidence, safe links, keyboard, mobile and accessibility`, async ({ page }, info) => {
  await page.route('**/api/observations', route => route.fulfill({ json: payload }));
  await page.goto('/'); await page.selectOption('#appearance', theme);
  await expect(page.getByText('Initialize the factory view', { exact: false })).toBeVisible();
  await expect(page.getByText('Evidence unavailable', { exact: true })).toBeVisible();
  await expect(page.getByText('waiting-review', { exact: true })).toBeVisible();
  await expect(page.locator('a[href^="javascript:"]')).toHaveCount(0);
  await expect(page.locator('img')).toHaveCount(0);
  await expect(page.locator('#active-count')).toHaveText('1');
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.keyboard.press('Control+Home'); await page.keyboard.press('Tab');
  await page.getByRole('button', { name: 'Refresh observations' }).focus();
  await page.keyboard.press('Enter'); await expect(page.locator('#refresh')).toBeEnabled();
  const evidence = process.env.SOFA_BROWSER_EVIDENCE;
  if (evidence) await mkdir(evidence, { recursive: true });
  await page.screenshot({ path: evidence ? join(evidence, `fixture-${theme}-desktop.png`) : info.outputPath(`${theme}-desktop.png`), fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: evidence ? join(evidence, `fixture-${theme}-mobile.png`) : info.outputPath(`${theme}-mobile.png`), fullPage: true });
  await page.route('**/api/observations', route => route.fulfill({ status: 503, body: 'Unavailable' }));
  await page.getByRole('button', { name: 'Refresh observations' }).click();
  await expect(page.getByRole('status')).toContainText('may be stale');
  await expect(page.locator('article')).toHaveCount(2);
});

function operationalPayload() {
  const repo = structuredClone(payload.repositories[0]);
  repo.runs = ['in_progress', 'queued', 'pending', 'requested', 'waiting', 'completed', 'mystery', 'in_progress', 'queued', 'waiting', 'requested', 'in_progress'].map((status, id) => ({
    id, name: `Workflow ${id}`, status, sourceRevision: `revision-${id}`, startedAt: '2026-10-26T05:00:00Z', updatedAt: '2026-10-26T06:00:00Z', url: `https://github.com/${repo.repository}/actions/runs/${id}`,
  }));
  repo.attention = [
    { number: 1, title: 'Information', labels: ['needs-info'], updatedAt: '2026-10-25T07:00:00Z' },
    { number: 2, title: 'Review', labels: ['waiting-review'], updatedAt: null },
    { number: 3, title: 'Both reasons', labels: ['waiting-review', 'needs-info', 'agent-working', 'ready'], updatedAt: 'invalid' },
    { number: 4, title: 'Agent only', labels: ['agent-working', 'ready'], updatedAt: '2026-10-27T07:00:00Z' },
  ].map(issue => ({ ...issue, url: `https://github.com/${repo.repository}/issues/${issue.number}` }));
  return { repositories: [repo, { ...structuredClone(repo), repository: 'owner/other', runs: repo.runs.slice(0, 1).map(run => ({ ...run, url: 'https://github.com/owner/other/actions/runs/0' })), attention: [] }, payload.repositories[1]] };
}
async function refreshInPlace(page) {
  const previous = await page.locator('article').first().elementHandle();
  await page.evaluate(() => document.getElementById('refresh').click());
  await page.waitForFunction(old => !old.isConnected, previous);
}

test('all active states, human waits, bounded evidence, source ages and local filtering', async ({ page }) => {
  let current = operationalPayload();
  await page.route('**/api/observations', route => route.fulfill({ json: current }));
  await page.goto('/');
  await expect(page.locator('#active-count')).toHaveText('11');
  await expect(page.locator('#attention-count')).toHaveText('3');
  await expect(page.locator('#repo-count')).toHaveText('2 / 3');
  await expect(page.locator('#count-scope')).toContainText('unknown remainder');
  const workflows = page.getByRole('heading', { name: 'Active workflows', exact: true }).locator('..');
  const waits = page.getByRole('heading', { name: 'Human intervention', exact: true }).locator('..');
  await expect(workflows.getByRole('link', { name: 'Workflow 11', exact: true })).toBeVisible();
  await expect(workflows.getByText('pending', { exact: true })).toBeVisible();
  await expect(workflows.getByText('requested', { exact: true }).first()).toBeVisible();
  await expect(workflows.getByText('waiting', { exact: true }).first()).toBeVisible();
  await expect(workflows.getByText('Elapsed from source start: 2 hours', { exact: false }).first()).toBeVisible();
  await expect(workflows.getByText('Source revision: revision-11')).toBeVisible();
  await expect(waits.getByRole('link')).toHaveCount(3);
  await expect(waits.getByText('Agent only', { exact: false })).toHaveCount(0);
  await expect(waits.getByText('Since source update (not wait duration): 1 day', { exact: false })).toBeVisible();
  await expect(waits.getByText('Since source update (not wait duration): unavailable', { exact: true })).toHaveCount(2);
  await expect(page.getByText('Latest 20 workflow runs and first 100 open issue records', { exact: false })).toBeVisible();
  const history = page.locator('article').first();
  await history.getByText('Recent completed runs', { exact: true }).click();
  await expect(history.getByRole('link', { name: 'Workflow 5', exact: true })).toBeVisible();
  await history.getByText('Unclassified source statuses', { exact: true }).click();
  await expect(history.getByText('mystery', { exact: true })).toBeVisible();
  await page.selectOption('#repository-filter', 'owner/other');
  await expect(page.locator('#active-count')).toHaveText('1');
  await expect(page.locator('#attention-count')).toHaveText('0');
  await expect(page.locator('article')).toHaveCount(1);
  await refreshInPlace(page);
  await expect(page.locator('#repository-filter')).toHaveValue('owner/other');
  await page.route('**/api/observations', route => route.fulfill({ status: 503 }));
  await page.getByRole('button', { name: 'Refresh observations' }).click();
  await expect(page.getByRole('status')).toContainText('may be stale');
  await expect(page.locator('#repository-filter')).toHaveValue('owner/other');
  await page.selectOption('#repository-filter', 'owner/sofa');
  await expect(page.locator('#active-count')).toHaveText('Unavailable');
  await expect(page.locator('#attention-count')).toHaveText('Unavailable');
  await expect(page.locator('#unknown-count')).toHaveText('1');
});

test('source-keyed focus survives same titles, changed href, reorder, history and removal', async ({ page }) => {
  const current = operationalPayload();
  await page.route('**/api/observations', route => route.fulfill({ json: current }));
  await page.goto('/');
  const source = page.locator('a[href="https://github.com/owner/lighthouse/actions/runs/0"]');
  await source.focus();
  current.repositories.reverse();
  const repo = current.repositories.find(repo => repo.repository === 'owner/lighthouse');
  repo.runs.reverse();
  const run = repo.runs.find(run => run.id === 0);
  run.name = 'Changed title'; run.url += '?check=updated';
  await refreshInPlace(page);
  await expect(page.getByRole('link', { name: 'Changed title' })).toBeFocused();
  run.status = 'completed';
  await refreshInPlace(page);
  await expect(page.getByRole('link', { name: 'Changed title' })).toBeFocused();
  await expect(page.getByRole('link', { name: 'Changed title' })).toBeVisible();
  await refreshInPlace(page);
  await expect(page.getByRole('link', { name: 'Changed title' })).toBeVisible();
  run.url = 'https://github.com/owner/other/actions/runs/0';
  await refreshInPlace(page);
  await expect(page.getByRole('navigation', { name: 'owner/lighthouse actions' }).getByRole('link', { name: 'Open workflow actions' })).toBeFocused();
  await page.locator('#appearance').focus();
  await refreshInPlace(page);
  await expect(page.locator('#appearance')).toBeFocused();
  const summary = page.locator('article').filter({ has: page.getByRole('heading', { name: 'owner/lighthouse', exact: true }) }).getByText('Unclassified source statuses', { exact: true });
  await summary.focus();
  await refreshInPlace(page);
  await expect(summary).toBeFocused();
  await expect(summary.locator('..')).not.toHaveAttribute('open');
  await page.getByRole('button', { name: 'Refresh observations' }).focus();
  await page.keyboard.press('Enter');
  await expect(page.locator('#refresh')).toBeEnabled();
  await expect(page.locator('#refresh')).toBeFocused();
  await page.getByRole('navigation', { name: 'owner/lighthouse actions' }).getByRole('link', { name: 'Open workflow actions' }).focus();
  current.repositories = current.repositories.filter(repo => repo.repository !== 'owner/lighthouse');
  await refreshInPlace(page);
  await expect(page.locator('#repository-filter')).toBeFocused();
});

test('missing, invalid and future times stay unavailable; malformed refresh retains stale source times', async ({ page }) => {
  const current = operationalPayload();
  current.repositories = [current.repositories[0]];
  for (const [i, value] of [undefined, null, 'invalid', '2026-10-27T07:00:00Z'].entries()) {
    current.repositories[0].runs[i].status = 'in_progress';
    current.repositories[0].runs[i].startedAt = value;
    current.repositories[0].runs[i].updatedAt = value;
  }
  await page.route('**/api/observations', route => route.fulfill({ json: current }));
  await page.goto('/');
  await expect(page.getByText('Elapsed from source start: unavailable', { exact: true })).toHaveCount(4);
  await expect(page.getByText('Since source update: unavailable', { exact: true })).toHaveCount(4);
  await expect(page.getByText('1970', { exact: false })).toHaveCount(0);
  const observation = page.getByText('Source observation time:', { exact: false });
  const text = await observation.textContent();
  await page.route('**/api/observations', route => route.fulfill({ contentType: 'application/json', body: '{' }));
  await page.getByRole('button', { name: 'Refresh observations' }).click();
  await expect(page.getByRole('status')).toContainText('may be stale');
  await expect(observation).toHaveText(text);
});

for (const theme of ['dark', 'light']) test(`${theme}: operational fixture phone layout, history, hostile links and accessibility`, async ({ page }, info) => {
  const current = operationalPayload();
  const repo = current.repositories[0];
  repo.runs[0].name = '<img src=x onerror=alert(1)>' + 'long-title'.repeat(20);
  repo.runs[0].sourceRevision = 'revision'.repeat(30);
  repo.runs[0].url = 'https://github.com/owner/elsewhere/actions/runs/1';
  repo.attention[0].labels.push('long-label'.repeat(30));
  repo.attention[0].url = 'javascript:alert(1)';
  await page.route('**/api/observations', route => route.fulfill({ json: current }));
  await page.goto('/'); await page.selectOption('#appearance', theme);
  await expect(page.locator('img, a[href^="javascript:"], a[href*="elsewhere"]')).toHaveCount(0);
  await page.locator('article').first().getByText('Recent completed runs', { exact: true }).click();
  const evidence = process.env.SOFA_BROWSER_EVIDENCE;
  if (evidence) await mkdir(evidence, { recursive: true });
  for (const [size, width, height] of [['desktop', 1280, 900], ['mobile', 390, 844]]) {
    await page.setViewportSize({ width, height });
    await page.locator('#repository-filter').focus();
    await page.keyboard.press('ArrowDown'); await page.keyboard.press('Enter');
    await page.selectOption('#repository-filter', '');
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    await page.screenshot({ path: evidence ? join(evidence, `fixture-operational-${theme}-${size}.png`) : info.outputPath(`${theme}-${size}.png`), fullPage: true });
    await page.selectOption('#repository-filter', 'owner/sofa');
    await expect(page.locator('#active-count')).toHaveText('Unavailable');
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    await page.selectOption('#repository-filter', '');
  }
});

test('unavailable observation timestamps and missing revisions are explicit', async ({ page }) => {
  const current = operationalPayload();
  current.repositories = [current.repositories[0]];
  current.repositories[0].observedAt = null;
  current.repositories[0].runs = [{ ...current.repositories[0].runs[0], sourceRevision: null }];
  await page.route('**/api/observations', route => route.fulfill({ json: current }));
  await page.goto('/');
  await expect(page.getByText('Source observation time: Time unavailable', { exact: true })).toBeVisible();
  await expect(page.getByText('Source revision: unavailable', { exact: true })).toBeVisible();
  await expect(page.getByText('Elapsed from source start: unavailable', { exact: true })).toBeVisible();
  await expect(page.getByText('Since source update: unavailable', { exact: true })).toBeVisible();
});

test('initial refresh failure does not fabricate observations', async ({ page }) => {
  await page.route('**/api/observations', route => route.fulfill({ status: 503 }));
  await page.goto('/');
  await expect(page.getByRole('status')).toContainText('No observation is available');
  await expect(page.locator('#active-count')).toHaveText('—');
});

for (const status of [401, 403, 409]) test(`authority loss ${status} clears previous source data and filter counts`, async ({ page }) => {
  let allowed = true;
  await page.route('**/api/observations', route => route.fulfill(allowed ? { json: payload } : { status }));
  await page.goto('/');
  const issue = page.getByRole('link', { name: '#45 Initialize the factory view' });
  await expect(issue).toBeVisible();
  await issue.focus();
  allowed = false;
  await page.evaluate(() => document.getElementById('refresh').click());
  await expect(issue).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Open account' })).toBeFocused();
  await expect(page.locator('#active-count')).toHaveText('—');
  await expect(page.locator('#attention-count')).toHaveText('—');
  await expect(page.locator('#repository-filter option')).toHaveCount(1);
  await expect(page.getByText('No authorized observation is available.')).toBeVisible();
});

test('empty evidence is not a claim of delivery success', async ({ page }) => {
  await page.route('**/api/observations', route => route.fulfill({ json: { repositories: [{ ...payload.repositories[0], runs: [], attention: [] }] } }));
  await page.goto('/');
  await expect(page.getByText('No workflow runs returned.')).toBeVisible();
  await expect(page.getByText('No attention labels in the collected issues.')).toBeVisible();
});

test('background refresh preserves keyboard focus and handles a removed issue', async ({ page }) => {
  let current = structuredClone(payload);
  await page.route('**/api/observations', route => route.fulfill({ json: current }));
  await page.goto('/');
  const issue = page.getByRole('link', { name: '#45 Initialize the factory view' });
  await issue.focus();
  const refreshWithoutMovingFocus = async () => {
    await page.evaluate(() => {
      window.previousLink = document.activeElement;
      document.getElementById('refresh').click();
    });
    await page.waitForFunction(() => !window.previousLink.isConnected);
  };
  await refreshWithoutMovingFocus();
  await expect(issue).toBeFocused();
  current.repositories[0].attention = [];
  await refreshWithoutMovingFocus();
  await expect(page.getByRole('navigation', { name: 'owner/lighthouse actions' }).getByRole('link', { name: 'Open workflow actions' })).toBeFocused();
});

test('refresh preserves a focused collapsed history disclosure', async ({ page }) => {
  await page.route('**/api/observations', route => route.fulfill({ json: payload }));
  await page.goto('/');
  const summary = page.getByText('Recent completed runs', { exact: true }).first();
  await summary.focus();
  await page.evaluate(() => {
    window.previousSummary = document.activeElement;
    document.getElementById('refresh').click();
  });
  await page.waitForFunction(() => !window.previousSummary.isConnected);
  await expect(summary).toBeFocused();
  await expect(summary.locator('..')).not.toHaveAttribute('open', '');
});
