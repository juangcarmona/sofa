# Presentations end-to-end pilot

Juan selected `juangcarmona/presentations` on 2026-10-24. This is the acceptance plan for future execution, not a report of a completed run. The first milestone is a story reaching triage/refine; the full pilot adds a recoverable implementation, a PR and authorized integration.

## Observed preparation state

Local checkout HEAD is `a4eaa4a`. Lighthouse `0.9.0` and SDLC `0.8.0` are installed; `lighthouse verify --format json` reports six passed checks, no blocking problems and three unconfigured findings (authorities, labels and evidence). The Lighthouse files and current application work are uncommitted. The root package has build/typecheck scripts but no `verify`. No authenticated runtime execution or `presentations` runner readiness was tested in this reconciliation.

Before running, preserve the current work and establish a committed pilot revision. Define the repository verification command, reconcile ProductShape/ArcShape requirements or explicit exceptions, adopt labels through Lighthouse's plan/apply route, and provision or verify the repository's own Bastion runner. Verify the repaired Lighthouse package in a clean consumer before upgrading. A locally installed package or passing foundation check does not satisfy these prerequisites.

## Story selection

Choose one existing or newly refined, low-risk presentation-site issue with observable acceptance criteria and no new infrastructure, secrets, authentication or production-data access. The exact story is selected at pickup; this plan does not invent a product feature. Keep the current talks and editable deck sources inside their repository conventions.

## Milestone A: issue to refined story

1. An authorized issue event enters the consumer's Lighthouse router. It resolves one route and one work item; a duplicate event produces no second execution.
2. The repository runner invokes the local SoFa bridge. SoFa validates identity, package compatibility, source revision, policy and capacity before allocating a run, lease and workspace.
3. The engine executes the Lighthouse triage role. The trusted publisher validates and records the structured result and allowed GitHub effect. Missing information produces a specific question and a waiting state.
4. A relevant authorized reply resumes that waiting run within its correction bound. An unrelated or bot comment does not restart it.
5. Lighthouse records any required `triaged` decision before starting refine. Refine writes a candidate story with testable acceptance and scope; the publisher applies it to the intended GitHub issue and records its receipt.
6. The run waits at `ready` when owner approval is required. Report links to the issue, workflow run, SoFa attempt, result and source revision. Do not call waiting a failure or approval.

Pass requires an actually published story or actionable clarification, not just a JSON artifact. The engine uses the intended service account and provider; a mocked run does not count.

## Milestone B: proposal to verified PR

After explicit gate decisions, Lighthouse proposes the change and records `planned`. SoFa admits implementation with a fresh attempt, approved scope and current revision. The engine changes its isolated workspace; the publisher opens one branch/PR only after validation and scoped checks. Record independent review and all required CI evidence. `integrate` retains the published `done` gate and does not create merge authority; `review` has no gate.

Include these fault injections before claiming recoverability:

| Case | Required observation |
| --- | --- |
| Duplicate event or bridge retry | Same idempotency identity returns the existing attempt; no duplicate PR or issue edit. |
| Forced worker termination after checkpoint | Durable interrupted state; a replacement uses a new fencing token and verified workspace/artifacts, or stops with a precise reason. |
| Stale worker returns late | Operational writes and every GitHub publication are rejected; the stale process has no independent write credential. |
| Publication times out after possible success | Reconcile remote state and record the receipt before retry; no duplicate remote object. |
| Blocked/failed stage or missing evidence | No dependent stage starts even if the job uploaded a well-formed result. |
| Cancellation, deadline or provider failure | Explicit outcome, stopped process tree, released or expired lease, cleaned or quarantined workspace. |
| Source or policy changes after approval | Revalidation or a fresh decision; no reuse of stale approval. |
| Concurrent request from another registration | Pilot global capacity remains one; the other request is queued or refused with an observable reason. |

## Milestone C: accepted delivery

Juan approves integration according to the pilot policy. The publisher checks the current PR head and required evidence, merges only with that authority and records the resulting default-branch commit. Verify the relevant post-merge checks and application behavior at that revision. Retain result/effect digests and durable evidence references before temporary Actions artifacts expire.

The acceptance report separates factory liveness, engine completion, stage evidence, GitHub checks, human decisions and delivered behavior. Production deployment, unattended merge, a central webhook, untrusted forks and multi-host scaling are outside this pilot. Failure or a pending owner decision must remain visible.
