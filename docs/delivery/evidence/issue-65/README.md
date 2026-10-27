# Lighthouse adoption acceptance — issue 65

Evidence collected on 2026-10-27 (UTC). Retained text logs normalize trailing whitespace and terminal color escapes; original Actions artifacts remain linked. This record separates deterministic verification, live workflow observations and owner acceptance.

The owner subsequently merged PR 67. See the [post-merge acceptance record](post-merge/README.md) for the fresh native-event exercise; the observations below retain their original pre-merge context.

## Reviewed revisions and authority

- SoFa PR [64](https://github.com/juangcarmona/sofa/pull/64) was independently reviewed at `45e66243219e51395cf85f3ef63429c72233fa2a`, checked locally and against both successful GitHub checks, then merged with the owner's explicit authorization as `ea2599a1d10c6c0850590aaa6f1ee560aa175ca4`. Issue 63 is closed with `status:done`.
- Adoption candidate `5151be389cb38f006f6f8b1ac22e4b51b13eb25d` pins Lighthouse `496562b1deb8b217b231a0d7baad8d10889485b0`, which contains merged Lighthouse [137](https://github.com/juangcarmona/lighthouse/pull/137) and [129](https://github.com/juangcarmona/lighthouse/pull/129).
- PR [67](https://github.com/juangcarmona/sofa/pull/67) is the separate adoption delivery and is not authorized for merge. The npm foundation package remains at its existing release; workflow sources and the execution adapter use the immutable source revision above.
- Existing SoFa and Lighthouse checkouts contained unrelated local changes. Separate worktrees were used. ProductShape, ArcShape, package locks, GitHub policy, lifecycle customizations and application behavior are unchanged.
- The official installer regenerated the existing router and the same five workers. Previous sources matched the old upstream templates. The compiler was gh-aw 0.93.2; generated locks were not edited manually. A second installation produced byte-identical projections: [receipt](installer-idempotence.json), [compiler log](install-workflows.log).

## Deterministic acceptance

| Check | Observed result | Evidence |
| --- | --- | --- |
| Full candidate verification on host | Exit 0; 27 Node, 38 .NET, 4 browser; no skipped tests | [Initial full pass](verify-retry.log), [final full pass](final-verify.log) |
| Container candidate verification through official host adapter | Both configured commands exit 0; 27 Node, 36 .NET, 4 browser | [Checks receipt](checks.json), [npm installation](container-check-1.log), [verification](container-check-2.log) |
| Separate Docker engine suite | Exit 0; two EngineTests | [Host log](host-tests.log) |
| Upstream environment, dispatch and issue contracts | 38 passed, zero skipped, including real PostgreSQL isolation and cleanup after failed/terminated checks | [Contract log](upstream-contracts-real.log) |
| Upstream completion contracts | 31 passed, zero skipped | [Completion log](completion-contracts.log) |
| Official image preflight | All pinned prerequisites pass; temporary service cleanup true | [Preflight receipt](host-preflight.json) |
| Candidate CI at 5151be3 | Both foundation and verify passed | [Foundation](https://github.com/juangcarmona/sofa/actions/runs/40510028528), [full CI](https://github.com/juangcarmona/sofa/actions/runs/40510028672) |

The [first host attempt](verify-initial-failure.log) failed with an internal CLR error, before tests completed. The entire command was repeated with `DOTNET_CLI_USE_MSBUILD_SERVER=0 MSBUILDDISABLENODEREUSE=1`; all checks passed. No test, gate or source change was made to suppress the failure. The container command already disables build servers through merged PR 64.

Container acceptance uses candidate-only Git metadata. It proves snapshot scanning, not prior Git history; the separate full host verification and GitHub CI cover repository history. The real termination test exercises check-container cleanup, not every possible Actions cancellation or forced host-death path.

## Host preparation

The installed images match the retained official [builder receipt](image-receipt.json). The operator configuration received only its `environments` map, preserving the existing read-only image, legacy implementation image, authentication volume, model, UID and locks. Both repository runners were idle before the update. The prior private configuration was saved outside the repository for rollback; no credentials are stored in this record.

The runner provides Linux amd64, Node 24, Docker and `flock`; its [configuration read](runner-environment-visibility.json) confirms the immutable mappings are visible inside the runner container. Its job-start hook serializes both existing gateway-capable runners across the shared port 8080; the engine has its separate lock. Official preflight checked npm, Git, Chromium, OpenCode 2.1.0 and .NET SDK 10.0.100. PostgreSQL 17 used an isolated temporary network and memory-backed test data. Agent and independent validation receive neither the host Docker socket nor a persistent orchestration database.

The host acceptance receipt records `serviceCleanup: true`. No container or network named for `lighthouse-sofa-65-preflight` or `lighthouse-sofa-65-acceptance` remained after execution. The upstream tests separately verified clean databases between phases and cleanup after failure and check termination.

## Live exercise

Fixture [66](https://github.com/juangcarmona/sofa/issues/66) requests one documentation sentence and the required OpenSpec planning artifacts. It explicitly forbids a merge or changes to checks, dependencies, workflow, harness or policy. The fixture is now closed without delivery or merge; the receipt links below retain its outcomes.

The owner-dispatched candidate [router](https://github.com/juangcarmona/sofa/actions/runs/40510013731) completed its separate route and dispatch jobs and started [refinement](https://github.com/juangcarmona/sofa/actions/runs/40510074324). The issue receipt records the workflow link, icon, request identity and worker identity. Refinement completed with evaluation `passed`, a `✅ Completed` receipt and reservation release; see the [request](refine-request.json), [evaluation](refine-envelope.json) and [runtime](refine-runtime.json). An owner `after_run` replay through [router 40510726120](https://github.com/juangcarmona/sofa/actions/runs/40510726120) authenticated that completion and dispatched [implementation 40510812648](https://github.com/juangcarmona/sofa/actions/runs/40510812648) independently. Implementation completed its workflow successfully but published a `blocked` assessment and draft [PR 68](https://github.com/juangcarmona/sofa/pull/68). The candidate contains one sentence and four OpenSpec planning files. Its [runtime receipt](implement-runtime.json) records the legacy image and `environment: null`, as expected for current main. The [independent checks](implement-checks.json) report npm installation and OpenSpec passing, but full verification failing. The issue shows `⏸️ Waiting for your response`, links the draft and run, and [releases its reservation](issue-66-after-implementation.json). A successful workflow did not become an approved implementation; see the [evaluation](implement-envelope.json).

The same authenticated implementation receipt was replayed in [run 40511908359](https://github.com/juangcarmona/sofa/actions/runs/40511908359). Preparation detected the consumed receipt; activation, agent, publication and conclusion were skipped. The [run snapshot](duplicate-receipt-run.json) proves no second admitted agent or PR. The issue retained the original worker identity. This test creates a deliberate workflow attempt, not a second execution of the implementation.

Owner [feedback](https://github.com/juangcarmona/sofa/pull/68#issuecomment-6443031423) asks to spell out the two npm commands in the same sentence. The existing default-branch [router 40511904732](https://github.com/juangcarmona/sofa/actions/runs/40511904732) acknowledged and applied it to the same PR with a non-forced second commit, moving head from `458d3b0a615c8a400df9ae707bdd862ea8d20c28` to `6bba7d8766b68eb7be5a9d4fb70fbaaafc8e48ce`. The [bot acknowledgment](https://github.com/juangcarmona/sofa/pull/68#issuecomment-6443147219) ties the update to the feedback identity. Its [assessment](feedback-envelope.json) remains blocked; the [host checks](feedback-checks.json) and [log](feedback-host-check.log) retain the independent result. The [issue observation](issue-66-after-feedback.json) confirms the reservation was released again. This native event uses main's installed adapter, not the unmerged candidate router; it does not prove the candidate's native feedback dispatch.

The fixture's [full GitHub verification](https://github.com/juangcarmona/sofa/actions/runs/40511764985) passed on the initial head. Its separate PR-event run [40511767726](https://github.com/juangcarmona/sofa/actions/runs/40511767726) reported `action_required`. The feedback head's PR-event run [40512708606](https://github.com/juangcarmona/sofa/actions/runs/40512708606) also reported `action_required`, with no successful check observed at that head. Neither result was substituted for the blocked independent container assessment or treated as owner approval. For the initial candidate, the publisher explicitly dispatched the allowed verification workflow; no forge rule was relaxed.

The candidate integration worker rejected the old pre-feedback head in [run 40512836613](https://github.com/juangcarmona/sofa/actions/runs/40512836613) with `Accepted PR identity or work-item authority changed`, before agent execution. The dispatch receipt changed to `❌ Failed or cancelled`; [run snapshot](integrate-stale-run.json), [failure log](integrate-stale.log). This was an expected freshness-guard rejection with `merge=false`, not an integration assessment or approval.

The current head was then tested with `merge=false` in [run 40513166718](https://github.com/juangcarmona/sofa/actions/runs/40513166718). Preparation rejected the still-draft PR with `PR is not eligible for this stage`; the agent and publisher were skipped: [run snapshot](integrate-draft-run.json), [failure log](integrate-draft.log). Both rejection paths finished before the PR was closed and left no active reservation. They demonstrate admission guards, not a passed integration assessment, a protected report publication or a successful merge.

PR 68 was [closed without merge](fixture-pr-closed.json) at head `6bba7d8766b68eb7be5a9d4fb70fbaaafc8e48ce`. Issue 66 was [closed as not planned](fixture-issue-closed.json), preserving its fixture outcome rather than claiming delivery or `status:done`. Temporary readiness/review labels were removed only after execution ended. The [complete issue comment snapshot](issue-66-comments.json) retains status icons, router/worker links, stage outcomes and the two rejected integration requests. Exactly one implementation PR was created and feedback updated its existing branch. No run-scoped agent, validation or PostgreSQL containers or networks remained; only the two pre-existing runner containers were running.

## Observed blockers

- The live agent reported a Gitleaks version mismatch and stopped after 26 of 27 Node tests, before runtime/browser checks. The independent host rerun passed all 27 Node tests and the repository static checks, then failed at the runtime command because Docker is unavailable in the legacy workload container. These are distinct observations; see the [independent log](live-implementation-host-check.log). Current main has no environment declaration or container-check policy, so the old full-suite command attempts nested Docker. No checks or tools were weakened to obtain a successful assessment.
- Lifecycle labels were not reconciled by this adapter: [before fixture cleanup](issue-66-after-feedback.json), issue 66 retained `status:backlog` alongside `ready` and `waiting-review`; its canonical status never advanced. Receipts and reservation overlays accurately record execution, but the canonical status label is stale. This adoption does not silently normalize unrelated issue labels or redefine the shared lifecycle.

## Acceptance boundaries

Manual router dispatch selects the reviewed workflow branch. Product revision, implementation policy and environment declaration still come from current `main`; these controls were not bypassed. Therefore pre-merge live execution cannot prove activation of this PR's environment or the native default-branch dispatch/feedback flow. Candidate container acceptance above proves the proposed environment independently. Native operation must be verified after owner acceptance and merge.

`reviewRequired: true` intentionally keeps candidate acceptance with the owner. Passing host checks can still yield a blocked assessment and draft PR. A successful Actions run, a successful stage evaluation, human acceptance and permission to merge are separate facts. GitHub's effective main rules require `verify`, resolved review threads, a PR and an up-to-date branch; the protected evidence branch is retained. The [live rules observation](github-rules.json) records both rulesets without changing them.

## Review and follow-up

Two independent read-only audits found no actionable defect in PR 64 or the adoption configuration. They do not decide the owner-only Definition of Done judgements. This PR remains unmerged for owner review; no scope decision, approval or successful end-to-end integration is inferred from these tests.

After owner acceptance and merge, verify a fresh small issue using native events: refinement completion must dispatch the new implementation worker, implementation and feedback runtime/check receipts must select the declared .NET/PostgreSQL profile (read-only refinement keeps its planning image), feedback must use the new independent dispatch path, and CI must run on the current feedback head. Inspect the canonical status-label drift and the PR-event `action_required` result before accepting native delivery. Keep merge authorization separate. These are explicit acceptance gaps, not new workers or redesign proposals.
