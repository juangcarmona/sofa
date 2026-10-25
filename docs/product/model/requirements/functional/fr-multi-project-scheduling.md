---
id: FR-MULTI-PROJECT-SCHEDULING
type: functional-requirement
title: Recommend work across projects
status: draft
derived-from: [UC-SCHEDULE-WORK, BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-EXECUTION-RUN]
verification: [{scenario: "Source observations preserve identity and limitations; stopping SoFa does not stop repository workflows; a read refresh cannot dispatch or publish work."}]
---

## Requirement

SoFa MUST aggregate repository queues, priorities, dependencies, active work and available capacity evidence. It MAY recommend the next work item with explicit provenance and unknowns. Repository workflows and the independent runtime own scheduling, capacity enforcement and dispatch. SoFa MUST NOT maintain a second execution queue or pause, resume or revoke repository execution as a side effect of observation.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.
