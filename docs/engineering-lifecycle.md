# Engineering lifecycle

**Project**: SoFa · **Adopted**: 2026-10-19 · **Last amended**: 2026-10-19

## Definition of Ready

Governs the **Ready** gate at the end of the `refine` capability: *is the problem sufficiently defined?* Verified semantically by an agent, which alerts a human when a dimension is missing, vague or contradictory rather than refusing outright.

An item is ready when every applicable dimension is present. Inapplicable ones are marked `N/A` with a one-line reason.

| # | Dimension | This project requires |
| --- | --- | --- |
| 1 | Title | Kept — states the desired outcome, not the activity |
| 2 | Description | Kept — explains the intent, problem and relevant context |
| 3 | Actor / stakeholder | Kept — identifies who needs or is affected by the change |
| 4 | Priority | Dropped — no formal prioritisation; items are picked up by need |
| 5 | Dependencies | Kept — must be explicitly listed, including "none" |
| 6 | Acceptance criteria | Kept — observable and verifiable conditions for success |
| 7 | Scope | Adapted — explicit out-of-scope statement required when ambiguity exists |
| 8 | Product rules | Kept — recorded in the ProductShape model and relevant specifications when present |
| 9 | Affected behaviour | Kept — when applicable, identify existing flows the change may alter |
| 10 | Quality expectations | Kept — security is relevant; performance and accessibility are stated when applicable |
| 11 | Test impact | Adapted — identified during Plan, not required at Ready |
| 12 | Existing decisions | Kept — ADRs (`docs/architecture/09-architecture-decisions/`), accepted product intent (`docs/product/`), and architecture (`docs/architecture/`) |
| 13 | Unknowns / risks | Kept — material uncertainty must be recorded, not necessarily resolved |

### Additional to this project

No additional dimensions. The reference dimensions are sufficient for this project.

## GitHub issue classification

GitHub labels carry two independent facts:

| Label family | Allowed values | Meaning |
| --- | --- | --- |
| Work type | `bug`, `enhancement`, `documentation` | What kind of work the issue describes |
| Lifecycle | `status:backlog`, `status:ready`, `status:proposed`, `status:in-progress`, `status:in-review`, `status:done` | Where the issue is in SoFa's delivery lifecycle |

An open issue has exactly one canonical `status:*` label. Work type is optional until classification is clear; do not invent a type to satisfy a transition. These labels do not replace GitHub's independent open/closed state, and labels such as `duplicate`, `good first issue`, and `help wanted` remain separate metadata. `blocked` and `owner-review` are optional overlays and never replace lifecycle status.

When creating an issue, validate the required Ready dimensions below (mark inapplicable dimensions `N/A` with a reason), check for likely duplicates using the title and distinctive terms, set `status:backlog`, and request no more than one existing work-type label.

## Definition of Done

Governs the **Done** gate at the end of the `implement` capability: *is the resulting change sufficiently verified?* The `integrate` capability refuses to merge a change that has not passed it.

**In this project, "done" means**: finished and proven, ready to integrate. Deployment is a separate act.

Lane is `deterministic` (an agent or CI settles it), `judgement` (a named role decides), or `both`. Judgement items must name a role from the **Roles** section.

| # | Dimension | This project requires | Lane | Role |
| --- | --- | --- | --- | --- |
| 1 | Acceptance criteria | A test per criterion where feasible; demonstrated in review | both | developer |
| 2 | Implementation completeness | No TODOs, no dead code, no silent scope loss; planned tasks finished or dropped with a recorded reason | both | developer |
| 3 | Tests | Relevant test suites pass; coverage is adequate to the change and its risk | both | developer |
| 4 | Quality checks | `npm run verify` and the checks configured in CI pass | deterministic | — |
| 5 | Security | No secrets are introduced; configured security checks pass; exposure is judged by reviewer | both | developer |
| 6 | Architecture / design conformance | Change matches the approach approved at the Plan gate; deviations recorded, not silent | judgement | developer |
| 7 | Documentation | User-facing, operational and developer documentation updated wherever behaviour changed | both | developer |
| 8 | Product / spec consistency | ProductShape intent and specifications are updated when behavior changes; product semantics change only through Product Changes | both | developer |
| 9 | Evidence | CI run linked; test results exist | deterministic | — |
| 10 | Human review | Self-review against every DoD dimension | judgement | developer |
| 11 | Known limitations | What is deliberately not addressed is stated in the pull request | both | developer |

### Additional to this project

No additional dimensions. The reference dimensions are sufficient for this project.

### Deterministic checks

