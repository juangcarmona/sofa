---
id: CHG-FACTORY-ENTRY-AUTHORITY-001
type: product-change
title: Separate requester intake and engineer authority in the personal factory
status: proposed
base-revision: '6c0b770'
operations:
  add:
    - ACT-REQUESTER
    - ACT-ENGINEER
    - BR-WORK-AUTHORIZATION
    - FR-AUTHORIZED-ENTRY
  modify:
    - ACT-FACTORY-OWNER
    - "ACT-JUAN"
    - UC-TRIAGE-BACKLOG
    - UC-SHAPE-WORK-UNIT
    - UC-EXECUTE-WORK-UNIT
    - JRN-ENGINEERING-EXECUTION
  remove: []
---

## Problem

The personal-factory model expresses intake and shaping as owner-driven steps and has no explicit requester or engineer capability boundary. This obscures who may initiate work, who may answer a question, and how routine execution can continue after the initiating chat closes.

## Intended Product Outcome

SoFa verifies repository capabilities, admits requesters through triage and authorized engineers through direct workflow entry, and preserves bounded authorization throughout execution and recovery. Per-run autonomy and integration permission are separate. Lighthouse owns routing and stage semantics; SoFa enforces execution and publication authority. An authenticated future control surface uses the same boundary.

## Rationale

Juan accepted this direction on 2026-10-24: "Vale, ese tiene algo de mejor pinta... Vamos por esa dirección, vale???" A personal factory can serve requests without introducing an organization or forcing its owner through requester intake. This proposal records the direction without claiming that the detailed baseline or runtime is already accepted.

## Affected Product Areas

Repository actors and capabilities, intake, shaping, execution authorization, clarification, continuity and integration decisions.

## Open Questions

None for these semantics. Actual requester admission, engineer grants, limits and automatic-merge classes are explicit project configuration, with no permission inferred when configuration is absent. Web authentication technology remains a later architecture decision.

## Product Acceptance

The model supports both entry channels; separates identity, entry, autonomy and merge; preserves owner revocation and existing risk rules; permits policy-bound requester continuation; and prevents answers, labels and engine output from widening authority.

## Out of Scope

Commercial tenancy, organization administration, new engine providers, live rollout, automatic merge activation, changes to the unauthenticated LAN surface and replacing Lighthouse lifecycle contracts.
