---
id: BR-UNKNOWN-RISK
type: business-rule
title: Expose unknown risk without granting authority
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-EXECUTION, BC-SCHEDULING]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
---

## Rule

SoFa MUST show absent, stale, ambiguous or conflicting risk evidence as a limitation requiring attention. Execution eligibility and required risk gates belong to the repository workflow. SoFa MUST NOT infer approval, dispatch work or impose an unconfigured gate from its own risk assessment.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
