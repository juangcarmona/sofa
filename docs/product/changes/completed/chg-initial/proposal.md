# Product Change refinement memory

**Change:** CHG-INITIAL  
**Sitting:** Reconcile the proposed SoFa product with the maintainer's external roadmap  
**Date:** 2026-10-22  
**Status:** Draft refinement; nothing approved, applied, or archived.

## Baseline status

- `docs/product/model/` contains no accepted product artifacts.
- The former Repository Attention proposal remains a separate historical draft and preserves `BR-OBSERVE-ONLY`.
- The Attention artifacts were copied during an earlier reconciliation step, then explicitly removed from this change after the maintainer excluded Attention from the product scope.
- This change is a separate draft proposal and cannot authorize execution, GitHub writes, privileged operations, merge, deployment, or autonomous work.

## Evidence brought into this sitting

- `/home/juan/Documents/SoFa-software-factory.md`: proposed SoFa vision, principles, bounded contexts, backlog, execution, scheduling, delivery, maintenance, runtime, security, and phased roadmap.
- `/home/juan/Documents/lighthouse-sofa-execution.md`: proposed dependency order between Lighthouse and SoFa, milestones M0–M5, and the first recoverable engineering execution.
- `/home/juan/Documents/lighthouse-engineering-foundation.md`: proposed Lighthouse boundary, standards, SDLC, evidence, and dogfooding roadmap.

These documents are maintainer-supplied product and roadmap evidence. They are not an accepted ProductShape baseline.

## Reconciliation report

### Affected and aligned

| Proposed node or area | External evidence | Current draft evidence | Classification |
| --- | --- | --- | --- |
| Factory vision | End-to-end personal engineering from backlog to verified delivery and maintenance | Intended Product Outcome and Engineering Execution journey | Affected and aligned |
| Backlog authority | Ideas may enter first, but must become issues and GitHub then remains source of truth | `BR-GITHUB-SOURCE`, `FR-BACKLOG-PROJECTION`, `TERM-IDEA` | Affected and aligned |
| Work shaping | Small units with priority, dependencies, acceptance, risk, and boundaries | `FR-WORK-UNIT-SHAPING`, `TERM-WORK-UNIT` | Affected and aligned |
| Recoverable execution | Leases, checkpoints, fencing, Worker recovery, OpenCode as executor | `BR-RECOVERABLE-EXECUTION`, `FR-RECOVERABLE-EXECUTION` | Affected and aligned |
| Scheduling | Multi-project selection with priority, WIP, dependencies, leases, and risk | `BC-SCHEDULING`, `FR-MULTI-PROJECT-SCHEDULING` | Affected and aligned |
| Delivery evidence | Verification, PR, CI, human integration, and post-delivery evidence | `BC-DELIVERY`, `FR-DELIVERY-EVIDENCE` | Affected and aligned |
| Runtime operations | Health, telemetry, maintenance, retention, and recovery posture | `BC-RUNTIME-OPERATIONS`, `FR-RUNTIME-HEALTH` | Affected and aligned |
| Retention policy | Shared standards, project overrides, and a common default | `BR-RETENTION-STANDARDS`, `FR-RETENTION-POLICY`, `TERM-RETENTION-POLICY` | Affected and aligned |
| Autonomy direction | Supervised starting point with a target of standards-bound unattended delivery | `BR-HUMAN-DECISION-BOUNDARIES`, external roadmap, recorded answer | Affected and aligned |
| Portfolio and project configuration | Projects, repositories, priorities, capacity, profiles, and policies | New `BC-PORTFOLIO-PROJECT-CONFIGURATION` | Affected and aligned |
| Authority boundaries | ProductShape, ArcShape, Lighthouse, and GitHub retain their distinct roles | Change rationale and acceptance criteria | Affected and aligned |
| LAN security | Existing unauthenticated surface remains non-privileged and observe-only | `CON-TRUSTED-LAN`, `BR-LAN-PRIVILEGE-BOUNDARY` | Affected and aligned |

### Checked and not yet represented as separate product nodes

