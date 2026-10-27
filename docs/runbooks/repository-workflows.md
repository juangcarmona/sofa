# Repository workflows

SoFa uses Lighthouse's repository workflows. GitHub runs the work; the web app observes it.

## Use the workflow

1. Add `refine` to an issue. If the agent asks a question, answer in the issue.
2. When refinement passes, implementation starts. Add `future` to pause it; remove that label to resume.
3. The agent plans with OpenSpec, writes the code, runs checks and opens a PR containing the plan and implementation.
4. Comment on the PR to request changes. The dedicated `apply-review` worker updates the same PR and branch, retaining the proposal or implementation stage. Approve the PR when it is ready; the merge still depends on CI and repository rules.
5. After merge and CI, the final review records the report and retrospective.

A separate `propose` request is still available for design work. There is no requirement to merge a proposal before implementation. Existing product decisions and security approvals still apply.

The router decides and dispatches; each stage has its own Actions execution. Issue receipts show the last observed phase with an icon and source links. Follow Actions for live status. A successful workflow can publish a blocked assessment; inspect the stage outcome before treating a gate as passed.

`agent-working` means an execution is active. `needs-info` means an answer is needed. `waiting-review` means there is a PR or a decision to review. Follow the run linked by the bot for logs, screenshots and checks.

## Operator setup

Register a repository-specific Linux runner with the `lighthouse-workflow` label. Provide Docker and `LIGHTHOUSE_ENGINE_CONFIG` pointing to the private engine configuration. Keep credentials outside the repository and web app. The implementation image must support the repository's checks, including .NET and the runtime tests.

The current gh-aw gateway uses host port 8080. Only one gateway job may occupy the host at a time. Bastion runner job hooks hold a shared host lock for the entire job; the engine lock alone does not cover gateway startup. Confirm this hook is active on every gateway-capable runner before running them together.

Enable `LIGHTHOUSE_AUTOMATION_ENABLED` after installing and checking the workflows. Keep default token permissions read-only. GitHub's setting allowing Actions to create and approve PRs is required for publication; the bot does not approve its own work.

The workflows pin Lighthouse `57d4e23da2831be5fad0331f1ba82326782943cc` and use gh-aw 0.93.2. Update them through the Lighthouse installer. This includes upstream PRs #129, #137 and #144. The npm package version and workflow pin are separate: CLI 0.11.2 / SDLC 0.13.0 remain installed while publication of CLI 0.11.3 / SDLC 0.14.0 awaits the protected release gate.

The installed workers are `refine`, `propose`, `implement`, `integrate`, `review` and `apply-review`. Optional maintenance is not installed; automatic recovery and aggregated maintenance reports remain disabled. GitHub Actions executes work and issues retain receipts and reservations. The [adoption evidence](../delivery/evidence/issue-72/README.md) records validation and the outstanding acceptance gaps.

## Reproducible application checks

`.lighthouse/environment.json` selects the official `dotnet` profile with disposable PostgreSQL 17. `.lighthouse/implement.json` installs locked npm dependencies and runs `verify:container` with CI browser settings. Candidate acceptance remains subject to owner review. The full `verify` CI suite and `test:runtime:host` retain the two Docker engine tests; container verification alone is insufficient for integration.

The operator configuration must map `environments.dotnet.image` and `environments.dotnet.postgresImage` to installed immutable image IDs from the official builder receipt. Keep the existing read-only image, legacy implementation image, provider volume, UID, model and lock. Use Lighthouse's pinned `src/examples/opencode-engine/build-environments.mjs` and its `ENVIRONMENTS.md` procedure when rebuilding. Never pass a host Docker socket into an agent or validation container.

Preflight verifies Node 24, npm, Git, Chromium, OpenCode 2.1.0 and .NET SDK 10.0.100. The host creates separate temporary databases and networks for agent work and independent checks, waits for PostgreSQL readiness and removes them afterward. PostgreSQL is test infrastructure, not an orchestration database. After cancellation, confirm both workload and database containers and their networks disappeared before considering cleanup complete.

To regenerate the existing workflow set from a verified Lighthouse checkout, first preview and review differences, then apply:

```sh
node /path/to/lighthouse/src/packages/sdlc/src/install-workflows.mjs . 57d4e23da2831be5fad0331f1ba82326782943cc --with-completion --replace-reviewed
node /path/to/lighthouse/src/packages/sdlc/src/install-workflows.mjs . 57d4e23da2831be5fad0331f1ba82326782943cc --with-completion --replace-reviewed --apply
```

Commit generated locks with their sources. The host authenticates product revision separately from workflow revision: a manual router dispatch can test this PR's workflow branch, while implementation policy and environment still come from the current default branch. Such a pre-merge run cannot prove activation of an unmerged environment declaration. Use isolated consumer acceptance for the candidate and retain this live-acceptance limitation until post-merge verification.

## Recovery

Set `LIGHTHOUSE_AUTOMATION_ENABLED=false` to pause new work. Inspect the active run before stopping a runner or removing `agent-working`.

Manual `integrate` and `review` dispatches remain available for recovery. Ordinary operation uses PR feedback, approval and CI events.

The [web view](../../README.md#open-the-factory-view) is still read-only. Closing it does not affect running workflows.
