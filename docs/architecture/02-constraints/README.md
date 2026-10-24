---
title: Architecture constraints
arc42-section: "02"
description: Technical, organizational, and convention constraints that shape the architecture.
---

# 2. Architecture constraints

## Technical constraints

The current repository is a scaffold. The inspected `src/` and `tests/` contain security tooling and tests, but no .NET application, Aspire AppHost, PostgreSQL integration, Worker, OpenCode adapter, gateway integration, self-hosted runner component, or SoFa workflow. The files under `docs/roadmap/` are therefore proposals and planning evidence, not proof of installed infrastructure.

The target retains the existing runtime proposal where it is supported by product and Lighthouse evidence:

- one modular .NET 10 application composed locally with Aspire;
- PostgreSQL as the minimum durable operational control-plane store;
- an independently supervised Worker inside the SoFa deployment;
- OpenCode as an external execution engine behind a model gateway; and
- one persistent Bastion host with ephemeral execution workspaces.

These technology choices remain implementation targets until their implementation phase is authorized. They do not authorize adding code in this architecture-alignment change.

The consumer repository selects Lighthouse SDLC package `0.8.0` through `.lighthouse/sdlc.json` and Lighthouse package `0.9.0` in the root manifest. The distributed route catalog defines the six stages and the routes `supervised`, `plan-only`, `unattended`, `hotfix`, and `stacked`; the route determines which stages and prerequisites are eligible. The current distributed package must be treated as the contract authority during implementation, including its `stage-result` schema and generated workflow assets.

GitHub Actions is the initial transport and orchestration boundary. A job is assigned to the capability `[self-hosted, linux, lighthouse-sdlc]`; the SoFa deployment on Bastion supplies that capability. No central SoFa webhook is required by the current evidence.

## Organizational constraints

- Lighthouse owns stages, routes, gates, skills, `stage-result` contracts, gh-aw workflows, router/workers, explicit composition, execution contracts, and delivery evidence.
- SoFa owns cross-project scheduling, leases, checkpoints, fencing, recovery, capacity, concurrency, runner supervision, workspace lifecycle, isolation, operational persistence, engine integration, runtime health, and Bastion operation.
- Bastion owns the persistent machine, deployed processes, local runner, model gateway, model services, and execution resources.
- ProductShape owns product intent; ArcShape architecture documentation explains the realized shape; ADRs preserve durable rationale.
- The runner is a deployable SoFa component. It is not a Lighthouse repository and must not become a second lifecycle implementation.

## Conventions

Lighthouse semantics are fixed at the integration boundary:

| Lighthouse contract | Meaning for SoFa |
| --- | --- |
| `triage` | Produces the `triaged` gate when its stage result is valid. |
| `refine` | Produces the `ready` gate when its stage result is valid. |
| `propose` | Produces the `planned` gate when its stage result is valid. |
| `implement` | Produces the `done` gate when its stage result is valid. |
| `integrate` | Produces an integration result and merge preparation; it does not imply that SoFa may merge. |
| `review` | Produces a review result with no new gate. |

`ready`, `planned`, and `done` are gates, not stages. `ff` is explicit composition of the configured route. A failed or blocked prerequisite stops the composition. SoFa must preserve this behavior and must not introduce `deploy` or any other lifecycle state.
