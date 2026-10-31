---
id: "QR-COLLECTION-VISIBILITY"
type: "quality-requirement"
title: "Explain collection execution without exposing credentials"
status: "draft"
quality-attribute: "observability"
applies-to: ["UC-REFRESH-ATTENTION"]
uses-terms: []
verification: [{"scenario": "Given a refresh with one inaccessible repository, Juan can associate that refresh with the per-repository outcomes, duration, and failure reason in the Aspire operational view, without exposing credentials in telemetry or the attention list."}]
---

## Requirement

The product MUST make each refresh and its per-repository outcomes, duration, and failures observable in the Aspire environment selected by CON-EXECUTION-HOST. It MUST keep credentials and authorization headers out of the attention list and telemetry.

## Measurement

For a refresh covering the five selected repositories, each attempted repository has an attributable outcome and duration; each failed attempt has a useful reason. Inspect captured output and telemetry with a distinctive test credential and confirm that neither the credential nor authorization headers are present. This measures SoFa’s own operation, not repository delivery performance.
