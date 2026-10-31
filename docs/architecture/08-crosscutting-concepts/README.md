---
title: Cross-cutting concepts
arc42-section: "08"
description: Concepts and rules that affect multiple building blocks.
---

# 08. Evidence, authority and failures

Observations retain source identity, revision, time and collection scope. Missing evidence is unknown, never healthy or delivered. Repository policies own authorization and gates. A future SoFa control records the user action in GitHub and does not grant additional permission. Runtime progress metadata excludes secrets and transcripts. A source failure affects the verification claim, not an independent execution.

<!-- pdac:cite id="BR-WORK-AUTHORIZATION" digest="sha256:157f762cba7de8c5e40b42a8dfaa83c58e09032ddbebba67065a439ad1e487ce" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed state and authority model

| Fact | Example | Authority |
| --- | --- | --- |
| Issue state | Open/closed | GitHub issue |
| Lifecycle stage and result | Refine passed; Propose blocked | Lighthouse source result |
| Requested command | Propose requested | Attributed user intent and receipt |
| Source execution | Queued, running, completed/failed | GitHub run and attempt |
| Active work/reservation | Reserved by a run, released or uncertain | Repository/runtime source |
| Human wait | Information, review, approval | Source request and applicable policy |

These facts coexist and are not flattened into one progress label. A successfully completed workflow can still carry a blocked stage result. A waiting-review label does not prove an active reservation or accepted proposal.

## Proposed identity and session handling

Use OIDC authorization-code flow with PKCE, issuer/audience/state/nonce checks through a standard library, server-managed sessions, Secure/HttpOnly cookies and CSRF protection on mutations. Match the permitted owner using stable issuer/subject, not email alone. GitHub linking is a separate authenticated ceremony, with an actor-preserving user authorization. Scope credentials to configured repositories and required operations. Expired/revoked source authority disables dependent writes; logout invalidates the application session. Production timeout/assurance policy is an explicit delivery decision, not an invented product constant.

## Proposed observation semantics

Store source identity/revision, collected time, source time when available, pagination coverage and errors independently. Availability, completeness, freshness and applicability are separate dimensions. Cached issue context can remain readable with a stale marker; it cannot authorize a write. Rate limiting uses bounded backoff and exposes retry state. Start with pull-based refresh while visible and explicit refresh; webhooks are not a first-delivery requirement. Restrict artifact size/type, sanitize rendered Markdown and never treat source content as instructions or executable HTML.

## Proposed durability and secrets

[ADR-0008](../09-architecture-decisions/0008-source-correlated-issue-commands.md) owns request persistence, duplicates and uncertain responses. Application data can be backed up; no restored receipt automatically resubmits work. Tokens are protected server-side and excluded from logs/evidence. Production secret and recovery procedures belong to infrastructure under ADR-0007. A journal is not an execution queue.

## Proposed health evidence

A signal definition identifies purpose, source adapter, calculation/version, unit, window, coverage, applicability and baseline. The detail view exposes contributing records. Candidate signals are source workflow duration/outcome for investigation; proposal/PR/check state for delivery decisions; installed-versus-selected baseline for maintenance; measured duration/tokens/storage for resource decisions. Missing tokens do not imply zero consumption, a run duration is not user attention time and a merged PR is not a production deployment. Additional metrics require a source definition before UI claims.
