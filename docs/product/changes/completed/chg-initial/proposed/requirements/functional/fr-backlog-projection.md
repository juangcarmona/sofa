---
id: FR-BACKLOG-PROJECTION
type: functional-requirement
title: Reconcile GitHub backlog evidence
status: draft
derived-from: [UC-TRIAGE-BACKLOG, BR-GITHUB-SOURCE]
uses-terms: [TERM-IDEA, TERM-WORK-UNIT]
verification: [{scenario: "Given a configured GitHub project and issue, a refresh preserves source identity, priority, status, timestamps, links, and collection limitations without creating duplicates."}, {scenario: "Given an issue without a GitHub priority, the projection applies the shared default for scheduling, marks it as derived, and does not write it back to GitHub."}, {scenario: "Given unavailable GitHub evidence, the projection reports the gap and does not mark the backlog clear."}]
---

## Requirement

The product MUST allow an idea to be captured before promotion, and MUST require that idea to become or link to a GitHub issue before it enters the normal backlog flow. It MUST read configured GitHub backlog evidence, preserve project and issue priority when present, apply the shared default priority policy when priority is absent, record the value's provenance, reconcile repeated observations by source identity, and expose missing or unavailable evidence explicitly. It MUST NOT replace GitHub as the backlog source of truth after promotion or write a derived default back to GitHub.

## Rationale

Triage needs a stable idea intake and coordination view without creating a second authoritative tracker.
