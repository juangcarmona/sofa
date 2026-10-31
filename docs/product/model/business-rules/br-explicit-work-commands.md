---
id: BR-EXPLICIT-WORK-COMMANDS
type: business-rule
title: Commands express intent without implying completion or approval
status: draft
applies-to: [UC-OPERATE-ISSUE, BC-BACKLOG-TRIAGE, BC-EXECUTION, BC-DELIVERY]
uses-terms: [TERM-WORK-COMMAND, TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
---

## Rule

SoFa MUST distinguish issue state and lifecycle stage, requested command, source execution state, active work or reservation, and a wait for information, review or approval. It MUST preserve triage → refine → propose → implement → integrate → review; approvals are advancement conditions, not additional stages.

Command availability MUST follow the repository's current capabilities, permissions, evidence, reservations and shared Lighthouse rules. A recommendation, fixed stage sequence or isolated label MUST NOT establish authority. SoFa MUST recheck dependent state before a write and explain denial or uncertainty.

Submitting a command MUST NOT imply source admission, successful execution, an existing artifact or approval. Repeated submissions and uncertain results MUST be resolved without silently authorizing duplicate work. Ordinary comments MUST NOT implicitly request continuation; a reply that requests continuation MUST identify that effect before submission under the source's shared contract.

## Rationale

The owner must understand what an action authorizes and what actually happened, including on a slow or interrupted mobile connection.

## Examples

After valid refinement, Propose may be recommended. It still requires current source evidence and no conflicting reservation. A submitted request can remain uncertain while reconciliation determines whether the source admitted it. A successful proposal leaves content review and implementation authority separate.

## Exceptions

None. Unsupported source semantics must be reported rather than approximated by a second SoFa workflow policy.
