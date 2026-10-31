---
id: QR-EXECUTION-SECURITY
type: quality-requirement
title: Protect factory sessions and source-authorized operations
status: draft
quality-attribute: security
applies-to: [UC-ACCESS-FACTORY, UC-OPERATE-ISSUE, UC-EXECUTE-WORK-UNIT, UC-VERIFY-DELIVERY, UC-MAINTAIN-RUNTIME]
uses-terms: [TERM-WORK-COMMAND, TERM-EXECUTION-RUN]
verification:
  - scenario: An anonymous caller cannot read protected operational data or invoke a GitHub mutation, including by directly calling server routes.
  - scenario: An identity outside the configured provider and owner boundary, a revoked GitHub connection and an expired SoFa session cannot authorize source operations.
  - scenario: Cross-site requests, forged identity associations and replayed authorization responses cannot create sessions or submit work.
  - scenario: Sign-out ends access through the SoFa session without cancelling a repository execution already admitted by the source.
---

## Requirement

SoFa MUST protect operational content and commands with external-provider authentication, a bounded session and server-side authorization. It MUST separate provider identity, SoFa access and GitHub capabilities, and fail closed for missing or invalid authority. It MUST protect identity and source credentials from page content, logs, error messages and published evidence.

Session expiry, sign-out, recognized revocation, source reconnection and provider unavailability MUST have explicit behavior. No authentication failure may create a password bypass or silently extend privileges. Effective authentication-strength, session and revocation policies MUST be defined and verified before deployment. Source permission checks and repository gates remain mandatory regardless of session validity.

## Measurement

Exercise sign-in, denied identities, direct unauthenticated routes, expiry, sign-out, recognized revocation, cross-site attempts, invalid callbacks, GitHub permission loss and provider failure. Inspect responses and retained evidence for secrets. Show that each denied case prevents its dependent read or mutation, and that loss of the SoFa session does not stop already admitted source execution. Validate the actual provider and GitHub actor path in addition to isolated tests.
