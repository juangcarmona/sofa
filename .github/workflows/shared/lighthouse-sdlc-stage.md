---
# Owned by @juangcarmona/lighthouse-sdlc. Shared stage mechanics for the local gh-aw workers.
description: Shared runner, input, evidence and stage-result mechanics for Lighthouse SDLC workers.
engine: copilot
permissions: read-all
checkout:
  fetch-depth: 0
tools:
  bash: [cat, find, git, node, npm, npx, rg]
  edit:
  github:
    toolsets: [repos, issues, pull_requests]
safe-outputs:
  report-failure-as-issue: false
  create-issue: false
  noop:
    max: 1
    report-as-issue: false
steps:
  - name: Set up Node.js for Lighthouse contract
    uses: actions/setup-node@949feb2413d6458794dcd2491c4babbbce0c15c1 # v7.1.0
    with:
      node-version: 22
      cache: npm
  - name: Install Lighthouse contract
    shell: bash
    run: npm ci --ignore-scripts
  - name: Materialize the host input
    shell: bash
    env:
      STAGE_INPUT: ${{ inputs.input }}
    run: |
      set -euo pipefail
      mkdir -p /tmp/gh-aw/agent
      printf '%s\n' "${STAGE_INPUT:-{}}" > /tmp/gh-aw/agent/input.json
  - name: Load the previous stage result
    if: inputs.previous-stage != ''
    uses: actions/download-artifact@v8
    with:
      name: lighthouse-sdlc-${{ inputs.run-id }}-${{ inputs.previous-stage }}
      path: /tmp/gh-aw/agent/previous
post-steps:
  - name: Preserve the stage result
    if: always()
    shell: bash
    run: |
      set -euo pipefail
      mkdir -p "$GITHUB_WORKSPACE/.lighthouse/gh-aw"
      result=/tmp/gh-aw/agent/stage-result.json
      if [ -f "$result" ]; then
        cp "$result" "$GITHUB_WORKSPACE/.lighthouse/gh-aw/stage-result.json"
      else
        echo "::error::The worker did not produce /tmp/gh-aw/agent/stage-result.json"
        exit 1
      fi
  - name: Validate the stage result and declared gate
    if: always()
    shell: bash
    env:
      EXPECTED_STAGE: ${{ env.LIGHTHOUSE_STAGE }}
      RESULT_FILE: /tmp/gh-aw/agent/stage-result.json
    run: |
      set -euo pipefail
      npx --no-install lighthouse-sdlc stage-result --input "$RESULT_FILE" --format json
      EXPECTED_STAGE="$EXPECTED_STAGE" RESULT_FILE="$RESULT_FILE" node --input-type=module <<'NODE'
      import { readFileSync } from 'node:fs';
      import { createRequire } from 'node:module';
      import { dirname, join } from 'node:path';

      const stage = process.env.EXPECTED_STAGE;
      const result = JSON.parse(readFileSync(process.env.RESULT_FILE, 'utf8'));
      const require = createRequire(import.meta.url);
      const packageRoot = dirname(require.resolve('@juangcarmona/lighthouse-sdlc/package.json'));
      const contract = JSON.parse(readFileSync(join(packageRoot, 'stages.json'), 'utf8'));
      const expected = contract.stages.find((item) => item.id === stage);
      if (!expected || result.stage !== stage || result.gate !== expected.gate) {
        console.error(`stage-result does not match stages.json for ${stage}`);
        process.exit(1);
      }
      NODE
  - name: Upload stage evidence
    if: always()
    uses: actions/upload-artifact@v7
    with:
      name: lighthouse-sdlc-${{ inputs.run-id }}-${{ env.LIGHTHOUSE_STAGE }}
      path: ${{ github.workspace }}/.lighthouse/gh-aw/stage-result.json
      if-no-files-found: warn
      retention-days: 14
---

# Lighthouse SDLC worker mechanics

The host input is in `/tmp/gh-aw/agent/input.json`. If this worker has a previous stage, its validated result is in `/tmp/gh-aw/agent/previous/stage-result.json`.
