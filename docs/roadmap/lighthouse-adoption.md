# Lighthouse adoption and conflict report

**Assessment date:** 2026-10-22. **Mode:** S0 reconciliation applied locally; no ProductShape update, ArcShape update, GitHub write, or remote mutation was run.

## Version report

Published versions were checked with npm registry metadata on 2026-10-22. SoFa's checked-in versions were read from `package.json` and `package-lock.json`.

| Integration/package | SoFa checkout | Published/current | Classification | Action |
| --- | --- | --- | --- | --- |
| `@juangcarmona/lighthouse` | `0.2.1` exact in manifest/lockfile | `0.2.1` (`latest`) | Current and locally adopted | Lighthouse owns its generated projection and guardrails. |
| `@juangcarmona/lighthouse-sdlc` | `0.2.0` selected in `lighthouse.json` and `.lighthouse/sdlc.json` | `0.2.0` (`latest`) | Current selection; temporary initializer drift reconciled | The published Lighthouse 0.2.1 initializer contains an old `0.1.0` default; SoFa selects the installed 0.2.0 package through the Lighthouse manifest. |
| `@juangcarmona/lighthouse-ui` | absent | `0.1.0` (`latest`) | Not installed; optional for S0 | Defer until a user-facing visual contract is accepted. |
| `@prodshape/cli` | `0.22.0` exact in manifest/lockfile | `0.22.0` (`latest`) | Current version; incomplete product baseline | Keep provider ownership. The model is empty and `CHG-INITIAL` is draft. |
| `@arcshape/cli` | `0.1.1` exact in manifest/lockfile | `0.1.3` (`latest`) | Stale version; session incomplete | Reconcile through the ArcShape CLI after the dirty-worktree review. |

The published `@juangcarmona/lighthouse-sdlc@0.2.0` package still exposes an embedded contract document whose contract version is `0.1.0`; that is distinct from the package version selected by SoFa. The SoFa control-plane selection is `0.2.0`, and this distinction remains a follow-up for the next Lighthouse release.

## Ownership and path inventory

| Area | Current SoFa evidence | Owner | Conflict/risk |
| --- | --- | --- | --- |
| Lighthouse manifest and projection | `lighthouse.json`, `LIGHTHOUSE.md`, `.lighthouse/sdlc.json`, guard, workflow, and hooks now exist and verify. | Lighthouse | Adopted locally; generated paths are provider-owned. |
| ProductShape | `.product/`, `docs/product/`, generated outputs, and `.agents/commands/product/**` are present. `.product/installation.lock.json` records ProductShape 0.22.0. | ProductShape | Provider-owned files are already present and the working tree contains untracked draft Product Change content. Do not overwrite. |
| ArcShape | `0.1.3` exact in the root manifest/lockfile; canonical twelve-directory layout validates. `.arcshape/sessions/attention-proposal/session.json` is untracked. | ArcShape | Structure migration applied through ArcShape CLI. Lighthouse authority dry-run reports twelve content conflicts because SoFa's section documents are project-authored and differ from ArcShape templates. Before this S0 material was added, the session had 281 evidence items: 251 pending, 7 reviewed, 5 excluded, and 18 changed. The current status is 321 items: 291 pending, 7 reviewed, 5 excluded, and 18 changed because the new planning documents are now in its evidence population. |
| Shared skills and agents | `.agents/skills/**` contains shared Agentic SDLC skills and ProductShape-managed skills; `.claude/skills/**` links to shared sources. | Agent Toolkit / ProductShape / SoFa-owned adaptations by path | Lighthouse SDLC would introduce another provider projection. Names and ownership must be mapped before installing; no duplicate hand-copied lifecycle skills. |
| Hooks | `.husky/commit-msg` and `.husky/pre-push` now call `scripts/lighthouse-guard.mjs`; old hooks are backed up outside the repository. | Lighthouse | Accepted and replaced through Lighthouse initialization. The old commitlint and branch checks remain available to CI. |
| CI | Existing `.github/workflows/verify.yml` remains; Lighthouse also generated `.github/workflows/lighthouse.yml` with read-only contents permission. | SoFa workflow / Lighthouse workflow by path | Existing Commitlint and branch scripts remain the CI defense for now. Consolidation waits for a future Lighthouse release with CI, guardrail, pipeline, and script capabilities; no privileged operation was added. |
| Runtime source | Current branch has scaffold-only source; `origin/main` has the read-only Attention app and trusted-LAN configuration. | SoFa | Lighthouse's generated boundaries must not erase the existing application or redefine Attention ownership. |
| Package versions | ProductShape current; ArcShape stale; Lighthouse 0.2.1 installed; SDLC 0.2.0 selected. | Each owning publisher/CLI | ArcShape still requires a separate provider-owned reconciliation. |

## Conflict classification

