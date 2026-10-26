import { defineConfig } from '@playwright/test';
import { existsSync } from 'node:fs';
export default defineConfig({
  testDir: './tests/browser', workers: 1, retries: 0,
  reporter: [['list'], ['html', { outputFolder: 'test-reports/browser', open: 'never' }]],
  use: { baseURL: 'http://127.0.0.1:5169', headless: true, launchOptions: { executablePath: existsSync('/usr/bin/chromium') ? '/usr/bin/chromium' : undefined }, trace: 'retain-on-failure' },
  webServer: { command: 'npm start', env: { SOFA_PORT: '5169', SOFA_HOST: '127.0.0.1' }, url: 'http://127.0.0.1:5169', reuseExistingServer: false },
});
