---
id: BR-WORK-AUTHORIZATION
type: business-rule
title: Enforce bounded authority for entry and continuation
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-EXECUTION, BC-SCHEDULING, BC-DELIVERY]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN, TERM-PROJECT]
---

## Rule

SoFa MUST verify the requesting actor and effective repository capabilities through a trusted boundary before dispatching privileged work. It MUST distinguish admitted requester intake from authorized engineer entry without requiring a GitHub organization. Juan retains factory policy and revocation authority. A requester may submit and answer questions; an engineer may directly request refinement, implementation or planning within granted capabilities.

SoFa MUST consume Lighthouse routing and stage contracts and retain the requester, authorizer, repository, work item, scope, source and policy revisions, permitted workflows and effects, limits, autonomy and separate integration authority. It MUST recheck applicable authority before dispatch and publication and reject revoked or stale grants. The engine and issue content cannot manufacture authorization.

Requester work MAY proceed automatically after triage under a previously authorized bounded intake policy. Engineers MAY skip requester triage while retaining the selected workflow's readiness, risk and evidence requirements. Planning-only authorization MUST NOT permit code application. Supervised or unattended progression MUST be a per-run authorization choice bounded by repository policy; it MUST NOT be inferred from job title or actor identity alone.

Trusted automation MAY continue within the same effective grant. A question response MUST bind the pending question and a permitted respondent; it supplies information without granting execution or merge permission. Work outside a grant, required human decisions, exhausted budgets and uncertain authorization MUST stop with a named next action. The existing unauthenticated LAN surface remains read-only; any future authenticated control surface MUST use the same server-side authorization checks.

## Rationale

The factory needs both a request channel and an engineering control channel while preserving its personal ownership and recoverable execution boundary.

## Examples

Juan requests direct refinement and later authorizes a bounded unattended implementation that stops before merge. An admitted requester answers a scope question and the existing authorized refinement resumes. A configured low-risk intake may continue without a new human acknowledgement at every stage.

## Exceptions

No exception permits a label, a model response or a bot identity to grant new authority. Security-critical and destructive decisions retain the existing human boundaries.
