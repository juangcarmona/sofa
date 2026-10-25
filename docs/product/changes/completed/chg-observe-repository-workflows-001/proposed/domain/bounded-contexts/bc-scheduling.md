---
id: BC-SCHEDULING
type: bounded-context
title: Work recommendations
status: draft
---

## Responsibility

Aggregate source queues and recommend eligible work across configured projects.

## Language

Priority, dependencies, capacity evidence, recommendation and uncertainty.

## Boundaries

This context never owns a queue, dispatches agents or imposes workflow gates.

## External Relationships

Reads BC-BACKLOG-TRIAGE and source capacity evidence for ACT-FACTORY-OWNER.
