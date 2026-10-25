---
id: BC-RUNTIME-OPERATIONS
type: bounded-context
title: Runtime health observation
status: draft
---

## Responsibility

Observe independent runtimes, runner availability, execution progress and evidence retention.

## Language

Health, elapsed time, availability, interruption and evidence age.

## Boundaries

Health findings do not dispatch, cancel or recover work and are not mandatory execution gates.

## External Relationships

Observes source runtime evidence and presents findings to ACT-FACTORY-OWNER.
