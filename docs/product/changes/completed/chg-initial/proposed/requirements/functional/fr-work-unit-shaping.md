---
id: FR-WORK-UNIT-SHAPING
type: functional-requirement
title: Shape small dependency-aware work units
status: draft
derived-from: [UC-SHAPE-WORK-UNIT, BR-HUMAN-DECISION-BOUNDARIES]
uses-terms: [TERM-WORK-UNIT]
verification: [{scenario: "Given a large or ambiguous issue, Juan can record one bounded work unit with scope, acceptance criteria, priority, risk, dependencies, verification, and a decision boundary."}, {scenario: "Given an unresolved dependency or missing product decision, the work unit cannot be marked ready."}, {scenario: "Given a missing engineering baseline or risk policy, the work unit can be shaped but cannot be authorized for dependent execution or delivery."}, {scenario: "Given unknown work-unit risk, the work unit can be shaped but cannot be authorized for execution until risk is classified or explicitly resolved."}, {scenario: "Given deterministic and semantic risk evidence, SoFa records a classification and confidence; given critical or ambiguous evidence, it routes to a human gate."}]
---

## Requirement

The product MUST support one or more small work units per source issue, with explicit dependencies, priority, scope, acceptance criteria, risk, and decision boundary. Readiness MUST show unresolved information instead of inferring it. Missing engineering baseline or risk-policy data MUST not prevent shaping, but MUST prevent authorization of dependent execution or delivery operations. Unknown risk MUST prevent execution authorization until classified or explicitly resolved.

## Rationale

Small bounded units are the scheduling and recovery granularity.
