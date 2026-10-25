---
id: CON-FACTORY-RUNTIME
type: constraint
title: Keep the execution runtime independent of SoFa
status: draft
applies-to: [BC-EXECUTION, BC-SCHEDULING, BC-DELIVERY, BC-RUNTIME-OPERATIONS]
---

## Constraint

Repository workflows MUST own lifecycle orchestration and use an independently operated execution runtime. SoFa MUST be an optional observer and verifier, with any future authenticated controls submitting actions through repository entrypoints. No SoFa database, scheduler, lease, supervisor or service MAY be required to execute repository work. This constraint does not select the future SoFa UI framework.

## Rationale

The owner supervises work across repositories without making that view a prerequisite for delivery.

## Consequences

Runtime operation and workflow gates remain outside SoFa. The observation surface can evolve without migrating repository execution.
