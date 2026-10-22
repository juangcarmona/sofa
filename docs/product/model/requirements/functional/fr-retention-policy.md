---
id: FR-RETENTION-POLICY
type: functional-requirement
title: Apply standard retention with project defaults
status: draft
derived-from: [BR-RETENTION-STANDARDS, UC-MAINTAIN-RUNTIME]
uses-terms: [TERM-RETENTION-POLICY, TERM-DELIVERY-EVIDENCE]
verification: [{scenario: "Given a project with no retention override, SoFa applies the shared default policy and exposes the effective policy."}, {scenario: "Given a project override outside accepted standards, SoFa rejects or reports the invalid policy rather than silently applying it."}]
---

## Requirement

The product MUST apply a shared default retention policy when a project has no configured override. Project-specific policies MUST be validated against accepted shared standards, and the effective policy, retention pressure, deletion limitations, and evidence availability MUST be visible to the owner.

## Rationale

Defaults provide consistent behavior for every project while standards-bound overrides support legitimate differences without making retention an implicit implementation detail.
