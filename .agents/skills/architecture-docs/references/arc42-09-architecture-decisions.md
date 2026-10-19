# arc42 09: Architecture Decisions

Use `docs/architecture/09-architecture-decisions.md` as the architecture-facing index of architecturally significant decisions.

## Ownership

- Use this section as a navigation index of decisions that materially affect structure, quality characteristics, important dependencies, interfaces or construction techniques.
- Give each entry a title, ADR link and affected architecture. Identify relevant arc42 sections or architectural areas in `Affected architecture`.
- Keep status, context, rationale, alternatives, consequences and decision history in `docs/adr/`, which remains authoritative for decision information. Do not copy those values into this index.
- Use implementation evidence to confirm the current realized state. A superseded or unimplemented ADR must not be presented as current architecture.
- Cite product intent in the architecture view that realizes it, not in this index.

## Apply this section

Keep the index thin and navigational. Add or update an ADR through the repository's ADR process when decision information is missing; then link all affected architecture views to that ADR where the resulting shape needs explanation.

## Document scaffold

```markdown
---
title: Architecture Decisions
arc42-section: "09"
description: Index of architecturally significant decisions and affected views.
---

# Architecture Decisions

| Decision | ADR | Affected architecture |
| --- | --- | --- |
```
