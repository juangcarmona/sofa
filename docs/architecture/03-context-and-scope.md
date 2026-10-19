---
title: System scope and context
arc42-section: "03"
description: The system boundary, its users, and its external interfaces.
---

# 3. System scope and context

**Approved architecture.** Juan accepted this architecture on 2026-10-19; issue 6 implements the initial slice. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Business context

Juan initiates a refresh and reads findings. GitHub is the external evidence authority. The Repository Attention boundary observes repository work; it owns no repository workflow execution.

See the accepted [bounded context](../product/model/domain/bounded-contexts/bc-repository-attention.md).

<!-- pdac:cite id="BC-REPOSITORY-ATTENTION" digest="sha256:4ff75cd42f29c3968ff00cce69788d83c35f8602c13ea36748b6583c7ef5beb9" -->

## Technical context

The browser accesses a local SoFa web host. That host reads GitHub over HTTPS with Juan’s existing authorized access. It emits operational telemetry to Aspire locally. GitHub remains the destination for investigation and all actions on findings.

No GitHub webhook receiver, runner integration, or agent connection is introduced. The network exposure policy is specified in [section 07](07-deployment-view.md).
