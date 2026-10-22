# Documentation

This directory separates product intent, architecture, decisions, and the shared engineering lifecycle. Each kind of truth has one owner.

| Location | Owns |
| --- | --- |
| [`product/`](product/README.md) | Product intent, managed by ProductShape. Accepted intent lives in the model; proposed semantic changes live in Product Changes. |
| [`architecture/`](architecture/README.md) | Reserved arc42 structure for the architecture that will realize accepted product intent. |
| [`engineering-lifecycle.md`](engineering-lifecycle.md) | Shared Agentic SDLC lifecycle, gates, roles, and repository workflow. |
| [`tooling.md`](tooling.md) | Installed tool versions, generated-file ownership, and verified tooling limitations. |
| `adr/` | Rationale and consequences of significant architecture decisions, when one is made. |
| `reviews/` | Per-work-item retrospectives after integration. |
| `delivery/` | Durable delivery plans and Fast Forward evidence when a work item requires them. |

The accepted product baseline is the Software Factory. There is currently no application runtime or accepted implementation architecture; future architecture work must start from the product model and new decisions.
