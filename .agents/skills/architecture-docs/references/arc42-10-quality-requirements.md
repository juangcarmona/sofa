# arc42 10: Quality Requirements

Use `docs/architecture/10-quality-requirements.md` to connect canonical product-intent quality requirements to architectural realization and evidence.

## Ownership

- Treat the project's product-intent source as authoritative for every quality requirement and its acceptance meaning. Do not redefine requirements or copy acceptance criteria.
- For each architecturally relevant `QR-*`, show the product-intent citation, architectural mechanisms, current evidence and any known realization gap.
- Keep section 01 limited to the three to five highest-priority quality goals; this section provides the broader, detailed view.
- Use specific, measurable usage, change, fault or failure scenarios from the project's product-intent source when they help evaluate the architecture.
- Link ADRs for quality-related choices and tests, telemetry, configuration or code as evidence.

## Apply this section

Maintain the flow `ProductShape quality requirement` to `architectural realization` to `evidence or known gap`. Group requirements only when the grouping preserves traceability. Exclude generic quality taxonomies and unevidenced compliance claims.

## Document scaffold

```markdown
---
title: Quality Requirements
arc42-section: "10"
description: Architectural realization and evidence for product quality requirements.
---

# Quality Requirements

## Quality Requirements Overview

| ProductShape requirement | Architectural realization | Evidence or gap |
| --- | --- | --- |

## Quality Scenarios

### [Quality Scenario]
```
