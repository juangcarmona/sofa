---
title: Architecture constraints
arc42-section: "02"
description: Technical, organizational, and convention constraints that shape the architecture.
---

# 2. Architecture constraints

**Approved design; implementation pending.** Juan accepted this architecture on 2026-10-19. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Technical constraints

The accepted [Bastion constraint](../product/model/requirements/constraints/con-bastion.md) governs placement and Aspire visibility. Exact SDK and Aspire versions will be selected and pinned during implementation after checking Bastion; no installation compatibility is asserted here.

<!-- pdac:cite id="CON-BASTION" digest="sha256:9272e2603ef2cd419ca1389de021c86b4a2f4311fa5312eb4c12c8d7d6d25f58" -->

## Organizational constraints

[Observe-only behavior](../product/model/business-rules/br-observe-only.md) excludes write adapters and agent dispatch. Product and merge authority remain with Juan.

<!-- pdac:cite id="BR-OBSERVE-ONLY" digest="sha256:47f0cac9c3d0f36bacd81107f8cdda2a2e21533db35b92501f684019da0dc8d3" -->

## Conventions

Keep application code under `src/`, tests under `tests/`, product semantics in ProductShape, and decision rationale in ADRs. Follow the [existing lifecycle](../engineering-lifecycle.md) for implementation.
