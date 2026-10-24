# Software factory delivery roadmap

The reconciled design was accepted by Juan on 2026-10-24. Runtime implementation remains pending.

The first consumer is `juangcarmona/presentations`, selected by Juan on 2026-10-24. The objective is an observable path from a GitHub story through triage/refine to a verified PR and an explicitly authorized integration, with execution on Bastion. Start with one supervised execution; expand autonomy only after the failure paths work.

## Sources of truth

| Question | Canonical source |
| --- | --- |
| What does SoFa do? | [ProductShape model](../product/README.md). The Software Factory change has been applied; artifact statuses still say `draft`. Do not interpret application of the change or validation as runtime acceptance. |
| How does SoFa implement it? | [Twelve architecture views](../architecture/README.md); durable choices in [ADRs](../architecture/09-architecture-decisions/README.md). |
| What are the delivery stages and result contracts? | Exact installed Lighthouse SDLC package, selected in `.lighthouse/sdlc.json`. |
| What work and code exist? | GitHub issues, commits, PRs and checks; SoFa stores references and observation time. |
| Which attempt owns execution? | Future SoFa operational store: admission, lease, fencing, checkpoints and effect receipts. |
| What is next? | This roadmap and its [bounded implementation backlog](implementation-backlog.md). |
| What proves delivery? | [Pilot acceptance](first-pilot.md), with real source, workflow, runtime and PR evidence. |

Lighthouse owns the baseline, harness and repository workflows. SoFa owns cross-repository selection and host operation. OpenCode executes a bounded stage; a model provider serves inference. GitHub Actions transports jobs to one runner registration per repository. These are responsibilities, not a requirement to create more repositories or packages.

## Current evidence and discrepancies

At SoFa commit `af8afec`, `src/` contains repository security tooling, not a factory application. No SoFa supervisor, PostgreSQL integration, execution bridge or engine adapter is implemented. The existing runner and OpenCode installation observations in [deployment](../architecture/07-deployment-view/README.md) are dated observations, not readiness certification for the pilot.

The inspected Lighthouse baseline is `434dfab`, with CLI `0.9.0` and SDLC `0.8.0`. Its gh-aw workers run Copilot; its router is manual-only. It needs consumer package resolution, strict completion/approval validation, automatic intake and a host handoff. Those changes belong to the [Lighthouse execution roadmap](https://github.com/juangcarmona/lighthouse/blob/main/docs/roadmap/factory-reconciliation.md), proposed in the paired reconciliation branch until merged.

The local `presentations` adoption passes required Lighthouse checks, but its newly installed configuration and workflows are uncommitted. ProductShape/ArcShape, labels and evidence are unconfigured; roles and adapters are empty; there is no root `verify` script. Readiness must be rechecked on the committed pilot revision. Preserve its current application and talk work during adoption.

Existing SoFa delivery instructions also use Agentic SDLC `status:*` lifecycle labels while Lighthouse owns a different stage/gate catalog. This is an adoption migration to resolve once, with explicit old-to-new state mapping and provider-supported generation. Do not run two label writers or edit generated skills to hide the conflict.

## Milestones

| Milestone | Owner and dependency | Demonstrable exit |
| --- | --- | --- |
| M0: reproducible consumer | Lighthouse L1; consumer preparation | Packed workflows resolve in a clean consumer, strict continuation tests pass, and committed `presentations` has verification, authorities or explicit exceptions, labels and its runner registration. |
| M1: real triage and refinement | Lighthouse L2/L3 plus SoFa bridge, minimal store and one engine worker | A story in `presentations` automatically reaches a persisted triage/refine result or a specific clarification; owner approval remains explicit at required gates. The refined story is actually visible in GitHub. |
| M2: recoverable implementation | SoFa lease/checkpoint/publication work; Lighthouse L4 | After approved proposal, one scoped change produces a real PR. Forced interruption, duplicate event, stale worker, timeout and uncertain publication pass the pilot's failure cases. |
| M3: verified delivery | Repository checks, independent review and explicit integration authorization | The selected PR merges only under policy; the exact default-branch commit and post-merge checks are recorded. No production deployment is inferred. |
| M4: controlled expansion | M3 evidence and operator policy decision | Add a second personal repository; prove shared admission and fairness; measure before raising concurrency towards five or six. Enable unattended work only for an explicitly accepted low-risk class. |

M0/M1 are the first proof of value. M2/M3 establish recoverable delivery. M4 is the medium-term evolution. Use acceptance evidence to sequence work rather than promise dates before engine authentication, consumer state and runner capacity are measured.

## Rollout policy

The pilot uses supervised gates, one globally active execution and the product default of one execution per project. Security impact, privileged operations and destructive data actions keep human decisions. Existing recovery and retention rules remain owned by the [product model](../product/model/business-rules/); the runtime must enforce them and report unsupported controls. Missing usage information is unknown, not zero cost; do not promise a hard monetary cap without a provider mechanism that enforces it.

The operator may pause admission and cancel a run. Raising capacity or replacing a human gate is a recorded policy change tied to scope and evidence. Before M4, demonstrate correct completion, intentional human waits, rejection of stale publication and recovery from host/worker failure. A digest should report actionable changes and required decisions rather than repeatedly announcing unchanged waits.

## Reference mechanisms

The reviewed [Platform documentation](https://platform.plainconcepts.dev/docs/how-we-work), [presentation](https://platform.ckgrafico.com/) and local Foundations, agent-harness, agentic-workflows and project-health sources support four useful mechanisms: deterministic event routing, shared installed instructions, validated outward effects and measured delivery. The presentation is explanatory; code and tests establish implemented behavior. Its four repositories need not become four repositories here: Lighthouse covers the reusable baseline/harness/workflow contract and SoFa covers personal operation and evidence consumption.

Retain bounded clarification, one event/one route, one PR per scoped change, explicit merge decisions and unknown metrics. Adapt organization-level runner infrastructure to repository registrations on Bastion. Keep provider selection separate from GitHub transport. Do not copy workflow policies, source files or provenance into Lighthouse.

Reference checkout revisions inspected: Foundations `ca8e414`, agent-harness `c491ef6`, agentic-workflows `7fd80c5` and project-health `75d57a8`. The engine path was traced through the reusable refine worker, shared OpenCode setup and compilation script; routing and staged-effect validation were checked in their executable actions. These revisions are comparison evidence, not dependencies of SoFa.
