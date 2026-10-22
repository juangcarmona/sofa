---
id: BR-HUMAN-DECISION-BOUNDARIES
type: business-rule
title: Human decisions remain explicit
status: draft
applies-to: [BC-EXECUTION, BC-SCHEDULING, BC-DELIVERY]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
---

## Rule

Security impact, privileged operations, and destructive data actions MUST remain explicit human decisions. Product intent, architecture, PR approval, integration, and delivery acceptance MAY use a later accepted policy with deterministic and semantic review, code ownership, evidence, and notifications. Green checks, eligibility, lease ownership, or mergeability MUST NOT be treated as approval.

## Rationale

Autonomy is useful only when its authority and stopping points are visible.

## Examples

The Worker may run a low-risk local test under policy, but it stops before a privileged command or destructive action. A policy-controlled integration may proceed when its evidence is complete; a human gate remains mandatory for critical impact.

## Exceptions

Only an explicitly accepted risk policy may substitute a named deterministic decision.
