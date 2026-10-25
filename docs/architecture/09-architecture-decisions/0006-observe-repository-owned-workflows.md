---
name: observe-repository-owned-workflows
description: SoFa observes and verifies independently orchestrated repository work.
status: Accepted
date: 2026-10-25
deciders: [Juan García Carmona]
tags: [github-actions, observation, runtime]
---

# ADR-0006: Observe repository-owned workflows

## Context

The pilot routed GitHub work into a SoFa supervisor and hid execution behind one bridge step. The owner explicitly corrected this boundary: SoFa is the cross-repository observer and verifier, while repository workflows and an independent runtime do the work. The owner accepted this direction and requested its implementation on 2026-10-25.

## Decision

GitHub repository workflows own event routing, stage execution, concurrency, authorization checks, publication and continuation using Lighthouse contracts. An operator-owned executor runs the isolated agent and has no GitHub credential or lifecycle authority. SoFa reads source evidence and exposes verification findings and decisions across repositories. Stopping SoFa must not stop repository delivery.

Future authenticated SoFa controls submit explicit user actions through the same GitHub entrypoints. They do not create an execution queue, policy engine or alternate approval path. The read-only observation surface cannot dispatch work.

This decision supersedes the assignment of runner, scheduler, supervisor and execution-store ownership to SoFa in ADR-0001, ADR-0002 and ADR-0004. ADR-0003's independent provider authentication boundary and ADR-0005's separation of entry from autonomy remain applicable; their earlier SoFa deployment descriptions are historical.

## Alternatives considered

Keeping SoFa in the execution path and improving only logs would retain the rejected dependency. Renaming its supervisor would retain the same ownership problem. Running the agent with publication credentials would weaken isolation. A new central workflow engine is unnecessary because GitHub already owns the repository execution graph.

## Consequences

GitHub shows the lifecycle execution and keeps running without SoFa. The runtime can be operated separately. SoFa's future UI and verification can evolve independently. Source collection may be incomplete or unavailable and must say so. The legacy pilot code remains explicitly opt-in for historical rollback; AppHost no longer starts it.

## References

- [Product Change](../../product/changes/completed/chg-observe-repository-workflows-001/change.md)
- [Runtime view](../06-runtime-view/README.md)
- [Lighthouse issue workflow adapter](https://github.com/juangcarmona/lighthouse/tree/main/src/packages/sdlc/workflows)
