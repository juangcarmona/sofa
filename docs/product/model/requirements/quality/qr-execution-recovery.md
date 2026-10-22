---
id: QR-EXECUTION-RECOVERY
type: quality-requirement
title: Recover execution without duplicate ownership
status: draft
quality-attribute: recoverability
applies-to: [UC-EXECUTE-WORK-UNIT, UC-MAINTAIN-RUNTIME]
uses-terms: [TERM-EXECUTION-RUN]
verification: [{scenario: "Given an interrupted run and a new Worker, the new Worker can identify the latest valid checkpoint and fencing token, while the old Worker cannot publish after lease loss."}]
---

## Requirement

Execution recovery MUST be durable, attributable, idempotent at checkpoint boundaries, and safe against stale owners.

## Measurement

Run the pilot interruption scenario repeatedly and verify one active lease, ordered checkpoints, a bounded recovery outcome, and no duplicate terminal success.
