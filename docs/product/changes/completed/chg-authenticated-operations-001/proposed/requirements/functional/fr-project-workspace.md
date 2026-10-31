---
id: FR-PROJECT-WORKSPACE
type: functional-requirement
title: Inspect a project through consistent list board and detail views
status: draft
derived-from: [UC-EXPLORE-FACTORY, UC-OPERATE-ISSUE, UC-INSPECT-PROJECT-SIGNAL]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: "Switching from project list to board retains the same filter scope and opens the same issue detail."
  - scenario: "An unknown stage remains visible without assigning a guessed lifecycle position, and moving between views does not mutate GitHub."
---

## Requirement

SoFa MUST offer project detail and list/board views of source work, active executions, human waits, results, configuration and available health signals. A board MUST explain its grouping and keep issue state, lifecycle stage and human wait distinguishable. All cards MUST open the shared issue detail. The project view MUST show source collection limitations and MUST NOT change lifecycle or priority as a side effect of navigation.

## Rationale

The owner can understand a project at a glance and then inspect the work behind it without adopting another tracker.
