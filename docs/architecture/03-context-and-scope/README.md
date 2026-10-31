---
title: System scope and context
arc42-section: "03"
description: The system boundary, its users, and its external interfaces.
---

# 03. System context

The owner uses GitHub directly or a future authenticated SoFa interface. GitHub owns issues, comments, runs, pull requests and artifacts. Lighthouse supplies the engineering contract and reusable workflows. The operator owns repository runner registrations and the independent executor. SoFa reads their evidence and explains limitations.

<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed responsibilities

| Participant | Owns | Interface to SoFa |
| --- | --- | --- |
| Owner | Explicit requests and human decisions | Authenticated browser on desktop or phone |
| Keycloak | Authentication and provider session | OIDC; separate production and development instances |
| SoFa | Authenticated access, issue context, user requests, source projections and explanations | Server session, repository adapter and durable command receipts |
| Lighthouse | Versioned eligibility, lifecycle, evidence and continuation semantics | Installed shared contract; no copied UI policy |
| GitHub and repository workflows | Actual actor authorization, admission, reservations, stage orchestration and publication | User-authorized API calls, run receipts, source records and evidence |
| Runtime and infrastructure | Agent execution, isolation, capacity, recovery, credentials and service operation | Workflow invocation and published operational evidence |

Authentication at Keycloak never substitutes for a GitHub user permission. Shared household identity does not admit all household accounts to SoFa. GitHub links remain useful for source inspection and unsupported exceptional actions; supported routine actions complete in the workspace.
