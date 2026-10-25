---
id: CHG-OBSERVE-REPOSITORY-WORKFLOWS-001
type: product-change
title: Observe and verify independently executed repository workflows
status: applied
base-revision: '0da307160377b6157dc7665254ac6f36cfa32f3b'
operations:
  add: []
  modify:
    - BR-RECOVERABLE-EXECUTION
    - BR-RETENTION-STANDARDS
    - BR-WORK-AUTHORIZATION
    - BR-UNKNOWN-RISK
    - BR-GITHUB-SOURCE
    - BR-CAPACITY-DEFAULTS
    - BR-SCHEDULER-SAFETY
    - BR-PRIORITY-DEFAULTS
    - CON-FACTORY-RUNTIME
    - FR-AUTHORIZED-ENTRY
    - FR-WORK-UNIT-SHAPING
    - FR-RECOVERABLE-EXECUTION
    - FR-RUNTIME-HEALTH
    - FR-MULTI-PROJECT-SCHEDULING
    - FR-RISK-CLASSIFICATION
    - FR-RETENTION-POLICY
    - BC-EXECUTION
    - BC-PORTFOLIO-PROJECT-CONFIGURATION
    - BC-SCHEDULING
    - BC-RUNTIME-OPERATIONS
    - UC-EXECUTE-WORK-UNIT
    - UC-SCHEDULE-WORK
    - UC-MAINTAIN-RUNTIME
    - JRN-ENGINEERING-EXECUTION
    - QR-EXECUTION-RECOVERY
    - TERM-EXECUTION-RUN
    - TERM-SCHEDULING-DECISION
    - FR-BACKLOG-PROJECTION
    - UC-SHAPE-WORK-UNIT
  remove: []
---

## Problem

The current definition makes SoFa a scheduler and execution supervisor. That makes repository automation depend on a product whose intended value is cross-repository observation and verification.

## Intended Product Outcome

SoFa presents source-backed work, execution progress, delivery evidence and decisions across repositories. Repository workflows own orchestration; an independent runtime executes agents. SoFa can be stopped without stopping delivery. Future authenticated controls use the same repository entrypoints as direct GitHub interaction.

## Rationale

Juan clarified that SoFa is an observer and verifier and explicitly accepted this separation on 2026-10-25, then requested its correction in Lighthouse, SoFa and the presentations pilot. This supersedes the earlier assignment of runtime ownership to SoFa. His acceptance was: “Eso es. Pues corrige Lighthouse y SoFa en esa dirección y deja el piloto de presentations funcionando desde sus workflows, sin depender de SoFa. Sin pisar lo que hay en curso.”

## Affected Product Areas

Work intake observations, recommendations, execution observations, runtime health, authorization provenance and delivery verification.

## Open Questions

None for this responsibility boundary. Future user-interface design is outside this change.

## Product Acceptance

Repository execution proceeds while SoFa is unavailable. Observations retain source identity and limitations. SoFa neither schedules agents nor supplies mandatory execution authorization; its optional controls record authenticated user requests in the repository workflow.

## Out of Scope

Implementation plans, delivery status, a new UI framework, changing Lighthouse stage semantics, and granting new implementation or merge authority.
