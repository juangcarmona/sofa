---
id: "CON-BASTION"
type: "constraint"
title: "Operate personally on Bastion with Aspire"
status: "draft"
applies-to: ["BC-REPOSITORY-ATTENTION"]
---

## Constraint

The first slice MUST run on Juan’s Ubuntu 26 Bastion machine with Aspire orchestration and operational telemetry available through the Aspire environment. It serves Juan personally and does not require a commercial hosted service or a new identity platform.

## Rationale

Juan explicitly selected his existing machine and Aspire, and wants a small starting point that can grow into his software factory.

## Consequences

Architecture must provide a repeatable local start and observable collection execution. Aspire is an imposed operating constraint, not evidence that repository delivery metrics already exist. Language, host structure, persistence, and interaction surface remain design decisions.
