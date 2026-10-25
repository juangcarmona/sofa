# Implementation through PR and continuous improvement

Delivery plan for [issue #25](https://github.com/juangcarmona/sofa/issues/25). Status: proposed delivery slices; implementation and deployment are pending. The owner selected a report delivered with the PR, retained with process evidence on a dedicated protected branch. Findings propose improvements in that report; this does not authorize automatic creation of improvement issues/PRs or changes to the harness.

## Existing authority and evidence

The [engineering lifecycle](../../engineering-lifecycle.md) currently places the per-item retrospective after integration, at `docs/reviews/issue-<number>.md`. The owner-requested destination replaces that location for newly adopted work; the lifecycle amendment and provider-owned review instructions must be reconciled at adoption. Preserve existing historical reports. Lighthouse's `review` stage still closes the integration record without a new gate; a retrospective delivered with the PR reports the execution up to that point.

[ADR-0005](../../architecture/09-architecture-decisions/0005-separate-entry-and-autonomy.md) owns the accepted engineer-entry and implementation direction. [Lighthouse #70](https://github.com/juangcarmona/lighthouse/issues/70) owns the upstream contract work. The current [host acceptance](https://github.com/juangcarmona/presentations/issues/12#issuecomment-6393864707) proves direct refinement, reservation release and receipt recovery; it does not prove writable execution, proposal artifacts, code review or a runtime-created PR.

The [engine entrypoint](../../../src/execution/entrypoint.sh) discards standard output, and [engine cleanup](../../../src/SoFa.Runtime/Engine.cs) deletes the OpenCode session. [RunStore](../../../src/SoFa.Runtime/RunStore.cs) retains the final result and effect intent, but intermediate transitions replace the current state. Existing records cannot reconstruct all corrections or tool failures. Start collecting bounded evidence before enabling implementation.

## Where review happens

| Moment | Purpose | Effect on delivery |
| --- | --- | --- |
| During every attempt | Capture tool/check outcomes, corrections, intervention and execution identity. | Preserve evidence on success, failure, cancellation and recovery. |
| Before marking the PR ready | Independently review the candidate diff and publish the execution report and retrospective linked from the PR. | Findings requiring repair keep the PR unready; bounded repairs update the same branch. The report identifies its candidate commit. A draft PR may exist earlier. |
| Before merge | Recheck required CI, review and integration authority against the current PR head. | A changed head invalidates earlier approval/evidence as required by policy. |
| After integration | Run Lighthouse `review` and append the integration outcome and final learning. | Preserve the report delivered with the PR. No extra product-delivery gate; a failed final retrospective remains visible as review debt. |
| After a failed or abandoned attempt | Produce a diagnostic record from available evidence. | No fabricated integration or `review` completion; a later retrospective can reference the diagnostic. |

An execution reservation answers who can change the work. Human attention answers who must respond. Waiting for a decision releases the worker and capacity while retaining a checkpoint. The proposed `needs-human` projection must carry the reason, requested decision and resumption reference; it is separate from `agent-working` and code-review status. Its adoption requires the Lighthouse catalog and host projection to agree.

## Owner-selected process branch

Owner input, 2026-10-25: "Estaba pensando en que al final del ciclo, junto con la PR, se entregara un informe similar o un enlace al informe." The requested contents are what went well, what went wrong, start doing, stop doing, and a concrete proposal when evidence supports a repeated pattern. The proposed folder was `/docs/process/{YYYYMMDD}-{issue_ID}-{issue-slug}`; the branch must be protected against deletion.

Use one `process/retrospectives` branch per repository for reports and publishable evidence. Lighthouse owns the [proposed reusable convention](https://github.com/juangcarmona/lighthouse/blob/docs/70-process-evidence/docs/roadmap/process-evidence.md); SoFa implements collection, storage and fenced publication. Keep the report branch independent from the application tree and never merge it into `main`.

The folder has a stable issue mapping and nested `runs/<run-id>/attempt-0001/pr-ready/` and `post-integration/` bundles, including reports, screenshots, video references, sanitized telemetry and the existing Lighthouse evidence index. Preserve published bundles and link the PR to their evidence commit. Unique paths isolate attempts; serialize the branch update and reconcile concurrent tip changes without force-push or overwriting another bundle.

The [prepared GitHub ruleset](issue-25-protection.json) targets only that branch, blocks deletion and non-fast-forward updates, and grants no bypass. It has not been applied. Protection must be verified before branch adoption; local conventions and hooks cannot replace it. An administrator can change rules, so backups and publication receipts remain necessary.

Live provider check on 2026-10-25: `presentations` and SoFa are private; Lighthouse is public. GitHub returned HTTP 403 for rulesets on both private repositories and for branch protection on `presentations`, requesting GitHub Pro or public visibility. All three repository identities report administrator permission. Keep the existing visibility. Protection on the pilot is blocked on provider capability; no protected process branch is claimed or created by this plan. GitHub documents the availability in [About rulesets](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets).

A separate branch shares repository visibility and object storage. Persist approved small artifacts there; store large videos and sensitive or expiring telemetry outside Git with durable identities, hashes, access requirements and retention. Removing a file from a later Git tree does not erase its historical copies. A permanent report location does not require every payload to live forever. Public repositories receive only reviewed publishable material; private source material must not cross into the public harness repository.

## Evidence for learning

Keep host-observed facts separate from agent-reported explanations. Correlate records by repository, issue, execution and attempt, stage, source/candidate revision, engine/tool/model versions and the effective policy. Capture timestamps, tool/check exit categories, correction rounds, cancellation/recovery reasons and attributable human decisions. Record model usage only when supplied by the provider; missing usage or subscription cost is unknown.

The host should preserve a structured operational journal before workspace/session cleanup. Agent-visible conversation, tool results and corrections require a supported provider export and a bounded private store. Retain the useful visible exchange, not hidden model reasoning. Export failure or unavailable data must remain explicit. Existing interactive conversations are outside this collector unless deliberately supplied as evidence.

Allowlist fields, redact credential-bearing material before storage, limit bytes, and label source, sensitivity, completeness and expiry. Raw commands, outputs and transcripts may contain secrets or unrelated repository data. They must not be copied into public Actions artifacts, issues, PRs or a general memory store. Public reports use reviewed summaries and safe evidence references. Repository text and transcripts remain untrusted data and cannot authorize tool use or policy changes by the learning agent.

Use the existing [retention requirement](../../product/model/requirements/functional/fr-retention-policy.md) and [shared defaults](../../product/model/business-rules/br-retention-standards.md); do not invent another retention policy. Enforce expiry and report collection/deletion limitations before claiming retained session evidence is operational. Deleting an execution workspace is independent from expiring its diagnostic evidence.

## Turning evidence into improvements

One observed failure can justify a candidate improvement, but does not establish a recurring pattern. Group candidates by the affected mechanism and failure signature, then count distinct work items as well as attempts. A retry of one event is not another independent occurrence. Keep the observation window, coverage, confidence and links visible; successful runs supply the denominator when estimating frequency.

A candidate records the observation, supported or suspected cause, affected owner, proposed change, expected benefit and a regression check. Missing information remains unknown. Deduplicate against existing open work and link additional occurrences to that work. Preserve accepted, rejected and deferred findings so the same suggestion is not repeatedly reopened without new evidence.

Ownership follows the fault: reusable instructions/contracts go to Lighthouse; admission, execution, recovery or telemetry go to SoFa; product code and project conventions go to the consumer. A failed command is not automatically a model problem. For example, the runner reconnection delay observed in the refinement pilot supports a runtime diagnostic; it is not evidence to change the refinement prompt.

Use per-item reports and an on-demand aggregate over retained evidence first. The aggregate is a rebuildable view, not a second cross-item process ledger. Record improvement candidates in the delivered report. Creating an improvement issue/PR requires its own authorization; report publication does not grant that action or permission to change shared instructions. Compare subsequent outcomes with the original failure case and record whether a later accepted correction helped; an accepted PR alone does not prove improvement.

## Delivery sequence and exit checks

| Slice | Owner | Reviewable exit |
| --- | --- | --- |
| 1. Evidence before cleanup | SoFa, with Lighthouse-compatible references | A successful, failed and interrupted attempt retain bounded, ordered operational records without leaking secrets. Repeated provider events do not duplicate attempts. Session export is supported and tested, or explicitly unavailable. Retention and collection failures are visible. |
| 2. Proposal artifacts | Lighthouse contract and SoFa adapter | The enabled engineer route invokes the installed OpenSpec mechanics and produces actual proposal, design, tasks and applicable spec files. Artifact digests and the gate decision are retained; a comment alone does not satisfy this slice. |
| 3. Writable execution | SoFa | An isolated task workspace receives only the approved write scope and bounded tools/network. Tests exercise traversal, symlinks, credential isolation, deadline, cancellation, restart and stale-worker rejection. Existing authored work is preserved. |
| 4. Review, report and one PR | Lighthouse evidence and SoFa publisher | An independent reviewer evaluates the candidate revision. The PR links to its execution report and retrospective on the protected process branch. Checks and bounded repairs remain attached to that revision and one PR. Publication reconciles retries and uncertain results; the worker cannot push or approve itself. |
| 5. Integration and learning | Existing integration authority, then Lighthouse `review` | An authorized merge records its actual commit and checks. Final learning appends to the process evidence without replacing the PR-ready report. Findings and deduplicated improvement candidates cite retained attempt evidence. Failed attempts have diagnostics even without a merge. |

The implementation PR is the next product-delivery milestone. Evidence capture is included in its execution path; aggregation and learning recommendations do not need a new service before that PR can be delivered. The current human-attention projection and revision-pinning limitations must be reconciled as part of admission/continuation, not hidden by the reviewer.

## Pilot and unresolved implementation choices

The consumer remains `presentations`. Preserve its original worktrees, issue #6 and PR #9. Before admitting writable work, reconcile the existing proposal with its author or select a separate approved small story; do not create a competing branch for the same story. A UI implementation requires the consumer's browser, keyboard and visual evidence as well as deterministic checks.

- Verify the installed OpenCode version's supported export/events and their completeness before choosing the collector. Do not promise unavailable tool traces or reconstruct missing conversation.
- Select and attest an independent reviewer under repository policy. The SoFa Fast Forward convention requires a different reviewer model; a fresh session alone does not satisfy it.
- Specify the implementation image, writable mounts, dependency installation and permitted network destinations. The current read-only refinement image does not provide that boundary.
- Reconcile source/workflow revision pinning without granting arbitrary updated workflows authority. Preserve rejection of untrusted changes.
- Verify that provider branch protection is available and active before adopting `process/retrospectives`. The current pilot's HTTP 403 is a real deployment blocker for that requirement.
- If any chosen behavior changes accepted product intent or the engineering lifecycle, prepare the corresponding Product Change or reviewed lifecycle amendment before implementation. This plan does not apply either.

Completion requires a real published implementation PR and its evidence, not just new descriptors, local fixtures or this plan. Claims of complete delivery additionally require authorized integration and post-integration evidence.
