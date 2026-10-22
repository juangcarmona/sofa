---
id: "CHG-REPOSITORY-ATTENTION-PROPOSAL"
type: "product-change"
title: "Identify repository work requiring Juan’s attention (historical proposal)"
status: "superseded"
base-revision: "23223e5ded1e124b9133f5087adac47ac967745e"
operations: {"add": ["ACT-JUAN", "ACT-GITHUB", "BC-REPOSITORY-ATTENTION", "TERM-ATTENTION-ITEM", "BR-OBSERVE-ONLY", "BR-EVIDENCED-ATTENTION", "UC-REFRESH-ATTENTION", "UC-REVIEW-ATTENTION", "JRN-REPOSITORY-ATTENTION", "CON-BASTION", "FR-ATTENTION-COLLECTION", "FR-ATTENTION-REVIEW", "QR-COLLECTION-VISIBILITY"], "modify": [], "remove": []}
---

## Problem

Juan must visit repositories and their CI results separately to discover failed checks, PRs awaiting his approval, blockers, and release failures. SoFa has no accepted product definition yet.

## Intended Product Outcome

Juan can refresh a small attention list for ArcShape, ProductShape, DomusMind, Raccord, and SoFa on Bastion, understand its evidence and collection gaps, and follow source links to act. This is a personal monitoring slice of his software factory.

## Rationale

The long-term direction is to launch and supervise work using existing tools and each repository’s SDLC, retaining Juan’s approval of product decisions and merges. This proposal only establishes observation and review.

Juan requested quick value without hundreds of hours of platform development, then clarified that the starting point should support disciplined growth rather than commit to disconnected scripts. The product therefore defines one bounded context, Repository Attention, and operational visibility through Aspire; it does not prescribe a deployment topology or UI.

The screenshots supplied during exploration are inspiration, including illustrative metrics. They do not establish implemented behavior or mandatory dashboards. Project Health source is unavailable and is not a dependency. Raccord’s modular-monolith and deferred-events decisions are architectural references, not accepted SoFa decisions.

### Consolidation note

This draft is retained as a historical proposal for the Repository Attention artifacts. Repository Attention is explicitly excluded from `CHG-INITIAL` and is not part of the current SoFa product candidate. This historical proposal remains draft and must not be approved or applied independently.

## Affected Product Areas

One primary user, one external evidence source, one refresh-and-review journey, and one bounded context. Rules preserve current evidence, source visibility, and human authority. The first slice includes no standards scoring or execution engine.

## Open Questions

No unresolved product question remains for this draft. Juan confirmed that his own open, non-draft PRs must appear as awaiting his approval without an explicit review request. This answer is incorporated in BR-EVIDENCED-ATTENTION and FR-ATTENTION-REVIEW.

The architecture proposal addresses the following design choices: choose the smallest interaction surface (terminal, report, or minimal endpoint), the application/module structure, and how each repository’s release workflows and blocker/review conventions are mapped. Scheduled collection, retention history, and freshness alerts are future scope; this slice refreshes on demand.

## Product Acceptance

- The portfolio is exactly the initial five repositories; no team administration or commercial offering is implied.
- Juan can refresh, understand findings and collection gaps, and follow evidence links.
- Old PR revisions, failed superseded attempts, unavailable data, and missing mappings cannot produce a misleading all-clear or current failure.
- SoFa runs locally with Aspire operational visibility; no rich UI is required by the definition.
- No source mutations, agent execution, product approvals, or merges occur through this slice.
- Every proposed artifact remains draft until human approval; structural validation is not approval.

## Out of Scope

Implementation, accepted architecture decisions, UI construction, event routing, autonomous agents, repository installers, automatic repairs or merges, multi-user identity, company onboarding, historical analytics, scheduled collection, notifications, standards scorecards, agent costs, and delivery/security composite metrics. Future capabilities require subsequent product changes.
