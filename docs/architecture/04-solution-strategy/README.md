---
title: Solution strategy
arc42-section: "04"
description: Fundamental decisions and strategies that shape the architecture.
---

# 04. Project source facts

The read-only Node collector now serves both the CLI and a native HTML/CSS/JS web interface. The web server uses Node’s HTTP library and Lighthouse UI tokens, keeping the existing collection adapter and adding no application framework. Repository selection is operator configuration. This delivers observation and source navigation; authenticated embedded workflow controls remain future work.

<!-- pdac:cite id="FR-BACKLOG-PROJECTION" digest="sha256:b98f45997965ba4d187781ca782921eb8c98cb8a9877de2081d26d1e6e5a4d55" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed extension strategy

Extend the web application with a server-side authenticated boundary and reusable query/command adapters. [ADR-0009](../09-architecture-decisions/0009-dotnet-and-aspire.md) selects .NET and Aspire; migrate the existing observer behavior to the .NET boundary and retain its presentation where suitable. Replace bounded attention-only reads for issue discovery and detail. Aspire composes the web process, development Keycloak and required development storage without restarting the retired supervisor. No frontend rewrite is required to establish the journey.

Use one same-origin backend for sessions, GitHub connection, current eligibility checks, mutation submission and read models. The browser receives sanitized view data and opaque request identifiers, never source tokens. Reuse a maintained OIDC implementation rather than implementing token validation. Authentication details follow ADR-0007; [ADR-0008](../09-architecture-decisions/0008-source-correlated-issue-commands.md) proposes command durability and correlation.

Build issue detail and its action/result view first. Search, inbox and boards reuse these records instead of implementing parallel action handlers. Signals consume shared/source definitions with inspectable inputs. Later history collection must not delay the first command.
