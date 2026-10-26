# Issue 42 retrospective

Date: 2026-10-26

[Issue #42](https://github.com/juangcarmona/sofa/issues/42) adopted CLI 0.11.2 and SDLC 0.13.0 through [PR #43](https://github.com/juangcarmona/sofa/pull/43), merged as `5dc0a1d00a93c82d8930b043480a40cfc7d85170`.

## What worked

Published npm artifacts and release tags were verified before adoption. The supported upgrade preserved provider ownership and repository policy, and its subsequent preview found no drift. Independent review and CI covered the exact merged PR head. Local verification passed 24 Node tests and 38 runtime tests plus repository and provider checks.

## What did not

Installing the CLI updates its dependency but preserves the explicitly selected SDLC version in `lighthouse.json`. Adoption required selecting 0.13.0 in that source manifest and regenerating the control-plane projection. Tooling notes still described obsolete automatic worker installation; this update corrected them. Recommended live GitHub-profile verification and delivery evidence indexing remain unconfigured.

## What to do next time

Check installed packages, selected manifest versions and generated projections together. Keep tooling instructions tied to the supported commands and owning manifests. Existing upgrade previews and verification suffice for this dependency-only change; no additional abstraction or policy is needed.

## Post-merge evidence

Both main-branch checks passed: [repository verification](https://github.com/juangcarmona/sofa/actions/runs/40254165432) and [foundation verification](https://github.com/juangcarmona/sofa/actions/runs/40254165450). The merged checkout passed Lighthouse verification and observed presentations with availability `available`, 20 runs and two attention items. No running service was redeployed.

## Effort

Human active time, agent active time, tokens and cost: not captured. The repository has no configured session-to-work-item usage mapping for this run.

## Amendments

None. This update changes no lifecycle gate, permission, product intent or architecture decision. Routine completion was delegated by the owner; the technical audit is not a personal human-review record or an `/ff` verdict.
