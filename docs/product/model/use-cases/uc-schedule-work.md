---
id: UC-SCHEDULE-WORK
type: use-case
title: Compare source queues and recommend work
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: []
bounded-context: BC-SCHEDULING
governed-by: [BR-HUMAN-DECISION-BOUNDARIES, BR-RECOVERABLE-EXECUTION]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-SCHEDULING-DECISION]
---

## Goal

Compare source queues and recommend work.

## Trigger

An owner requests a refresh or a configured source observation is refreshed.

## Preconditions

Repository identity is configured; unavailable source access is explicit.

## Main Flow

1. SoFa reads repository priorities, dependencies and queue evidence.
2. SoFa presents candidates and limitations without dispatching work.
3. Juan may request an action through the repository entrypoint directly or a separately authenticated SoFa control.
4. The repository workflow owns authorization and scheduling.

## Alternative Flows

Unavailable or stale evidence is shown with a source link and observation time.

## Failure Conditions

An observation cannot authorize work or claim success without evidence.

## Postconditions

The owner can inspect source-backed state without SoFa becoming an execution dependency.
