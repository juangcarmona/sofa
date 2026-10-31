---
title: Introduction and goals
arc42-section: "01"
description: System purpose, stakeholders, and architecture goals.
---

# 01. Observe the factory across repositories

SoFa gives the owner a source-backed view of work, execution and delivery across repositories. Repository workflows orchestrate the lifecycle; an independent runtime executes agents. SoFa remains optional.

<!-- pdac:cite id="FR-AUTHORIZED-ENTRY" digest="sha256:33f2e096f91ca23a5fa8bc58fa6e0165659fc1d1cb099a8b6c9b7236e240276b" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed operational product

The [draft Product Change](../../product/changes/active/chg-authenticated-operations-001/change.md) extends the observed baseline to a single authenticated workspace for daily work. The first proof is issue context → eligible Refine/Propose or comment → source receipt → correlated result, usable from a phone. Portfolio, intervention, project and signal views extend the same source projections in later deliveries. These are proposed views of draft intent, not implemented behavior or accepted baseline changes.

The owner remains the primary operator. Existing requester and engineer distinctions are repository capabilities, not a new organization-management product. Architectural success means trustworthy action outcomes, explainable incomplete evidence and execution that remains independent of SoFa.
