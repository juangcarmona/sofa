---
id: UC-EXECUTE-WORK-UNIT
type: use-case
title: Inspect one repository-owned execution
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-OPENCODE, ACT-ENGINEER]
bounded-context: BC-EXECUTION
governed-by: [BR-HUMAN-DECISION-BOUNDARIES, BR-RECOVERABLE-EXECUTION, BR-LAN-PRIVILEGE-BOUNDARY, BR-WORK-AUTHORIZATION]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
---

## Goal

Inspect one repository-owned execution.

## Trigger

An owner requests a refresh or a configured source observation is refreshed.

## Preconditions

Repository identity is configured; unavailable source access is explicit.

## Main Flow

1. The repository workflow authorizes and starts its independent runtime.
2. SoFa observes stage progress and source-linked results when reachable.
3. SoFa exposes missing or conflicting evidence and the source action needed.
4. Execution and publication continue when SoFa is stopped.

## Alternative Flows

Unavailable or stale evidence is shown with a source link and observation time.

## Failure Conditions

An observation cannot authorize work or claim success without evidence.

## Postconditions

The owner can inspect source-backed state without SoFa becoming an execution dependency.
