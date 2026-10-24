---
title: Introduction and goals
arc42-section: "01"
description: System purpose, stakeholders, and architecture goals.
---

# 1. Introduction and goals

## Requirements overview

SoFa is the operational runtime and coordination plane for the Software Factory. It schedules eligible work across projects and supervises bounded concurrent executions on the persistent Bastion machine. It does not replace the repository lifecycle owned by Lighthouse.

The target architecture realizes the current product intent for multi-project scheduling and recoverable execution while keeping GitHub as the backlog and delivery evidence authority. Product intent remains owned by ProductShape; this document describes the architectural realization only.

<!-- pdac:cite id="BC-SCHEDULING" digest="sha256:82103b159b35fce748e529845258bc4f2cdbff39ffdf18f6382556f07658b215" -->

<!-- pdac:cite id="BC-EXECUTION" digest="sha256:f931310bc9f10fc41e950104e0392e0145394ebd4b3b16ea4c5b19a1d6ed353e" -->

The delivery lifecycle is delegated to the current Lighthouse contract: `triage`, `refine`, `propose`, `implement`, `integrate`, and `review`, with `triaged`, `ready`, `planned`, and `done` as gates. SoFa may schedule and supervise a Lighthouse job, but it cannot add lifecycle stages, skip gates, approve a human decision, or infer merge, release, or production authority from successful automation.

The initial execution path is deliberately narrow:

```text
GitHub event
  -> Lighthouse router
  -> Lighthouse stage worker
  -> GitHub Actions job
  -> SoFa runner capability on Bastion
  -> isolated execution
  -> stage result and delivery evidence
```

There is no requirement in the inspected Lighthouse contract for a central SoFa webhook. A future ingress may be added as an adapter, but it must preserve the same ownership and gate boundaries.

## Quality goals

| Goal | Architectural meaning | Initial measure |
| --- | --- | --- |
| Authority preservation | Lighthouse remains the source of lifecycle and stage semantics; SoFa is never a second lifecycle engine. | Every execution references one Lighthouse route and stage; no `deploy` state exists. |
| Recoverability | A lost process cannot silently become success, and an eligible run can resume from a valid checkpoint. | Lease expiry, fencing, checkpoint sequence, cancellation, timeout, and recovery outcome are durable. |
| Isolation | One execution cannot contaminate another workspace or publish after its authority expires. | One worker and one ephemeral workspace per run, bounded concurrent capacity on Bastion, no host Docker socket, fencing rejection for stale workers. |
| Evidence integrity | Operational state, stage result, delivery evidence, telemetry, and human decisions remain distinguishable. | Each category has an explicit reference or record type. |
| Operability | The persistent Bastion can be diagnosed and maintained without turning telemetry into product truth. | Health exposes worker, lease, database, engine, gateway, capacity, and recovery signals. |
| Bounded complexity | The first slice works on one Bastion without assuming a fleet, broker, autoscaler, or microVM. | One deployable SoFa runtime, one persistent database, repository-scoped runner registrations, and bounded host concurrency. |

## Stakeholders

| Stakeholder | Concern | Authority |
| --- | --- | --- |
| Juan, the factory operator | Product intent, architecture, risk, human gates, integration, and delivery decisions. | Human decision authority. |
| Lighthouse maintainers and consumer repositories | Correct stage execution, route composition, stage results, and delivery evidence. | Lifecycle and repository workflow authority. |
| SoFa runtime operator | Scheduling, leases, recovery, worker supervision, workspace lifecycle, and runtime health. | Operational runtime authority within accepted policy. |
| Bastion operator | Persistent machine, deployed SoFa process, self-hosted runner, model gateway, and local resources. | Infrastructure authority. |
| GitHub Actions | Event delivery, workflow orchestration, job execution, artifacts, checks, and GitHub evidence. | Workflow platform authority. |
| OpenCode and model services | Agent engine execution and model inference. | Engine/provider authority; neither owns lifecycle or merge decisions. |
