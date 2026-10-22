---
id: BC-RUNTIME-OPERATIONS
type: bounded-context
title: Runtime operations
status: draft
---

## Responsibility

Make worker health, leases, checkpoints, telemetry, maintenance, retention, and recovery posture visible.

## Language

Heartbeat, stale lease, health, maintenance, telemetry, retention, and recovery-required.

## Boundaries

Operational health is not product evidence and does not grant execution or delivery authority.

## External Relationships

Observes BC-EXECUTION and BC-DELIVERY while surfacing operational action to ACT-FACTORY-OWNER.
