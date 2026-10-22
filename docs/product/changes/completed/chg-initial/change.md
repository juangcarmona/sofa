---
id: CHG-INITIAL
type: product-change
title: Establish SoFa as a personal Software Factory
status: applied
base-revision: '23223e5ded1e124b9133f5087adac47ac967745e'
operations:
  add:
    - ACT-JUAN
    - ACT-GITHUB
    - ACT-FACTORY-OWNER
    - ACT-GITHUB-BACKLOG
    - ACT-OPENCODE
    - BC-BACKLOG-TRIAGE
    - BC-PORTFOLIO-PROJECT-CONFIGURATION
    - BC-EXECUTION
    - BC-SCHEDULING
    - BC-DELIVERY
    - BC-RUNTIME-OPERATIONS
    - TERM-WORK-UNIT
    - TERM-IDEA
    - TERM-PROJECT
    - TERM-RETENTION-POLICY
    - TERM-EXECUTION-RUN
    - TERM-DELIVERY-EVIDENCE
    - TERM-SCHEDULING-DECISION
    - TERM-RUNTIME-HEALTH
    - BR-GITHUB-SOURCE
    - BR-PRIORITY-DEFAULTS
    - BR-CAPACITY-DEFAULTS
    - BR-SCHEDULER-SAFETY
    - BR-UNKNOWN-RISK
    - BR-HUMAN-DECISION-BOUNDARIES
    - BR-RECOVERABLE-EXECUTION
    - BR-LAN-PRIVILEGE-BOUNDARY
    - BR-RETENTION-STANDARDS
    - JRN-ENGINEERING-EXECUTION
    - UC-TRIAGE-BACKLOG
    - UC-SHAPE-WORK-UNIT
    - UC-EXECUTE-WORK-UNIT
    - UC-SCHEDULE-WORK
    - UC-VERIFY-DELIVERY
    - UC-MAINTAIN-RUNTIME
    - CON-FACTORY-RUNTIME
    - CON-TRUSTED-LAN
    - FR-BACKLOG-PROJECTION
    - FR-PROJECT-DISCOVERY
    - FR-RISK-CLASSIFICATION
    - FR-WORK-UNIT-SHAPING
    - FR-RECOVERABLE-EXECUTION
    - FR-MULTI-PROJECT-SCHEDULING
    - FR-DELIVERY-EVIDENCE
    - FR-RUNTIME-HEALTH
    - FR-RETENTION-POLICY
    - QR-EXECUTION-RECOVERY
    - QR-EXECUTION-SECURITY
  modify: []
  remove: []
---

## Problem

SoFa has no accepted Product Definition. This `CHG-INITIAL` proposal establishes the complete Software Factory product: backlog, execution, delivery, scheduling, and maintenance capabilities. The earlier Repository Attention proposal is retained separately as historical draft material and is not part of this product scope.

## Intended Product Outcome

SoFa is a personal Software Factory in which Juan can capture ideas, promote them to GitHub issues, select GitHub work from any configured project, shape a small work unit, run an authorized recoverable engineering loop, inspect evidence through verification and delivery, and schedule work across projects within explicit risk and human-decision boundaries.

This change proposes the Software Factory outcome as SoFa's initial product definition. Execution, GitHub writes, autonomous work, and delivery require this change and the relevant human decisions to be accepted first.

## Rationale

The attention slice provides visibility but cannot close the loop from backlog to verified delivery. A small vertical factory slice gives SoFa durable evidence and recovery before broader scheduling or autonomy is attempted. GitHub remains the backlog authority, ProductShape remains the product authority, ArcShape remains the architecture authority, and Lighthouse supplies the shared engineering baseline rather than becoming SoFa's orchestration layer.

### Refinement decision — earlier baseline proposal

Juan's answer: “2, unico base inicial...”.

The earlier interview answer proposed combining the Attention and Software Factory proposals. The subsequent product discussion established that the Software Factory is the true initial product; Repository Attention is therefore retained only as historical material outside this change.

### Refinement decision — product scope

Juan's answer: “El objetivo completo, producto completo... (lo que hayamos conseguido hasta el momento, es otra cosa...)”.

The product definition therefore targets the complete Software Factory outcome described by the roadmap, from work intake through verified delivery and maintenance. Delivery is phased; the current implementation slice does not narrow the product definition or authorize execution.

### Refinement decision — idea intake

Juan's answer: “Ambas, pero no es tan completo, porque las ideas pueden (DEBEN) transformarse en issues y ya seguir el flujo habitual.”

SoFa may capture and manage ideas before promotion, but it does not create a second authoritative backlog. A promoted idea MUST become or link to a GitHub issue; GitHub then owns the normal backlog, triage, work-unit, execution, and delivery flow.

### Refinement decision — project and runtime boundaries

Juan's answer: “Vale, bien, por ahora bien, no descarto que hagamos algo al respecot de wokrspace y runtime en el futuro... pero de momento vale... Ojalá lleguemos a eso algún día. (pero ólvidémoslo por ahora ;) )”.

