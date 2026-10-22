---
id: BR-PRIORITY-DEFAULTS
type: business-rule
title: Missing priority uses a shared scheduling default
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-SCHEDULING]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-SCHEDULING-DECISION]
---

## Rule

When GitHub does not provide a project or issue priority, SoFa MUST apply the shared default priority policy for scheduling. It MUST preserve source priority when present, record whether the value was sourced or defaulted, and MUST NOT write a derived default back to GitHub.

## Rationale

Scheduling can continue across projects without inventing a second authoritative backlog or silently losing provenance.

## Examples

An issue without a GitHub priority receives the shared scheduling default, and its scheduling evidence says `prioritySource: shared-default`. A later GitHub priority replaces the derived value on refresh without writing the previous default to GitHub.

## Exceptions

An explicitly accepted policy may refine the shared default, but it must preserve provenance and GitHub authority.
