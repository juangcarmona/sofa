---
id: FR-IDEA-PROMOTION
type: functional-requirement
title: Explore an idea and promote or link it to an issue
status: draft
derived-from: [UC-TRIAGE-BACKLOG, BR-GITHUB-SOURCE, BR-WORK-AUTHORIZATION]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: "An owner explores an idea, reviews its proposed issue in a selected repository and explicitly creates it or links an existing issue."
  - scenario: "A lost creation response is reconciled before retry and the idea retains the confirmed issue identity."
---

## Requirement

SoFa MUST let the owner capture and revisit an idea, clarify its intended outcome and scope, choose a repository and review the proposed issue content. It MUST show candidate existing issues before explicit promotion or linking, and preserve the confirmed GitHub identity. Uncertain creation MUST be reconciled before creating another issue. An idea MUST NOT enter normal repository execution before promotion or linking. Exploration does not require an embedded terminal or automatically start an agent; any agent assistance uses an explicit supported repository capability.

## Rationale

Ideas become executable backlog while retaining user control over what enters a repository.
