---
name: start-with-a-single-modular-application
description: Start SoFa with one application and an isolated Attention module, using Aspire for local orchestration and telemetry.
status: Accepted
date: 2026-10-19
deciders: ["Juan G. Carmona"]
tags: ["foundation", "modularity", "aspire"]
---

# ADR-0001: Start with a single modular application

## Context

SoFa contains a small application foundation and its accepted initial product change establishes the complete Software Factory. Juan asks for a foundation that can grow without a large initial build. Aspire is a stated operating constraint. The interaction surface is not fixed by the product definition. Earlier Repository Attention architecture remains historical proposal material.

Raccord’s modular-monolith and deferred-events decisions provide a precedent for explicit boundaries and incremental infrastructure. They are references, not existing SoFa decisions.

## Decision

We will implement one C#/.NET application with an ASP.NET Core host and modular business boundaries. An Aspire AppHost will compose local execution and telemetry. Choose and pin compatible SDK/package versions during implementation.

Each approved factory capability will own its rules and use-case interfaces behind explicit module boundaries. The host composes those modules without taking ownership of product decisions or external-source semantics. Dependencies point inward; no business classification belongs in the host or adapter.

Keep the initial runtime small and modular. Add a separate module when an approved capability establishes another business boundary. Introduce persistence or asynchronous infrastructure only for a concrete requirement; introduce independently deployed services only for a demonstrated operational or ownership need.

This balances a small local foundation and standard Aspire/.NET integration with deliberate growth. It does not adopt the rest of Raccord’s module contracts, persistence, or shared-kernel machinery.

## Consequences

- **Positive:** One application is simple to run locally. Factory rules can be tested independently from presentation and external systems.
- **Negative:** This introduces a .NET toolchain alongside existing npm documentation tooling. Runtime security and recovery controls remain implementation work.
- **Neutral:** Modules remain in one process initially. Execution, hooks, history, and scheduling are product capabilities with explicit boundaries; no generic platform framework is created now.

## Alternatives considered

- **Independent shell scripts:** Useful for disposable investigation, but scattered classification and output logic would make this product boundary harder to maintain.
- **A structured command-line application:** A viable smaller presentation option. Not preferred because a single local page makes grouped findings, coverage, and evidence links easier to revisit; the core boundary would support a CLI later if needed.
- **Node.js application:** Viable and aligned with current documentation tooling. Not preferred for this proposal because the requested Aspire-centered foundation and neighboring .NET application precedent favor a consistent application stack. Existing npm tooling does not determine the product runtime.
- **Separate API and SPA:** Adds a frontend build and deployment boundary without a requirement that needs them.
- **Distributed services, queue, and database:** Add operational and consistency costs before the attention slice needs durable state or asynchronous consumers.

## References

- [Proposed solution strategy](../architecture/04-solution-strategy/README.md)
- [Raccord ADR-0001](https://github.com/juangcarmona/raccord/blob/main/docs/adr/0001-modular-monolith.md), inspected in the sibling checkout
- [Raccord ADR-0020](https://github.com/juangcarmona/raccord/blob/main/docs/adr/0020-synchronous-module-contracts-with-deferred-events.md), inspected in the sibling checkout
- [Aspire service defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults)
