# Proposal

## Why

Issue #69 checks native Lighthouse delivery after adoption #65 and merged PR #67. The test guide should name the host-only engine tests and make clear that container verification cannot authorize integration.

## What Changes

- Add one sentence to `tests/README.md` naming both Docker engine tests and requiring the separate host suite and full CI before integration.
- Preserve all existing wording, commands, tests, dependencies, workflows and policies.
- Retain this plan and the documentation change in the same implementation PR, with actual check results and unmet gates reported separately.
- Apply PR #70 owner feedback by spelling out `npm run test:runtime:host` and `npm run verify` in the added sentence, without changing either EngineTests name or pre-existing wording.

## Capabilities

### New Capabilities

None. This is a documentation clarification with no behavior change.

### Modified Capabilities

None. `.openspec.yaml` sets `skip_specs: true`; no specification delta is appropriate.

## Impact

Only the test guide and issue-local planning artifacts change. ProductShape intent, accepted ADR-0006, runtime isolation and integration authority remain unchanged. The coordinating host owns worker, source-link, evaluation and reservation-release receipts; this documentation candidate does not implement or certify those operations.
