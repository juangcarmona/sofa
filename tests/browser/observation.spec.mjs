import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

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
  await page.screenshot({ path: info.outputPath(`${theme}-desktop.png`), fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: info.outputPath(`${theme}-mobile.png`), fullPage: true });
  await page.route('**/api/observations', route => route.fulfill({ status: 503, body: 'Unavailable' }));
  await page.getByRole('button', { name: 'Refresh observations' }).click();
  await expect(page.getByRole('status')).toContainText('may be stale');
  await expect(page.locator('article')).toHaveCount(2);
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
