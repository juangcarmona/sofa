# arc42 05: Building Block View

Use `docs/architecture/05-building-block-view.md` as the canonical static decomposition of the system.

## Ownership

- Always document level 1: the overall system as a white box containing its major black boxes. Explain the decomposition and keep external interfaces consistent with section 03.
- Describe each important black box by name, responsibility and interfaces. Add quality characteristics, source location or open risks only when useful.
- Refine selected black boxes as level-2 or deeper white boxes. Prefer relevance over completeness: expand only important, surprising, risky, complex or volatile blocks.
- Map each architectural building block to stable source location(s) where useful. Do not attempt to classify every source directory, package or file into a building block. Source remains implementation truth.
- Keep shared mechanisms in section 08 and dynamic cooperation in section 06. Link ADRs that explain significant decomposition choices.

## Apply this section

Use hierarchical diagrams and compact black-box tables. Preserve abstraction at each level, hide black-box internals until the next refinement level, and justify every white-box structure. Cite the project's product-intent source only when intent directly explains a responsibility or boundary.

## Document scaffold

```markdown
---
title: Building Block View
arc42-section: "05"
description: Static decomposition, responsibilities, interfaces, and dependencies.
---

# Building Block View

## Whitebox Overall System

### Overview

### Decomposition Rationale

### Contained Building Blocks

| Building block | Responsibility | Interfaces | Source location |
| --- | --- | --- | --- |

### Important Interfaces

## Level 2

### Whitebox: [Selected Building Block]

## Level 3

### Whitebox: [Selected Nested Building Block]
```

Repeat whitebox headings only for selected refinements. Remove level 2 or level 3 when no block merits that detail.
