---
id: BR-EXPLAINABLE-SIGNALS
type: business-rule
title: Signals must support decisions with inspectable evidence
status: draft
applies-to: [UC-INSPECT-PROJECT-SIGNAL, UC-EXPLORE-FACTORY, UC-RESOLVE-INTERVENTION, BC-RUNTIME-OPERATIONS]
uses-terms: [TERM-PROJECT, TERM-DELIVERY-EVIDENCE]
---

## Rule

Every displayed signal MUST identify the decision it supports, its source, observation time, scope, calculation or source verdict, and limitations. SoFa MUST distinguish measured facts from derived assessments and distinguish complete, incomplete, stale, unavailable and not-applicable evidence. Coverage and freshness are independent: an incomplete observation can also be stale. Missing values MUST NOT become zero or passing results. Trends MUST identify comparable windows and changes to the calculation or baseline. Shared definitions belong to Lighthouse; SoFa MUST NOT copy screenshot scores, thresholds or policies into a new authority.

## Rationale

A factory dashboard is useful when the owner can verify why it calls for a decision and understand when its evidence is insufficient.

## Examples

A workflow failure count opens its contributing runs and time window. Token usage measured for three of five runs states that coverage. A repository with no deployment process shows deployment frequency as not applicable. A declared harness version differing from an explicitly selected baseline is a deviation, not automatically a security finding.

## Exceptions

No composite health score or default alert threshold is implied by this rule. Demonstration data must be explicitly separated from real observations.
