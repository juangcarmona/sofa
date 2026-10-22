---
id: BR-RETENTION-STANDARDS
type: business-rule
title: Retention follows shared standards and defaults
status: draft
applies-to: [BC-PORTFOLIO-PROJECT-CONFIGURATION, BC-RUNTIME-OPERATIONS, BC-DELIVERY]
uses-terms: [TERM-RETENTION-POLICY, TERM-DELIVERY-EVIDENCE]
---

## Rule

Every project MUST have an effective retention policy for execution metadata and delivery evidence. A project-specific policy MUST conform to accepted shared standards. If no project-specific policy is configured, SoFa MUST apply the shared default policy. Detailed telemetry MAY have a separate bounded retention period, but its policy and limitations MUST be visible.

The planning default is 90 days for execution metadata and checkpoints after terminal completion, 365 days for delivery evidence and decision records, and 30 days for detailed telemetry. These periods are configuration defaults and remain subject to the shared standards policy.

## Rationale

Consistent defaults prevent silent data loss or indefinite accumulation while allowing project-specific needs within common standards.

## Examples

A project without an override uses the factory default. A project override that would violate a mandatory minimum is rejected or reported as invalid.

## Exceptions

Only an explicitly accepted Product Change or standards policy may define an exception.