Portfolio and Project Configuration is a product capability. Workspace Management and Agent Runtime remain internal factory capabilities for now; future product treatment is deliberately parked rather than rejected.

### Refinement decision — first delivery verification

Juan's answer: “1, yo creo,verificar commit y artefactos o pipelines de main.... no crees? Me dejo algo?”.

The first pilot verifies the exact post-integration commit on the default branch, the relevant pipelines and their runs, and the generated artifact identities and results. It also records source links, timestamps, human integration authorization, and any unavailable or stale evidence. Production deployment is not required for the first pilot; it remains part of the complete product target for a later delivery capability.

### Refinement decision — retention policy

Juan's answer: “1 peroc on unos estandards, si no se configura, tener algo por defecto y tenerlo así para todos.”

Every project has an effective retention policy governed by shared standards. A project may configure an override within those standards; if it does not, the shared default applies. Exact durations remain a later standards/configuration decision.

### Refinement decision — autonomy direction

Juan's answer: “De momento si, pero... Al final a mi me interesa automatizar el delivery... es decir, idealmente preferiría desentenderme a saco.”

The initial safety boundary is supervised: only local, reversible, deterministic operations may proceed automatically. The complete product target includes progressively unattended delivery when a project-specific risk policy, Lighthouse standards, evidence, and recovery controls make it eligible. Autonomy remains explicit, configured, attributable, and revocable; it is never inferred from a green check or a generic product setting.

### Refinement decision — human-gate policy

Juan's answer: “3, porque producto y arquitectura están super ubicados y podemos establecer mecanismos de revisión, deteministas (code ownership) y semánticos, avisos, etcétera... Es decir, un trabajo que tenga impactos \"críticos\" podemos llevarlo aun human gate...”.

The product does not impose a permanent human-only boundary for product, architecture, GitHub integration, or delivery. These decisions may become autonomous when deterministic checks, semantic review, code ownership, notifications, evidence, recovery, and emergency-stop controls support them. Work with critical impact MUST route to a human gate. The concrete critical-impact taxonomy remains a later standards and configuration decision.

### Refinement decision — initial critical-impact taxonomy

Juan's answer: “3”.

Initially, security, privileged operations, and destructive data actions are critical by default and MUST route to a human gate. Product and architecture changes are not automatically critical; they may proceed through code ownership, deterministic checks, semantic review, notifications, evidence, recovery, and emergency-stop controls. Detailed thresholds and exceptions remain standards and configuration work.

### Refinement decision — initial project scope

Juan's answer: “3, creo que Attention no aporta nada y que podríamos quitarlo del producto...”.

The factory is not limited to SoFa or to a fixed five-repository set. It may operate on any project configured in SoFa, subject to project policy, authorization, risk controls, and the applicable engineering baseline. Repository Attention is excluded from the product scope. Its existing implementation and historical draft remain preserved, but are not part of this initial product definition.

### Refinement decision — minimum project configuration

Juan's answer: “diría que 3”.

A project requires only a repository identity and its default branch to be configured explicitly. SoFa discovers additional project, engineering, workflow, policy, and delivery metadata from the configured sources. Discovery results, gaps, and uncertainty must remain visible; SoFa must not silently treat unavailable metadata as confirmed.

### Refinement decision — partial project discovery

Juan's answer: “1”.

An incomplete discovery does not prevent project registration. SoFa MUST block only the operations that depend on unavailable or conflicting metadata, record the reason, and keep the project and its known evidence visible.

### Refinement decision — incomplete baseline or risk policy

Juan's answer: “1... Pero 3 no es mala tampoco, no descartemos”.

Intake, triage, and work-unit shaping remain available when the engineering baseline or risk policy has not been discovered. Execution and delivery MUST remain blocked when they depend on missing baseline or risk-policy data. Shared defaults remain a possible future policy mechanism, but are not enabled by this draft decision.

### Refinement decision — missing priority defaults

Juan's answer: “3”.

When GitHub does not provide a project or issue priority, SoFa applies the shared default priority policy for scheduling. It preserves any source priority, records whether the value was sourced or defaulted, and does not write the default back to GitHub.

### Refinement decision — priority ordering

Juan's answer: “1”.

Scheduling orders eligible work by project priority first and issue priority second. Dependency readiness, risk policy, WIP, active leases, and available capacity determine eligibility before that ordering is applied.

### Refinement decision — capacity policy

Juan's answer: “3”.

SoFa derives live capacity from worker availability and active leases, applies a project-configurable capacity limit, and uses a shared default limit when no project override exists. The exact default and limit values remain configuration decisions.

### Refinement decision — capacity unit and default

Juan's answer: “1”.

Capacity is measured in concurrent work-unit executions. The shared default is one active execution per project, subject to the global limit imposed by available workers. A project override MUST NOT exceed live global worker capacity.

### Refinement decision — scheduler activation

Juan's answer: “1”.

The scheduler continuously evaluates eligible work and may dispatch it automatically within the accepted policies. It MUST support pausing and revoking dispatch authority, and every automatic decision MUST remain observable and recoverable.

