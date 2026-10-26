# Repository workflows

SoFa uses the Lighthouse SDLC adapter pinned to `63c64b779ef18959a9c993902a5e5e1225a16947`, including owner replies and proposal PR feedback from Lighthouse #121–#123. The npm dependency remains 0.13.0 until the next package release; the router and five workers execute the reviewed source pin. Regenerate them through its workflow installer and gh-aw 0.93.2; do not edit generated locks. The previous Copilot adapter and its obsolete triage worker have been removed.

## Operator setup

Register a repository-specific Linux runner with the `lighthouse-workflow` label. Keep its credentials and work directory separate from other repositories. Provide Docker access and `LIGHTHOUSE_ENGINE_CONFIG` pointing to the operator's private engine configuration and authorized OpenCode profile. These credentials do not belong in the repository or the web app.

The current gh-aw gateway uses host port 8080. On a shared host, run only one gateway-capable runner at a time: confirm the previous runner is idle before stopping it, then start the intended repository's runner. The engine lock alone does not serialize gateway startup. Preserve stopped runner configuration for rollback. An unattended multi-repository queue still needs a shared capacity solution.

After merging the generated workflows, set the repository Actions variable `LIGHTHOUSE_AUTOMATION_ENABLED=true`. Keep default token permissions read-only. Proposal and implementation publishing require GitHub's grouped setting allowing Actions to create and approve pull requests; Lighthouse publishers create PRs but do not approve reviews. The exception is recorded in `.lighthouse/github.json`.

## Request a stage

As the repository owner, apply `refine` to an open issue or dispatch **Lighthouse SDLC router** with its issue number and `stage=refine`. The router verifies the actor before admitting work. The worker reserves the issue with `agent-working`, loads the pinned harness, runs OpenCode, validates the result, publishes it and releases the reservation. Follow **OpenCode progress and result**, the job summary and the run artifacts for progress and evidence.

If refinement or proposal is blocked awaiting your decision, answer in a new owner comment. The router acknowledges receipt and resumes that same stage after checking its pending report, current source and prior intent. A blocked proposal may carry `waiting-review`; it does not require another stage label. When a proposal PR exists, review it there. Owner comments, inline feedback and submitted change requests update that same PR. Approval is acknowledged for the reviewed commit; it does not merge or start implementation. The bot links the execution and reports completion or a blocker. Implementation PR reviews still use the completion workflow. A successful refinement does not authorize later stages. Request `propose` separately. Implementation also needs the repository's reviewed implementation policy and approved proposal; this initialization does not invent either. Integration and review dispatches require the PR number and exact accepted head, with an explicit merge authorization for integration.

Lifecycle `status:*` labels remain separate from stage requests and operational flags. Preserve one canonical lifecycle label. A reservation describes active agent work; waiting for information or review is not a reservation.

The lifecycle document still contains historical statements that the repository has no application behavior and cannot configure branch protection. The executable checks now include the web and retained runtime, and the live `main` ruleset requires `verify`. Those factual notes need a maintainer-reviewed lifecycle amendment; this initialization does not change its judgement gates.

## Observe and recover

The [web view](../../README.md#open-the-factory-view) reads GitHub and links to the source issue or run. Stopping it does not affect workflows. It cannot dispatch stages or approve changes.

To pause new admissions, set `LIGHTHOUSE_AUTOMATION_ENABLED=false`. Inspect running jobs before stopping a runner. After a cancellation or failure, check the publisher cleanup and the issue reservation before retrying; do not remove a reservation while its run is still active. Restore the previous idle runner only after the current gateway has stopped.
