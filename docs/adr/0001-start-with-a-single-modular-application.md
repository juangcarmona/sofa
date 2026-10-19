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

SoFa currently contains repository scaffolding and no application runtime. The accepted [initial product change](../product/changes/completed/chg-initial/change.md) describes a local attention list, and Juan asks for a foundation that can grow into his software factory without a large initial build. Aspire is a stated operating constraint. The interaction surface is not fixed by the product definition.

Raccord’s modular-monolith and deferred-events decisions provide a precedent for explicit boundaries and incremental infrastructure. They are references, not existing SoFa decisions.

## Decision

We will, implement one C#/.NET application with an ASP.NET Core host, a Repository Attention module, and a GitHub read adapter. An Aspire AppHost will compose local execution and telemetry. Choose and pin compatible SDK/package versions during implementation.

The Attention module will own pure evidence classification and refresh orchestration behind narrow use-case interfaces. The GitHub adapter implements an Attention-owned evidence-source port. The web host composes these blocks and provides one server-rendered page with refresh and evidence links. Dependencies point inward; no business classification belongs in the host or adapter.

Keep only the latest snapshot in memory. Begin with the four source projects described in the [building block view](../architecture/05-building-block-view.md). Add a separate module when an approved capability establishes another business boundary. Introduce persistence or asynchronous infrastructure only for a concrete requirement; introduce independently deployed services only for a demonstrated operational or ownership need.

This balances a usable local surface and standard Aspire/.NET integration with a small codebase. It does not adopt the rest of Raccord’s module contracts, persistence, or shared-kernel machinery.

## Consequences

- **Positive:** One application is simple to run locally. The core rules can be tested without GitHub or a browser, and future presentation or collection triggers can reuse the same use cases.
- **Negative:** This introduces a .NET toolchain alongside existing npm documentation tooling. A page requires request protection and rendering checks. In-memory state disappears on restart.
- **Neutral:** The first module remains synchronous within the process while network calls are asynchronous. Execution, hooks, history, and scheduling need later product decisions; no generic platform framework is created now.

## Alternatives considered

- **Independent shell scripts:** Useful for disposable investigation, but scattered classification and output logic would make this product boundary harder to maintain.
- **A structured command-line application:** A viable smaller presentation option. Not preferred because a single local page makes grouped findings, coverage, and evidence links easier to revisit; the core boundary would support a CLI later if needed.
- **Node.js application:** Viable and aligned with current documentation tooling. Not preferred for this proposal because the requested Aspire-centered foundation and neighboring .NET application precedent favor a consistent application stack. Existing npm tooling does not determine the product runtime.
- **Separate API and SPA:** Adds a frontend build and deployment boundary without a requirement that needs them.
- **Distributed services, queue, and database:** Add operational and consistency costs before the attention slice needs durable state or asynchronous consumers.

## References

- [Proposed solution strategy](../architecture/04-solution-strategy.md)
- [Raccord ADR-0001](https://github.com/juangcarmona/raccord/blob/main/docs/adr/0001-modular-monolith.md), inspected in the sibling checkout
- [Raccord ADR-0020](https://github.com/juangcarmona/raccord/blob/main/docs/adr/0020-synchronous-module-contracts-with-deferred-events.md), inspected in the sibling checkout
- [Aspire service defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults)
