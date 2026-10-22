---
title: System scope and context
arc42-section: "03"
description: The system boundary, its users, and its external interfaces.
---

# 3. System scope and context

**Proposal only.** No application implementation or accepted architecture decision exists yet. Current product intent is the applied [CHG-INITIAL](../../product/changes/completed/chg-initial/change.md). This page retains the former Attention proposal for historical traceability.

## Business context

Juan initiates a refresh and reads findings. GitHub is the external evidence authority. The Repository Attention boundary observes repository work; it owns no repository workflow execution.

See the historical proposed [bounded context](../../product/changes/superseded/chg-repository-attention-proposal/proposed/domain/bounded-contexts/bc-repository-attention.md).

## Technical context

The browser accesses a local SoFa web host. That host reads GitHub over HTTPS with Juan’s existing authorized access. It emits operational telemetry to Aspire locally. GitHub remains the destination for investigation and all actions on findings.

No GitHub webhook receiver, runner integration, or agent connection is introduced. The network exposure policy is specified in [section 07](../07-deployment-view/README.md).
