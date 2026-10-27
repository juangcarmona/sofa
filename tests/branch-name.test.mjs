import test from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';

const check = branch => spawnSync(process.execPath, ['scripts/check-branch-name.mjs', branch], { encoding: 'utf8' });

test('accepts numbered authored branches and dependency-provider branches', () => {
  for (const branch of ['lighthouse/implement-47-aabbccddeeff', 'lighthouse/propose-47-aabbccddeeff', 'fix/37-dependency-branch-checks', 'docs/25-implementation-learning', 'dependabot/npm_and_yarn/dot-agents/skills/architecture-docs/snapshot/dompurify-3.4.17', 'dependabot/npm_and_yarn/dot-agents/skills/architecture-docs/snapshot/multi-010aad1c49', 'dependabot/github_actions/actions/checkout-7']) {
    const result = check(branch);
    assert.equal(result.status, 0, `${branch}: ${result.stderr}`);
  }
});

test('keeps malformed and unnumbered authored branches invalid', () => {
  for (const branch of ['lighthouse/implement-0-aabbccddeeff', 'lighthouse/implement-47-main', 'lighthouse/arbitrary-47-aabbccddeeff', '', 'main', 'fix/no-issue', 'fix/0-not-valid', 'feature/37-change', 'dependabot/npm_and_yarn/', 'dependabot/npm_and_yarn//vite', 'dependabot/unknown/vite', 'dependabot/npm_and_yarn/../vite']) {
    assert.equal(check(branch).status, 1, branch);
  }
});
