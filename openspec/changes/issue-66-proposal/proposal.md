# Proposal

## Why

The container verification guidance does not name the two excluded Docker engine tests or explicitly tie their trusted-host results to integration. Issue #66 requests a small clarification of the existing rule, not a new verification policy.

## What Changes

- Add one sentence to `tests/README.md` naming both host-only EngineTests and stating that container verification alone does not authorize integration before the host suite and complete CI suite pass.
- Preserve all existing wording, commands and behavior.
- Apply PR #68 feedback by spelling out `npm run verify:container` and `npm run test:runtime:host` in the added sentence, without changing either EngineTests name or the integration prerequisite.

## Capabilities

### New Capabilities

None. This is documentation of an existing rule; `.openspec.yaml` sets `skip_specs: true`.

### Modified Capabilities

None. No specification-level behavior changes. The OpenSpec inventory found no existing root or specifications.

## Impact

Only `tests/README.md` and this issue's planning artifacts change. No source, tests, dependencies, workflow, policy, harness, ProductShape or ADR changes are needed. Required checks remain unchanged; unavailable execution prerequisites must block the assessment without discarding the documentation candidate.
