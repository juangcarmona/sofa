# Lighthouse feedback-worker adoption

[Issue 72](https://github.com/juangcarmona/sofa/issues/72) updates the workflow adoption from `496562b1deb8b217b231a0d7baad8d10889485b0` to `57d4e23da2831be5fad0331f1ba82326782943cc`. The latter includes Lighthouse PRs #129, #137 and [#144](https://github.com/juangcarmona/lighthouse/pull/144). The maintainer explicitly authorized the official `apply-review` worker on 2026-10-27 and requested optional maintenance remain disabled. This delivery remains subject to owner review and merge.

## Scope and reproduction

The official source installer generated the router and six workers with gh-aw 0.93.2, using `--with-completion --replace-reviewed --apply`. Its source lives at `src/packages/sdlc/src/install-workflows.mjs` in the pinned Lighthouse revision. The [runbook](../../../runbooks/repository-workflows.md) includes the complete preview and apply commands. Existing sources matched their former upstream templates before replacement. A second installation produced byte-identical workflow files, including the new lock. Generated files were not edited manually.

`apply-review` applies authorized owner feedback to the existing proposal or implementation PR. It retains the underlying lifecycle stage, validates the source comment and current head, and uses the issue reservation. It does not authorize integration. Maintenance is absent, and the live repository variable inventory contained only `LIGHTHOUSE_AUTOMATION_ENABLED=true`; recovery, maintenance and report-index variables were unset. No scheduler, orchestration database or runtime service was added.

SoFa's `.lighthouse/environment.json`, implementation checks, review requirement, GitHub policy, engineering lifecycle, ProductShape and ArcShape artifacts remain unchanged. The original worktrees and their unrelated changes were preserved. The installed host images still resolve to .NET `sha256:9706b4e2d73acf34a1ee339145296442dcbd81178d16f0b09f2808b29f436372` and PostgreSQL `sha256:858bb5e98eed02eb8cafbcdc007f9a65d1b613972028944a8ec2133ccaf80d27`. The official environment builder and execution-environment implementation have no changes between the two workflow pins; rebuilding these images was unnecessary.

The npm registry still reports CLI 0.11.2 and SDLC 0.13.0. Git contains the 0.11.3 / 0.14.0 version commit, but [release 40522455418](https://github.com/juangcarmona/lighthouse/actions/runs/40522455418) was waiting for owner approval of `npm-publish` when checked. Package manifests and the lockfile therefore remain at published versions. This is a workflow-source update, not a completed npm-package upgrade.

## Validation

Validation results and commands are recorded alongside this report. The required status check was read from the effective `main` ruleset: `verify`, GitHub Actions integration 15368, strict freshness. Human acceptance remains a separate lifecycle requirement even though the ruleset requires zero approval votes.

| Check | Observed result |
| --- | --- |
| Official installer and gh-aw 0.93.2 | Exit 0; all six workers compiled. [Installer log](install.log). A repeat installation produced the same [workflow SHA-256 inventory](workflow-sha256.json). |
| Workflow validation | Router and all six locks: exit 0 with actionlint 1.7.12 and Lighthouse's supported exceptions. [Exact commands and results](workflow-validation.json). |
| Upstream contracts at the adopted revision | Exit 0; 39 tests passed, none skipped. [Contract log](upstream-contracts.log), including feedback identity, duplicate admission, completion and same-branch publication. |
| Locked installation | `npm ci`: exit 0. Audit reported seven low-severity advisories; no unrelated dependency rewrite was performed. |
| Full SoFa verification | `CI=true DOTNET_CLI_USE_MSBUILD_SERVER=0 MSBUILDDISABLENODEREUSE=1 npm run verify`: exit 0; 27 Node, 38 .NET and four browser tests passed. [Verification log](verify.log). Includes the two host Docker engine tests. |
| Foundation and conventions | `npx --no-install lighthouse verify`, `node scripts/lighthouse-guard.mjs verify` and `npm run check:branch`: exit 0. |

The local host used Node 22.22.1, .NET 10.0.113 and Docker 29.1.3; PR CI installs Node 24 and .NET 10. The branch was zero commits behind `origin/main` when checked. The linked PR's checks supply the final commit-specific CI result.

The upstream contract command was `node --test src/tests/implement-dispatch.test.mjs src/tests/proposal-review.test.mjs src/tests/sdlc-worker-completion.test.mjs src/tests/worker-catalogue.test.mjs src/tests/worker-recovery.test.mjs src/tests/worker-reports.test.mjs`, run from the pinned Lighthouse checkout. Recovery/report tests establish upstream contracts only; these optional features remain disabled in SoFa.

Unconfigured actionlint initially rejected `concurrency.queue`, generated `job.workflow_ref` and optional admission outputs, plus the custom runner label. Validation then used the runner label and the exact scoped exceptions from upstream [`scripts/verify-issue-workflows.sh`](https://github.com/juangcarmona/lighthouse/blob/57d4e23da2831be5fad0331f1ba82326782943cc/scripts/verify-issue-workflows.sh). The result is not an unfiltered actionlint pass. The compiler also reports upstream checkout/setup-node version notices; the generated sources remain installer-owned.

Compilation and contract tests cannot establish native event behavior before these workflows reach the default branch. In particular, this change does not claim live `apply-review` acceptance or resolution of the previous experiment's gaps. The prior test issue and PR are already closed without merge; no replacement fixture was left open.

## What to investigate with Lighthouse

The [previous acceptance report](https://github.com/juangcarmona/sofa/blob/3e75c0ea9275fa04661d8265399d288e4c290de3/docs/delivery/evidence/issue-65/post-merge/README.md), proposed separately in [PR 71](https://github.com/juangcarmona/sofa/pull/71), retains the raw evidence. These observations were made against the old pin. They are investigation inputs, not claims that the new revision has reproduced or fixed them.

| Observation | Evidence and requested investigation |
| --- | --- |
| Refinement passed but automatic implementation was not observed before manual recovery. | [Refine 40518750390](https://github.com/juangcarmona/sofa/actions/runs/40518750390) finished at 14:15:58 UTC; the report's 14:18:18 observation found no continuation. [Recovery 40519851066](https://github.com/juangcarmona/sofa/actions/runs/40519851066) admitted one implementation. Ask Lighthouse to trace completion-event delivery and router admission. The bounded observation does not establish the cause or rule out delayed delivery. |
| Feedback updated the same PR, but its publisher reported that CI could not start. | [Feedback worker 40521552820](https://github.com/juangcarmona/sofa/actions/runs/40521552820) saved head `e17e05b5f58315b40b11cd186818a9cf2c8654bd`; [manual verification 40522957843](https://github.com/juangcarmona/sofa/actions/runs/40522957843) passed on that head. Ask for the exact dispatch error and verify token permissions, workflow eligibility and branch/head binding. PR-event workflow approval is separate from this explicit dispatch failure. |
| The canonical `status:backlog` label remained alongside `ready` and `waiting-review`. | The report's issue snapshots preserve the mismatch. Reconcile the shared worker overlays with SoFa's adopted canonical lifecycle contract. Current `issue-actions.mjs` still publishes overlay labels without updating `status:*`; this update does not fix that integration gap. |
| Gitleaks failed its version check inside the agent but passed in fresh independent validation. | The report retains both agent verification logs and independent check receipts. Investigate the scanner process, environment and cache in both contexts before assigning the defect to Lighthouse or SoFa. The new pin does not change `implementation-runtime.mjs` or `execution-environment.mjs`. |

Waiting for owner review is expected. The blocked agent assessments, successful independent checks and successful Actions workflows are different facts; none supplies human approval. The initial agent browser failures also remain in the report, although its feedback browser run and independent checks passed. A slow Chromium download in recovered full CI is not evidence of an application failure.

PR #144 separates feedback handling and adds optional recovery/reporting. Its recovery is restricted to eligible technical failures before publication; it does not establish repair of a successful refinement with missing continuation, a published PR whose CI dispatch failed, stale canonical labels or an agent assessment waiting for review. Maintenance is deliberately disabled in this adoption. These findings are documented here for handoff; this change does not post an upstream issue or imply that Lighthouse has accepted a defect report.