### Refinement decision — scheduler pause and revocation authority

Juan's answer: “1”.

Juan can always pause or revoke the scheduler. SoFa may also auto-pause or revoke dispatch when it detects critical security impact, loss of required runtime health, or lease/fencing anomalies. Each action MUST record its reason and affected scope.

### Refinement decision — scheduler resume policy

Juan's answer: “1”.

Transient runtime-health pauses MAY resume automatically after the health condition is verified as recovered. Security-critical pauses and lease/fencing anomaly pauses MUST remain stopped until Juan decides to resume or otherwise resolve them.

### Refinement decision — unknown work-unit risk

Juan's answer: “1”.

A work unit with unknown risk may proceed through intake, triage, and shaping, but MUST NOT be authorized for execution until its risk is classified or an explicit decision resolves the uncertainty.

### Refinement decision — risk classification

Juan's answer: “1”.

Deterministic checks and semantic review MAY classify work-unit risk. Critical or ambiguous results MUST route to a human gate; classification MUST record its evidence and confidence rather than silently selecting a low-risk default.

### Refinement decision — risk-classification disagreement

Juan's answer: “1”.

When deterministic and semantic risk assessments disagree, SoFa MUST use the higher risk classification and route the work to a human gate. It MUST retain both assessments and the disagreement evidence.

### Refinement decision — execution recovery

Juan's answer: “1”.

After Worker failure, SoFa MAY retry or recover automatically from the latest valid checkpoint when the execution policy, lease, fencing, and workspace evidence permit it. Any uncertainty MUST stop recovery for a human decision.

### Refinement decision — recovery retry bounds

Juan's answer: “1 (y creo que hay muchas muchas preguntas innecesarias, cuántas te quedan??)”.

Automatic recovery MUST use a bounded retry count or time budget. When the bound is reached, the run MUST become blocked and request a human decision. The exact bound is configuration work, not another product-boundary decision.

## Affected Product Areas

The change adds Portfolio and Project Configuration, Backlog and Triage, Execution, Scheduling, Verification and Delivery, and Runtime Operations as separate bounded contexts. It adds idea promotion into GitHub, GitHub backlog evidence, work-unit dependencies and priority, recoverable execution, human gates, risk-based autonomy, multi-project selection, and health/maintenance visibility. Workspace Management and Agent Runtime remain internal capabilities for now. It introduces no multi-tenant identity or commercial offering.

## Open Questions

No unresolved product-boundary questions remain for this sitting. The accepted planning defaults are recorded in the proposed rules; implementation still needs issues for exact policy storage, enforcement, telemetry, and control-boundary authentication.

## Product Acceptance

- The product candidate does not include Repository Attention; its earlier draft is historical source material only.
- A project requires only repository identity and default branch as explicit initial configuration; additional metadata is discovered and evidenced.
- Partial discovery does not hide or invalidate a project; only dependent operations are blocked with an explicit reason.
- Missing baseline or risk policy does not block intake, triage, or shaping; it blocks dependent execution and delivery operations.
- Missing GitHub priority uses the shared default for scheduling and is recorded as derived rather than source data.
- Eligible work is ordered by project priority, then issue priority; dependencies, risk, WIP, leases, and capacity are eligibility checks.
- Capacity combines live worker and lease state with a project limit, falling back to a shared default when no override exists.
- Capacity is measured in concurrent work-unit executions; the shared default is one active execution per project, bounded by available workers.
- Unknown-risk work may be shaped but cannot be authorized for execution until classified or explicitly resolved.
- Deterministic and semantic mechanisms may classify risk; critical or ambiguous results require a human gate with evidence and confidence recorded.
- Disagreement between deterministic and semantic risk assessments uses the higher risk and requires a human gate, preserving both assessments.
- Worker failure may trigger automatic checkpoint recovery when policy and evidence permit; uncertainty requires a human decision.
- Recovery uses a bounded retry count or time budget, then blocks for a human decision.
- Scheduling continuously evaluates eligible work and supports policy-controlled automatic dispatch, pause, and revocation.
- Juan always retains pause and revocation authority; critical security, health, and lease anomalies may trigger an automatic fail-safe pause or revocation.
- GitHub is the source of backlog and repository evidence; local SoFa state is a coordination projection with source identity and timestamps.
- Work units carry priority, dependencies, acceptance criteria, risk, and a decision boundary.
- Execution can be interrupted and recovered without duplicate active ownership or an unsupported success claim.
- Verification distinguishes deterministic checks, source evidence, human decisions, and unknowns.
- No unauthenticated read surface can launch work, mutate GitHub, use privileged commands, approve integration, or claim delivery.
- The definition does not authorize implementation, infrastructure installation, Product Change approval/application, merge, deployment, or autonomous work.

## Out of Scope

Implementation, PostgreSQL or OpenCode installation, application authentication implementation, detailed database schema, deployment automation, a generic orchestration framework, brokers, microservices, multi-user identity, commercial onboarding, and changes to accepted product intent. Those require approved follow-on decisions.
