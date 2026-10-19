---
title: Risks and technical debt
arc42-section: "11"
description: Known risks, technical debt, and their planned responses.
---

# 11. Risks and technical debt

**Approved design; implementation pending.** Juan accepted this architecture on 2026-10-19. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Risks

| Risk | Response before or during implementation |
| --- | --- |
| Review labels and release workflows differ between repositories | Inspect the five repositories and record explicit mappings; show unconfigured categories honestly |
| Old failed release targets create permanent noise | Juan confirmed newest target per workflow; use the latest attempt and show incomplete coverage when target identity cannot be established |
| Access, rate limits, pagination, or changing PR heads hide evidence | Bounded collection with explicit coverage outcomes; revision recheck; adapter fixtures |
| Proposed .NET/Aspire versions are not verified on Bastion | Check installed SDK and supported package versions, then pin before scaffolding |
| In-memory snapshots disappear on restart | Explicit empty state; introduce persistence only when history or durable execution is approved |

## Technical debt

There is no application implementation to assess. A database, queue, scheduler, agent engine, authentication platform, and plugin SDK are deferred capabilities, not implied implementation debt.
