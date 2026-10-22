---
title: Deployment view
arc42-section: "07"
description: Infrastructure and environment mapping for the system.
---

# 7. Deployment view

**Proposal only.** No application implementation or accepted architecture decision exists yet. Current product intent is the applied [CHG-INITIAL](../../product/changes/completed/chg-initial/change.md). The deployment below is the historical Attention proposal.

## Infrastructure overview

Bastion hosts the Aspire AppHost, SoFa web process, and Aspire dashboard. No database, queue, separate frontend server, or monitored-repository installation is proposed.

## Deployment mapping

| Software | Placement | Connections |
| --- | --- | --- |
| Web host, Attention, GitHub adapter | One local SoFa process | HTTPS to GitHub; OTLP to Aspire |
| AppHost and dashboard | Bastion local processes | Resource supervision and telemetry |
| Browser | Juan’s session | Loopback; SSH forwarding if browsing from another machine |

Bind the application and dashboard to loopback, retain Aspire dashboard authentication, and avoid public ingress. A remote access requirement would need a separate security decision. AppHost starts the application; unattended service installation and boot supervision are outside this proposal.

This realizes the historical [Bastion constraint](../../product/changes/superseded/chg-repository-attention-proposal/proposed/requirements/constraints/con-bastion.md). Aspire’s [AppHost configuration](https://learn.microsoft.com/en-us/dotnet/aspire/app-host/configuration) and [dashboard](https://learn.microsoft.com/dotnet/aspire/dashboard) document orchestration and telemetry capabilities; they do not prove this repository has configured them.
