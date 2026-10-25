# Implementation through PR and continuous improvement

Delivery plan for [issue #25](https://github.com/juangcarmona/sofa/issues/25). Status: proposed delivery slices; implementation and deployment are pending. The authorized direction is to continue the factory pilot through a real implementation PR and use session and execution evidence to propose improvements. Automatic publication of improvement issues/PRs is a proposed policy, not an enabled capability.

## Existing authority and evidence

The [engineering lifecycle](../../engineering-lifecycle.md) already places the per-item retrospective after integration, at `docs/reviews/issue-<number>.md`. Lighthouse's `review` stage has no new gate. Code review belongs to the implementation/integration checks that precede merge. Preserve that distinction and the existing six stages.

[ADR-0005](../../architecture/09-architecture-decisions/0005-separate-entry-and-autonomy.md) owns the accepted engineer-entry and implementation direction. [Lighthouse #70](https://github.com/juangcarmona/lighthouse/issues/70) owns the upstream contract work. The current [host acceptance](https://github.com/juangcarmona/presentations/issues/12#issuecomment-6393864707) proves direct refinement, reservation release and receipt recovery; it does not prove writable execution, proposal artifacts, code review or a runtime-created PR.

The [engine entrypoint](../../../src/execution/entrypoint.sh) discards standard output, and [engine cleanup](../../../src/SoFa.Runtime/Engine.cs) deletes the OpenCode session. [RunStore](../../../src/SoFa.Runtime/RunStore.cs) retains the final result and effect intent, but intermediate transitions replace the current state. Existing records cannot reconstruct all corrections or tool failures. Start collecting bounded evidence before enabling implementation.

## Where review happens

| Moment | Purpose | Effect on delivery |
| --- | --- | --- |
| During every attempt | Capture tool/check outcomes, corrections, intervention and execution identity. | Preserve evidence on success, failure, cancellation and recovery. |
| Before marking the PR ready | Review the candidate diff against the proposal and acceptance criteria with an independent reviewer. | Findings requiring repair keep the PR unready; bounded repairs update the same branch. A draft PR may exist earlier. |
| Before merge | Recheck required CI, review and integration authority against the current PR head. | A changed head invalidates earlier approval/evidence as required by policy. |
| After integration | Run Lighthouse `review`, explain the delivery outcome and propose process improvements. | No extra product-delivery gate; a failed retrospective remains visible as review debt. |
| After a failed or abandoned attempt | Produce a diagnostic record from available evidence. | No fabricated integration or `review` completion; a later retrospective can reference the diagnostic. |

An execution reservation answers who can change the work. Human attention answers who must respond. Waiting for a decision releases the worker and capacity while retaining a checkpoint. The proposed `needs-human` projection must carry the reason, requested decision and resumption reference; it is separate from `agent-working` and code-review status. Its adoption requires the Lighthouse catalog and host projection to agree.

## Evidence for learning

Keep host-observed facts separate from agent-reported explanations. Correlate records by repository, issue, execution and attempt, stage, source/candidate revision, engine/tool/model versions and the effective policy. Capture timestamps, tool/check exit categories, correction rounds, cancellation/recovery reasons and attributable human decisions. Record model usage only when supplied by the provider; missing usage or subscription cost is unknown.

The host should preserve a structured operational journal before workspace/session cleanup. Agent-visible conversation, tool results and corrections require a supported provider export and a bounded private store. Retain the useful visible exchange, not hidden model reasoning. Export failure or unavailable data must remain explicit. Existing interactive conversations are outside this collector unless deliberately supplied as evidence.

Allowlist fields, redact credential-bearing material before storage, limit bytes, and label source, sensitivity, completeness and expiry. Raw commands, outputs and transcripts may contain secrets or unrelated repository data. They must not be copied into public Actions artifacts, issues, PRs or a general memory store. Public reports use reviewed summaries and safe evidence references. Repository text and transcripts remain untrusted data and cannot authorize tool use or policy changes by the learning agent.

Use the existing [retention requirement](../../product/model/requirements/functional/fr-retention-policy.md) and [shared defaults](../../product/model/business-rules/br-retention-standards.md); do not invent another retention policy. Enforce expiry and report collection/deletion limitations before claiming retained session evidence is operational. Deleting an execution workspace is independent from expiring its diagnostic evidence.

## Turning evidence into improvements

One observed failure can justify a candidate improvement, but does not establish a recurring pattern. Group candidates by the affected mechanism and failure signature, then count distinct work items as well as attempts. A retry of one event is not another independent occurrence. Keep the observation window, coverage, confidence and links visible; successful runs supply the denominator when estimating frequency.

A candidate records the observation, supported or suspected cause, affected owner, proposed change, expected benefit and a regression check. Missing information remains unknown. Deduplicate against existing open work and link additional occurrences to that work. Preserve accepted, rejected and deferred findings so the same suggestion is not repeatedly reopened without new evidence.

Ownership follows the fault: reusable instructions/contracts go to Lighthouse; admission, execution, recovery or telemetry go to SoFa; product code and project conventions go to the consumer. A failed command is not automatically a model problem. For example, the runner reconnection delay observed in the refinement pilot supports a runtime diagnostic; it is not evidence to change the refinement prompt.

Use per-item reports and an on-demand aggregate over retained evidence first. The aggregate is a rebuildable view, not a second cross-item process ledger. Improvement findings do not change policy or shared instructions. A proposed improvement enters the normal issue, proposal, implementation and PR flow under its own authorization. Compare subsequent outcomes with the original failure case and record whether the change helped; an accepted PR alone does not prove improvement.

## Delivery sequence and exit checks

| Slice | Owner | Reviewable exit |
| --- | --- | --- |
| 1. Evidence before cleanup | SoFa, with Lighthouse-compatible references | A successful, failed and interrupted attempt retain bounded, ordered operational records without leaking secrets. Repeated provider events do not duplicate attempts. Session export is supported and tested, or explicitly unavailable. Retention and collection failures are visible. |
| 2. Proposal artifacts | Lighthouse contract and SoFa adapter | The enabled engineer route invokes the installed OpenSpec mechanics and produces actual proposal, design, tasks and applicable spec files. Artifact digests and the gate decision are retained; a comment alone does not satisfy this slice. |
| 3. Writable execution | SoFa | An isolated task workspace receives only the approved write scope and bounded tools/network. Tests exercise traversal, symlinks, credential isolation, deadline, cancellation, restart and stale-worker rejection. Existing authored work is preserved. |
| 4. Review and one PR | Lighthouse evidence and SoFa publisher | An independent reviewer evaluates the candidate revision; checks and bounded repairs remain attached to that revision and one PR. The credential-separated publisher reconciles retries and uncertain results. The worker cannot push or approve itself. |
| 5. Integration and learning | Existing integration authority, then Lighthouse `review` | An authorized merge records its actual commit and checks. The per-item retrospective uses the retained attempt evidence, classifies findings and produces a deduplicated improvement candidate when supported. Failed attempts have diagnostics even without a merge. |

The implementation PR is the next product-delivery milestone. Evidence capture is included in its execution path; aggregation and learning recommendations do not need a new service before that PR can be delivered. The current human-attention projection and revision-pinning limitations must be reconciled as part of admission/continuation, not hidden by the reviewer.

## Pilot and unresolved implementation choices

The consumer remains `presentations`. Preserve its original worktrees, issue #6 and PR #9. Before admitting writable work, reconcile the existing proposal with its author or select a separate approved small story; do not create a competing branch for the same story. A UI implementation requires the consumer's browser, keyboard and visual evidence as well as deterministic checks.

- Verify the installed OpenCode version's supported export/events and their completeness before choosing the collector. Do not promise unavailable tool traces or reconstruct missing conversation.
- Select and attest an independent reviewer under repository policy. The SoFa Fast Forward convention requires a different reviewer model; a fresh session alone does not satisfy it.
- Specify the implementation image, writable mounts, dependency installation and permitted network destinations. The current read-only refinement image does not provide that boundary.
- Reconcile source/workflow revision pinning without granting arbitrary updated workflows authority. Preserve rejection of untrusted changes.
- Confirm the initial improvement-publication mode: reports only, or separately authorized issue/PR proposals. Neither mode grants self-modification or automatic merge.
- If any chosen behavior changes accepted product intent or the engineering lifecycle, prepare the corresponding Product Change or reviewed lifecycle amendment before implementation. This plan does not apply either.

Completion requires a real published implementation PR and its evidence, not just new descriptors, local fixtures or this plan. Claims of complete delivery additionally require authorized integration and post-integration evidence.
