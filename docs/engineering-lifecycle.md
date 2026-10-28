# Engineering lifecycle

**Project**: SoFa · **Adopted**: 2026-10-19 · **Last amended**: 2026-10-27

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

Lighthouse owns workflow labels, stage transitions and gate results. SoFa does not define a parallel lifecycle label family. The installed Lighthouse SDLC contract and its generated router are authoritative.

| Label family | Allowed values | Meaning |
| --- | --- | --- |
| Work type | `bug`, `enhancement`, `documentation` | Optional classification of the work |
| Lighthouse request | `triage`, `refine`, `propose`, `implement` | Owner-added request labels consumed by the installed Lighthouse router |
| Lighthouse state/evidence | Labels declared by the installed catalog, including `triaged`, `ready`, `needs-info`, `waiting-review`, reservations and PR projections | Current Lighthouse result and execution evidence; Lighthouse owns the meanings and transitions |

Do not add or maintain `status:*` labels. An issue waiting in the backlog has no active Lighthouse request label; GitHub open/closed state remains authoritative for whether the issue is open. Do not invent a Lighthouse request label to imitate a status. Labels such as `duplicate`, `good first issue`, and `help wanted` remain separate metadata.

When creating an issue, validate the required Ready dimensions below (mark inapplicable dimensions `N/A` with a reason), check for likely duplicates using the title and distinctive terms, and add no more than one existing work-type label. Add a Lighthouse request label separately when the owner requests that stage.

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
| 10 | Human review | Review every DoD dimension on the implementation PR before integration | judgement | developer |
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

The checks cover the web observer and retained runtime. Required checks must run on the candidate; missing tooling is reported as unavailable.

## Lighthouse stages and gates

The installed Lighthouse contract defines stages, request labels, gates, worker behavior and evidence. This table records only how SoFa uses that contract; it does not redefine stages or introduce another status system.

| Lighthouse stage | Request | Lighthouse result / effect | Owner boundary |
| --- | --- | --- | --- |
| `triage` | Owner adds `triage` | Lighthouse publishes `triaged` on a passed gate | Owner decides whether to request refinement |
| `refine` | Owner adds `refine` | Lighthouse publishes `ready` on a passed gate, or `needs-info` when blocked | A passed gate does not approve a proposal or merge |
| `propose` | Owner adds `propose` after readiness | Lighthouse publishes proposal evidence and `waiting-review` | Owner reviews the proposal; it does not authorize implementation |
| `implement` | Owner adds `implement`, or Lighthouse's configured continuation requests it | Lighthouse publishes the implementation PR and its `pr-draft` / `pr-ready` projection | Owner retains PR acceptance and merge decisions |
| `integrate` | Lighthouse completion workflow and explicit owner merge decision | GitHub records the merge and closes a linked issue when requested | The merge decision remains explicit |
| `review` | Lighthouse completion workflow after integration | Lighthouse records the retrospective result | No SoFa lifecycle label transition |

The `transition-work-item` capability follows the Lighthouse request and result contract. It must not swap `status:*` labels or infer a gate from a label name alone. An enabled `/ff` invocation authorizes only routine writes permitted by Lighthouse; owner decisions and required approvals remain explicit.

Pull requests are not work items for this lifecycle transition workflow. The issue API exposes an issue-shaped record for a pull request; the skill must inspect the `pull_request` field and refuse it. Reopen, block, and deprioritise remain human-only decisions and are never performed by the transition skill. If a work item is closed, that GitHub state is reported independently from its lifecycle label.

### Transitions no command ever fires

| Transition | Meaning | Who fires it |
| --- | --- | --- |
| Reopen | Reversing Done back to an earlier state | human only |
| Block | Item is blocked by a dependency or external factor | human only |
| Deprioritise | Item moved back to Backlog from a later state | human only |

## Work item fields

Fields required to create or update a GitHub issue. Lighthouse owns its stage inputs, gate results and evidence envelope.

| Field | Identifier | Required by | Notes |
| --- | --- | --- | --- |
| Lighthouse request/result | Lighthouse-declared action labels and authenticated stage evidence | Lighthouse stages | Do not introduce a parallel SoFa status label |
| Work type | one of `bug`, `enhancement`, `documentation` | issue creation / classification | Optional; at most one, and independent of lifecycle |
| Acceptance criteria | `## Acceptance Criteria` section in issue body | `/refine` (Ready gate) | Markdown heading in the issue body |
| Work item link | `Closes #<number>` in PR body | `/implement` | Placed in the PR body, not the title |

## Label and WIP conventions

Use the labels declared by the installed Lighthouse catalog for stage requests, state, reservation and PR projection. Keep work-type and other metadata labels independent. Do not recreate Lighthouse semantics with `status:*` labels. New issues start open without a lifecycle label; the owner adds a Lighthouse request label to start a stage.

The GitHub workflow retains reports and retrospectives on the protected `process/retrospectives` branch and links them from the issue and PR. Older local reviews remain historical evidence.

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

Create issue-linked branches as `<type>/<issue-number>-<kebab-case-slug>`, for example `feat/47-export-report` or `fix/183-session-resume`. Allowed types are `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`, and `revert`. The Lighthouse publisher owns its `lighthouse/implement-<issue>-<run>` and `lighthouse/propose-<issue>-<run>` branches; manual work uses the conventions above.

Use Conventional Commit messages in the form `<type>(<optional-scope>): <description>`. Husky runs commitlint for each commit and validates the branch before push. CI repeats both checks for pull requests because local hooks can be bypassed. Pull requests must link their issue with `Closes #<number>` in the body.

The adopted GitHub profile requires `verify` on `main`. The publisher checks the effective repository rules before merging; a successful agent result does not override them.

## Work in progress

**WIP cap**: 3 · **Query used at pickup**: count distinct open issues with Lighthouse `implement`, `pr-draft`, or `pr-ready` labels · **Sprint commitment**: Not used · **Board**: GitHub Issues; no separate board is part of this scaffold. Column limits are not set by any tool; the WIP cap is enforced by the `wip-query` role using current Lighthouse implementation requests and PR projections.

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
| 2026-10-28 | Issue labels, stages, gates and WIP | Use the installed Lighthouse SDLC contract as the workflow authority; remove the parallel `status:*` convention | Lighthouse 0.15.0 supplies request labels, gates, state evidence, reservations and PR projections |
