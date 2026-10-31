---
id: FR-AUTHORIZED-ENTRY
type: functional-requirement
title: Submit explicit commands through verified repository authority
status: draft
derived-from: [UC-ACCESS-FACTORY, UC-OPERATE-ISSUE, BR-WORK-AUTHORIZATION, BR-HUMAN-DECISION-BOUNDARIES, BR-UNKNOWN-RISK, BR-LAN-PRIVILEGE-BOUNDARY, BR-EXPLICIT-WORK-COMMANDS]
uses-terms: [TERM-WORK-COMMAND, TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: The authenticated owner requests Propose from valid refinement; server and repository checks retain the actual GitHub actor, and the resulting request is correlated to the exact source execution and published result.
  - scenario: Anonymous, unauthorized, cross-site or stale submissions are denied with a safe explanation and no source mutation.
  - scenario: Double submission and a lost response resolve to an existing request or an explicit uncertain state rather than duplicate work or a guessed run match.
  - scenario: Stopping SoFa after source admission leaves repository execution able to finish and publish its result.
---

## Requirement

SoFa MUST let the authenticated, source-authorized owner explicitly request supported Refine and Propose commands from issue context. It MUST verify current identity, source permission, capabilities, evidence and reservations on the server, consume the shared Lighthouse contract and submit through the normal repository entrypoint. GitHub MUST verify the actual user; a bot identity MUST NOT impersonate the owner.

SoFa MUST retain the distinction between submission, source admission or rejection, discovery of an exactly correlated execution, progress, human wait and published result. It MUST handle duplicate and uncertain requests without silent replay and preserve source identities, state freshness and limitations. An accepted request does not approve its future output or authorize a later stage.

SoFa MUST NOT independently admit agents, allocate execution capacity, resume an execution outside source policy or publish agent stage effects. Repository execution MUST remain available when SoFa is unavailable.

## Rationale

The owner can operate repository work from one place while source authorization and workflow controls retain their authority.
