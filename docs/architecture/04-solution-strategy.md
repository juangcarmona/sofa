---
title: Solution strategy
arc42-section: "04"
description: Fundamental decisions and strategies that shape the architecture.
---

# 4. Solution strategy

**Approved architecture.** Juan accepted this architecture on 2026-10-19; issue 6 implements the initial slice. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Technology decisions

[ADR-0001](../adr/0001-start-with-a-single-modular-application.md) establishes a C#/.NET application with an ASP.NET Core host and an Aspire AppHost. Juan accepted this decision on 2026-10-19.

## Top-level decomposition

Start with one business module, Repository Attention. Separate its rules and refresh orchestration from the GitHub adapter and presentation. A single server-rendered page offers refresh and evidence links; no separate frontend build is proposed.

## Quality realization

| Driver | Approach | Owning view |
| --- | --- | --- |
| Keep rules independent of tools | Dependencies point toward the Attention module | [Building blocks](05-building-block-view.md) |
| Preserve useful partial results | Per-category outcomes and one immutable refresh snapshot | [Runtime](06-runtime-view.md) |
| Operate locally with visibility | Aspire orchestration and OTLP telemetry | [Deployment](07-deployment-view.md) |
| Keep future growth deliberate | Add modules only for approved capabilities | [ADR-0001](../adr/0001-start-with-a-single-modular-application.md) |
