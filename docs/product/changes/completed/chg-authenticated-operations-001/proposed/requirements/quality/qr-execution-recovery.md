---
id: QR-EXECUTION-RECOVERY
type: quality-requirement
title: Recover execution without duplicate ownership
status: draft
quality-attribute: recoverability
applies-to: [UC-EXECUTE-WORK-UNIT, UC-MAINTAIN-RUNTIME]
uses-terms: [TERM-EXECUTION-RUN]
verification: [{scenario: "Given a SoFa restart during source-owned execution, the owner reconnects to the same source run and sees its outcome or an explicit evidence gap without another execution being requested."}]
---

## Requirement

SoFa MUST expose source recovery evidence and uncertainty without owning execution or recovery. Its outage MUST NOT stop repository workflows.

## Measurement

Stop SoFa during a repository execution and verify that the workflow still publishes its result. Refresh the observation and verify it preserves the original run and outcome.
