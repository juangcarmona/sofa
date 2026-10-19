# arc42 08: Crosscutting Concepts

Use `docs/architecture/08-crosscutting-concepts.md` for shared architectural mechanisms that create consistency across multiple building blocks.

## Ownership

- Select only practices, patterns, rules or solution ideas actually shared by several relevant building blocks or infrastructure elements.
- Explain how each mechanism works, where it applies, where it does not apply and how implementations remain consistent.
- Link affected building blocks to the canonical concept instead of repeating the mechanism in every block description.
- Cite the project's product-intent source for requirements, rules or constraints that the mechanism realizes. Keep domain definitions and policy meaning in ProductShape.
- Link ADRs for why a mechanism was selected and code, configuration or tests for current evidence.

## Apply this section

Create one subsection per important mechanism and adapt its form to the concern: concise prose, model excerpt, runtime scenario or verified implementation pointer. Treat possible topics as discovery prompts, not as a checklist to fill. Exclude one-block behavior, generic technology tutorials and low-level recipes.

## Document scaffold

```markdown
---
title: Crosscutting Concepts
arc42-section: "08"
description: Shared architectural mechanisms applied across multiple building blocks.
---

# Crosscutting Concepts

## [Concept]

### Scope

### Mechanism

### Evidence
```

Repeat the concept block only for mechanisms shared across multiple building blocks. Name concrete concepts rather than retaining generic category headings.
