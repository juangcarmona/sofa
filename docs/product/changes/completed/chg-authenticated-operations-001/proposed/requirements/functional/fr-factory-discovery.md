---
id: FR-FACTORY-DISCOVERY
type: functional-requirement
title: Search and filter repository work across the factory
status: draft
derived-from: [UC-EXPLORE-FACTORY, BR-GITHUB-SOURCE]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: "An issue without attention labels is found by its repository and number, and title search preserves the chosen filters."
  - scenario: "A partially collected repository remains visibly incomplete and a failed repository is not counted as empty."
---

## Requirement

SoFa MUST provide a joint repository and issue view with search and combinable filters for repository, issue state, lifecycle stage, execution and human wait. Source identity MUST survive navigation and refresh. Collection coverage, pagination and filters MUST be visible; the owner MUST be able to open ordinary issues as well as attention items.

## Rationale

Finding work across repositories is a daily operation rather than a tour of GitHub tabs.
