---
id: FR-INTERVENTION-INBOX
type: functional-requirement
title: Show and resolve work needing the owner
status: draft
derived-from: [UC-RESOLVE-INTERVENTION, BR-EXPLICIT-WORK-COMMANDS]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: "A pending question opens the actual source question and an explicitly labeled reply-and-continue action when supported."
  - scenario: "A failed or uncertain response leaves the item unresolved; a later source-confirmed resolution removes the current wait while preserving history."
---

## Requirement

SoFa MUST provide an intervention inbox identifying the issue or run, type and reason for the wait, requested information or decision, responsible actor when known, age, source and available actions. Repeated observations MUST reconcile the same source request without duplicating it. Resolution MUST come from refreshed source evidence. Ordering MUST be explainable and MUST NOT invent urgency or authorization.

## Rationale

The owner needs a reliable answer to what needs intervention, not only a list of labeled issues.
