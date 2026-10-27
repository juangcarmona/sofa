# Design

## Context

See `proposal.md` for motivation. The existing README explains the container and trusted Docker host suites; `scripts/verify-runtime.mjs` selects `SoFa.Runtime.Tests.EngineTests` for the host suite. Its two test methods are `EngineCannotWriteSourceOrSeePublisherCredentialOrDockerSocket` and `CancellationRemovesTheRunningContainer`.

## Goals / Non-Goals

**Goals:** Make the existing integration prerequisite explicit in one added sentence, verifiable against the test names and an exact-content diff.

**Non-Goals:** Change verification behavior, grant Docker access, install another runtime, change accepted product or architecture decisions, or claim owner acceptance.

## Decisions

- Append the sentence to the existing verification paragraph rather than restructure it, preserving every original byte and command.
- For PR #68 feedback, expand only the two abbreviated commands in that added sentence to their full `npm run` forms, consistent with the surrounding paragraph. Keep both exact EngineTests names and the integration prerequisite intact.
- Use the explicit documentation-only specification opt-out rather than invent a behavioral requirement. No spec delta is appropriate.
- Validate content with a targeted assertion and the required repository checks; no new test files or visual screenshots are warranted for this Markdown-only clarification.

## Risks / Trade-offs

- Full verification needs tooling and trusted-host facilities unavailable in an isolated engine → retain actual check logs, keep unmet gates explicit and return blocked when necessary; do not repair host tooling or weaken checks.
- A passing container result could be mistaken for permission to integrate → name both excluded tests and require the host suite and complete CI suite explicitly.

## Migration Plan

No deployment or data migration. Review the documentation and plan together; rollback is removal of the added sentence and issue-specific planning artifacts. Publication and later integration decisions remain with the trusted host and owner.
