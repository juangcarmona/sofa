---
name: lighthouse-delivery-workflow-sofa-bastion-runtime
description: Lighthouse owns repository delivery workflow while SoFa owns supervised execution on Bastion.
status: Accepted
date: 2026-10-24
deciders: [Juan García Carmona]
tags: [lighthouse, sofa, bastion, runtime, github-actions]
---

# ADR-0001: Lighthouse owns delivery workflow and SoFa owns the Bastion execution runtime

## Context

SoFa must execute agentic engineering work across consumer repositories, but the repository lifecycle already exists in Lighthouse. Lighthouse defines routes, stages, gates, skills, stage-result contracts, gh-aw workflows, router/workers, explicit composition, and delivery evidence. The current consumer repository selects Lighthouse through `.lighthouse/sdlc.json` and the observed route catalog contains `triage`, `refine`, `propose`, `implement`, `integrate`, and `review`.

SoFa has no implemented runtime yet. Its target problem is operational: schedule eligible work, provide one bounded execution capacity on a persistent Bastion, supervise the runner and engine, isolate a workspace, persist recovery state, and prevent stale work from publishing. The initial environment is one persistent Bastion with a self-hosted GitHub runner, SoFa process, model gateway, local/provider model services, and ephemeral execution resources.

The architecture must preserve Lighthouse's authority without turning SoFa into a second lifecycle engine or introducing an unneeded central webhook. It must also keep operational state separate from Lighthouse stage results, delivery evidence, telemetry, and human decisions.

## Decision

We will:

1. Keep Lighthouse authoritative for repository delivery workflow: stages, routes, gates, skills, gh-aw workflows, router/workers, explicit `ff` composition, stage-result contracts, and delivery evidence.
2. Keep SoFa authoritative for cross-project scheduling, leases, checkpoints, fencing, recovery, capacity, concurrency, runner supervision, workspace lifecycle, isolation, operational persistence, engine integration, runtime health, and Bastion operation.
3. Deploy the runner as a SoFa component on Bastion. It supplies the GitHub Actions capability `[self-hosted, linux, lighthouse-sdlc]` and hands the explicit Lighthouse job contract to the SoFa supervisor.
4. Use this initial trigger path: GitHub event -> Lighthouse router -> Lighthouse worker -> GitHub Actions job -> SoFa runner on Bastion -> isolated execution -> Lighthouse stage result and delivery evidence.
5. Keep Bastion persistent while making the execution worker and workspace ephemeral per run. The first slice has at most one active SoFa execution worker and does not require autoscaling, KEDA, a host fleet, a broker, ephemeral machines, or a microVM.
6. Use leases, fencing tokens, monotonic checkpoint sequences, bounded cancellation/timeouts, and explicit recovery results so a stale worker cannot publish after authority expires.
7. Keep OpenCode external to lifecycle control and use the model gateway as the model-routing, masking, metering, security, and health boundary. SoFa supervises and correlates bounded operational metadata; it does not become a model server or duplicate the gateway.
8. Preserve Lighthouse gates `triaged`, `ready`, `planned`, and `done`. `ready`, `planned`, and `done` are gates, not stages. No `deploy` stage or state is introduced. Successful automation never implies human approval, merge, release, or production acceptance.
9. Do not introduce a central SoFa webhook in the first slice. A future ingress may be added only as an adapter that preserves the same Lighthouse and SoFa ownership boundaries.

## Consequences

- **Positive:** The first runtime can integrate with the existing Lighthouse contract instead of reimplementing repository lifecycle behavior.
- **Positive:** A single persistent Bastion is sufficient for the first vertical slice while execution state and workspaces remain bounded and recoverable.
- **Positive:** Fencing, checkpointing, and evidence references make failure and recovery explicit rather than optimistic.
- **Positive:** Human authority remains visible and cannot be inferred from a green stage, CI, or mergeable pull request.
- **Negative:** SoFa depends on the exact Lighthouse package and workflow contract and must verify version compatibility during implementation.
- **Negative:** One Bastion and one active worker impose a hard capacity limit and a single-host failure domain.
- **Negative:** Trusted-repository isolation is not sufficient for arbitrary untrusted forks; stronger isolation must be added before those inputs are admitted.
- **Neutral:** The wire-level Lighthouse-to-SoFa contract, authentication mechanism, database schema, workspace driver, and OpenCode cancellation protocol remain implementation decisions constrained by this boundary.

## Alternatives considered

- **SoFa owns a new lifecycle and stage state machine:** rejected because it duplicates Lighthouse authority and would allow lifecycle drift.
- **A central SoFa webhook invokes the agent directly:** rejected for the first slice because the current Lighthouse workflow already provides event routing, composition, and a GitHub Actions job boundary.
- **Invoke the agent directly from the GitHub event:** rejected because it bypasses the current Lighthouse router, worker, stage-result, and safe-output controls.
- **A multi-host or autoscaled runtime:** rejected because the initial case is one persistent Bastion and no evidence requires fleet orchestration yet.
- **Docker Sandbox or a microVM for every first-slice run:** rejected as an initial requirement because the first slice is limited to trusted repositories; it remains the evolution path for untrusted code.

## References

- [SoFa architecture documentation](../architecture/README.md)
- [SoFa runtime architecture proposal and refinement](../roadmap/runtime-architecture.md)
- [SoFa first pilot](../roadmap/first-pilot.md)
- [Recoverable execution product requirement](../product/model/requirements/functional/fr-recoverable-execution.md)
- [Lighthouse routes](https://github.com/juangcarmona/lighthouse/blob/main/src/packages/sdlc/routes.json)
- [GitHub Agentic Workflows](https://github.github.com/gh-aw/)
- [Plain Concepts Platform: how we work](https://platform.plainconcepts.com/docs/how-we-work)
