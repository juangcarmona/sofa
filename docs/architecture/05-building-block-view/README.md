---
title: Building block view
arc42-section: "05"
description: The static decomposition of the system into building blocks.
---

# 05. Implemented application blocks

| Building block | Responsibility | Source |
| --- | --- | --- |
| Web entrypoint | HTTPS application, safe hosting/logging and module composition | [SoFa.Web](../../../src/SoFa.Web/Program.cs) |
| Workspace module | Protected account/observation routes, owner admission, process-local sessions and explicit GitHub connection | [Module entrypoint](../../../src/SoFa.Modules.Workspace/WorkspaceModule.cs) |
| Source query adapters | Effective user/app permission checks, bounded/coalesced observations and account-status projection | [Source adapters](../../../src/SoFa.Modules.Workspace/Source/) |
| Browser presentation | Source-keyed observations, account states and removal of data after authority loss | [Browser assets](../../../src/web/public/) |
| Development composition | Workspace plus isolated identity container; external mode omits the provider | [AppHost composition](../../../src/SoFa.AppHost/WorkspaceComposition.cs) |
| Historical observation CLI | Explicit operator-authorized GitHub reads outside the web application | [Legacy collector](../../../src/observation/github.mjs) |

The workspace module is an application library, not an independently deployed service. Its query handlers are internal and sealed; compiler checks enforce module boundaries. Session/source mechanisms have one canonical explanation in [section 08](../08-crosscutting-concepts/README.md). The AppHost does not reference or start the retained execution runtime.

<!-- pdac:cite id="BC-EXECUTION" digest="sha256:a12712bab0b780d1a2132400bed82c4cca7c3ceeb187911dde55366d6581821e" -->

## Target blocks not yet implemented

Full paginated issue/conversation context, source capability/receipt adapters, a durable application command journal and signal projections remain target work. A future receipt reconciler observes exact source acknowledgments and does not schedule or automatically resubmit work. [ADR-0008](../09-architecture-decisions/0008-source-correlated-issue-commands.md) owns the proposed durability decision.
