---
id: BC-EXECUTION
type: bounded-context
title: Execution observation
status: draft
---

## Responsibility

Observe repository workflow runs, independent runtime outcomes and recovery evidence.

## Language

Runs, attempts, source revisions, outcomes and observed recovery.

## Boundaries

This context does not execute agents, acquire leases, schedule work or publish effects.

## External Relationships

Reads repository workflows and runtime evidence and supplies BC-DELIVERY.
