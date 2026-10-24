---
id: UC-EXECUTE-WORK-UNIT
type: use-case
title: Execute and recover one work unit
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-OPENCODE, ACT-ENGINEER]
bounded-context: BC-EXECUTION
governed-by: [BR-HUMAN-DECISION-BOUNDARIES, BR-RECOVERABLE-EXECUTION, BR-LAN-PRIVILEGE-BOUNDARY, BR-WORK-AUTHORIZATION]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
---

## Goal

Perform authorized work with durable ownership, checkpoints, cancellation, and recovery.

## Trigger

A ready work unit is dispatched under an accepted policy.

## Preconditions

The work unit is eligible, the Worker is healthy, and the control boundary authorizes the selected capability.

## Main Flow

1. SoFa verifies the effective authorization, Lighthouse decision, source and policy before the Worker acquires a lease and creates a run.
2. OpenCode follows the Lighthouse harness in the scoped workspace, producing the actual required OpenSpec artifacts, code and verification outcomes. A planning-only run stops before code application.
3. The Worker records checkpoints and verification results, and continues only within the effective autonomy and limits. Required human decisions remain explicit. Integration uses a separate authorization.
4. The run ends in success, failure, blocked, or interrupted state.

## Alternative Flows

After interruption, a new Worker validates the latest checkpoint, fencing token, workspace identity, and policy. It resumes automatically when all evidence permits; otherwise it stops for Juan.

## Failure Conditions

Lease loss, policy violation, unavailable OpenCode, or missing checkpoint evidence prevents a success claim.

## Postconditions

The run and its evidence are durable, attributable, and safe to inspect or recover.
