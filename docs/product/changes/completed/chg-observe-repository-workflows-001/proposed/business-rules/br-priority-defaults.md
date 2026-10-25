---
id: BR-PRIORITY-DEFAULTS
type: business-rule
title: Keep recommendations separate from source priority
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-SCHEDULING]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-SCHEDULING-DECISION]
---

## Rule

SoFa MUST preserve source project and issue priorities. When a recommendation uses a default, it MUST mark the value as derived and MUST NOT write it back or change the repository scheduling policy. Recommendations cannot authorize work or bypass a human gate.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
