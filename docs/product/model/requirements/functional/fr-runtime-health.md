---
id: FR-RUNTIME-HEALTH
type: functional-requirement
title: Surface runtime health and maintenance needs
status: draft
derived-from: [UC-MAINTAIN-RUNTIME, BR-RECOVERABLE-EXECUTION]
uses-terms: [TERM-EXECUTION-RUN]
verification: [{scenario: "Given a stale Worker heartbeat, expired lease, or missing checkpoint, the runtime reports a recovery-required condition with its source and age."}, {scenario: "Given unavailable telemetry or database health, SoFa reports unknown rather than healthy."}]
---

## Requirement

The product MUST expose Worker liveness, lease age, checkpoint progress, database connectivity, OpenCode outcome, verification failures, recovery-required runs, and maintenance limitations. Health observation MUST NOT mutate repository work as a side effect.

## Rationale

The factory cannot safely schedule or recover work without knowing whether its own control plane is trustworthy.
