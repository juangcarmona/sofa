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

SoFa MUST expose source recovery evidence and uncertainty without owning execution or recovery. Its outage MUST NOT stop repository workflows.

## Measurement

Stop SoFa during a repository execution and verify that the workflow still publishes its result. Refresh the observation and verify it preserves the original run and outcome.
