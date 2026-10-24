---
id: FR-AUTHORIZED-ENTRY
type: functional-requirement
title: Admit work through requester and engineering channels
status: draft
derived-from: [BR-WORK-AUTHORIZATION, BR-HUMAN-DECISION-BOUNDARIES, BR-UNKNOWN-RISK, BR-LAN-PRIVILEGE-BOUNDARY]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: A verified engineer can request refine without a triage predecessor, while an admitted requester follows intake and cannot authorize implementation.
  - scenario: An admitted request can continue under a standing bounded policy without requiring a new owner acknowledgement at every phase.
  - scenario: A valid clarification resumes only its pending question; an unrelated actor, forged role or bot cannot expand the authorization.
  - scenario: Duplicate events preserve one active attempt and one publication, and stale or revoked authority prevents further dispatch and effects.
  - scenario: A bounded implementation records OpenSpec artifacts, PR and CI evidence; merge requires its separate effective authorization.
  - scenario: An unauthenticated web request cannot dispatch work and an authenticated control adapter cannot bypass repository capabilities.
---

## Requirement

SoFa MUST admit requester and engineering work using verified repository capabilities and the installed Lighthouse decision contract. It MUST retain the bounded authorization through queued work, execution, clarification, recovery and publication. Identity, workflow entry, autonomy and integration authority MUST remain independently inspectable. Unsupported contracts and missing capability evidence MUST stop dependent work before model execution.

## Rationale

A durable authorization record lets the factory operate after the initiating chat closes without asking a human to repeat already delegated routine decisions.
