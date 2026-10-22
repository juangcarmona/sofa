---
title: Risks and technical debt
arc42-section: "11"
description: Known risks, technical debt, and their planned responses.
---

# 11. Risks and technical debt

**Proposal only.** No application implementation or accepted architecture decision exists yet. Product intent is the applied [CHG-INITIAL](../../product/changes/completed/chg-initial/change.md). The Attention risks below are historical proposal material.

## Risks

| Risk | Response before or during implementation |
| --- | --- |
| Review labels and release workflows differ between repositories | Inspect the five repositories and record explicit mappings; show unconfigured categories honestly |
| Old failed release targets create permanent noise | Define which targets constitute current release evidence before implementing that collector; obtain Juan’s decision if this changes product semantics |
| Access, rate limits, pagination, or changing PR heads hide evidence | Bounded collection with explicit coverage outcomes; revision recheck; adapter fixtures |
| No accepted product baseline yet | Keep this architecture proposed; generate ProductShape citations after acceptance |
| Proposed .NET/Aspire versions are not verified on Bastion | Check installed SDK and supported package versions, then pin before scaffolding |
| In-memory snapshots disappear on restart | Explicit empty state; introduce persistence only when history or durable execution is approved |

## Technical debt

There is no application implementation to assess. A database, queue, scheduler, agent engine, authentication platform, and plugin SDK are deferred capabilities, not implied implementation debt.
