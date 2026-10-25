---
title: Quality requirements
arc42-section: "10"
description: Quality goals and concrete scenarios for evaluating them.
---

# 10. Acceptance scenarios

Stop SoFa and run repository refinement: the workflow must publish its result. Refresh an inaccessible repository: the observer must report unavailable evidence. Inspect a run: preparation, execution, validation, publication and cleanup must be distinguishable. Change the issue during execution: publication must stop. A read refresh must never dispatch or cancel work.

<!-- pdac:cite id="QR-EXECUTION-RECOVERY" digest="sha256:bd809a3cd54d1b9f7a423becb4bf5514f033b0018a1316db4c36141847c893df" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
