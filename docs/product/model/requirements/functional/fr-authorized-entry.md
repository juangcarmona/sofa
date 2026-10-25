---
id: FR-AUTHORIZED-ENTRY
type: functional-requirement
title: Observe authorized workflow entry
status: draft
derived-from: [BR-WORK-AUTHORIZATION, BR-HUMAN-DECISION-BOUNDARIES, BR-UNKNOWN-RISK, BR-LAN-PRIVILEGE-BOUNDARY]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
verification: [{scenario: "Source observations preserve identity and limitations; stopping SoFa does not stop repository workflows; a read refresh cannot dispatch or publish work."}]
---

## Requirement

SoFa MUST show the requesting actor, repository workflow, permitted stage, authorization evidence and current decision boundary from source records. An authenticated SoFa control adapter MAY submit an explicit user action to GitHub through the same repository entrypoint. It MUST NOT independently admit, dispatch, resume or publish agent work. Missing authorization evidence MUST appear as unknown. Repository execution MUST remain available when SoFa is unavailable.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.
