---
id: BR-LAN-PRIVILEGE-BOUNDARY
type: business-rule
title: The read surface cannot grant privilege
status: draft
applies-to: [BC-EXECUTION, BC-DELIVERY, BC-RUNTIME-OPERATIONS]
uses-terms: [TERM-EXECUTION-RUN]
---

## Rule

The existing unauthenticated trusted-LAN application surface MUST remain non-privileged. It MUST NOT launch an execution run, acquire a privileged lease, expose a shell or secret, mutate GitHub, approve integration, or dispatch OpenCode. Any future control surface MUST have a separately accepted authentication and authorization boundary.

## Rationale

Network reachability is not identity or authorization.

## Examples

An unauthenticated page may show operational status, but following its links cannot start or resume a run or grant additional authority.

## Exceptions

None until a separate security decision is accepted.
