---
title: System scope and context
arc42-section: "03"
description: The system boundary, its users, and its external interfaces.
---

# 03. System context

The owner uses GitHub directly or a future authenticated SoFa interface. GitHub owns issues, comments, runs, pull requests and artifacts. Lighthouse supplies the engineering contract and reusable workflows. The operator owns Bastion, repository runner registrations and the independent executor. SoFa reads their evidence and explains limitations.

<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
