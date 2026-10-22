---
id: BR-GITHUB-SOURCE
type: business-rule
title: GitHub remains the backlog source of truth
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-DELIVERY]
uses-terms: [TERM-IDEA, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
---

## Rule

SoFa MAY capture an idea before it has a GitHub identity. Before the idea enters the normal backlog flow, it MUST become or link to a GitHub issue. SoFa MUST preserve GitHub issue, project, pull request, check, review, workflow, commit, and artifact identities when projecting or explaining work. Local state may coordinate execution, but it MUST NOT silently replace GitHub backlog state or present an unverified local claim as source fact.

## Rationale

The factory spans repositories; source identity and evidence links prevent a second, drifting backlog.

## Examples

A repeated import updates a known issue projection rather than creating a duplicate. An idea without a promoted GitHub issue remains an idea and is not eligible for normal work-unit execution. A missing GitHub response is reported as unavailable rather than clear.

## Exceptions

None without an explicitly accepted Product Change.
