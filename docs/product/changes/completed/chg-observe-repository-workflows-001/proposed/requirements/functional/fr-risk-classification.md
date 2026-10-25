---
id: FR-RISK-CLASSIFICATION
type: functional-requirement
title: Verify risk decisions from repository evidence
status: draft
derived-from: [BR-UNKNOWN-RISK]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
verification: [{scenario: "Source observations preserve identity and limitations; stopping SoFa does not stop repository workflows; a read refresh cannot dispatch or publish work."}]
---

## Requirement

SoFa MUST expose repository risk classifications, their confidence, evidence, age and applicable human decisions. It MAY flag missing, conflicting or stale assessments for review. It MUST NOT turn a local assessment into workflow authorization or impose an implicit execution gate. Required checks and stopping rules belong to the repository workflow and Lighthouse contract.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.
