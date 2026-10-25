---
id: FR-RECOVERABLE-EXECUTION
type: functional-requirement
title: Observe execution and recovery evidence
status: draft
derived-from: [UC-EXECUTE-WORK-UNIT, BR-RECOVERABLE-EXECUTION, CON-FACTORY-RUNTIME, CON-TRUSTED-LAN]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
verification: [{scenario: "Source observations preserve identity and limitations; stopping SoFa does not stop repository workflows; a read refresh cannot dispatch or publish work."}]
---

## Requirement

SoFa MUST project execution identity, source revision, workflow stage, timestamps, result, cancellation and recovery evidence from repository workflows and their independent runtimes. It MUST distinguish an observed retry from a verified recovery and expose absent evidence. SoFa MUST NOT acquire execution leases, own checkpoints, supervise workers or publish stage effects. Its availability MUST NOT be required for repository execution or recovery.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.
