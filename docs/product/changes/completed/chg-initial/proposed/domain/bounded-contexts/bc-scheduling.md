---
id: BC-SCHEDULING
type: bounded-context
title: Multi-project scheduling
status: draft
---

## Responsibility

Continuously evaluate and select eligible work across projects using priority, dependencies, risk, WIP, leases, capacity, and the accepted autonomy policy.

## Language

Eligibility, queue, project, dependency readiness, concurrency, WIP, risk profile, and dispatch decision.

## Boundaries

Scheduling does not implement work, change priorities silently, infer a human approval from eligibility, or dispatch after its authority is paused or revoked. Juan can always pause or revoke it, and critical security, health, or lease/fencing anomalies may trigger an automatic fail-safe pause or revocation.

## External Relationships

Reads work-unit state from BC-BACKLOG-TRIAGE and dispatches only policy-allowed work to BC-EXECUTION.
