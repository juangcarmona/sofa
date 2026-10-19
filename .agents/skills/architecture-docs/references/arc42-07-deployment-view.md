# arc42 07: Deployment View

Use `docs/architecture/07-deployment-view.md` to describe relevant runtime infrastructure and map software building blocks onto it.

## Ownership

- Document infrastructure nodes, locations, environments, processors or containers, network channels and trust boundaries only to the detail needed to understand software deployment.
- Make the mapping from section-05 building blocks or deployable artifacts to infrastructure elements explicit. Infrastructure without that mapping is not a complete deployment view.
- Represent every materially different environment or deployment variant. Explain relevant topology motivation and quality or performance characteristics.
- Refine complex infrastructure elements hierarchically when a level-1 overview is insufficient.
- Cite the project's product-intent source when constraints or quality requirements drive topology, isolation, availability or placement. Infrastructure and composition code remain implementation truth; ADRs own consequential platform choices.

## Apply this section

Use diagrams, mapping tables and short explanations. Separate architectural topology from runbooks, deployment procedures, copied infrastructure-as-code and volatile resource inventories.

## Document scaffold

```markdown
---
title: Deployment View
arc42-section: "07"
description: Runtime infrastructure and the mapping of software building blocks onto it.
---

# Deployment View

## Infrastructure Level 1

### Overview

### Topology Rationale

### Quality and Performance Characteristics

### Building Block Mapping

| Building block or artifact | Infrastructure element | Environment |
| --- | --- | --- |

## Infrastructure Level 2

### [Selected Infrastructure Element]
```

Repeat the level-1 view for materially different environments. Remove level 2 when the overview contains enough detail.
