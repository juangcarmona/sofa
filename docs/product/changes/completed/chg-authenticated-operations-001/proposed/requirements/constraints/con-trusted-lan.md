---
id: CON-TRUSTED-LAN
type: constraint
title: Authenticate the operational workspace on LAN and VPN
status: draft
applies-to: [BC-PORTFOLIO-PROJECT-CONFIGURATION, BC-BACKLOG-TRIAGE, BC-EXECUTION, BC-DELIVERY, BC-RUNTIME-OPERATIONS]
---

## Constraint

The operational SoFa workspace MUST require authentication through the configured external identity provider for reading as well as acting. It MUST support the owner's desktop and phone over LAN or VPN through a protected connection. Network reachability MUST NOT establish identity, permission or approval.

Privileged operations MUST pass a separately defined and verified server authorization boundary and the source repository's controls. The existing anonymous observation implementation MUST NOT acquire write privileges through deployment or configuration alone. Its replacement by the authenticated workspace requires the reviewed security design.

## Rationale

The factory must be operable from a phone without extending trust to every device on the network or every account at an identity provider.

## Consequences

Loopback-only control is no longer the product requirement. Identity-provider trust, session policy, source-account linking and protected transport must be established before enabling operations. Public hosting and identity administration for multiple users are outside this personal-factory change.
