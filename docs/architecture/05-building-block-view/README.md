---
title: Building block view
arc42-section: "05"
description: The static decomposition of the system into building blocks.
---

# 05. Responsibilities

The observation adapter under `src/observation/` collects GitHub evidence. `src/web/server.mjs` serves fixed assets and a read-only snapshot endpoint with a shared, coalesced 15-second cache. `src/web/public/` renders source timestamps, unavailable evidence, attention labels and recent runs using text nodes and repository-scoped GitHub links. Repository routers and workers execute independently; the retired runtime remains disabled.

<!-- pdac:cite id="BC-EXECUTION" digest="sha256:a12712bab0b780d1a2132400bed82c4cca7c3ceeb187911dde55366d6581821e" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed application blocks

These are logical modules of one application, not new services. They are not present in the current code.

| Block | Responsibility | Key boundary |
| --- | --- | --- |
| Workspace UI | Issue detail, action explanation, progress; later inbox/project/health/ideas | Does not evaluate independent policy or hold tokens |
| Session and GitHub connection | OIDC session, allowed owner, explicit GitHub linking and revalidation | Provider subject differs from source actor; no email-based implicit linking |
| Issue/query adapter | Paginated issues/conversations, evidence and capability snapshots | Cache keyed by authorized source scope; missing data stays missing |
| Command adapter | Validate intent, recheck source, persist receipt and submit explicit mutation | Delegates eligibility/admission to shared/source authority |
| Receipt reconciler | Match exact source identifiers, reconcile uncertain responses and refresh progress | Observes admitted requests; never schedules or automatically resubmits work |
| Application store | Sessions, protected connections, command journal; later idea drafts and observations | No execution leases, worker queue or checkpoints |
| Signal projection | Versioned inputs and calculations, comparable history and coverage | Lighthouse/source semantics; no implicit score policy |

The command store belongs to SoFa availability and recovery only. A repository admitted run never reads it to execute.
