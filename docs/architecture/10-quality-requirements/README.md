---
title: Quality requirements
arc42-section: "10"
description: Quality goals and concrete scenarios for evaluating them.
---

# 10. Acceptance scenarios

Stop SoFa and run repository refinement: the workflow must publish its result. Refresh an inaccessible repository: the observer must report unavailable evidence. Inspect a run: preparation, execution, validation, publication and cleanup must be distinguishable. Change the issue during execution: publication must stop. A read refresh must never dispatch or cancel work.

<!-- pdac:cite id="QR-EXECUTION-RECOVERY" digest="sha256:735fb5afb4c75ba744b7864c7ead1822b6e5ef1f1397460de6df582feaee38a6" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed delivery acceptance

| Journey boundary | Evidence required |
| --- | --- |
| Authentication and source authority | Actual local Keycloak login through Aspire; denied owner, expired session, CSRF rejection; separately verified GitHub actor |
| Context | Real issue with paginated conversation, previous stage result, missing artifact and stale refresh shown honestly |
| Command | Eligible Refine and Propose, changed-state denial, repeated click, lost response, restart, competing source reservation and exact run mapping |
| Conversation | Ordinary comment on needs-info does not resume; explicit supported reply does; uncertain post creates at most one source comment |
| Independent execution | Close SoFa after source admission and recover the same result after reopening |
| Mobile and accessibility | Complete the real issue journey on a phone over LAN/VPN; keyboard/focus behavior and readable status without color-only meaning |
| Later capabilities | Inbox resolution uses source evidence; search includes non-attention issues; boards preserve scope; signal inputs reproduce a calculation; idea promotion reconciles a lost response |

Fixtures establish deterministic behavior but do not substitute for the real source/mobile acceptance. Run the repository's verify pipeline and preserve relevant screenshots, source identities, run attempts and outcome limitations in the delivery PR. Production recovery is checked independently from the local development demonstration.
