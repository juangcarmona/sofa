---
title: Cross-cutting concepts
arc42-section: "08"
description: Concepts and rules that affect multiple building blocks.
---

# 8. Cross-cutting concepts

**Proposal only.** No application implementation or accepted architecture decision exists yet. Current product intent is the applied [CHG-INITIAL](../../product/changes/completed/chg-initial/change.md). The Attention rules below are historical proposal material.

## Evidence and configuration

Configure repository identities and per-repository blocker, owner-review, and release-workflow mappings as explicit data. Validate configuration before collecting; missing mapping produces unconfigured coverage. Do not infer semantics from workflow display names or issue titles. The historical proposed [evidence rule](../../product/changes/superseded/chg-repository-attention-proposal/proposed/business-rules/br-evidenced-attention.md) owns classification policy for Attention.

Use stable source IDs to deduplicate approval signals. Preserve source timestamps separately from collection timestamps. Escape all repository-provided text in HTML and allow only validated GitHub HTTPS evidence links.

## Credentials and local request protection

Inject an authorized read credential at startup through local secret configuration. Never commit it, render it, or log authorization headers or response bodies. Request only access needed to read the selected repositories. The adapter exposes no write operations, even if the supplied credential permits writes. Access limitations remain visible per category.

Protect refresh with normal ASP.NET Core antiforgery handling and host/origin restrictions. Localhost binding alone does not protect against browser requests from unrelated sites.

## Observability

Instrument each refresh and repository collection with correlated activities, durations, outcome counts, and sanitized errors. Use bounded metric dimensions such as repository, category, and outcome; keep PR IDs and refresh IDs out of metric labels. HTTP bodies, titles, credentials, and headers are excluded from telemetry.

Use standard .NET OpenTelemetry configuration and Aspire’s [service defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults). Telemetry diagnoses collection; the attention snapshot represents product findings. Aspire MCP may inspect operational evidence later, but is not a collection dependency.

This realizes the historical [collection visibility requirement](../../product/changes/superseded/chg-repository-attention-proposal/proposed/requirements/quality/qr-collection-visibility.md). No telemetry backend or long-term retention service is added.
