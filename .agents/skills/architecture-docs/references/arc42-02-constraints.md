# arc42 02: Constraints

Use `docs/architecture/02-constraints.md` to show where architectural design freedom is restricted and what architectural consequences follow.

## Ownership

- Record technical, organizational, political, development and convention constraints only when they materially restrict architecture work.
- Distinguish externally imposed constraints from team decisions. A selected response belongs in an ADR when its rationale, alternatives or consequences matter.
- Cite the project's product-intent source for product-owned constraints and describe only their architectural consequences.
- Link policies, manifests or configuration as evidence instead of copying operational settings or coding guidance.

## Apply this section

Group constraints by useful categories and state the source, restriction and consequence of each. Mark negotiable constraints as such. Exclude preferences and decisions disguised as constraints, because they hide where design freedom actually existed.

## Document scaffold

```markdown
---
title: Architecture Constraints
arc42-section: "02"
description: Constraints that restrict architecture decisions and their consequences.
---

# Architecture Constraints

| Constraint | Source | Architectural consequence | Negotiable |
| --- | --- | --- | --- |
```
