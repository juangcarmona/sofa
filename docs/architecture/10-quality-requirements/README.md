---
title: Quality requirements
arc42-section: "10"
description: Quality goals and concrete scenarios for evaluating them.
---

# 10. Quality requirements

## Quality tree

1. **Authority safety**
   - Lighthouse lifecycle semantics are preserved.
   - SoFa cannot infer human acceptance or merge authority.
2. **Recoverable execution**
   - A run has one active lease and a monotonic checkpoint sequence.
   - A stale worker cannot publish after fencing.
3. **Isolation**
   - A workspace and execution worker are scoped to one run.
   - Cleanup or quarantine prevents cross-run contamination.
4. **Operability**
   - Bastion health, bounded capacity, leases, workers, engine, gateway, and recovery posture are observable.
5. **Evidence integrity**
   - Operational state, stage results, delivery evidence, telemetry, and human decisions remain separate.
6. **Bounded first slice**
   - The architecture runs several isolated executions on one persistent Bastion without a fleet, broker, autoscaler, or microVM requirement; the initial capacity target is five to six, subject to measurement.

## Quality scenarios

| Scenario | Stimulus | Required response |
| --- | --- | --- |
| Lost worker | Heartbeats stop before the run completes. | Mark the run interrupted, prevent stale publication, and either recover from a valid checkpoint or stop for a human decision. |
| Lease race | An old worker publishes after expiry while a new worker owns the run. | Reject the old publication using the fencing token and retain the new owner's state. |
| Stage failure | A Lighthouse worker returns failed, blocked, unavailable, or missing evidence. | Stop the explicit composition and retain the Lighthouse result; do not invoke the next stage. |
| Green automation | CI and a stage result are successful. | Record evidence, but do not infer PR approval, merge, release, or production acceptance. |
| Workspace contamination | A previous run leaves files or processes behind. | Clean or quarantine the workspace and refuse reuse until identity and ownership are verified. |
| Untrusted repository | A fork or code classification requires stronger isolation. | Reject or route to a future sandbox/microVM profile; do not silently use the trusted profile. |
| Bastion degradation | Database, runner, gateway, or model service becomes unhealthy. | Expose health degradation, stop unsafe dispatch, and retain recovery-required state. |

## Pilot evidence

The [presentations pilot](../../roadmap/first-pilot.md) owns the concrete acceptance scenarios. Its evidence must cover duplicate intake, same-key payload conflicts, stale publication, uncertain remote effects, cancellation, host capacity contention and source changes after approval. A successful recovery fixture does not prove that the real engine or GitHub publisher observes the same boundary.
