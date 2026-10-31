---
name: source-correlated-issue-commands
description: Proposed authenticated issue commands with durable receipts and source-owned admission.
status: Proposed
date: 2026-10-26
deciders: [Juan García Carmona]
tags: [commands, github, correlation, recovery]
---

# ADR-0008: Source-correlated issue commands

## Context

The owner requires issue context, Refine/Propose, safe comments and visible outcomes from SoFa. The [Product Change](../../product/changes/active/chg-authenticated-operations-001/change.md) remains draft. Existing ADR-0006 keeps execution in repository workflows. The current read-only Node server and empty Aspire AppHost do not implement this boundary.

Issue #47 has a passed refinement and a blocked proposal report; its scope and evidence are preserved. The installed router has no client request identifier and owner comments on needs-info may resume refinement. These are gaps to close in shared contracts, not reasons to create a second SoFa policy.

## Decision proposed

Keep one web application with a same-origin server boundary, reusing the observation adapter. Add standard OIDC sessions and a separately linked GitHub App user authorization; verify actual actor and permissions server-side and again in the repository. No installation bot impersonation and no browser-held GitHub tokens.

Persist command intent before sending, with owner/source actor, repository/issue, action, payload digest, expected source/evidence version, shared contract version, idempotency key and timestamps. Store source acknowledgment, run ID/attempt or comment ID and reconciliation evidence separately. Use transactional uniqueness for an existing idempotency key: same payload returns its existing receipt; a changed payload conflicts. Also prevent concurrent equivalent requests for the same source work while a prior submission is unresolved. Source-side concurrency and deduplication remain authoritative across callers and devices.

Propose SQLite on local persistent disk for the first single-instance application's journal and sessions, isolated from the retired execution store. This is a small transactional application store, not a workflow queue. Protect source credentials separately and encrypt sensitive connection data with a key outside the database. If deployment requires multiple writers or remote shared storage, revisit the choice before implementation; no network-filesystem SQLite deployment is proposed. Back up through a supported consistent mechanism and reconcile restored receipts before sending anything.

Use the GitHub workflow-dispatch response's exact run identifier when available. Current official REST documentation describes HTTP 200 with workflow_run_id and URLs; compatibility depends on the selected API version. This solves a received response, not a response lost after admission. The shared Lighthouse contract must carry a stable request identifier into source evidence and reject/reconcile duplicate intent. Do not infer correspondence from the newest issue/stage/actor run, labels or time proximity. Unsupported source capability disables the dependent action with an explanation.

The journal states describe transmission: recorded, submitting, acknowledged, rejected, or uncertain. They are separate from source queued/running/completed status and from stage outcome. On timeout or crash around sending, reconcile by source request/run identity before any new send. A source workflow existence proves dispatch, not stage admission or a produced artifact. An uncertain result with no conclusive receipt stays uncertain; offer refresh and source inspection, never a silent retry.

Ordinary comments require a shared explicit no-continuation intent recognized by the repository. Reply-and-continue binds the pending question and existing grant and is offered only when supported. Both need source comment identity and uncertain-post reconciliation. Do not rewrite labels to work around router behavior. Preserve task-digest invalidation and warn the owner when a comment can invalidate active publication.

Refresh source state and capability decisions before mutations; the source rechecks at admission and publication. Start with paginated reads and bounded polling while the UI is visible. The application's reconciler only reads source state; it cannot schedule or auto-resume work. Stop/restart SoFa without stopping admitted repository execution.

## Alternatives considered

- Labels alone or the latest matching run: insufficient for authority, duplicate prevention and exact correlation.
- A local-only idempotency table: insufficient after a lost remote response or for other callers; needs source reconciliation.
- A source receipt alone: valuable on success but insufficient if the response is lost before persistence.
- Reusing the legacy SoFa supervisor/store: contradicts ADR-0006 and adds execution ownership to an interaction requirement.
- A second standalone API/service or frontend rewrite: no first-delivery need; keep logical modules inside the existing application.
- PostgreSQL for application receipts: viable, but not required by the proposed single-instance workload; Keycloak's production database is a different responsibility.

## Consequences

A durable journal adds migrations, protected storage and recovery obligations, but makes mobile reconnects and uncertain writes explainable. SoFa can remain unavailable while work runs. Lighthouse/source support is a real dependency; UI code cannot approximate it. The GitHub user-token path, current API response and shared contract adoption must be demonstrated against a real repository before delivery is claimed.

## References

- [Shared command contract backlog](https://github.com/juangcarmona/lighthouse/issues/119)

- [Refinement evidence](https://github.com/juangcarmona/sofa/issues/47#issuecomment-6410966096)
- [Blocked proposal evidence](https://github.com/juangcarmona/sofa/issues/47#issuecomment-6412062941)
- [Current workflow dispatch API](https://docs.github.com/en/rest/actions/workflows#create-a-workflow-dispatch-event)
- [GitHub user authorization](https://docs.github.com/en/apps/creating-github-apps/authenticating-with-a-github-app/authenticating-with-a-github-app-on-behalf-of-a-user)
- [SQLite atomic transactions](https://www.sqlite.org/atomiccommit.html)
- [Identity and environment proposal](0007-shared-identity-custody-and-recovery.md)
