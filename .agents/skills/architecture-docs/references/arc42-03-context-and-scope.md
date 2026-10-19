# arc42 03: Context and Scope

Use `docs/architecture/03-context-and-scope.md` to define the system boundary, every relevant communication partner and the external interfaces across that boundary.

## Ownership

- Treat the system as a black box. Show users, neighboring systems, domain inputs and outputs, and relevant external influences.
- Separate business context from technical context when that distinction clarifies domain exchanges versus channels, protocols or transmission media.
- Map domain inputs and outputs to their technical channels. Keep internal decomposition in section 05 and detailed infrastructure topology in section 07.
- Cite the project's product-intent source when a boundary, actor interaction or exchange derives from product intent. Verify integrations against code and configuration; link ADRs for consequential interface choices.

## Apply this section

Prefer a context diagram plus a compact interface table. Keep the overview lean but account for every external interface; cluster similar partners only by an explicit criterion. Call out boundary risks here briefly and make section 11 their canonical assessment.

## Document scaffold

```markdown
---
title: Context and Scope
arc42-section: "03"
description: System boundary, communication partners, external interfaces, and channels.
---

# Context and Scope

## Business Context

## Technical Context

## Input and Output Channel Mapping
```
