# arc42 01: Introduction and Goals

Use `docs/architecture/01-introduction-and-goals.md` to orient architecture readers and identify the product and quality drivers that shape the architecture.

## Ownership

- Keep the system introduction and requirements overview short. Link to the project's product-intent source instead of reproducing product goals, features, actors, use cases or requirements.
- Identify the architecture documentation audience and its expectations only when that information determines scope or detail. ProductShape remains authoritative for product actors and stakeholders.
- Surface only the three to five highest-priority quality goals here. Section 10 owns the complete mapping from quality requirements to architectural realization and evidence.
- Verify the system summary against implementation evidence. Link strategy and ADRs for architectural responses and rationale.

## Apply this section

Write the minimum orientation needed to understand the remaining views. Make each driver concrete, prioritized and traceable to its ProductShape owner. Keep architecture prose focused on why the driver matters to the system shape, not on restating the driver.

## Document scaffold

```markdown
---
title: Introduction and Goals
arc42-section: "01"
description: Architectural orientation, drivers, quality goals, and documentation stakeholders.
---

# Introduction and Goals

## Requirements Overview

## Quality Goals

## Stakeholders
```

Use `Stakeholders` for consumers and maintainers of the architecture documentation and their expectations. Link the project's product-intent source for product stakeholders rather than copying its catalogue.
