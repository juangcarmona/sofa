# Lighthouse adoption and conflict report

**Assessment date:** 2026-10-22. **Mode:** S0 reconciliation and cleanup branch; no ProductShape update, ArcShape content overwrite, GitHub write, or remote mutation was run.

## Version report

Published versions were checked with npm registry metadata on 2026-10-22. SoFa's checked-in versions were read from `package.json` and `package-lock.json`.

| Integration/package | SoFa checkout | Published/current | Classification | Action |
| --- | --- | --- | --- | --- |
| `@juangcarmona/lighthouse` | `0.2.1` exact in manifest/lockfile | `0.2.1` (`latest`) | Current and locally adopted | Lighthouse owns its generated projection and guardrails. |
| `@juangcarmona/lighthouse-sdlc` | `0.2.0` selected in `lighthouse.json` and `.lighthouse/sdlc.json` | `0.2.0` (`latest`) | Current selection; temporary initializer drift reconciled | The published Lighthouse 0.2.1 initializer contains an old `0.1.0` default; SoFa selects the installed 0.2.0 package through the Lighthouse manifest. |
| `@juangcarmona/lighthouse-ui` | absent | `0.1.0` (`latest`) | Not installed; optional for S0 | Defer until a user-facing visual contract is accepted. |
| `@prodshape/cli` | `0.22.0` exact in manifest/lockfile | `0.22.0` (`latest`) | Current version; applied product baseline | Keep provider ownership. The model contains the Software Factory baseline. |
| `@arcshape/cli` | `0.1.3` exact in manifest/lockfile | `0.1.3` (`latest`) | Current version; local analysis session incomplete | Keep ArcShape content under provider ownership and review the session separately. |

The published `@juangcarmona/lighthouse-sdlc@0.2.0` package still exposes an embedded contract document whose contract version is `0.1.0`; that is distinct from the package version selected by SoFa. The SoFa control-plane selection is `0.2.0`, and this distinction remains a follow-up for the next Lighthouse release.

## Ownership and path inventory

| Area | Current SoFa evidence | Owner | Conflict/risk |
| --- | --- | --- | --- |
| Lighthouse manifest and projection | `lighthouse.json`, `LIGHTHOUSE.md`, `.lighthouse/sdlc.json`, guard, workflow, and hooks now exist and verify. | Lighthouse | Adopted locally; generated paths are provider-owned. |
| ProductShape | `.product/`, `docs/product/`, generated outputs, and `.agents/commands/product/**` are present. `.product/installation.lock.json` records ProductShape 0.22.0; the model contains the applied Software Factory baseline. | ProductShape | Provider-owned files are already present. Do not overwrite. The completed change record contains stale wording that must be corrected through ProductShape if it is retained. |
| ArcShape | `0.1.3` exact in the root manifest/lockfile; canonical twelve-directory layout validates. `.arcshape/sessions/attention-proposal/session.json` is untracked. | ArcShape | Structure migration applied through ArcShape CLI. Lighthouse authority dry-run reports twelve content conflicts because SoFa's section documents are project-authored and differ from ArcShape templates. Before this S0 material was added, the session had 281 evidence items: 251 pending, 7 reviewed, 5 excluded, and 18 changed. The current status is 321 items: 291 pending, 7 reviewed, 5 excluded, and 18 changed because the new planning documents are now in its evidence population. |
| Shared skills and agents | `.agents/skills/**` contains shared Agentic SDLC skills and ProductShape-managed skills; `.claude/skills/**` links to shared sources. | Agent Toolkit / ProductShape / SoFa-owned adaptations by path | Lighthouse SDLC would introduce another provider projection. Names and ownership must be mapped before installing; no duplicate hand-copied lifecycle skills. |
| Hooks | `.husky/commit-msg` and `.husky/pre-push` now call `scripts/lighthouse-guard.mjs`; old hooks are backed up outside the repository. | Lighthouse | Accepted and replaced through Lighthouse initialization. The old commitlint and branch checks remain available to CI. |
| CI | Existing `.github/workflows/verify.yml` remains; Lighthouse also generated `.github/workflows/lighthouse.yml` with read-only contents permission. | SoFa workflow / Lighthouse workflow by path | Existing Commitlint and branch scripts remain the CI defense for now. Consolidation waits for a future Lighthouse release with CI, guardrail, pipeline, and script capabilities; no privileged operation was added. |
| Runtime source | This cleanup branch has no application source; its `origin/main` base still contains the read-only Attention app and trusted-LAN configuration until the cleanup is integrated. | SoFa | The removed Attention slice is not part of the current product baseline. |
| Package versions | ProductShape 0.22.0, ArcShape 0.1.3, Lighthouse 0.2.1, and SDLC 0.2.0 are selected locally. Lighthouse 0.3.0 is not on this branch; PR #8 remains a separate open adoption change. | Each owning publisher/CLI | Do not describe 0.3.0 as merged or current in this branch. |

## Conflict classification

