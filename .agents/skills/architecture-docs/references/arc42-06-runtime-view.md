# arc42 06: Runtime View

Use `docs/architecture/06-runtime-view.md` for representative, architecturally significant interactions between building-block instances.

## Ownership

- Select a small set of scenarios covering critical use cases, external interactions, startup or administration behavior, consistency boundaries, and important error or failure paths.
- Name participants with the building blocks defined in section 05. A runtime view explains their cooperation; it does not introduce a second decomposition.
- Describe schematic or partial scenarios when they expose the architectural mechanism more clearly than exhaustive end-to-end detail.
- Cite the project's product-intent source when a use case, rule or requirement governs the scenario, but describe the architectural interaction rather than duplicating product steps.
- Verify sequences against code and tests. Link ADRs for significant interaction or failure-handling choices.

## Apply this section

Choose the lightest notation that makes order, responsibility and notable state or failure transitions clear: numbered steps, sequence diagram, activity diagram or state machine. Exclude endpoint catalogues, routine control flow and scenarios with no architectural consequence.

## Document scaffold

```markdown
---
title: Runtime View
arc42-section: "06"
description: Architecturally significant runtime scenarios and building-block interactions.
---

# Runtime View

## [Runtime Scenario]

[Diagram or ordered interaction]

### Notable Interactions

### Failure Behavior
```

Repeat the scenario block only for representative scenarios. Remove `Failure Behavior` when failure handling is not architecturally relevant.
