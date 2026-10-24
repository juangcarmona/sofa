---
id: UC-SHAPE-WORK-UNIT
type: use-case
title: Shape a dependency-aware work unit
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-ENGINEER, ACT-REQUESTER]
bounded-context: BC-BACKLOG-TRIAGE
governed-by: [BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES, BR-WORK-AUTHORIZATION]
uses-terms: [TERM-WORK-UNIT]
---

## Goal

Turn one candidate issue into the smallest independently verifiable work unit.

## Trigger

An authorized engineer requests refinement directly, or admitted requester intake continues under policy.

## Preconditions

The source item and its priority are known, and unresolved dependencies are recorded.

## Main Flow

1. The Lighthouse harness refines scope, acceptance criteria, risk, decision boundary and verification under the recorded authorization. It asks the appropriate decision owner only for information or decisions it cannot resolve from evidence.
2. SoFa records dependencies and rejects a ready state when a prerequisite is unresolved.
3. Complete refinement continues under its effective authorization or waits for the human decision required by policy. A matching answer resumes the pending question without expanding the grant.

## Alternative Flows

A candidate is split into ordered work units or returned to triage when the scope is too large.

## Failure Conditions

Missing product or architecture intent blocks readiness instead of being inferred.

## Postconditions

One bounded work unit is ready, blocked, or explicitly awaiting a decision.
