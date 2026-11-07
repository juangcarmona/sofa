---
title: Solution strategy
arc42-section: "04"
description: Fundamental decisions and strategies that shape the architecture.
---

# 04. Solution strategy

The application serves the existing HTML/CSS/JavaScript presentation through a .NET server boundary. The workspace module contains owner sessions, GitHub user connection and bounded source reads. Separate query handlers project account authority and observations. The historical Node collector remains an explicit CLI tool and is never an authentication fallback.

<!-- pdac:cite id="FR-BACKLOG-PROJECTION" digest="sha256:b98f45997965ba4d187781ca782921eb8c98cb8a9877de2081d26d1e6e5a4d55" -->

| Driver | Realization | Canonical detail |
| --- | --- | --- |
| Independent execution | Keep repository workflows and the executor outside the application | [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) |
| Authenticated access | Maintained cookie/OIDC middleware and explicit GitHub user authorization | [Cross-cutting mechanisms](../08-crosscutting-concepts/README.md), [ADR-0007](../09-architecture-decisions/0007-shared-identity-custody-and-recovery.md) |
| Established application direction | One workspace process and an isolated development provider in Aspire | [Deployment](../07-deployment-view/README.md), [ADR-0009](../09-architecture-decisions/0009-dotnet-and-aspire.md) |
| Source attribution | Check effective user/app authority before reading or reusing source data | [Runtime interactions](../06-runtime-view/README.md) |

<!-- pdac:cite id="FR-AUTHORIZED-ENTRY" digest="sha256:9d9205257c47d81d2f01866ea9d643bc5f169f4295cf37a600086a405eab8463" -->

## Remaining extension strategy

Paginated issue context, discovery, intervention requests and boards must share source identities and the same authorized adapters. Issue mutations, durable command receipts, explicit continuation and signal projections are not implemented by the identity slice. [ADR-0008](../09-architecture-decisions/0008-source-correlated-issue-commands.md) remains Proposed; no application journal or execution database is introduced here.
