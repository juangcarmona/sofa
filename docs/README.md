# Documentation

This directory separates product intent, architecture, decisions, and the shared engineering lifecycle. Each kind of truth has one owner.

| Location | Owns |
| --- | --- |
| [`product/`](product/README.md) | Product intent, managed by ProductShape. Accepted intent lives in the model; proposed semantic changes live in Product Changes. |
| [`architecture/`](architecture/README.md) | Canonical arc42 views of the target architecture, observed implementation and explicit gaps. |
| [`engineering-lifecycle.md`](engineering-lifecycle.md) | Shared Agentic SDLC lifecycle, gates, roles, and repository workflow. |
| [`tooling.md`](tooling.md) | Installed tool versions, generated-file ownership, and verified tooling limitations. |
| `reviews/` | Per-work-item retrospectives after integration. |
| `delivery/` | Durable delivery plans and Fast Forward evidence when a work item requires them. |

The accepted product baseline is the Software Factory. The target implementation architecture is documented in the twelve arc42 sections and [ADR-0006](architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md). The read-only observer is implemented under `src/observation/`. Repository execution is independent under [ADR-0006](architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md); the old [pilot runbook](runbooks/supervised-pilot.md) remains historical.

The [software factory roadmap](roadmap/software-factory.md) is the delivery entrypoint: it identifies the sources of truth, reconciled gaps and ordered milestones. The [first pilot](roadmap/first-pilot.md) uses `presentations`; architecture details remain in the twelve ArcShape sections rather than in parallel roadmap designs.
