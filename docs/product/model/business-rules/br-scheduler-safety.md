---
id: BR-SCHEDULER-SAFETY
type: business-rule
title: Observe repository scheduling and stop decisions
status: draft
applies-to: [BC-SCHEDULING, BC-EXECUTION, BC-RUNTIME-OPERATIONS]
uses-terms: [TERM-EXECUTION-RUN, TERM-SCHEDULING-DECISION]
---

## Rule

Repository workflows and the independent runtime own admission, concurrency, cancellation and recovery limits. SoFa MUST surface their decisions and anomalies with source, scope and time. It MUST NOT autonomously pause, resume or revoke work. An authenticated user control may request a supported source-side cancellation; the repository records and enforces it. SoFa unavailability MUST NOT alter execution eligibility.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
