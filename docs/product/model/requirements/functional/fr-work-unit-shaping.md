---
id: FR-WORK-UNIT-SHAPING
type: functional-requirement
title: Inspect and request bounded work in repository workflows
status: draft
derived-from: [UC-SHAPE-WORK-UNIT, BR-HUMAN-DECISION-BOUNDARIES]
uses-terms: [TERM-WORK-UNIT]
verification: [{scenario: "Source observations preserve identity and limitations; stopping SoFa does not stop repository workflows; a read refresh cannot dispatch or publish work."}]
---

## Requirement

SoFa MUST show work-unit scope, acceptance criteria, dependencies, risk and decision boundaries from repository evidence. It MAY let an authenticated owner submit a shaping or refinement request through the same GitHub workflow entrypoint available without SoFa. Readiness belongs to the repository workflow; SoFa MUST NOT create a parallel readiness decision or run the agent itself.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.