- Workspace Management and Agent Runtime are separate logical components in the external roadmap, while the current change represents OpenCode and workspace references as actors/constraints inside Execution. This sitting parks them as internal capabilities rather than separate product contexts; future product treatment remains possible.
- Evidence and Engineering Health are combined across Delivery and Runtime Operations in the current change. The external roadmap does not require a separate service or bounded context.
- The external roadmap names ideas as an upstream input to the backlog. This sitting resolves that gap: SoFa may own ideas before promotion, but GitHub owns the promoted backlog and all subsequent flow.

### Conscious exclusions

- No multi-user identity, commercial onboarding, broker, microservice decomposition, or generic orchestration platform.
- No PostgreSQL or OpenCode installation in this refinement.
- Repository Attention is not part of this product candidate; its separate draft remains historical source material.
- No product acceptance, Product Change approval/application, GitHub write, merge, deployment, or autonomous execution.

## Ranked question queue

No further product question is queued for this sitting. Exact retention durations and detailed thresholds or exceptions remain standards/configuration follow-ups, not unresolved changes to the product boundary.

## Recorded answer

### Product boundary for the first accepted baseline

**Question:** Does the product definition include the full factory outcome (triage through verified delivery) as the target product, with capabilities activated in phases, or does the first accepted baseline intentionally stop before execution and delivery?

**Answer, verbatim:** “El objetivo completo, producto completo... (lo que hayamos conseguido hasta el momento, es otra cosa...)”.

**Interpretation:** The target product is the complete Software Factory. The implementation roadmap may deliver only a slice at a time; that does not reduce the intended product scope.

### Idea intake ownership

**Question:** Is SoFa expected to manage ideas before they become GitHub work, or is GitHub the only backlog input?

**Answer, verbatim:** “Ambas, pero no es tan completo, porque las ideas pueden (DEBEN) transformarse en issues y ya seguir el flujo habitual.”

**Interpretation:** SoFa can capture and manage ideas before promotion. Promotion to or linkage with a GitHub issue is mandatory before normal backlog flow; GitHub remains authoritative after promotion.

### Project and runtime boundaries

**Question:** Should Portfolio/Project Configuration, Workspace Management, and Agent Runtime remain components inside the initial modular application, or become product contexts with explicit user-facing responsibilities?

**Answer, verbatim:** “Vale, bien, por ahora bien, no descarto que hagamos algo al respecot de wokrspace y runtime en el futuro... pero de momento vale... Ojalá lleguemos a eso algún día. (pero ólvidémoslo por ahora ;) )”.

**Interpretation:** Portfolio and Project Configuration is a product capability now. Workspace Management and Agent Runtime remain internal capabilities; their possible future product treatment is parked.

### First delivery verification

**Question:** Does verified delivery mean post-merge artifact verification only for the first slice, with production deployment deferred?

**Answer, verbatim:** “1, yo creo,verificar commit y artefactos o pipelines de main.... no crees? Me dejo algo?”.

**Interpretation:** The first pilot verifies the exact post-integration default-branch commit, relevant pipelines, generated artifacts, source identities, timestamps, human integration authorization, and limitations. Production deployment is deferred for the first pilot, while remaining part of the complete product target.

### Retention policy

**Question:** What retention/deletion policy should apply to execution metadata and delivery evidence?

**Answer, verbatim:** “1 peroc on unos estandards, si no se configura, tener algo por defecto y tenerlo así para todos.”

**Interpretation:** Every project receives an effective retention policy governed by shared standards. Project overrides are allowed within those standards; absent an override, the shared default applies. Exact durations remain a later standards/configuration decision.

### Autonomy direction

**Question:** Which low-risk operations may proceed without an individual Juan confirmation, and which risk categories always stop for him?

**Answer, verbatim:** “De momento si, pero... Al final a mi me interesa automatizar el delivery... es decir, idealmente preferiría desentenderme a saco.”

**Interpretation:** Start with supervised operation and automatic local reversible checks. The product target includes unattended delivery under explicit project policies, standards, evidence, recovery, and revocation controls. The exact risk matrix remains a future refinement question.

### Human-gate policy

**Question:** Should some operations remain permanently human-only, or should product, architecture, integration, and delivery decisions be automatable when deterministic and semantic review mechanisms classify their risk?

**Answer, verbatim:** “3, porque producto y arquitectura están super ubicados y podemos establecer mecanismos de revisión, deteministas (code ownership) y semánticos, avisos, etcétera... Es decir, un trabajo que tenga impactos \"críticos\" podemos llevarlo aun human gate...”.

