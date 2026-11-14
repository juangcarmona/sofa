# Documentation

This directory separates product intent, architecture, decisions, and the shared engineering lifecycle. Each kind of truth has one owner.

| Location | Owns |
| --- | --- |
| [`product/`](product/README.md) | Product intent, managed by ProductShape. Accepted intent lives in the model; proposed semantic changes live in Product Changes. |
| [`architecture/`](architecture/README.md) | Canonical arc42 views of the target architecture, observed implementation and explicit gaps. |
| [`tooling.md`](tooling.md) | Installed tool versions, generated-file ownership, and verified tooling limitations. |
| `delivery/` | Delivery plans and verification evidence. |

The accepted product baseline covers cross-repository observation, verification and authenticated source operations. The target implementation architecture is documented in the twelve arc42 sections and [ADR-0006](architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md). The Lighthouse Control Center workspace is implemented under `src/SoFa.Web/` and `src/SoFa.Modules.Workspace/`; its bounded observation presentation remains under `src/web/public/`. The historical collector under `src/observation/` is separate tooling. See the [development identity runbook](runbooks/development-identity.md) for supported startup and acceptance limits. Repository execution is independent under [ADR-0006](architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md); the old [pilot runbook](runbooks/supervised-pilot.md) remains historical.

The [Lighthouse Control Center evolution roadmap](roadmap/lighthouse-control-center.md) is the delivery entrypoint: it reconciles current source and active PRs with the proposed product/provider extension, ordered increments and rename gates. The [historical delivery roadmap](roadmap/software-factory.md) preserves superseded runtime history. The [first pilot](roadmap/first-pilot.md) uses `presentations`; architecture details remain in the twelve ArcShape sections rather than in parallel roadmap designs.

The [intervention inbox runbook](runbooks/intervention-inbox.md) explains the read-only wait projection and its source-confirmation limits.

The [repository configuration guide](repository-configuration.md) describes the bounded, read-only configuration inspection and the values it leaves unknown.
