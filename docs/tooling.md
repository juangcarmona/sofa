# Tooling and adoption

## ProductShape

`@prodshape/cli@0.22.0` is pinned exactly in `package.json` and `package-lock.json`; this was npm's `latest` tag on 2026-10-19. ProductShape owns `docs/product/`, `.product/config.yaml`, templates, installation metadata, and generated Codex integrations. Refresh managed integrations with `npx prodshape integration update`; check them with `npm run product:integrity` and `npm run product:doctor`.

The accepted product model is intentionally empty. Create the initial Product Change only after SoFa's product intent has been explained and reviewed.

## ArcShape

`@arcshape/cli@0.1.1` is pinned exactly in the package manifest and lockfile; this was npm's `latest` tag on 2026-10-19. ArcShape owns the twelve arc42 sections and its generated architecture skills and commands. Validate the structure with `npm run architecture:check` and update it through `npx arcshape`.

Architecture content remains intentionally unfilled until product intent and implementation evidence exist. Do not replace unknown facts with template assumptions.

## Agentic SDLC and shared skills

The portable Agentic SDLC skills and shared `adr`, `architecture-docs`, `git-worktrees`, and `rebase-safely` skills are installed for Codex under `.agents/skills/`. `skills-lock.json` records their source and content. Lifecycle conventions and project-owned role adaptations live in `docs/engineering-lifecycle.md` and `.agents/skills/`; Claude Code skill links point back to the portable sources where available.

Restore or update upstream skills through the skills CLI and review the resulting diff. Do not hand-edit installed package capabilities. The project owns its lifecycle document and adapted role skills.

## Runtime baseline

The scaffold requires Node.js 22 or newer and currently uses npm with a committed lockfile. Application stack and deployment tooling are undecided.

## Contribution conventions

Conventional Commit messages are checked locally by Husky and commitlint. The pre-push hook checks the issue-linked branch name. Pull request CI repeats both checks so bypassing a local hook does not skip validation. See `docs/engineering-lifecycle.md` for the branch format and allowed prefixes.

## Secret scanning

`npm run lint:docs` uses `markdownlint-cli2@0.23.3` on repository-owned Markdown, excluding generated skills and ProductShape files. `npm run security:check` installs Gitleaks 8.29.1 into a user cache, verifies the official release checksum, and scans all Git history, staged changes, and the working tree with full redaction. CI fetches full history for the scan. The narrow allowlist in `.gitleaks.toml` exempts only the ArcShape validator's literal `arc42-section` metadata comparison; generated caches and Git internals are excluded. The scanner tests include an ignored synthetic secret and verify that its value is never printed. The scan requires network access to the official Gitleaks release for a first-time installation and supports Linux x64 and Windows x64.

## Dependency audit

The initial audit on 2026-10-19 reported 16 advisory entries in the development toolchain (5 low, 5 moderate, 6 high). `npm audit fix` made no lockfile changes; npm's forced recommendation would downgrade the current Markdown linter across a major version. High findings include the ProductShape `fast-glob` → `micromatch` → `braces` chain ([GHSA-vfj7-8cjw-p6xm](https://github.com/advisories/GHSA-vfj7-8cjw-p6xm)) and the Markdown lint dependency tree; ArcShape's Mermaid CLI tree also includes the reported KaTeX advisory. These are development-tool dependencies, not SoFa runtime dependencies. Recheck after upstream releases; CI does not claim a clean dependency audit.
