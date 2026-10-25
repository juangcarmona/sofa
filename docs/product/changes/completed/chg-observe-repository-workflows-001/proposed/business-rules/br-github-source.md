---
id: BR-GITHUB-SOURCE
type: business-rule
title: GitHub owns work and workflow evidence
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-DELIVERY]
uses-terms: [TERM-IDEA, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
---

## Rule

SoFa MAY capture ideas before promotion. Normal repository work MUST have a GitHub issue identity. Issues, comments, pull requests, checks, workflow runs, source revisions and artifacts remain authoritative in GitHub. SoFa projections MUST retain source identities, timestamps and limitations. They MUST NOT replace source state or become a mandatory coordinator for execution.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
