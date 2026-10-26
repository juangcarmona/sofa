# Tooling and adoption

## Lighthouse

The exact installed CLI version is pinned in `package.json` and `package-lock.json`; `lighthouse.json` selects the SDLC package. Preview updates with `npx lighthouse upgrade --scope all`, then apply reviewed changes and run `npx lighthouse verify --format json`. `.lighthouse/upgrade.json` tracks managed-file ownership. The current adoption uses CLI 0.11.2 and SDLC 0.13.0.

Workflow installation is separate and opt-in. Foundation upgrades preserve existing workflows; SoFa remains an observer of independently executed repository workflows. The labels and recommended GitHub profile capabilities are already adopted. No `visual-theme` profile is selected.

## ProductShape

`@prodshape/cli@0.23.0` is pinned exactly in `package.json` and `package-lock.json`. Lighthouse records ProductShape `0.23.0` as the adopted authority in `lighthouse.json`; the ProductShape installation remains provider-owned. Refresh managed integrations with `npx prodshape integration update`; check them with `npm run product:integrity` and `npm run product:doctor`.

The accepted ProductShape model contains the Software Factory definition. Product intent changes go through Product Changes; do not edit the accepted model directly.

## ArcShape

`@arcshape/cli@0.1.6` is pinned exactly in the package manifest and lockfile. Lighthouse records ArcShape `0.1.6` as the adopted authority in `lighthouse.json`; ArcShape owns the twelve arc42 sections and its generated architecture skills and commands. Validate the structure with `npm run architecture:check` and update it through `npx arcshape`.

The twelve-section architecture structure now contains the reviewed Software Factory target, including the Lighthouse/SoFa/Bastion boundary and ADR-0001. It describes the target and current evidence; it does not prove that a runtime is installed. Continue to update the sections through ArcShape's ownership and do not replace unknown implementation facts with assumptions.

## Agentic SDLC and shared skills

The portable Agentic SDLC skills and shared `architecture-docs`, `git-worktrees`, and `rebase-safely` skills are installed for Codex under `.agents/skills/`. The shared `adr` skill has been removed; ArcShape owns this repository's ADR workflow under `docs/architecture/09-architecture-decisions/`. `skills-lock.json` records the remaining installed shared skills. Lifecycle conventions and project-owned role adaptations live in `docs/engineering-lifecycle.md` and `.agents/skills/`; Claude Code skill links point back to the portable sources where available.

Restore or update upstream skills through the skills CLI and review the resulting diff. Do not hand-edit installed package capabilities. The project owns its lifecycle document and adapted role skills.

## Runtime baseline

See the root README for the current Node.js, .NET and Docker verification requirements. The observer runs independently of the retained legacy runtime; ADR-0006 defines that boundary.

## Contribution conventions

Conventional Commit messages are checked locally by Husky and commitlint. The pre-push hook checks the issue-linked branch name. Pull request CI repeats both checks so bypassing a local hook does not skip validation. See `docs/engineering-lifecycle.md` for the branch format and allowed prefixes.

## Secret scanning

`npm run lint:docs` uses `markdownlint-cli2@0.23.3` on repository-owned Markdown, excluding generated skills and ProductShape files. `npm run security:check` installs Gitleaks 8.29.1 into a user cache, verifies the official release checksum, and scans all Git history, staged changes, and the working tree with full redaction. CI fetches full history for the scan. The narrow allowlist in `.gitleaks.toml` exempts only the ArcShape validator's literal `arc42-section` metadata comparison; generated caches and Git internals are excluded. The scanner tests include an ignored synthetic secret and verify that its value is never printed. The scan requires network access to the official Gitleaks release for a first-time installation and supports Linux x64 and Windows x64.

## Dependency audit

The initial audit on 2026-10-19 reported 16 advisory entries in the development toolchain (5 low, 5 moderate, 6 high). `npm audit fix` made no lockfile changes; npm's forced recommendation would downgrade the current Markdown linter across a major version. High findings include the ProductShape `fast-glob` → `micromatch` → `braces` chain ([GHSA-vfj7-8cjw-p6xm](https://github.com/advisories/GHSA-vfj7-8cjw-p6xm)) and the Markdown lint dependency tree; ArcShape's Mermaid CLI tree also includes the reported KaTeX advisory. These are development-tool dependencies, not SoFa runtime dependencies. Recheck after upstream releases; CI does not claim a clean dependency audit.
