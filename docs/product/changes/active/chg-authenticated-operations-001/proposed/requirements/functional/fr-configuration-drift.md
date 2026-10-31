---
id: FR-CONFIGURATION-DRIFT
type: functional-requirement
title: Inspect versions configuration and declared deviations
status: draft
derived-from: [UC-INSPECT-PROJECT-SIGNAL, BR-EXPLAINABLE-SIGNALS]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
verification:
  - scenario: "A declared Lighthouse or harness version is compared with a named compatible baseline and the exact source revision is inspectable."
  - scenario: "Missing configuration is shown as unknown or not applicable with a reason, and viewing a deviation performs no upgrade."
---

## Requirement

SoFa MUST show discovered workflow, harness and relevant configuration versions with their source revision and discovery time. A deviation MUST name the expected baseline and explain its origin; latest available and required by policy MUST remain different facts. Source-owned recommendations can link to relevant work, but viewing a deviation MUST NOT update packages or rewrite repository configuration.

## Rationale

Configuration visibility helps the owner decide which repositories need maintenance without imposing a second adoption policy.
