---
id: UC-SHAPE-WORK-UNIT
type: use-case
title: Shape a dependency-aware work unit
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: []
bounded-context: BC-BACKLOG-TRIAGE
governed-by: [BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES]
uses-terms: [TERM-WORK-UNIT]
---

## Goal

Turn one candidate issue into the smallest independently verifiable work unit.

## Trigger

Juan selects a triaged candidate.

## Preconditions

The source item and its priority are known, and unresolved dependencies are recorded.

## Main Flow

1. Juan defines scope, acceptance criteria, risk, decision boundary, and verification.
2. SoFa records dependencies and rejects a ready state when a prerequisite is unresolved.
3. Juan accepts the work unit for scheduling.

## Alternative Flows

A candidate is split into ordered work units or returned to triage when the scope is too large.

## Failure Conditions

Missing product or architecture intent blocks readiness instead of being inferred.

## Postconditions

One bounded work unit is ready, blocked, or explicitly awaiting a decision.