**Interpretation:** SoFa does not impose a permanent human-only boundary for product, architecture, GitHub integration, or delivery. Those decisions may become autonomous when policy, deterministic checks, semantic review, code ownership, notifications, evidence, recovery, and emergency stop controls support them. Work classified as having critical impact must be routed to a human gate. The concrete critical-impact taxonomy remains a later refinement/configuration decision.

### Initial critical-impact taxonomy

**Question:** Which impact categories should initially require a human gate?

**Answer, verbatim:** “3”.

**Interpretation:** Initially, only security, privileged operations, and destructive data actions are critical by default and require a human gate. Product and architecture changes are not automatically critical; they may proceed through code ownership, deterministic checks, semantic review, notifications, evidence, recovery, and emergency-stop controls. Detailed thresholds and exceptions remain standards/configuration work.

### Unified initial baseline

**Question:** Should the Attention proposal and the Software Factory proposal be combined into one reviewed initial baseline?

**Answer, verbatim:** “2, unico base inicial...”.

**Interpretation:** At that point in the interview, the candidate was intended to contain both proposals. A later scope decision supersedes this interpretation and excludes Repository Attention from the product candidate.

### Initial project scope

**Question:** Should the first factory capability be restricted to SoFa, the five Attention repositories, or any project configured in SoFa?

**Answer, verbatim:** “3, creo que Attention no aporta nada y que podríamos quitarlo del producto...”.

**Interpretation:** The factory is not limited to SoFa or to a fixed five-repository set. It may operate on any project configured in SoFa, subject to project policy, authorization, risk controls, and the applicable engineering baseline. The related suggestion to remove Repository Attention is resolved by the following scope decision.

### Repository Attention removal

**Question:** Should Repository Attention remain in the SoFa product candidate, remain only as an internal diagnostic, or be removed from the product scope?

**Answer, verbatim:** “1”.

**Interpretation:** Repository Attention is removed from the product scope. Its existing implementation and historical draft are not deleted or approved; they remain material outside the Software Factory product candidate.

### Minimum project configuration

**Question:** What must be configured explicitly before SoFa can manage a project?

**Answer, verbatim:** “diría que 3”.

**Interpretation:** Explicit configuration requires only repository identity and default branch. SoFa discovers additional project, engineering, workflow, policy, and delivery metadata from configured sources. Discovery results, gaps, and uncertainty must remain visible; unavailable metadata must not be silently treated as confirmed.

### Partial project discovery

**Question:** What should happen when automatic project discovery is incomplete?

**Answer, verbatim:** “1”.

**Interpretation:** SoFa registers the project with the known repository and branch identity. It blocks only operations that depend on unavailable or conflicting metadata, records the reason, and keeps the known project evidence visible.

### Incomplete baseline or risk policy

**Question:** If the engineering baseline or risk policy is not discovered, should SoFa block all project activity, or only the operations that depend on that information?

**Answer, verbatim:** “1... Pero 3 no es mala tampoco, no descartemos”.

**Interpretation:** Intake, triage, and work-unit shaping remain available. Execution and delivery are blocked when they depend on missing baseline or risk-policy data. Shared defaults remain a possible future policy mechanism, but are not enabled by this draft decision.

### Missing priority defaults

**Question:** If GitHub does not provide a project or issue priority, should SoFa block scheduling, use a local override, or apply a shared default?

**Answer, verbatim:** “3”.

**Interpretation:** SoFa applies a shared default priority for scheduling when GitHub has no priority. It preserves source priority when present, records whether the value was sourced or defaulted, and does not write the default back to GitHub.

### Priority ordering

**Question:** When both project and issue priorities exist, how should scheduling combine them?

**Answer, verbatim:** “1”.

**Interpretation:** Scheduling orders eligible work by project priority first and issue priority second. Dependency readiness, risk policy, WIP, active leases, and available capacity determine eligibility before ordering.

### Capacity policy

**Question:** Should capacity be derived automatically, configured explicitly, or use both mechanisms?

**Answer, verbatim:** “3”.

**Interpretation:** SoFa derives live capacity from worker availability and active leases, applies a project-configurable capacity limit, and uses a shared default limit when no project override exists. Exact values remain configuration decisions.

