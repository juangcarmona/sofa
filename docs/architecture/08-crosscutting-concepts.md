---
title: Cross-cutting concepts
arc42-section: "08"
description: Concepts and rules that affect multiple building blocks.
---

# 8. Cross-cutting concepts

**Approved design; implementation pending.** Juan accepted this architecture on 2026-10-19. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Evidence and configuration

Configure repository identities and per-repository blocker, owner-review, and release-workflow mappings as explicit data. Validate configuration before collecting; missing mapping produces unconfigured coverage. Do not infer semantics from workflow display names or issue titles. The accepted [evidence rule](../product/model/business-rules/br-evidenced-attention.md) owns classification policy.

<!-- pdac:cite id="BR-EVIDENCED-ATTENTION" digest="sha256:b1faa84dc95658ee7d2fd1cb6ea258028a75e61cd71ccddb54cbd133bd1edf37" -->

Use stable source IDs to deduplicate approval signals. Preserve source timestamps separately from collection timestamps. Escape all repository-provided text in HTML and allow only validated GitHub HTTPS evidence links.

## Credentials and local request protection

Inject an authorized read credential at startup through local secret configuration. Never commit it, render it, or log authorization headers or response bodies. Request only access needed to read the selected repositories. The adapter exposes no write operations, even if the supplied credential permits writes. Access limitations remain visible per category.

Protect refresh with normal ASP.NET Core antiforgery handling and host/origin restrictions. Localhost binding alone does not protect against browser requests from unrelated sites.

## Observability

Instrument each refresh and repository collection with correlated activities, durations, outcome counts, and sanitized errors. Use bounded metric dimensions such as repository, category, and outcome; keep PR IDs and refresh IDs out of metric labels. HTTP bodies, titles, credentials, and headers are excluded from telemetry.

Use standard .NET OpenTelemetry configuration and Aspire’s [service defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults). Telemetry diagnoses collection; the attention snapshot represents product findings. Aspire MCP may inspect operational evidence later, but is not a collection dependency.

This realizes the accepted [collection visibility requirement](../product/model/requirements/quality/qr-collection-visibility.md). No telemetry backend or long-term retention service is added.

<!-- pdac:cite id="QR-COLLECTION-VISIBILITY" digest="sha256:7fab4b62a3d1df9ae9aedf6f30fbedae3e12e5bda23ad56add997cde4aa2b29f" -->