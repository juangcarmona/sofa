# arc42 12: Glossary

Use `docs/architecture/12-glossary.md` for architecture-specific technical terms needed to read the architecture consistently.

## Ownership

- Define repository-specific technical names and abbreviations whose meaning is important and not obvious.
- Use one preferred term and one concise definition to prevent synonyms and homonyms from fragmenting architecture prose.
- Reference the project's product-intent source for domain terminology and product concepts. The product-intent source remains their canonical glossary; this section never redefines them.
- Link an ADR or stable implementation source only when it owns a technical term's meaning.

## Apply this section

Use a compact alphabetical table and remove trivia or ordinary technology names. Add a term when inconsistent interpretation would change how readers understand an architecture view.

## Document scaffold

```markdown
---
title: Glossary
arc42-section: "12"
description: Architecture-specific technical terms and abbreviations.
---

# Glossary

| Term | Definition |
| --- | --- |
```
