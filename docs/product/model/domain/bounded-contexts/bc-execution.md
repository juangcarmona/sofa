---
id: BC-EXECUTION
type: bounded-context
title: Recoverable execution
status: draft
---

## Responsibility

Run one authorized work unit through a leased, checkpointed engineering loop and expose explicit interruption and recovery outcomes.

## Language

Execution runs, stages, leases, checkpoints, attempts, recovery, capabilities, and outcomes.

## Boundaries

This context does not choose global scheduling policy, approve products, merge pull requests, or deploy.

## External Relationships

Consumes a ready work unit from BC-BACKLOG-TRIAGE, invokes ACT-OPENCODE under policy, and reports evidence to BC-DELIVERY.
