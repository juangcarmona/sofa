---
name: lighthouse-delivery-workflow-sofa-execution-host-runtime
description: Lighthouse owns repository delivery workflow while SoFa owns supervised execution on the execution host.
status: Superseded
date: 2026-10-24
deciders: [Juan García Carmona]
tags: [lighthouse, sofa, execution-host, runtime, github-actions]
---

# ADR-0001: Lighthouse owns delivery workflow and SoFa owns the execution runtime

Superseded by [ADR-0006](0006-observe-repository-owned-workflows.md) after the owner changed the product boundary. The remainder preserves the original decision.

## Context

SoFa must execute agentic engineering work across consumer repositories, but the repository lifecycle already exists in Lighthouse. Lighthouse defines routes, stages, gates, skills, stage-result contracts, gh-aw workflows, router/workers, explicit composition, and delivery evidence.

SoFa has no implemented runtime yet. Its target problem is operational: schedule eligible work, provide bounded execution capacity on a persistent execution host, supervise the runner and engine, isolate a workspace, persist recovery state, and prevent stale work from publishing. The initial environment is one persistent execution host with self-hosted GitHub runner registrations, a SoFa process, a model gateway, local/provider model services, and ephemeral execution resources.

## Decision

We will:

1. Keep Lighthouse authoritative for repository delivery workflow: stages, routes, gates, skills, gh-aw workflows, router/workers, explicit `ff` composition, stage-result contracts, and delivery evidence.
2. Keep SoFa authoritative for cross-project scheduling, leases, checkpoints, fencing, recovery, capacity, concurrency, runner supervision, workspace lifecycle, isolation, operational persistence, engine integration, runtime health, and execution host operation.
3. Deploy the runner as a SoFa component on the execution host. It supplies `[self-hosted, linux, lighthouse-sdlc]` and hands the explicit Lighthouse job contract to the SoFa supervisor.
4. Use this initial trigger path: GitHub event -> Lighthouse router -> Lighthouse worker -> GitHub Actions job -> SoFa runner on the execution host -> isolated execution -> Lighthouse stage result and delivery evidence.
5. Keep the execution host persistent while making one execution worker and workspace ephemeral per run. The first slice does not require autoscaling, KEDA, a host fleet, a broker, ephemeral machines, or a microVM. Bounded parallel capacity on that host is defined by [ADR-0002](0002-bounded-parallel-execution.md).
6. Use leases, fencing tokens, monotonic checkpoint sequences, bounded cancellation/timeouts, and explicit recovery results so a stale worker cannot publish after authority expires.
7. Keep OpenCode external to lifecycle control. The initial execution path invokes persistent OpenCode with the operator's OpenAI Codex/ChatGPT authentication; provider routing and model selection remain behind the OpenCode/provider boundary and may use a gateway when that gateway supports the selected provider authentication.
8. Preserve Lighthouse gates `triaged`, `ready`, `planned`, and `done`. `ready`, `planned`, and `done` are gates, not stages. No `deploy` stage or state is introduced.
9. Do not introduce a central SoFa webhook in the first slice. A future ingress may be added only as an adapter that preserves the same ownership boundaries.

## Consequences

- **Positive:** SoFa integrates with the existing Lighthouse contract instead of reimplementing repository lifecycle behavior.
- **Positive:** A single persistent execution host is sufficient for the first vertical slice while execution state and workspaces remain bounded and recoverable.
- **Positive:** Fencing, checkpointing, and evidence references make failure and recovery explicit.
- **Negative:** SoFa depends on the exact Lighthouse package and workflow contract and must verify version compatibility during implementation.
- **Negative:** One execution host and its bounded worker capacity impose a hard resource limit and a single-host failure domain.
- **Neutral:** The wire-level handoff schema, authentication mechanism, database schema, workspace driver, and OpenCode cancellation protocol remain implementation decisions constrained by this boundary.

## Alternatives considered

- **SoFa owns a new lifecycle and stage state machine:** rejected because it duplicates Lighthouse authority and enables lifecycle drift.
- **A central SoFa webhook invokes the agent directly:** rejected for the first slice because Lighthouse already provides event routing, composition, and a GitHub Actions job boundary.
- **Invoke the agent directly from the GitHub event:** rejected because it bypasses Lighthouse router, worker, stage-result, and safe-output controls.
- **A multi-host or autoscaled runtime:** rejected because the initial case is one persistent execution host and no evidence requires fleet orchestration.
- **Docker Sandbox or a microVM for every first-slice run:** rejected as an initial requirement because the first slice is limited to trusted repositories; it remains the evolution path for untrusted code.

## References

- [SoFa architecture documentation](../README.md)
- [SoFa runtime architecture](../../roadmap/runtime-architecture.md)
- [SoFa first pilot](../../roadmap/first-pilot.md)
- [Recoverable execution product requirement](../../product/model/requirements/functional/fr-recoverable-execution.md)
- [Lighthouse routes](https://github.com/juangcarmona/lighthouse/blob/main/src/packages/sdlc/routes.json)
- [GitHub Agentic Workflows](https://github.github.com/gh-aw/)
- [Plain Concepts Platform: how we work](https://platform.plainconcepts.com/docs/how-we-work)
