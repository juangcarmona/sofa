---
id: CON-TRUSTED-LAN
type: constraint
title: Keep execution off the unauthenticated LAN surface
status: draft
applies-to: [BC-EXECUTION, BC-DELIVERY, BC-RUNTIME-OPERATIONS]
---

## Constraint

The existing trusted-LAN application surface MUST remain non-privileged and read-only. Execution control, GitHub mutation, secret use, privileged commands, integration, and delivery authorization MUST use a separately authenticated boundary accepted before implementation.

The planning default is a loopback-only, separately authenticated control boundary. Privileged control is not exposed over the LAN.

## Rationale

LAN reachability is not authentication, and the current application was designed without a privileged control boundary.

## Consequences

The design must isolate the control boundary, secrets, Worker dispatch, and authorization checks. A future LAN control requirement needs a new security decision.
