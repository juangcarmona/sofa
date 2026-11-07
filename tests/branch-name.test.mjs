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

test('generated foundation guard and manifest admit the established automation branches', async () => {
  const { readFile, mkdtemp, rm } = await import('node:fs/promises');
  const { tmpdir } = await import('node:os');
  const { join } = await import('node:path');
  const { fileURLToPath } = await import('node:url');
  const manifest = JSON.parse(await readFile(new URL('../lighthouse.json', import.meta.url), 'utf8'));
  const pattern = new RegExp(manifest.repository.conventions.branchPattern, 'u');
  const guard = fileURLToPath(new URL('../scripts/lighthouse-guard.mjs', import.meta.url));
  const directory = await mkdtemp(join(tmpdir(), 'sofa-branch-guard-'));
  try {
    assert.equal(spawnSync('git', ['init', '--quiet', directory]).status, 0);
    for (const [allowed, branches] of [
      [true, ['fix/49-identity-boundary', 'lighthouse/propose-49-2de94e5eefdb', 'lighthouse/implement-99-aabbccddeeff', 'dependabot/npm_and_yarn/snapshot/tool-1.0.0', 'dependabot/github_actions/actions/checkout-7']],
      [false, ['feature/49-identity', 'fix/0-invalid', 'lighthouse/implement-0-aabbccddeeff', 'lighthouse/implement-99-main', 'lighthouse/arbitrary-49-aabbccddeeff', 'dependabot/unknown/tool']],
    ]) {
      for (const branch of branches) {
        assert.equal(pattern.test(branch), allowed, `manifest: ${branch}`);
        assert.equal(spawnSync('git', ['symbolic-ref', 'HEAD', `refs/heads/${branch}`], { cwd: directory }).status, 0);
        const result = spawnSync(process.execPath, [guard, 'verify'], { cwd: directory, encoding: 'utf8' });
        assert.equal(result.status, allowed ? 0 : 1, `guard: ${branch}: ${result.stderr}`);
      }
    }
  } finally { await rm(directory, { recursive: true, force: true }); }
});
