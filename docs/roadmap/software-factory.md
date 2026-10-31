# Software factory delivery roadmap

Historical runtime plan. [ADR-0006](../architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md) supersedes SoFa execution ownership; consult the current architecture before using this plan.

The reconciled direction was accepted by Juan on 2026-10-24 and revised the same day to distinguish requester intake, engineer entry and per-run autonomy. Supervised triage/refine is implemented; the two-entry runtime and complete delivery loop remain pending.

The first consumer is `juangcarmona/presentations`, selected by Juan on 2026-10-24. The objective is an observable path from a GitHub story through triage/refine to a verified PR and an explicitly authorized integration, with execution on [development host]. Start with one supervised execution; expand autonomy only after the failure paths work.

## Sources of truth

| Question | Canonical source |
| --- | --- |
| What does SoFa do? | [ProductShape model](../product/README.md). The Software Factory change has been applied; artifact statuses still say `draft`. Do not interpret application of the change or validation as runtime acceptance. |
| How does SoFa implement it? | [Twelve architecture views](../architecture/README.md); durable choices in [ADRs](../architecture/09-architecture-decisions/README.md). |
| What are the delivery stages and result contracts? | Exact installed Lighthouse SDLC package, selected in `.lighthouse/sdlc.json`. |
| What work and code exist? | GitHub issues, commits, PRs and checks; SoFa stores references and observation time. |
| Which attempt owns execution? | SoFa PostgreSQL store for admission, lease, fencing and effect receipts; general checkpoint recovery remains to be demonstrated. |
| What is next? | This roadmap and its [bounded implementation backlog](implementation-backlog.md). |
| What proves delivery? | [Pilot acceptance](first-pilot.md), with real source, workflow, runtime and PR evidence. |

Lighthouse owns the baseline, harness and repository workflows. SoFa owns cross-repository selection and host operation. OpenCode executes a bounded stage; a model provider serves inference. GitHub Actions transports jobs to one runner registration per repository. These are responsibilities, not a requirement to create more repositories or packages.

## Current evidence and discrepancies

The [pilot evidence](first-pilot.md) records real triage and refinement through GitHub Actions, SoFa, PostgreSQL and isolated OpenCode. The consumer has committed Lighthouse CLI `0.9.1` and SDLC `0.9.0`; its talk improvements were preserved and published. Stage results are validated comments, and only the owner-authored, owner-triggered supervised route is enabled. Implementation through a runtime-created PR and general recovery remain unproved.

The published Lighthouse host contract requires a triage predecessor and owner comment for refine. Its declared unattended route still lists supervised human gates. [The revised contract work](https://github.com/juangcarmona/lighthouse/issues/70) addresses those gaps; installing a newer package alone will not prove event routing, publication or autonomous delivery.

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

Retain bounded clarification, one event/one route, one PR per scoped change, explicit merge decisions and unknown metrics. Adapt organization-level runner infrastructure to repository registrations on [development host]. Keep provider selection separate from GitHub transport. Do not copy workflow policies, source files or provenance into Lighthouse.

Reference checkout revisions inspected: Foundations `ca8e414`, agent-harness `c491ef6`, agentic-workflows `7fd80c5` and project-health `75d57a8`. The engine path was traced through the reusable refine worker, shared OpenCode setup and compilation script; routing and staged-effect validation were checked in their executable actions. These revisions are comparison evidence, not dependencies of SoFa.

## Two-entry delivery revision

[ADR-0005](../architecture/09-architecture-decisions/0005-separate-entry-and-autonomy.md) records the agreed direction. [CHG-FACTORY-ENTRY-AUTHORITY-001](../product/changes/completed/chg-factory-entry-authority-001/change.md) proposes the product semantics. The earlier M0–M4 milestones remain evidence milestones; their manual stage-by-stage sequence does not prescribe the revised workflow.

| Slice | Required delivery | Acceptance evidence |
| --- | --- | --- |
| E1: authorized entry | Consume the shared Lighthouse decision contract and carry verified actor, capabilities and bounded authorization through the bridge, host and publisher. | An engineer starts refine without triage; unknown actors and forged, stale or revoked authority are refused before model use. |
| E2: intake and continuation | Add requester triage, scoped clarification, dependency-aware refinement and validated label/issue effects. | Both entry channels work; a complete refinement continues under policy; a relevant reply resumes once and unrelated replies do nothing. |
| E3: implementation | Execute the harness, persist real OpenSpec files and checkpoints, publish one PR and repair it within bounds. | Close the initiating chat; receive a verified implementation PR, with fault-injection and provenance evidence. |
| E4: integration | Evaluate current-head checks and separate integration authorization. | Explicit owner merge initially; automatic merge only for separately configured classes after its acceptance cases pass. |

Track this work in [#22](https://github.com/juangcarmona/sofa/issues/22), with Lighthouse [#70](https://github.com/juangcarmona/lighthouse/issues/70) owning reusable decisions. The existing local source branches and consumer talk work remain independent. The future web is another authenticated adapter to the same boundary, not another lifecycle or an unauthenticated control path.
