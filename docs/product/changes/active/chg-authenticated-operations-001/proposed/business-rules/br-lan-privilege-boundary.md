---
id: BR-LAN-PRIVILEGE-BOUNDARY
type: business-rule
title: Network access and sign-in do not grant repository authority
status: draft
applies-to: [UC-ACCESS-FACTORY, UC-OPERATE-ISSUE, BC-EXECUTION, BC-DELIVERY, BC-RUNTIME-OPERATIONS]
uses-terms: [TERM-WORK-COMMAND, TERM-EXECUTION-RUN]
---

## Rule

SoFa MUST authenticate access to the operational workspace through its configured external identity provider and admit only its configured owner. It MUST verify action-specific source authorization on the server; a valid provider session, matching email address or trusted-LAN address MUST NOT substitute for the actual GitHub user's authority.

The anonymous observation surface MUST remain non-privileged while it exists. No anonymous route may mutate GitHub, expose a shell or secret, approve integration or request execution. Introducing authenticated controls requires an explicitly reviewed boundary; browser controls alone cannot enforce it.

## Rationale

The person who can reach the service, the person who has signed in, and the person authorized by a repository are distinct checks.

## Examples

A valid provider identity outside the configured owner boundary is denied access. The owner signs in successfully but cannot submit a command until the connected GitHub account has the required permission and the repository allows the action.

## Exceptions

None that grants authority from network access or identity alone.