The commands that produce the deterministic lane's evidence. `verify-like-ci` derives these from the project's pipeline configuration rather than from this list; the list records what the team expects to see, so a silent disappearance is noticed.

| Check | How it runs | Evidence it produces |
| --- | --- | --- |
| Test runner | `npm test` | Node test runner passes; behavior tests are added as implementation begins |
| Markdown lint | `npm run lint:docs` | Markdown conventions pass for repository-owned documents |
| Product model | `npm run product:validate` | ProductShape validates the model and Product Changes |
| Managed integrations | `npm run product:integrity` and `npm run product:doctor` | ProductShape-managed files match the checked-in configuration |
| Architecture docs | `npm run architecture:check` | ArcShape validates all twelve arc42 sections |
| Secret scanning | `npm run security:check` | Gitleaks scans all refs, staged changes, and the working tree with redacted output |
| CI | GitHub Actions `verify` workflow | Repository verification passes on the proposed commit |

**Note**: This scaffold has no application build or product behavior yet. Add checks when implementation tooling is adopted; do not report absent checks as passing.

## Lifecycle stages and states

How the loop's stages map onto this project's work-item states. GitHub issue state remains separate; one canonical `status:*` label records lifecycle status.

| Stage | State on entry | State on exit | Transition | Fired by |
| --- | --- | --- | --- | --- |
| `refine` | `status:backlog` | `status:ready` | Set Ready after the gate passes | `refine` |
| `propose` (`/plan` alias) | `status:ready` | `status:proposed` | Set after the plan passes review | `propose` |
| `implement` | `status:proposed` | `status:in-progress` → `status:in-review` | Set in-progress on entry; set in-review when Done passes and the PR is ready | `implement` |
| `integrate` | `status:in-review` | `status:done`, issue closed | Merge the linked PR; verify issue closure, then set done | `integrate` |
| `review` | `status:done` | `status:done` | Create the per-item retrospective; no lifecycle transition | `review` |

The transition skill reads this table before each write. It rereads the issue immediately before writing and stops if GitHub open/closed state or lifecycle labels changed. After the one-operation label swap it rereads and verifies exactly one `status:*` label remains and that unrelated labels were preserved. An enabled `/ff` invocation authorizes routine transitions without per-write confirmation; other human-only transitions still stop.

Pull requests are not work items for this lifecycle transition workflow. The issue API exposes an issue-shaped record for a pull request; the skill must inspect the `pull_request` field and refuse it. Reopen, block, and deprioritise remain human-only decisions and are never performed by the transition skill. If a work item is closed, that GitHub state is reported independently from its lifecycle label.

### Transitions no command ever fires

| Transition | Meaning | Who fires it |
| --- | --- | --- |
| Reopen | Reversing Done back to an earlier state | human only |
| Block | Item is blocked by a dependency or external factor | human only |
| Deprioritise | Item moved back to Backlog from a later state | human only |

## Work item fields

Fields a transition's screen requires. GitHub Issues has no transition screens; the lifecycle state is carried as a `status:<state>` label.

| Field | Identifier | Required by | Notes |
| --- | --- | --- | --- |
| Lifecycle state | one canonical `status:*` label | every transition | Exactly one on each open issue; replace the old status in the same update |
| Work type | one of `bug`, `enhancement`, `documentation` | issue creation / classification | Optional; at most one, and independent of lifecycle |
| Acceptance criteria | `## Acceptance Criteria` section in issue body | `/refine` (Ready gate) | Markdown heading in the issue body |
| Work item link | `Closes #<number>` in PR body | `/implement` | Placed in the PR body, not the title |

## Label conventions

Use the shared lifecycle labels `status:backlog`, `status:ready`, `status:proposed`, `status:in-progress`, `status:in-review`, and `status:done`. Keep type and other metadata labels independent. `blocked` and `owner-review` are optional overlays. New issues start at `status:backlog`.

Post-merge retrospectives use one file per item at `docs/reviews/issue-<number>.md`. The baseline date is 2026-10-19; earlier merges are historical and excluded from review debt. A separate cross-item process ledger is not used.

## Fast Forward

Invoke `/agentic-sdlc:ff <work-item-id>` in Claude Code or `ff <work-item-id>` in a portable skill host. It composes the existing lifecycle stages, creates durable run evidence, and uses a different reviewer model. An enabled invocation authorizes routine lifecycle-owned writes; it stops at every owner-only decision, required human approval, deployment, failed check, or repository red line.

