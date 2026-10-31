---
id: FR-PROJECT-HEALTH
type: functional-requirement
title: Explain project health and trends without invented scores
status: draft
derived-from: [UC-INSPECT-PROJECT-SIGNAL, BR-EXPLAINABLE-SIGNALS]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: "Opening a delivery trend shows contributing source records, windows and formula; missing intervals are not plotted as zero."
  - scenario: "Incomplete resource measurements and non-applicable deployment metrics retain different visible states."
---

## Requirement

SoFa MUST present source-backed signals for delivery, adoption of the harness and workflows, quality and resource consumption when their data is available. Each signal MUST have a decision purpose and drill-down to its inputs, formula or source assessment, scope, age, coverage and limitations. Trend comparisons MUST expose incompatible samples and changes in definition. A composite score, universal target or copied threshold MUST NOT be inferred from reference screenshots.

## Rationale

The owner can decide whether a project needs investigation and verify the basis of that decision.
