---
id: BR-CAPACITY-DEFAULTS
type: business-rule
title: Report source capacity and limits
status: draft
applies-to: [BC-PORTFOLIO-PROJECT-CONFIGURATION, BC-SCHEDULING]
uses-terms: [TERM-PROJECT, TERM-EXECUTION-RUN, TERM-SCHEDULING-DECISION]
---

## Rule

SoFa MUST preserve the capacity limits, reservations and queue state reported by repository workflows and runtime operators. It MUST distinguish unavailable capacity from zero capacity and identify every derived recommendation. Enforcement belongs to those sources; SoFa MUST NOT allocate workers or impose its own mandatory global capacity limit.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
