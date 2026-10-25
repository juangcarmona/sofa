---
id: BR-RETENTION-STANDARDS
type: business-rule
title: Retain observations without controlling source retention
status: draft
applies-to: [BC-PORTFOLIO-PROJECT-CONFIGURATION, BC-RUNTIME-OPERATIONS, BC-DELIVERY]
uses-terms: [TERM-RETENTION-POLICY, TERM-DELIVERY-EVIDENCE]
---

## Rule

SoFa MUST retain its own observations according to an explicit policy and report the retention and availability of source evidence. It MUST NOT delete repository or runtime evidence while refreshing observations. Missing retained evidence limits verification claims and cannot be interpreted as success.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Examples

An unavailable workflow is shown as unknown with its source link; no replacement execution starts in SoFa.

## Exceptions

None that makes SoFa an implicit execution authority.
