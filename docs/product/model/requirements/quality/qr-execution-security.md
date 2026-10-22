---
id: QR-EXECUTION-SECURITY
type: quality-requirement
title: Keep execution authority off the read surface
status: draft
quality-attribute: security
applies-to: [UC-EXECUTE-WORK-UNIT, UC-VERIFY-DELIVERY, UC-MAINTAIN-RUNTIME]
uses-terms: [TERM-EXECUTION-RUN]
verification: [{scenario: "Given an unauthenticated request to the existing LAN application surface, no execution, GitHub write, secret, privileged command, integration approval, or delivery authorization is reachable."}]
---

## Requirement

The product MUST isolate execution control and secrets from the existing unauthenticated LAN read surface and MUST fail closed when the control boundary is missing or unauthorized.

## Measurement

Exercise every read route and attempted control action from the LAN surface and verify no Worker dispatch, GitHub mutation, secret disclosure, privileged command, or approval occurs; inspect telemetry for credentials and authorization headers.
