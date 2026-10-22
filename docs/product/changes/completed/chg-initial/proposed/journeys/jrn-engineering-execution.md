---
id: JRN-ENGINEERING-EXECUTION
type: journey
title: Move a small work unit to verified delivery
status: draft
primary-actor: ACT-FACTORY-OWNER
steps: [{use-case: UC-TRIAGE-BACKLOG}, {use-case: UC-SHAPE-WORK-UNIT}, {use-case: UC-SCHEDULE-WORK}, {use-case: UC-EXECUTE-WORK-UNIT}, {use-case: UC-VERIFY-DELIVERY}]
---

## Intended Outcome

Juan can move one small GitHub work item through triage, dependency-aware execution, recovery if interrupted, verification, authorized integration, and delivery evidence.

## Entry Conditions

The source issue exists, the relevant Product Change and architecture decisions are accepted, and the selected work unit is within the allowed risk policy.

## Journey Narrative

Juan reviews source evidence, sets priority and boundaries, accepts a small work unit, supervises its leased execution, inspects deterministic and human evidence, and authorizes the external decisions that remain his responsibility.

## Variants and Branches

Missing evidence, failed checks, stale leases, unavailable workers, changed dependencies, or a required human decision stop the journey with an explicit outcome.

## Completion Conditions

The work unit has a source-linked result, recovery history where applicable, verification evidence, an explicit integration decision, and a verified delivery result or a clearly recorded limitation.
