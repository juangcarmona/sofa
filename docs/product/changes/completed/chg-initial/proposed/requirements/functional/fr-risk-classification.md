---
id: FR-RISK-CLASSIFICATION
type: functional-requirement
title: Classify work-unit risk with evidence
status: draft
derived-from: [BR-UNKNOWN-RISK]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
verification: [{scenario: "Given deterministic and semantic evidence for a work unit, SoFa records a risk classification, confidence, and evidence references."}, {scenario: "Given critical or ambiguous risk evidence, SoFa creates a human-gate decision instead of authorizing execution."}, {scenario: "Given disagreement between deterministic and semantic assessments, SoFa uses the higher risk, preserves both assessments, and creates a human-gate decision."}]
---

## Requirement

The product MUST support risk classification from deterministic checks and semantic review. It MUST record classification, confidence, evidence references, and observed time. Critical or ambiguous results MUST route to a human gate. When assessments disagree, it MUST use the higher risk, preserve both assessments, and route to a human gate. Unknown risk MUST remain ineligible for execution until classification or an explicit decision is recorded.

## Rationale

Risk-based autonomy requires an attributable classification and a safe path for uncertainty.
