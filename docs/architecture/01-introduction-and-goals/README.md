---
title: Introduction and goals
arc42-section: "01"
description: System purpose, stakeholders, and architecture goals.
---

# 01. Observe the factory across repositories

SoFa gives the owner a source-backed view of work, execution and delivery across repositories. Repository workflows orchestrate the lifecycle; an independent runtime executes agents. SoFa remains optional.

<!-- pdac:cite id="FR-AUTHORIZED-ENTRY" digest="sha256:9d9205257c47d81d2f01866ea9d643bc5f169f4295cf37a600086a405eab8463" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Accepted operational product direction

The [applied Product Change](../../product/changes/completed/chg-authenticated-operations-001/change.md) extends the accepted product baseline to a single authenticated workspace for daily work. The first proof is issue context → eligible Refine/Propose or comment → source receipt → correlated result, usable from a phone. Portfolio, intervention, project and signal views extend the same source projections in later deliveries. The architecture views below remain proposed and the behavior is not yet implemented.

The owner remains the primary operator. Existing requester and engineer distinctions are repository capabilities, not a new organization-management product. Architectural success means trustworthy action outcomes, explainable incomplete evidence and execution that remains independent of SoFa.
