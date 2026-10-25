---
id: FR-RETENTION-POLICY
type: functional-requirement
title: Expose retention and evidence availability
status: draft
derived-from: [BR-RETENTION-STANDARDS, UC-MAINTAIN-RUNTIME]
uses-terms: [TERM-RETENTION-POLICY, TERM-DELIVERY-EVIDENCE]
verification: [{scenario: "Source observations preserve identity and limitations; stopping SoFa does not stop repository workflows; a read refresh cannot dispatch or publish work."}]
---

## Requirement

SoFa MUST expose the effective retention policies and evidence availability reported by configured sources. It MAY apply a retention policy to its own observation cache. It MUST NOT delete repository artifacts, agent sessions or runtime state as a side effect of collection, and MUST distinguish expired or unavailable evidence from successful delivery.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.
