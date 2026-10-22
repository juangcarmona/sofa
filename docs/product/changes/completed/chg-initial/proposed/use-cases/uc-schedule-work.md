---
id: UC-SCHEDULE-WORK
type: use-case
title: Schedule eligible work across projects
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: []
bounded-context: BC-SCHEDULING
governed-by: [BR-HUMAN-DECISION-BOUNDARIES, BR-RECOVERABLE-EXECUTION]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-SCHEDULING-DECISION]
---

## Goal

Select the next eligible work within project priority, dependency, WIP, lease, and risk limits.

## Trigger

The scheduler's evaluation loop runs, Juan requests a recommendation, or an accepted policy permits dispatch.

## Preconditions

The work unit is ready and its dependencies, risk profile, and execution policy are known.

## Main Flow

1. SoFa filters blocked, leased, capacity-limited, and policy-ineligible work.
2. SoFa orders eligible candidates using project and issue priority.
3. SoFa dispatches within the accepted policy when authority is active, or records a recommendation when dispatch is not permitted.
4. SoFa records the decision, pauses, or revokes dispatch when policy, Juan, or a fail-safe condition requires it.

## Alternative Flows

No eligible work, a policy stop, a dependency change, a paused scheduler, revoked dispatch authority, or fail-safe condition produces an explicit idle or blocked result.

## Failure Conditions

Scheduling never silently changes source priority or bypasses a human gate.

## Postconditions

The selected work unit is queued with a traceable scheduling decision or remains visibly unselected.
