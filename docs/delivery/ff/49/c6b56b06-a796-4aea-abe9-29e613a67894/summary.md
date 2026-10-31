# Fast Forward run — SoFa #49

- Status: running; plan reviewed, final-head recheck pending
- Started: 2026-10-31T01:02:56Z
- Starting commit: `2a5610d390bd2acbbdc702ad43dd65cfe30c4ca5`
- Executor: `gpt-6.1-terra`
- Independent reviewer selected for a future review: `gpt-6-astra`
- Correction rounds: at most 3 per gate
- Issue: [SoFa #49](https://github.com/juangcarmona/sofa/issues/49)

## Resumed refinement — 2026-10-31

The owner requested continuation after PR #83. The [new refinement run](https://github.com/juangcarmona/sofa/actions/runs/41188212887) evaluated source `824131a785d4edd97adbe84850d5c9a26756a0d4`, published a [passed Ready result](https://github.com/juangcarmona/sofa/issues/49#issuecomment-6541654459), and released its reservation. The `future` label holds automatic implementation while ADR-0007 and ADR-0008 remain Proposed. The first [proposal run](https://github.com/juangcarmona/sofa/actions/runs/41188613274) was requested once; its blocked result and successful continuation are recorded below. This evidence does not approve the proposed architecture or implementation.

## Proposal — blocked, compatibility evidence collected

The [proposal run](https://github.com/juangcarmona/sofa/actions/runs/41188613274) completed successfully as a workflow but [published a blocked planned gate](https://github.com/juangcarmona/sofa/issues/49#issuecomment-6541743353). It could not access the package identity guide in its checkout and asked for exact dependency pins. It released its reservation and retained the `future` implementation hold.

The exact adopted Lighthouse 0.12.0 package contains the guide, SHA-256 `fa3d111def4ddd98da5e9795f3e019f9a9e33c9f7c94f9d4c100fc98935adabb`. A disposable Keycloak 26.8.1 container published OIDC discovery with PKCE S256, and an ASP.NET Core OIDC 10.0.14 project compiled against .NET 10. These probes do not demonstrate a complete sign-in or approve the proposed architecture. The [owner reply](https://github.com/juangcarmona/sofa/issues/49#issuecomment-6541763350) supplies the sources and candidates; [the resumed proposal](https://github.com/juangcarmona/sofa/actions/runs/41189371172) passed as correction round 1 and published [PR #84](https://github.com/juangcarmona/sofa/pull/84). Independent review and owner decisions remain outstanding.

## Independent review and verification

The independent `gpt-6-astra` reviewer approved the proposal at `06a0e6b33c3b07ba28d9c8ec141fedc44625c6e8`, subject to owner decisions on the proposed architecture and session policy. The reviewer found stale present-tense wording in this historical record. Repository CI also failed Markdown lint on the generated OpenSpec files. Both issues were corrected; the OpenSpec content is unchanged apart from whitespace and link formatting. `openspec validate issue-49-proposal --strict` and `npm run verify` passed locally. The independent reviewer must recheck the final PR head; neither approval nor green checks authorize implementation.

## Earlier follow-up after PR #81 — 2026-10-31

[SoFa PR #81](https://github.com/juangcarmona/sofa/pull/81) merged at `3ff107978412818bff2a85993516493f71d23296`. `main` now contains Lighthouse CLI 0.12.0, SDLC 0.16.0 and the regenerated official workflows. At that point the host Docker cleanup probe was successful, but no fresh `refine` run had demonstrated end-to-end publication on the new version. Issue #49 was open with `refine` and without `agent-working`; PR #82 awaited a product decision. Subsequent events are recorded above and below. A green workflow does not itself accept a proposal or pass a gate.

Later on 2026-10-31, Juan explicitly accepted the Product Change and requested that PR #82 be merged. `prodshape change apply CHG-AUTHENTICATED-OPERATIONS-001` added 19 and modified 9 accepted product artifacts on the PR branch, then archived the change as applied. ADR-0007/0008 remain Proposed; their architectural decisions were not explicitly accepted. The accepted model and changed architecture citations validate. A fresh `refine` is still needed after the product baseline reaches `main`; the old blocked result is unchanged.

The retained Lighthouse envelope redacts an infrastructure host name; the linked Actions run is the source of the original artifact.

## Initial finding

The issue references active Product Change `CHG-AUTHENTICATED-OPERATIONS-001` and proposed ADR-0007/0008 paths that are absent from this starting `main`. The accepted baseline contains `FR-AUTHORIZED-ENTRY` and ADR-0006, which allow an authenticated adapter to submit explicit actions through GitHub while keeping SoFa out of execution authority. Product scope and architectural decisions remain human-owned. A later local-worktree audit found the complete draft change and both proposed ADRs as uncommitted files in `docs/48-product-operations`; the initial search of Git refs had missed them. They are now copied unchanged into [PR #82](https://github.com/juangcarmona/sofa/pull/82) for review, with the original dirty worktree preserved.

## Stage evidence

### Refine — blocked

Added the existing Lighthouse `refine` request label once. Router run [41153445104](https://github.com/juangcarmona/sofa/actions/runs/41153445104) accepted the owner event and dispatched [refine run 41153474276](https://github.com/juangcarmona/sofa/actions/runs/41153474276). The issue records the dispatch receipt as comment 6536906830. This is dispatch evidence, not a Ready result.

Lighthouse produced a validated result artifact with `status=blocked` at the Ready gate. It found the cited Product Change `CHG-AUTHENTICATED-OPERATIONS-001` and proposed ADR-0007/0008 absent from the supplied `main` checkout. At dispatch time, they existed only as uncommitted files in another local worktree and were therefore unavailable to the worker. The worker also marked OIDC guidance unavailable because it did not find `assets/profiles/identity.md` in its checkout. I verified the adopted `@juangcarmona/lighthouse` 0.11.6 registry package and lockfile integrity; its installed package contains that guide. SoFa's dependency has the guide, while the Lighthouse worker checkout did not expose it. The artifact says the issue is not ready for proposal until Juan supplies or explicitly reconciles the missing product/architecture references.

The proposed issue result comment was **not** published: cleanup failed with `EACCES: permission denied, unlink '/home/juan/.local/share/lighthouse-runtime/runner-sofa/_work/_temp/lighthouse-engine-t2WcCC/output/execution/engine.json'`, then the publisher's “Require an executed agent” guard failed. The execution directory is owned by UID/GID 20003 with mode 755, which prevents the host cleanup process from unlinking the container-owned file. I traced this to version skew: the worker artifact used Lighthouse SDLC 0.15.2 at harness revision `cee9f62`, whose cleanup did not restore ownership of read-only execution receipts. Lighthouse PR #200 (`106d118`) added ownership restoration, and the later published SDLC 0.16.0 includes it. I reproduced the exact chown cleanup successfully on this host with the pinned engine image and UID mapping. SoFa must activate its upgrade in PR #81 before retrying. The envelope and runtime receipt are preserved alongside this summary.

The reservation-release step succeeded; `agent-working` is gone. The issue still has the `refine` request label and the failed-execution receipt. I did not run a correction because the missing product/architecture references and their disposition require the owner's input; refinement also cannot be retried safely until the runner cleanup/input defect is addressed. `propose`, `implement`, `integrate` and `review` were not run.

The executed Lighthouse worker resolved to `openai/gpt-6.1-sol`; its provider identity was unverified in the runtime receipt. No independent review was needed because no proposal was produced. The configured Codex executor is `gpt-6.1-terra`; the independent reviewer selected for a future review is `gpt-6-astra`, distinct from both.

## Follow-up diagnosis

The exact run used Lighthouse SDLC 0.15.2 at `cee9f62f8161771ea16366c6360b82e0efad045c`. Its worker created `output/execution/engine.json` as UID 20003 and later attempted to remove the temporary tree as the runner user, without first reclaiming ownership. Lighthouse main's merged PR #200 (`106d11843329`) adds `restoreWorkspaceOwnership` before removal. Lighthouse CLI 0.12.0 and SDLC 0.16.0 have since been published; [SoFa PR #81](https://github.com/juangcarmona/sofa/pull/81) installs them through the official CLI and workflow installer. A local Docker probe with the current pinned engine image successfully changed the receipt tree from UID 20003 back to the runner UID; this confirms host Docker can perform the cleanup. The remaining prerequisites to resume are activation of PR #81 and the owner's explicit disposition of the Product Change and ADRs in draft PR #82. Neither PR is a Ready result.

## Local validation

The first `npm run verify` stopped at Markdown lint because this summary used a bare issue URL. I corrected the link and reran the complete command successfully: 209 Markdown files, 27 Node tests, secret scan, ProductShape validation/integrity/doctor, 12 ArcShape sections, 38 .NET runtime tests and 4 Playwright browser tests passed. `run.json` parses as JSON. This validates the local evidence package; it does not change the blocked Lighthouse Ready gate or the failed publication.
