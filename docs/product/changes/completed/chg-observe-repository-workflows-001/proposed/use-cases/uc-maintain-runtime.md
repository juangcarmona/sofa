---
id: UC-MAINTAIN-RUNTIME
type: use-case
title: Inspect health of independent execution infrastructure
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: []
bounded-context: BC-RUNTIME-OPERATIONS
governed-by: [BR-RECOVERABLE-EXECUTION, BR-LAN-PRIVILEGE-BOUNDARY]
uses-terms: [TERM-EXECUTION-RUN, TERM-RUNTIME-HEALTH, TERM-RETENTION-POLICY]
---

## Goal

Inspect health of independent execution infrastructure.

## Trigger

An owner requests a refresh or a configured source observation is refreshed.

## Preconditions

Repository identity is configured; unavailable source access is explicit.

## Main Flow

1. SoFa reads available runner and runtime observations.
2. SoFa reports source, age, failure and missing evidence.
3. Juan operates the independent runtime through its own tools.
4. Observation does not mutate execution or repository work.

## Alternative Flows

Unavailable or stale evidence is shown with a source link and observation time.

## Failure Conditions

An observation cannot authorize work or claim success without evidence.

## Postconditions

The owner can inspect source-backed state without SoFa becoming an execution dependency.
