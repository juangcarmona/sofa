---
id: UC-TRIAGE-BACKLOG
type: use-case
title: Triage GitHub backlog work
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-GITHUB-BACKLOG]
bounded-context: BC-BACKLOG-TRIAGE
governed-by: [BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES]
uses-terms: [TERM-IDEA, TERM-WORK-UNIT]
---

## Goal

Capture or promote an idea, then understand candidate GitHub work, project and issue priority, dependencies, risk, and missing information.

## Trigger

Juan requests a backlog refresh or reviews a changed source item.

## Preconditions

GitHub evidence is reachable or its limitation is explicit.

## Main Flow

1. SoFa captures a new idea or reads configured GitHub projects and issues.
2. If the candidate is an idea, Juan promotes it to or links it with a GitHub issue before normal backlog processing.
3. SoFa reconciles GitHub source identities and records priority, status, dependencies, and evidence time.
4. Juan selects a candidate for shaping or marks it needing clarification.

## Alternative Flows

Access, pagination, or mapping failures remain visible and do not create a false empty backlog.

## Failure Conditions

Unknown project priority, missing source evidence, missing baseline/risk-policy data, or unknown work-unit risk prevents a work unit from being authorized for dependent execution or delivery, while triage and shaping remain available.

## Postconditions

The local coordination view points to GitHub evidence for promoted work and records what remains unresolved. Unpromoted ideas remain in idea state and are not ready for execution.
