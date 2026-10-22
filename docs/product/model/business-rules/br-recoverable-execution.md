---
id: BR-RECOVERABLE-EXECUTION
type: business-rule
title: Execution is recoverable and fenced
status: draft
applies-to: [BC-EXECUTION, BC-RUNTIME-OPERATIONS]
uses-terms: [TERM-EXECUTION-RUN, TERM-WORK-UNIT]
---

## Rule

Every execution run MUST have a durable lease, ordered checkpoints, and an explicit terminal or interrupted outcome. A stale or expired owner MUST NOT publish progress after a newer fencing token exists. After Worker failure, recovery MAY retry or resume automatically from a valid checkpoint when policy, lease, fencing, and workspace evidence permit it. Automatic recovery MUST use a bounded retry count or time budget; any uncertainty or exhausted bound MUST stop for a human decision. Recovery MUST NOT claim success from missing liveness.

The planning default is a maximum of three recovery attempts or 30 minutes of cumulative recovery time, whichever is reached first.

## Rationale

Process loss and machine restarts are expected in a personal factory.

## Examples

An interrupted implementation resumes automatically after its last verified checkpoint when the workspace and fencing evidence match. A lease collision leaves the second attempt waiting or blocked rather than running concurrently; uncertain workspace identity stops for a human decision.

## Exceptions

None for a run claimed as recoverable.
