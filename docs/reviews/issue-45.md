# Issue #45 retrospective

The first SoFa observation web app and repository-owned OpenCode workflows were delivered in [PR #46](https://github.com/juangcarmona/sofa/pull/46). The read-only page displays real Lighthouse and SoFa evidence and links to GitHub. The new repository runner executes independently of the web service. The original worktrees and authored assets were preserved.

## Evidence

- [PR verification](https://github.com/juangcarmona/sofa/actions/runs/40287589000) and [merged-main verification](https://github.com/juangcarmona/sofa/actions/runs/40288811583) passed.
- [Owner-requested refinement of issue #47](https://github.com/juangcarmona/sofa/actions/runs/40287921209) completed with OpenCode logs, validated output and reservation cleanup.
- [Desktop](../process/20261026-45-initialize-sofa/screenshots/desktop.png) and [mobile](../process/20261026-45-initialize-sofa/screenshots/mobile.png) captures show real GitHub observations during the refinement.
- [Acceptance metadata](../process/20261026-45-initialize-sofa/evidence.json) binds the commit, checks and known limits. Browser screenshots and accessibility reports are also retained in the CI artifact.

## What went well

Reusing the existing collector and Lighthouse tokens kept the web slice small. Published workflow regeneration produced identical files on a second pass. Independent review caught a keyboard-focus regression that the initial accessibility scan missed; the fix and a dedicated regression test are included. The runner uses its own registration and workspace.

## What went wrong

Initial browser checks found a brand-name accessibility mismatch and light-theme contrast failure. Both were corrected before merge. A local .NET restore terminated with an internal CLR error once; the unchanged restore and the full verification passed on retry. No permanent runtime workaround was added for an unreproduced tool failure. The first merged-main CI attempt spent more than eight minutes downloading Ubuntu browser dependencies and was cancelled; the complete workflow was retried on a fresh runner. This was setup latency, not an application test failure. The adapter publishes its `ready` outcome label but does not map SoFa's canonical `status:*` labels; the executor synchronized `status:ready` after the run completed. Future adopter reconciliation should make this mapping explicit.

## Start doing

Keep background refresh in keyboard regression coverage; the new browser test now enforces this. Preserve source-run correlation as an explicit requirement for future authenticated controls, tracked by [issue #47](https://github.com/juangcarmona/sofa/issues/47). Propose a maintainer-reviewed amendment for the lifecycle document's outdated scaffold and protection statements; this delivery leaves its judgement gates intact.

## Stop doing

Do not treat an accessibility scan as evidence that focus survives dynamic updates. Do not start multiple host-network gh-aw gateways on Bastion: their shared port requires one active runner until capacity handling is redesigned.

## Remaining limits and effort

The app has no mutation API. Authentication, owner-preserving dispatch and request-to-run correlation remain future work. The presentations runner is stopped and preserved; multi-repository unattended capacity is not solved by this initialization. Executor token use, cost and active-work duration were not collected, so no estimates are reported. GitHub run timestamps are retained in the acceptance metadata. The independent technical audit is not a fabricated human review.
