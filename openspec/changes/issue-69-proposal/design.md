# Design

## Context

See `proposal.md` for motivation. The final paragraph of `tests/README.md` already explains container verification and the trusted-host command. `EngineTests.cs` contains two tests excluded from container verification by the runtime script's class filter.

## Goals / Non-Goals

**Goal:** Make the host-only test identities and integration prerequisite explicit in one added sentence.

**Non-goals:** Alter tests, commands, isolation, dependencies, workflows, ProductShape scope, ADR-0006 or approval rules; merge or claim owner acceptance.

## Decisions

- Append one sentence to the existing container-verification paragraph rather than rewrite it. This preserves author-owned wording and commands while keeping related guidance together.
- Use the exact `EngineTests.EngineCannotWriteSourceOrSeePublisherCredentialOrDockerSocket` and `EngineTests.CancellationRemovesTheRunningContainer` names from source.
- For PR #70 feedback, make the separate host suite (`npm run test:runtime:host`) and full suite (`npm run verify`) explicit within that same added sentence; retain the full CI prerequisite.
- Use OpenSpec's documentation-only opt-out instead of adding a spec: observable system behavior is unchanged.
- Check the addition against the original guide and test declarations without adding a permanent test or dependency. Run the prescribed container and OpenSpec checks and retain logs outside source.

## Risks / Trade-offs

- Container success could be mistaken for integration approval → explicitly require the separate host suite and full CI; report owner judgement as pending.
- Docker tests cannot run inside this workload → leave them to a trusted host; do not expose a Docker socket or repair tooling.
- Operational fixture receipts are outside this engine's authority → record them as unverified host follow-up, not passed evidence.

## Migration Plan

No deployment or migration. Revert only the added sentence and issue-local planning artifacts if the fixture is withdrawn. The operator intends to close the test PR without merging.
