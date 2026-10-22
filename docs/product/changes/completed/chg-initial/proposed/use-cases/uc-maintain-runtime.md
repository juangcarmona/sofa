---
id: UC-MAINTAIN-RUNTIME
type: use-case
title: Maintain runtime health and recovery posture
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: []
bounded-context: BC-RUNTIME-OPERATIONS
governed-by: [BR-RECOVERABLE-EXECUTION, BR-LAN-PRIVILEGE-BOUNDARY]
uses-terms: [TERM-EXECUTION-RUN, TERM-RUNTIME-HEALTH, TERM-RETENTION-POLICY]
---

## Goal

Detect unhealthy workers, stale leases, missing checkpoints, retention pressure, and recovery-required runs.

## Trigger

A health observation or scheduled maintenance window occurs.

## Preconditions

Runtime signals and durable run state are available or their absence is visible.

## Main Flow

1. SoFa observes worker heartbeat, lease, database, OpenCode, and evidence health.
2. SoFa records an operational finding and safe maintenance recommendation.
3. Juan decides whether to recover, pause, or change policy.

## Alternative Flows

An unavailable telemetry path is reported as unknown rather than healthy.

## Failure Conditions

Maintenance cannot delete or mutate source work as a side effect of health observation.

## Postconditions

The operational state and next human action are explicit.
