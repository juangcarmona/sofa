---
id: FR-MULTI-PROJECT-SCHEDULING
type: functional-requirement
title: Schedule eligible work across projects
status: draft
derived-from: [UC-SCHEDULE-WORK, BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-EXECUTION-RUN]
verification: [{scenario: "Given ready work across two projects, the scheduler continuously measures concurrent work-unit executions, derives live capacity from workers and leases, applies project limits or the one-per-project shared default, checks dependency readiness, WIP, and risk policy, then orders eligible work by project priority and issue priority."}, {scenario: "Given Juan pauses or revokes dispatch, the scheduler does not dispatch and records the reason and scope."}, {scenario: "Given a critical security, health, or lease/fencing anomaly, the scheduler auto-pauses or revokes dispatch and records the reason and scope."}, {scenario: "Given verified recovery from a transient health failure, the scheduler may resume; given a security-critical or lease/fencing pause, it remains stopped until Juan decides to resume."}, {scenario: "Given no eligible work or a human gate, the scheduler records why it did not dispatch."}]
---

## Requirement

The product MUST continuously measure capacity in concurrent work-unit executions, derive live capacity from worker availability and active leases, and apply a project-configurable capacity limit or a shared default of one active execution per project. It MUST cap project overrides by live global worker capacity and determine eligibility using dependency readiness, WIP, and risk policy. It MUST then select or recommend eligible work across projects using project priority first and issue priority second, using the shared default when either source priority is absent. It MUST support Juan's pause/revocation authority and automatic fail-safe pause or revocation for critical security, health, or lease/fencing anomalies, recording reason and scope. It MAY auto-resume a health-only pause after verified recovery, but MUST require Juan's decision to resume security-critical or lease/fencing anomaly pauses. It MUST NOT write derived values to GitHub or bypass a human gate.

## Rationale

Personal capacity is shared across repositories, so scheduling must make trade-offs visible.
