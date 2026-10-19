---
title: Deployment view
arc42-section: "07"
description: Infrastructure and environment mapping for the system.
---

# 7. Deployment view

**Approved architecture.** Juan accepted this architecture on 2026-10-19; issue 6 implements the initial slice. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Infrastructure overview

Bastion hosts the Aspire AppHost, SoFa web process, and Aspire dashboard. No database, queue, separate frontend server, or monitored-repository installation is proposed.

## Deployment mapping

| Software | Placement | Connections |
| --- | --- | --- |
| Web host, Attention, GitHub adapter | One local SoFa process | HTTPS to GitHub; OTLP to Aspire |
| AppHost and dashboard | Bastion local processes | Resource supervision and telemetry |
| Browser | Juan’s session | Loopback by default; explicitly configured Bastion LAN interface for Juan’s other machine |

The dashboard stays on loopback with Aspire authentication. At Juan’s explicit request on 2026-10-19, the application can bind to a specific LAN IPv4 address through local AppHost configuration. This trusted-LAN mode permits LAN devices to view findings and refresh without an application login; no public ingress is configured. The explicit host allowlist and antiforgery protection remain in place. AppHost starts the application; unattended service installation and boot supervision are outside this proposal.

This realizes the accepted [Bastion constraint](../product/model/requirements/constraints/con-bastion.md). Aspire’s [AppHost configuration](https://learn.microsoft.com/en-us/dotnet/aspire/app-host/configuration) and [dashboard](https://learn.microsoft.com/dotnet/aspire/dashboard) document orchestration and telemetry capabilities; they do not prove this repository has configured them.

<!-- pdac:cite id="CON-BASTION" digest="sha256:9272e2603ef2cd419ca1389de021c86b4a2f4311fa5312eb4c12c8d7d6d25f58" -->