1. **Dirty checkout:** branch `refactor/6-remove-attention-slice` contains the explicit cleanup diff plus the ignored local `.arcshape` analysis session. Treat unrelated files as user work. S0 does not reset, stash, or overwrite them.
2. **Provider-owned ProductShape files:** `.product/installation.lock.json`, `.product/templates/**`, `.agents/commands/product/**`, and ProductShape-managed skills match the 0.22.0 installation lock according to `npm run product:integrity`. They are not hand-edit targets.
3. **ArcShape content drift:** the root package is current at 0.1.3. The canonical directory structure validates, and the cleanup branch intentionally regenerated the twelve section documents as empty provider templates after removing the Attention implementation. The local analysis session is incomplete; it remains ignored provider state and must not overwrite authored content.
4. **Lighthouse adoption:** the baseline, hooks, projection, guard, workflow, and SDLC selection are present. `lighthouse verify` currently fails only because the manifest declares `infra/` and that boundary is absent on this branch. ProductShape is applied; ArcShape structure validates, while its local analysis session remains incomplete.
5. **Lifecycle overlap:** SoFa already has shared Agentic SDLC skills and an adopted lifecycle. Lighthouse's SDLC package is a versioned provider-neutral contract, not a reason to duplicate or replace the existing lifecycle document by hand. The integration plan must state which paths Lighthouse projects and which remain SoFa/Agent Toolkit-owned.
6. **Security boundary:** the base branch's historical Attention app can be explicitly exposed on a trusted LAN address and has no application authentication. It must remain read-only until removed by this cleanup. Privileged execution, GitHub writes, and autonomous work cannot be added to that surface.

## Non-destructive reconciliation plan and result

1. Capture the current path/owner map and preserve the dirty checkout. The existing hooks and generated projection were backed up outside the repository before replacement.
2. Run the exact published Lighthouse CLI in dry-run mode, review missing paths, drift, and conflicts, then apply only the Lighthouse-owned baseline paths.
3. Decide whether Lighthouse's ProductShape and ArcShape authority preparation is appropriate for SoFa. If accepted, let their own CLIs initialize or reconcile those providers; never replace `.product/` or `docs/architecture/` with Lighthouse copies.
4. Resolve the ArcShape package drift to published 0.1.3 through npm/package-lock update and `arcshape` validation, after checking the session and changed generated files. This is now complete; the remaining authority dry-run conflicts are intentional content differences.
5. Install the exact published Lighthouse CLI. Apply its hooks, projection, guard, workflow, and control-plane paths through `lighthouse init`/`lighthouse reconcile`; set the selected SDLC package to `0.2.0` in `lighthouse.json` and regenerate the provider-owned projections through `lighthouse reconcile`.
6. Keep ProductShape, ArcShape, and Agent Toolkit source paths authoritative. The cleanup branch regenerated the ArcShape section documents through the owning CLI after removing the obsolete implementation. Do not add inferred architecture facts to the templates; future architecture content must be authored only after the product boundary and architecture decisions are reviewed.
7. Keep SoFa's existing Commitlint and branch-name checks in CI while Lighthouse owns the local hooks. Revisit this boundary when Lighthouse publishes the planned CI, guardrail, pipeline, and script capabilities.
8. Verify with `lighthouse verify --format json`, `npm run verify`, and direct hook checks. Do not claim S1 complete until the ArcShape reconciliation, workflow review, current SHA, and CI evidence are reviewable.

## S0 evidence

The published Lighthouse CLI `0.2.1` was first run with `lighthouse init --name sofa --dry-run` in an isolated clean worktree at the current SoFa HEAD. It proposed creating `lighthouse.json`, `infra/`, `.lighthouse/`, `LIGHTHOUSE.md`, `scripts/lighthouse-guard.mjs`, `.github/workflows/lighthouse.yml`, and `.lighthouse/sdlc.json`. It reported conflicts for the existing hooks and reported ProductShape and ArcShape as adoption gaps.

The published ProductShape CLI `0.22.0` dry-run detected Claude Code and Agent Skills, no SDD framework, preserved 28 existing ProductShape paths, proposed 0 overwrites, and reported 0 conflicts. The checked-in ProductShape model is the applied Software Factory baseline; the CLI evidence confirms that its provider-owned layout can be retained.

The published ArcShape CLI `0.1.3` generated the canonical root and twelve-section structure. The cleanup branch then regenerated the section documents through that CLI after removing the Attention implementation; ArcShape link validation passes. The local `.arcshape` analysis session remains ignored and uncommitted provider state.

The current branch passes three of four Lighthouse checks. The repository-layout check fails because `infra/` is declared by `lighthouse.json` but absent; this is recorded rather than repaired in the cleanup slice. `npm run verify` and direct hook checks remain to be run after the cleanup diff is complete.

## Required reconciliation decisions

- The factory Product Change is the initial baseline: `CHG-INITIAL` is applied in ProductShape; the former Attention proposal remains separate historical material.
- The Lighthouse-managed hooks, guard, projection, workflow, manifest, and control-plane paths are accepted for local adoption; the generated workflow still needs a later CI consolidation review.
- The ArcShape canonical directory structure and root `@arcshape/cli@0.1.3` dependency are accepted locally. Lighthouse authority version recording remains a separate metadata step; the provider dry-run conflicts are accepted as project-owned content, not migration blockers.
- Decision recorded: ArcShape's generated structure and validation remain authoritative; its empty section templates must not be filled with inferred facts. Future architecture content is authored in those section documents only after the architecture is accepted.
- Decision recorded: keep the existing CI Commitlint and branch checks for now; Lighthouse owns local hooks, and a future Lighthouse CI/pipeline release will be evaluated before consolidation.
- Whether the next Lighthouse release removes the old `0.1.0` initializer/contract drift.
- Whether the optional UI package is needed; S0 has no accepted UI requirement.
- How the existing shared Agentic SDLC lifecycle maps to Lighthouse SDLC routes without installing two competing copies.
- Which execution operations, if any, are allowed under each future risk profile and which always require Juan.
