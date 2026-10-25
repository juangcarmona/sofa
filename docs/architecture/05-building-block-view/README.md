---
title: Building block view
arc42-section: "05"
description: The static decomposition of the system into building blocks.
---

# 05. Responsibilities

The observation adapter under `src/observation/` collects GitHub evidence. Future projection and verification components will explain source results without changing them. Repository routers and stage workers are Lighthouse components outside SoFa. Agent process supervision and isolation belong to the independent executor. The retired `SoFa.Runtime`, bridge and engine sources are retained for rollback and are not active product building blocks.

<!-- pdac:cite id="BC-EXECUTION" digest="sha256:a12712bab0b780d1a2132400bed82c4cca7c3ceeb187911dde55366d6581821e" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
