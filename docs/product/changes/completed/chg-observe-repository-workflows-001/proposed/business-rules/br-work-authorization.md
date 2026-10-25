---
id: BR-WORK-AUTHORIZATION
type: business-rule
title: Repository workflows own work authorization
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-EXECUTION, BC-SCHEDULING, BC-DELIVERY]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-PROJECT]
---

## Rule

Repository workflows MUST verify actors, effective capabilities, stage scope and continuation authority through the installed Lighthouse contract. The independent runtime MUST execute only its bounded request. SoFa MUST observe these decisions without granting new authority. A future authenticated SoFa control MUST record the explicit user action in GitHub and use the same workflow path; its unauthenticated read surface MUST remain read-only. Requester replies supply information within an existing grant; engineers may request direct entry within repository policy. Neither a model response nor a SoFa finding authorizes implementation or integration.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
