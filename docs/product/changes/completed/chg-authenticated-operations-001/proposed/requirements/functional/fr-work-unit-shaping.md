---
id: FR-WORK-UNIT-SHAPING
type: functional-requirement
title: Inspect and request bounded work in repository workflows
status: draft
derived-from: [UC-SHAPE-WORK-UNIT, UC-OPERATE-ISSUE, BR-HUMAN-DECISION-BOUNDARIES, BR-EXPLICIT-WORK-COMMANDS]
uses-terms: [TERM-WORK-UNIT, TERM-WORK-COMMAND]
verification:
  - scenario: An issue detail preserves scope, acceptance criteria, dependencies, risk and current decision boundaries from source evidence.
  - scenario: An eligible authenticated owner explicitly requests refinement through the normal repository path; a refreshed page alone never requests work.
  - scenario: Revised scope makes earlier refinement stale under the repository contract and SoFa explains the need for new refinement rather than treating an old Ready verdict as current.
---

## Requirement

SoFa MUST show work-unit scope, acceptance criteria, dependencies, risk and decision boundaries from repository evidence. It MUST let the authenticated and source-authorized owner request refinement through the same repository workflow entrypoint available without SoFa, when the current contract permits it. Readiness belongs to the repository workflow; SoFa MUST NOT create a parallel readiness decision or run the agent itself. A prior refinement remains evidence for its original scope and revision, not automatic authorization for changed work.

## Rationale

An explicit issue action turns the source-backed factory view into a useful operating surface without replacing engineering authority.
