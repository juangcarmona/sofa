# arc42 11: Risks and Technical Debt

Use `docs/architecture/11-risks-and-technical-debt.md` for the current prioritized assessment of architecturally significant risks and technical debt.

## Ownership

- Record the risk or debt, architectural impact, current evidence, priority and mitigation or decision needed.
- Examine external interfaces, processes, data structures, source code, infrastructure and quality gaps for evidence-backed concerns.
- Cite the project's product-intent source when a constraint or quality requirement establishes impact or tolerance.
- Link issues for tracked work, ADRs for decisions and implementation sources for current conditions. This section owns the architectural assessment, not task execution.
- Remove resolved entries or retain only a link to durable decision history when it remains architecturally relevant.

## Apply this section

Order entries by priority and distinguish observed debt from uncertain risk. Exclude feature backlogs, ordinary cleanup, resolved history and speculation without evidence.

## Document scaffold

```markdown
---
title: Risks and Technical Debt
arc42-section: "11"
description: Prioritized architectural risks, technical debt, evidence, and mitigation.
---

# Risks and Technical Debt

| Risk or debt | Architectural impact | Evidence | Priority | Mitigation or decision |
| --- | --- | --- | --- | --- |
```