1. **Dirty checkout:** branch `chore/2-sofa-banner-ideas` has a committed banner history plus pre-existing modified architecture and ADR-index files, an untracked `.arcshape` session, and untracked `CHG-INITIAL` proposal files. Treat all as user work. S0 does not reset, stash, stage, or overwrite them.
2. **Provider-owned ProductShape files:** `.product/installation.lock.json`, `.product/templates/**`, `.agents/commands/product/**`, and ProductShape-managed skills match the 0.22.0 installation lock according to `npm run product:integrity`. They are not hand-edit targets.
3. **ArcShape content drift:** the root package is now current at 0.1.3. The canonical directory migration is complete and the current architecture files validate, but the authority dry-run correctly reports twelve content conflicts against ArcShape's blank templates. The analysis session is incomplete and has changed evidence. Recording authority adoption must happen as a reviewed reconciliation; it must not overwrite the authored content.
4. **Lighthouse adoption:** the baseline, hooks, projection, guard, workflow, and SDLC selection are now present and `lighthouse verify` passes. ProductShape and ArcShape authority adoption remains incomplete.
5. **Lifecycle overlap:** SoFa already has shared Agentic SDLC skills and an adopted lifecycle. Lighthouse's SDLC package is a versioned provider-neutral contract, not a reason to duplicate or replace the existing lifecycle document by hand. The integration plan must state which paths Lighthouse projects and which remain SoFa/Agent Toolkit-owned.
6. **Security boundary:** the remote default branch's Attention app can be explicitly exposed on a trusted LAN address and has no application authentication. It must remain read-only. Privileged execution, GitHub writes, and autonomous work cannot be added to that surface.

## Non-destructive reconciliation plan and result

1. Capture the current path/owner map and preserve the dirty checkout. The existing hooks and generated projection were backed up outside the repository before replacement.
2. Run the exact published Lighthouse CLI in dry-run mode, review missing paths, drift, and conflicts, then apply only the Lighthouse-owned baseline paths.
3. Decide whether Lighthouse's ProductShape and ArcShape authority preparation is appropriate for SoFa. If accepted, let their own CLIs initialize or reconcile those providers; never replace `.product/` or `docs/architecture/` with Lighthouse copies.
4. Resolve the ArcShape package drift to published 0.1.3 through npm/package-lock update and `arcshape` validation, after checking the session and changed generated files. This is now complete; the remaining authority dry-run conflicts are intentional content differences.
5. Install the exact published Lighthouse CLI. Apply its hooks, projection, guard, workflow, and control-plane paths through `lighthouse init`/`lighthouse reconcile`; set the selected SDLC package to `0.2.0` in `lighthouse.json` and regenerate the provider-owned projections through `lighthouse reconcile`.
6. Keep ProductShape, ArcShape, and Agent Toolkit source paths authoritative. The ArcShape README conflict was resolved by using the provider's root navigation and migrating SoFa's authored section content into the provider's canonical directories. SoFa's existing architecture content is intentionally retained; the twelve content conflicts against ArcShape's starter templates are accepted and require no overwrite.
7. Keep SoFa's existing Commitlint and branch-name checks in CI while Lighthouse owns the local hooks. Revisit this boundary when Lighthouse publishes the planned CI, guardrail, pipeline, and script capabilities.
8. Verify with `lighthouse verify --format json`, `npm run verify`, and direct hook checks. Do not claim S1 complete until the ArcShape reconciliation, workflow review, current SHA, and CI evidence are reviewable.

## S0 evidence

The published Lighthouse CLI `0.2.1` was first run with `lighthouse init --name sofa --dry-run` in an isolated clean worktree at the current SoFa HEAD. It proposed creating `lighthouse.json`, `infra/`, `.lighthouse/`, `LIGHTHOUSE.md`, `scripts/lighthouse-guard.mjs`, `.github/workflows/lighthouse.yml`, and `.lighthouse/sdlc.json`. It reported conflicts for the existing hooks and reported ProductShape and ArcShape as adoption gaps.

The published ProductShape CLI `0.22.0` dry-run detected Claude Code and Agent Skills, no SDD framework, preserved 28 existing ProductShape paths, proposed 0 overwrites, and reported 0 conflicts. This confirms that ProductShape's current provider-owned layout can be retained; it does not establish an accepted model.

The published ArcShape CLI `0.1.3` dry-run proposed creating the README under each of the twelve numbered section directories and reported one conflict: `docs/architecture/README.md` already existed with different content. The root README was backed up, the provider CLI generated the canonical root and section structure, and the existing authored section documents were migrated into those directories with corrected relative links. ArcShape validation now passes. A later Lighthouse authority dry-run reports the twelve authored section documents as content conflicts against its templates; they are preserved intentionally.

The adopted baseline now passes `lighthouse verify --format json` with 4 passed checks and no failures, drift, or unavailable checks. `npm run verify` also passes: documentation lint, 16 tests, security scan, ProductShape validation/integrity/doctor, and ArcShape link validation. The new hooks reject an invalid conventional commit, accept a valid one, and accept the current branch.

## Required reconciliation decisions

- Whether the factory Product Change is the initial baseline or a later overlay is resolved: the factory is `CHG-INITIAL`; the former Attention proposal remains separate historical material. Neither draft is approved or applied.
- The Lighthouse-managed hooks, guard, projection, workflow, manifest, and control-plane paths are accepted for local adoption; the generated workflow still needs a later CI consolidation review.
- The ArcShape canonical directory structure and root `@arcshape/cli@0.1.3` dependency are accepted locally. Lighthouse authority version recording remains a separate metadata step; the provider dry-run conflicts are accepted as project-owned content, not migration blockers.
- Decision recorded: ArcShape templates must not replace SoFa's existing architecture content. Future improvements happen in SoFa's section documents, while ArcShape remains the structural and validation authority.
- Decision recorded: keep the existing CI Commitlint and branch checks for now; Lighthouse owns local hooks, and a future Lighthouse CI/pipeline release will be evaluated before consolidation.
- Whether the next Lighthouse release removes the old `0.1.0` initializer/contract drift.
- Whether the optional UI package is needed; S0 has no accepted UI requirement.
- How the existing shared Agentic SDLC lifecycle maps to Lighthouse SDLC routes without installing two competing copies.
- Which execution operations, if any, are allowed under each future risk profile and which always require Juan.
