---
id: UC-TRIAGE-BACKLOG
type: use-case
title: Triage GitHub backlog work
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-GITHUB-BACKLOG, ACT-REQUESTER, ACT-ENGINEER]
bounded-context: BC-BACKLOG-TRIAGE
governed-by: [BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES, BR-WORK-AUTHORIZATION]
uses-terms: [TERM-IDEA, TERM-WORK-UNIT]
---

## Goal

Capture or promote an idea, then understand candidate GitHub work, project and issue priority, dependencies, risk, and missing information.

## Trigger

An admitted requester submits work, an engineer requests triage, or an authorized policy requests backlog refresh.

## Preconditions

GitHub evidence is reachable or its limitation is explicit.

## Main Flow

1. SoFa captures a new idea or reads configured GitHub projects and issues.
2. If the candidate is an idea, Juan explores its outcome and scope, selects a repository, inspects potentially related issues and reviews the proposed issue before explicitly creating or linking it. SoFa preserves the confirmed issue identity before normal backlog processing.
3. SoFa reconciles GitHub source identities and records priority, status, dependencies, and evidence time.
4. The Lighthouse intake result selects continuation under the effective policy, requests information, refers a decision to an engineer, or rejects the request with a reason. Direct engineer entry is admitted separately and does not require this triage run.

## Alternative Flows

Access, pagination, or mapping failures remain visible and do not create a false empty backlog. An uncertain issue creation is reconciled before retry; exploration and promotion do not implicitly request execution.

## Failure Conditions

Unknown project priority, missing source evidence, missing baseline/risk-policy data, or unknown work-unit risk prevents a work unit from being authorized for dependent execution or delivery, while triage and shaping remain available.

## Postconditions

The local coordination view points to GitHub evidence for promoted work and records what remains unresolved. Unpromoted ideas remain in idea state and are not ready for execution.
