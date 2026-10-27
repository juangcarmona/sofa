# Repository workflows

SoFa uses Lighthouse's repository workflows. GitHub runs the work; the web app observes it.

## Use the workflow

1. Add `refine` to an issue. If the agent asks a question, answer in the issue.
2. When refinement passes, implementation starts. Add `future` to pause it; remove that label to resume.
3. The agent plans with OpenSpec, writes the code, runs checks and opens a PR containing the plan and implementation.
4. Comment on the PR to request changes. The agent updates the same branch. Approve the PR when it is ready; the merge still depends on CI and repository rules.
5. After merge and CI, the final review records the report and retrospective.

A separate `propose` request is still available for design work. There is no requirement to merge a proposal before implementation. Existing product decisions and security approvals still apply.

`agent-working` means an execution is active. `needs-info` means an answer is needed. `waiting-review` means there is a PR or a decision to review. Follow the run linked by the bot for logs, screenshots and checks.

## Operator setup

Register a repository-specific Linux runner with the `lighthouse-workflow` label. Provide Docker and `LIGHTHOUSE_ENGINE_CONFIG` pointing to the private engine configuration. Keep credentials outside the repository and web app. The implementation image must support the repository's checks, including .NET and the runtime tests.

The current gh-aw gateway uses host port 8080. On Bastion, keep only one gateway-capable runner active at a time. Confirm it is idle before switching repositories; the engine lock does not cover gateway startup.

Enable `LIGHTHOUSE_AUTOMATION_ENABLED` after installing and checking the workflows. Keep default token permissions read-only. GitHub's setting allowing Actions to create and approve PRs is required for publication; the bot does not approve its own work.

The workflows pin Lighthouse `a0a946cb74b8c2b5ac2478a9fab689df652d3b9f` and use gh-aw 0.93.2. Update them through the Lighthouse installer. The npm package version and workflow pin are separate until the next release.

## Recovery

Set `LIGHTHOUSE_AUTOMATION_ENABLED=false` to pause new work. Inspect the active run before stopping a runner or removing `agent-working`.

Manual `integrate` and `review` dispatches remain available for recovery. Ordinary operation uses PR feedback, approval and CI events.

The [web view](../../README.md#open-the-factory-view) is still read-only. Closing it does not affect running workflows.
