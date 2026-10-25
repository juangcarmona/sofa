# Presentations end-to-end pilot

Historical runtime plan. [ADR-0006](../architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md) supersedes SoFa execution ownership; consult the current architecture before using this plan.

Juan selected `juangcarmona/presentations` on 2026-10-24. This document tracks the pilot acceptance boundary and links its recorded evidence. The first milestone is a story reaching triage/refine; the full pilot adds a recoverable implementation, a PR and authorized integration.

## Verified preparation

The consumer adopts published Lighthouse CLI `0.9.1` and SDLC `0.9.0` at source/workflow revision `f92952632d8c4852ae984e147bec82076bd2169e`. [PR #3](https://github.com/juangcarmona/presentations/pull/3) preserved and published the presentation improvements; [PR #5](https://github.com/juangcarmona/presentations/pull/5) established the pilot, followed by workflow and evidence-scope corrections. Typecheck and the full build, including both decks, passed. The original working directories retain their ongoing work.

Bastion runs a dedicated repository runner, a persistent SoFa supervisor with PostgreSQL, and an isolated OpenCode 2.1.0 engine using the authorized OpenAI subscription. A real provider request passed. Host policy admits only `triage` and `refine`, one active execution and at most three attempts per issue/stage/source, with a ten-minute execution limit. This is the supervised rollout configuration, not a general factory sizing limit.

`lighthouse verify` reports no blocking failures or managed-file drift. The pilot explicitly records unconfigured ProductShape/ArcShape authorities, labels and evidence integration; these remain adoption work. The selected issue supplies product intent for this small additive change. Stage output is published as a validated issue comment; it does not install authorities, change labels or imply an owner gate decision.

## Selected story

[Presentations #6](https://github.com/juangcarmona/presentations/issues/6) adds a keyboard skip link to the shared catalogue layout. The first Tab exposes “Skip to content”; activation moves focus to main content on catalogue and talk-detail pages. Acceptance covers both themes and narrow viewports, preserving existing talks, deck sources and design. Typecheck, the full deck/site build and browser evidence belong to implementation acceptance.

## Milestone A: issue to refined story

1. An authorized issue event enters the consumer's Lighthouse router. It resolves one route and one work item; a duplicate event produces no second execution.
2. The repository runner invokes the local SoFa bridge. SoFa validates identity, package compatibility, source revision, policy and capacity before allocating a run, lease and workspace.
3. The engine executes the Lighthouse triage role. The trusted publisher validates and records the structured result and allowed GitHub effect. Missing information produces a specific question and a waiting state.
4. A relevant authorized reply resumes that waiting run within its correction bound. An unrelated or bot comment does not restart it.
5. Lighthouse records any required `triaged` decision before starting refine. Refine writes a candidate story with testable acceptance and scope; the publisher applies it to the intended GitHub issue and records its receipt.
6. The run waits at `ready` when owner approval is required. Report links to the issue, workflow run, SoFa attempt, result and source revision. Do not call waiting a failure or approval.

Pass requires an actually published story or actionable clarification, not just a JSON artifact. The engine uses the intended service account and provider; a mocked run does not count.

### Observed milestone A result — 2026-10-24

[Triage Actions run 40118570862](https://github.com/juangcarmona/presentations/actions/runs/40118570862) completed with a passing `triaged` report and [published receipt](https://github.com/juangcarmona/presentations/issues/6#issuecomment-6387645389). The owner authorized refinement against that result. The first refinement published a blocked report because host-verified approval evidence was absent from engine context; [SoFa #20](https://github.com/juangcarmona/sofa/pull/20) corrected that gap after 35 runtime tests, repository CI and an independent audit passed.

[Refinement Actions run 40120599045](https://github.com/juangcarmona/presentations/actions/runs/40120599045) then passed and [published the refined story](https://github.com/juangcarmona/presentations/issues/6#issuecomment-6387924060). The result has no missing evidence, unavailable checks or pending review for refinement. Its operational state is `waiting-human` at `ready`. The story defines six acceptance criteria, scope, risks, dependencies and later implementation checks; it does not claim those later checks have run.

| Evidence | Value |
| --- | --- |
| Triage attempt | `8b6c16d7862efa8592ea174db11396846a1f993a3ce65ca1525221ac73aed222` |
| Triage result SHA-256 | `2023761bf71e9a16b36ec1774663bbaf044ce90b27111ead49d1e09c1d1188b2` |
| Refinement authorization | [Owner comment 6387898735](https://github.com/juangcarmona/presentations/issues/6#issuecomment-6387898735) |
| Refinement attempt | `797feb06169c10945b0fc47d79834e46a46bce263a3d1311e21d8fff5bea177d` |
| Refinement result SHA-256 | `c3ea295a92b8b75d7aab31830cf39de754a28b59d8d9867e5abbf3f9be45a619` |
| Runtime source | SoFa `04024c1f5127a16ebedc1fffeb626b97c55c87bc` |
| Runtime assembly SHA-256 | `209cf90a9102c10d7ce0f27d329e19fa2c69f361bb3afea9e41c5904cdb2500b` |

The operator compared the remote comment with the persisted publication intent, recomputed the result digest and matched the Actions artifact to the PostgreSQL request/result/receipt. The source revision remained unchanged; the engine container and temporary workspace were removed and the supervisor returned healthy. PostgreSQL retains the outcomes, including the blocked attempt; Actions artifacts expire after 14 days.

Milestone A is demonstrated through real GitHub, runner, SoFa and OpenCode execution. The human `ready` decision remains pending. Only triage/refine are currently implemented and enabled; milestones B/C require proposal, implementation and integration adapters, their validation and explicit gate decisions. No product implementation PR or accepted delivery is claimed.

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

## Revised acceptance: two entries and policy-bound continuation

The accepted direction is recorded in [ADR-0005](../architecture/09-architecture-decisions/0005-separate-entry-and-autonomy.md). Historical milestone A proves the supervised triage/refine slice only. Its manual predecessor approval and `ready` wait are historical pilot behavior, not mandatory stops for every future engineer instruction.

[Presentations PR #9](https://github.com/juangcarmona/presentations/pull/9) contains a manually authored plan. It does not prove that SoFa executed OpenSpec proposal, implementation or publication. Preserve it as existing work; do not merge or overwrite it as an implementation outcome.

The next acceptance run must distinguish these cases:

| Input or condition | Required observation |
| --- | --- |
| Owner requests refine with verified engineering capability | Refinement starts without a triage run or copied run hash. |
| Admitted requester opens an issue | Triage evaluates the request under the configured intake policy; no engineering or merge privilege is inferred. |
| Engineer requests implement | The harness checks definition and risk, then produces real OpenSpec artifacts before code application; incomplete definition asks for the missing decision. |
| Refine needs an answer | Only a permitted answer to that pending question resumes it; the original authority and limits remain unchanged. |
| Refine is complete or splits work | Validated effects update the issue and configured labels; dependencies prevent premature child execution. |
| CI fails on the implementation PR | A bounded correction updates the same PR, rechecks CI and retains evidence for each attempt. |
| Duplicate, revoked authority, source drift or stale worker | No duplicate run or effect and no publication under stale authority. |
| Implementation is verified | Integration remains subject to its separately configured authority; unattended execution does not imply merge permission. |

Run E1 first with the owner's verified capability and the published compatible contract. Exercise requester roles in isolated adapter fixtures before enabling another real principal. E3 acceptance requires actual OpenSpec files, source changes, PR, checks and source-bound evidence from the runtime; manually preparing those artifacts in this chat does not count. Keep global pilot capacity one until shared admission and recovery are demonstrated.
