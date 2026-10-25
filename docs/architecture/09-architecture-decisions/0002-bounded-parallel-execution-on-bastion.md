---
name: bounded-parallel-execution-on-bastion
description: One persistent Bastion supports several isolated SoFa executions through repository-scoped runner registrations.
status: Superseded
date: 2026-10-24
deciders: [Juan García Carmona]
tags: [sofa, bastion, github-actions, runners, concurrency, isolation]
---

# ADR-0002: Bounded parallel execution on one Bastion

Superseded by [ADR-0006](0006-observe-repository-owned-workflows.md) after the owner changed the product boundary. The remainder preserves the original decision.

## Context

SoFa is a personal factory spanning multiple personal GitHub repositories. GitHub's native self-hosted runner registration is repository-scoped for this deployment; one physical Bastion can therefore host several runner registrations, but GitHub does not provide a single account-wide registration that replaces them.

The factory also needs several agentic flows to run concurrently, for example five or six trusted repositories, without allowing one repository's checkout, process state, credentials, or outputs to contaminate another run. The existing product model assigns global capacity and concurrency to SoFa and keeps the default project limit at one active execution per project.

## Decision

1. Keep one persistent Bastion host for the initial deployment.
2. Install one GitHub Actions runner registration per participating personal repository, with a distinct directory, service unit, working directory, and repository registration token. The registrations may share the dedicated local `github-runner` OS account.
3. Treat the registrations as transport slots, not as SoFa workers or capacity policy. Every accepted run enters the shared SoFa admission boundary.
4. Allow bounded parallel execution on Bastion. Each admitted run receives exactly one execution worker and one isolated workspace/sandbox. The initial capacity target is five to six concurrent trusted-repository runs, subject to measured memory, disk, model-service, and thermal/network headroom.
5. Enforce one active execution per project by default, while allowing future explicit project limits that never exceed healthy global capacity.
6. Keep the workspace and per-run execution state ephemeral. Persist only the operational state, recovery metadata, and evidence references defined by the architecture; do not persist runner registration tokens or full prompts/transcripts by default.
7. Do not introduce a central SoFa webhook or a new Lighthouse lifecycle stage to achieve this concurrency. The current Lighthouse router, stages, routes, gates, `ff` composition, stage results, and delivery evidence remain authoritative.

This decision amends only the single-worker capacity clause in ADR-0001. Its Lighthouse/SoFa ownership boundary, persistent Bastion, external engine, recovery, fencing, and no-implicit-deploy decisions remain in force.

## Consequences

- **Positive:** One Bastion can serve multiple personal repositories without requiring a GitHub organization.
- **Positive:** Parallel jobs are isolated by run rather than by a long-lived per-repository workspace.
- **Positive:** SoFa can make global capacity, per-project limits, leases, health, and recovery observable and enforceable.
- **Negative:** The host becomes a shared failure and resource domain; exact capacity must be measured and surfaced as health.
- **Negative:** Every participating personal repository requires its own GitHub runner registration and service configuration.
- **Negative:** The current Lighthouse job executes directly on the selected runner; the SoFa admission handoff and sandbox lifecycle remain implementation work.

## Alternatives considered

- **One GitHub runner registration for all personal repositories:** rejected because the native personal-account deployment has no documented account-wide runner registration scope.
- **One runner registration per repository with no SoFa admission:** rejected because independent registrations could exceed the host's safe capacity and bypass leases, fencing, and isolation.
- **One global execution only:** rejected because the factory requires several agentic flows to progress concurrently.
- **A GitHub organization-level runner:** not selected for this personal-repository deployment; it would change repository ownership/placement rather than solve the current personal-account topology.
- **A central webhook that invokes agents directly:** rejected for this phase because it would bypass the current Lighthouse router and workflow boundary.

## References

- [ADR-0001: Lighthouse owns delivery workflow and SoFa owns the Bastion execution runtime](0001-lighthouse-delivery-workflow-sofa-bastion-runtime.md)
- [SoFa runtime architecture](../../roadmap/runtime-architecture.md)
- [Multi-project scheduling product requirement](../../product/model/requirements/functional/fr-multi-project-scheduling.md)
- [GitHub self-hosted runners](https://docs.github.com/en/actions/concepts/runners/self-hosted-runners)
