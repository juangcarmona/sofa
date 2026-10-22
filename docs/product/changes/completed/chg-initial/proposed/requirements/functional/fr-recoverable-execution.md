---
id: FR-RECOVERABLE-EXECUTION
type: functional-requirement
title: Execute work with leases and checkpoints
status: draft
derived-from: [UC-EXECUTE-WORK-UNIT, BR-RECOVERABLE-EXECUTION, CON-FACTORY-RUNTIME, CON-TRUSTED-LAN]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
verification: [{scenario: "Given an eligible low-risk work unit, one Worker acquires a lease, creates a run, invokes OpenCode within policy, and persists ordered checkpoints."}, {scenario: "Given a Worker stops after a checkpoint, a replacement Worker automatically resumes only when policy, fencing, and workspace validation permit it, without duplicate active ownership."}, {scenario: "Given repeated recovery failure reaches the configured retry or time bound, the run blocks for a human decision."}, {scenario: "Given uncertain checkpoint, fencing, workspace, or policy evidence, recovery stops for a human decision and cannot claim success."}]
---

## Requirement

The product MUST persist an execution run, lease, fencing token, checkpoint sequence, cancellation outcome, recovery outcome, and source/workspace references. After Worker failure, it MUST support automatic retry or recovery from the latest valid checkpoint when policy and evidence permit, bounded by a configured retry count or time budget. It MUST stop for a human decision when evidence is uncertain or the bound is exhausted. It MUST prevent the unauthenticated application surface from dispatching execution or using privileged capabilities.

## Rationale

Durability and fencing turn process loss into a visible recovery path rather than silent data loss or duplicate work.
