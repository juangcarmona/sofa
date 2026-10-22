---
title: Architecture constraints
arc42-section: "02"
description: Technical, organizational, and convention constraints that shape the architecture.
---

# 2. Architecture constraints

**Proposal only.** No application implementation or accepted architecture decision exists yet. Current product intent is the applied [CHG-INITIAL](../../product/changes/completed/chg-initial/change.md). The Attention constraints below are historical proposal material.

## Technical constraints

The historical [Bastion constraint](../../product/changes/superseded/chg-repository-attention-proposal/proposed/requirements/constraints/con-bastion.md) governs the former Attention proposal. Exact SDK and Aspire versions will be selected and pinned during implementation after checking Bastion; no installation compatibility is asserted here.

## Organizational constraints

[Observe-only behavior](../../product/changes/superseded/chg-repository-attention-proposal/proposed/business-rules/br-observe-only.md) belongs to the former Attention proposal and does not define the Software Factory. Product and merge authority remain with Juan after the initial factory change.

## Conventions

Keep application code under `src/`, tests under `tests/`, product semantics in ProductShape, and decision rationale in ADRs. Follow the [existing lifecycle](../../engineering-lifecycle.md) for implementation.
