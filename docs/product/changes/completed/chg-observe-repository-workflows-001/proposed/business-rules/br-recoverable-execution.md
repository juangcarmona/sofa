---
id: BR-RECOVERABLE-EXECUTION
type: business-rule
title: Verify source recovery evidence
status: draft
applies-to: [BC-EXECUTION, BC-RUNTIME-OPERATIONS]
uses-terms: [TERM-EXECUTION-RUN, TERM-WORK-UNIT]
---

## Rule

The repository workflow and independent runtime own bounded attempts, cancellation and recovery. SoFa MUST preserve their run identities and report whether recovery is evidenced, failed or unknown. It MUST NOT invent leases, checkpoints or successful recovery from missing liveness. Source-side recovery policies remain inspectable; SoFa itself is not part of the recovery path.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
