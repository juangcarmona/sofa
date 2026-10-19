# Documentation

This directory separates product intent, architecture, decisions, and the shared engineering lifecycle. Each kind of truth has one owner.

| Location | Owns |
| --- | --- |
| [`product/`](product/README.md) | Product intent, managed by ProductShape. Accepted intent lives in the model; proposed semantic changes live in Product Changes. |
| [`architecture/`](architecture/README.md) | How the system realizes accepted product intent, documented with ArcShape and arc42. |
| [`engineering-lifecycle.md`](engineering-lifecycle.md) | Shared Agentic SDLC lifecycle, gates, roles, and repository workflow. |
| [`tooling.md`](tooling.md) | Installed tool versions, generated-file ownership, and verified tooling limitations. |
| `adr/` | Rationale and consequences of significant architecture decisions, when one is made. |
| `reviews/` | Per-work-item retrospectives after integration. |
| `delivery/` | Durable delivery plans and Fast Forward evidence when a work item requires them. |

SoFa is currently a scaffold. Product and architecture statements remain intentionally unfilled until the product is defined.