| Setting | This project requires |
| --- | --- |
| Enabled | Yes when the active harness can attest a distinct reviewer model |
| Executor and reviewer | Resolve at run time; reviewer must be a different model |
| Maximum correction rounds | 3 per gate |
| Substitutable judgement items | None by default; owner remains responsible for every Definition of Done judgement item |
| Durable evidence | `docs/delivery/ff/<work-item-id>/<run-id>/run.json` and `summary.md` |
| Human-only or protected gates | Product-scope and accepted-decision changes, owner-only judgements, required forge approvals, deployment, and repository red lines |

`/ff` may prepare and update a PR, push its branch, and merge only when every configured gate passes and the forge permits it. The routine PR review can be performed by the configured independent reviewer; owner-only decisions and deployment still stop automation.

## Branch and commit conventions

Create issue-linked branches as `<type>/<issue-number>-<kebab-case-slug>`, for example `feat/47-export-report` or `fix/183-session-resume`. Allowed types are `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`, and `revert`. Do not use a tool or agent name as a branch prefix.

Use Conventional Commit messages in the form `<type>(<optional-scope>): <description>`. Husky runs commitlint for each commit and validates the branch before push. CI repeats both checks for pull requests because local hooks can be bypassed. Pull requests must link their issue with `Closes #<number>` in the body.

The `verify` CI check reports violations. It is not a merge gate until GitHub requires that check in the repository's ruleset; this repository currently cannot expose branch protection settings through its GitHub plan.

## Work in progress

**WIP cap**: 3 · **Query used at pickup**: `gh issue list --label "status:in-progress" --state open` + `gh issue list --label "status:in-review" --state open` · **Sprint commitment**: Not used · **Board**: GitHub Issues with `status:` labels; no separate board is part of this scaffold. Column limits are not set by any tool; the WIP cap is enforced by the `wip-query` role at pickup.

## Roles

Who decides the judgement items above. A role is a person, a team, or a named responsibility, not a job title for its own sake.

| Role | Who | Decides |
| --- | --- | --- |
| developer | Solo developer (Juan García Carmona) | All Definition of Done judgement dimensions: acceptance criteria, implementation completeness, tests adequacy, security exposure, architecture conformance, documentation, spec consistency, human review, known limitations |

## Installed roles

Project-owned role adaptations are sourced from `.agents/skills/`; `.claude/skills/` links back to those sources. Upstream lifecycle and shared skills are restored from `skills-lock.json`. These role adaptations follow the shared Agentic SDLC 0.2.2 practices adopted across the repositories on 2026-10-19.

| Role skill | Adapted from | Installed | Projected to |
| --- | --- | --- | --- |
| `read-work-item` | Agentic SDLC GitHub Issues template | 2026-10-19 | Codex, Claude Code |
| `read-work-item-links` | Agentic SDLC GitHub Issues template | 2026-10-19 | Codex, Claude Code |
| `create-work-item` | Agentic SDLC GitHub Issues template | 2026-10-19 | Codex, Claude Code |
| `update-work-item` | Agentic SDLC GitHub Issues template | 2026-10-19 | Codex, Claude Code |
| `transition-work-item` | Agentic SDLC GitHub Issues template | 2026-10-19 | Codex, Claude Code |
| `comment-work-item` | Agentic SDLC GitHub Issues template | 2026-10-19 | Codex, Claude Code |
| `create-branch` | Agentic SDLC branch template | 2026-10-19 | Codex, Claude Code |
| `checkout-branch` | Agentic SDLC branch template | 2026-10-19 | Codex, Claude Code |
| `open-pull-request` | Agentic SDLC GitHub template | 2026-10-19 | Codex, Claude Code |
| `inspect-pull-request` | Agentic SDLC GitHub template | 2026-10-19 | Codex, Claude Code |
| `update-pull-request` | Agentic SDLC GitHub template | 2026-10-19 | Codex, Claude Code |
| `merge-pull-request` | Agentic SDLC GitHub template | 2026-10-19 | Codex, Claude Code |
| `inspect-ci-result` | Agentic SDLC GitHub Actions template | 2026-10-19 | Codex, Claude Code |
| `wip-query` | Agentic SDLC GitHub Projects template | 2026-10-19 | Codex, Claude Code |

## Provided roles

No specification framework is installed. No plan-artifact roles are provided externally; delivery planning uses native plan mode and durable Markdown plans under `docs/delivery/plans/` when a work item needs one.

## Amendment log

The `review` capability proposes changes to this file from delivery evidence. Every amendment is shown as a diff and confirmed by a human; nothing edits this file silently.

| Date | Section | Change | Why |
| --- | --- | --- | --- |
| 2026-10-19 | All | Initial adoption of the shared lifecycle | Align SoFa with the Agentic SDLC practices used across the other repositories |
