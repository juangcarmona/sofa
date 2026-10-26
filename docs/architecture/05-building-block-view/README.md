---
title: Building block view
arc42-section: "05"
description: The static decomposition of the system into building blocks.
---

# 05. Responsibilities

The observation adapter under `src/observation/` collects GitHub evidence. `src/web/server.mjs` serves fixed assets and a read-only snapshot endpoint with a shared, coalesced 15-second cache. `src/web/public/` renders source timestamps, unavailable evidence, attention labels and recent runs using text nodes and repository-scoped GitHub links. Repository routers and workers execute independently; the retired runtime remains disabled.

<!-- pdac:cite id="BC-EXECUTION" digest="sha256:a12712bab0b780d1a2132400bed82c4cca7c3ceeb187911dde55366d6581821e" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