### Capacity unit and default

**Question:** What unit and default should the capacity policy use?

**Answer, verbatim:** “1”.

**Interpretation:** Capacity is measured in concurrent work-unit executions. The shared default is one active execution per project, bounded by the global capacity of available workers. Project overrides cannot exceed live global worker capacity.

### Scheduler activation

**Question:** Should scheduling be continuous and automatic within policy, manual, or batch-based?

**Answer, verbatim:** “1”.

**Interpretation:** SoFa continuously evaluates eligible work and may dispatch it automatically within accepted policies. It must support pause and revocation, and each automatic decision must remain observable and recoverable.

### Scheduler resume policy

**Question:** After an automatic pause, should SoFa resume automatically or require Juan's decision?

**Answer, verbatim:** “1”.

**Interpretation:** Transient runtime-health pauses may resume automatically after verified recovery. Security-critical pauses and lease/fencing anomaly pauses remain stopped until Juan decides to resume or resolve them.

### Unknown work-unit risk

**Question:** What should happen when a work unit's risk is unknown?

**Answer, verbatim:** “1”.

**Interpretation:** The work unit may proceed through intake, triage, and shaping, but cannot be authorized for execution until its risk is classified or an explicit decision resolves the uncertainty.

### Risk classification

**Question:** How should SoFa classify work-unit risk?

**Answer, verbatim:** “1”.

**Interpretation:** Deterministic checks and semantic review may classify risk. Critical or ambiguous results go to a human gate, and classification records evidence and confidence instead of silently selecting a low-risk default.

### Risk-classification disagreement

**Question:** What should happen when deterministic and semantic risk assessments disagree?

**Answer, verbatim:** “1”.

**Interpretation:** SoFa uses the higher risk classification and routes the work to a human gate. It preserves both assessments and the evidence of disagreement.

### Execution recovery

**Question:** If a Worker fails during execution, should recovery be automatic or require Juan?

**Answer, verbatim:** “1”.

**Interpretation:** SoFa may retry or recover automatically from the latest valid checkpoint when policy, lease, fencing, and workspace evidence permit it. Any uncertainty stops recovery for a human decision.

### Recovery retry bounds

**Question:** What should happen if automatic recovery fails repeatedly?

**Answer, verbatim:** “1 (y creo que hay muchas muchas preguntas innecesarias, cuántas te quedan??)”.

**Interpretation:** Recovery uses a bounded retry count or time budget. Once the bound is reached, the run blocks for a human decision. The exact bound is configuration work. This answer also closes the detailed product-question interview for this sitting; remaining items are configuration or implementation issues.

### Maintainer acceptance of planning defaults

**Answer, verbatim:** “Ok a todo. Dale. Aprobado.”

**Interpretation:** The maintainer accepts the proposed planning defaults: critical-impact human gates, 90/365/30-day retention periods, three recovery attempts or 30 minutes, one active execution per project with no override by default, and loopback-only authenticated control. This records acceptance of the defaults for planning; it does not approve, apply, or archive the Product Change.

### Scheduler pause and revocation authority

**Question:** Who may pause or revoke the scheduler?

**Answer, verbatim:** “1”.

**Interpretation:** Juan can always pause or revoke the scheduler. SoFa may also auto-pause or revoke dispatch when it detects critical security impact, loss of required runtime health, or lease/fencing anomalies. Each action must record its reason and affected scope.

## Readiness conclusion for this sitting

The external roadmap and the current draft are substantially aligned. The factory may operate on any project configured in SoFa, subject to project policy, authorization, risk controls, and the applicable engineering baseline. Idea intake is a pre-issue stage that must promote into GitHub before normal flow, Portfolio/Project Configuration is a product capability, Workspace Management and Agent Runtime remain internal capabilities, the first pilot verifies the integrated default-branch commit, pipelines, and artifacts without requiring production deployment, retention uses standards-bound project policies with a shared default, and unattended product, architecture, integration, and delivery decisions are a long-term target under explicit risk controls. Repository Attention is excluded from the product candidate. Initially, security, privileged operations, and destructive data actions require human gates; product and architecture changes do not automatically do so. No unresolved product-boundary questions remain for this sitting. The accepted planning defaults now guide the next configuration and implementation issues. Nothing is approved, applied, or archived.
