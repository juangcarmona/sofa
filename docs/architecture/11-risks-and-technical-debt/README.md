---
title: Risks and technical debt
arc42-section: "11"
description: Known risks, technical debt, and their planned responses.
---

# 11. Remaining limitations

The current observer reads only the latest twenty runs and the first hundred open issue records per repository. It is not a complete audit and does not verify artifact contents. The UI, continuous collection and authenticated controls remain future work. The independent refinement adapter admits the owner only and does not implement later stages. Runner loss may leave a reservation requiring operator reconciliation. Legacy execution code remains opt-in for rollback and must not regain product authority.

<!-- pdac:cite id="FR-DELIVERY-EVIDENCE" digest="sha256:5a56ea8e0030730dfdf0d8fd51027c45c2e6ddd65393b6d7726c85c27e543074" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
