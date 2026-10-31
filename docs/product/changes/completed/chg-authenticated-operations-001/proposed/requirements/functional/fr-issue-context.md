---
id: FR-ISSUE-CONTEXT
type: functional-requirement
title: Explain the issue and its next permitted actions
status: draft
derived-from: [UC-OPERATE-ISSUE, BR-GITHUB-SOURCE, BR-EXPLICIT-WORK-COMMANDS]
uses-terms: [TERM-WORK-UNIT, TERM-WORK-COMMAND, TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: An owner opens a refined issue and reads its description, conversation, prior results, active work, human waits and related evidence without leaving SoFa; a valid Propose action has a plain-language effect explanation.
  - scenario: A stale refinement, unavailable contract or active reservation explains why a dependent command is unavailable instead of treating a ready label as sufficient.
  - scenario: Incomplete or old source collections show their bounds and observation time; omitted records are not presented as proof that no work exists.
---

## Requirement

SoFa MUST show issue description and conversation, open or closed state, lifecycle stage and prior results, requesting and working identities when evidenced, execution progress or reservation, pending human information or decisions, and related proposals, PRs, checks and evidence. It MUST present source identities, observation time and collection limitations.

The issue view MUST explain a recommended action and other permitted actions, including Refine and Propose where supported. Unavailable actions MUST have understandable reasons. Results and material evidence summaries MUST be readable within SoFa; source links support inspection and exceptional recovery rather than the routine action journey.

## Rationale

The owner should not have to reconstruct a decision by navigating disconnected repository pages or interpreting labels.
