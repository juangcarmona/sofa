# Lighthouse 0.11.4 / SDLC 0.15.0 adoption

[Issue 74](https://github.com/juangcarmona/sofa/issues/74) records the 2026-10-28 upgrade from CLI 0.11.2 / SDLC 0.13.0 and workflow revision `57d4e23da2831be5fad0331f1ba82326782943cc`. The candidate uses published CLI 0.11.4 / SDLC 0.15.0 and workflow revision `ffa7e92613ad55989eb16133b1df26d2b7072603`. The maintainer explicitly authorized the official triage worker and periodic router reconciliation; optional maintenance remains disabled. The runner/repository overview request is recorded separately in [issue 57](https://github.com/juangcarmona/sofa/issues/57), with no feature implementation in this update.

## Reproduction and preserved configuration

Run `npm install --save-dev --save-exact @juangcarmona/lighthouse@0.11.4`, record SDLC 0.15.0 in the repository-owned `lighthouse.json`, preview `npx --no-install lighthouse upgrade --scope all --format json`, then apply with `npx --no-install lighthouse upgrade --scope all --apply`. The [reviewed upgrade plan](upgrade-plan.json) contains no ownership conflicts. The CLI regenerates its instructions, command aliases, SDLC metadata and ownership hashes; it preserves adapted lifecycle skills and all other repository policy.

Generate workflows with the commands in the [runbook](../../../runbooks/repository-workflows.md). The installer is the published SDLC package, and gh-aw 0.93.2 generates the locks. Existing sources matched their former upstream templates before replacement. A repeat installation must be byte-identical, including the issue form. No generated workflow or instruction file was edited by hand.

ProductShape 0.23.0, ArcShape 0.1.6, `.lighthouse/environment.json`, `.lighthouse/implement.json`, `.lighthouse/github.json`, the engineering lifecycle and host engine configuration are preserved. No identity, accepted product or application architecture changes are made. Existing .NET/PostgreSQL images and the operator-selected model/authentication are retained. Worker profiles and delegation remain opt-in and unconfigured. No SoFa scheduler, orchestration database or new runtime service is introduced.

## Behavior and validation boundary

The new request form and owner-authorized triage support initial classification and explained estimates. The refinement publisher explicitly requests verified continuation, preserving `future` and deduplication. The router reconciles implementation PR readiness after supported events and every five minutes through GitHub Actions. `apply-review` has the upstream preparation permission needed to return an implementation PR to draft while correcting it. PR state is projected onto the issue with source links; post-merge review owns issue closure.

This schedule is part of the official router and is gated by `LIGHTHOUSE_AUTOMATION_ENABLED`. The separate maintenance workflow is absent and its recovery/report variables remain unset. The optional direct CI callback was not added to SoFa's verification workflow; readiness uses the installed event/scheduled router path. Current repository rules still require the `verify` check from GitHub Actions integration 15368 and strict freshness. Successful checks do not supply human approval.

Historical observations in [issue 72's report](../issue-72/README.md) remain unchanged. Upstream release fixes and regression tests are evidence of changed code; they do not prove a new native SoFa execution. This unmerged candidate does not claim that the old CI-dispatch or agent Gitleaks failures are resolved, nor that all canonical lifecycle labels are reconciled. Native triage, continuation and PR-readiness acceptance remains a post-merge check.

## Verification evidence

| Check | Result |
| --- | --- |
| Locked consumer installation | `npm ci`: exit 0; CLI 0.11.4 and SDLC 0.15.0 resolved. The audit reports seven low-severity advisories, without an unrelated dependency rewrite. |
| Managed upgrade | Official `upgrade --scope all --apply`: exit 0. Repeat preview contains only `keep` operations. Foundation verification has zero blocking failures or drift; recommended evidence-index and live-profile verification remain separately unconfigured. |
| Workflow installer | Seven workers compiled with gh-aw 0.93.2; [installer log](install.log). Repeat generation matches the [SHA-256 inventory](generated-sha256.json), including the request form. |
| Workflow validation | Router and seven locks pass actionlint 1.7.12 with the exact upstream gh-aw compatibility exceptions and custom runner label; [commands/results](workflow-validation.json). This is not an unfiltered actionlint pass. |
| Upstream regression contracts | 82 passed, zero failures/skips at `ffa7e92`; [log](upstream-contracts.log). Covers triage, refinement continuation, PR readiness, feedback identity, completion and opt-in profile/delegation contracts. Testing optional features does not enable them. |
| Full SoFa verification | `CI=true DOTNET_CLI_USE_MSBUILD_SERVER=0 MSBUILDDISABLENODEREUSE=1 npm run verify`: exit 0. 27 Node, 38 .NET and four browser tests passed; [log](verify.log). Includes both host Docker engine tests. |
| Labels | Official capability reconciliation creates only `triaged`, `pr-draft` and `pr-ready`; [reviewed plan](labels-plan.json). Existing labels and issue assignments are preserved. Repeated planning reports zero operations. |
| Repository checks | Foundation guard, branch convention and `git diff --check` passed. PR CI supplies final commit-specific verification, including commit conventions. |

The local host used Node 22.22.1, npm 9.2.0 and .NET 10.0.113; CI installs Node 24 and .NET 10. The isolated Lighthouse test checkout initially lacked dependencies; npm 9 rejected its upstream lockfile with a missing `playwright-core@1.57.0` entry. Installing with npm 11.21.1 succeeded without tracked changes, and all selected contracts then passed. SoFa's own `npm ci` succeeded with the host npm.

The upstream test command was `node --test src/tests/implement-dispatch.test.mjs src/tests/proposal-review.test.mjs src/tests/sdlc-worker-completion.test.mjs src/tests/request-triage.test.mjs src/tests/pr-state.test.mjs src/tests/completion.test.mjs src/tests/model-profiles.test.mjs src/tests/profile-delegation.test.mjs`. The official environment builder and environment declaration implementation did not change between workflow pins. Host inspection still resolves the .NET image to `sha256:9706b4e2d73acf34a1ee339145296442dcbd81178d16f0b09f2808b29f436372` and PostgreSQL to `sha256:858bb5e98eed02eb8cafbcdc007f9a65d1b613972028944a8ec2133ccaf80d27`; rebuilding was unnecessary.
