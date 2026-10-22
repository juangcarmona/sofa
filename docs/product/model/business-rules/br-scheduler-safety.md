---
id: BR-SCHEDULER-SAFETY
type: business-rule
title: Scheduler pause and revocation are fail-safe controls
status: draft
applies-to: [BC-SCHEDULING, BC-EXECUTION, BC-RUNTIME-OPERATIONS]
uses-terms: [TERM-EXECUTION-RUN, TERM-SCHEDULING-DECISION]
---

## Rule

Juan MUST always be able to pause or revoke scheduler dispatch. SoFa MAY also auto-pause or revoke dispatch when it detects critical security impact, loss of required runtime health, or lease/fencing anomalies. A runtime-health pause MAY resume automatically only after verified recovery. Security-critical and lease/fencing anomaly pauses MUST remain stopped until Juan decides to resume or resolve them. Every pause, revocation, and resume MUST record its reason, scope, source, and observed time.

## Rationale

Unattended operation needs an immediate human stop and an automatic fail-safe when continuing could make recovery or attribution unsafe.

## Examples

Juan pauses all dispatch before maintenance. A transient health failure pauses dispatch and resumes it after verified recovery. A fencing anomaly auto-revokes dispatch for the affected work unit or scheduler scope and records the anomaly instead of allowing a stale Worker to continue; Juan must resolve it before resumption.

## Exceptions

None for Juan's pause or revocation authority. Automatic triggers may be extended only by an explicitly accepted policy.
