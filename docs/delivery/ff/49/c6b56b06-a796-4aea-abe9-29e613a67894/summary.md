# Fast Forward run — SoFa #49

- Status: blocked
- Started: 2026-10-31T01:02:56Z
- Starting commit: `2a5610d390bd2acbbdc702ad43dd65cfe30c4ca5`
- Executor: `gpt-6.1-terra`
- Independent reviewer selected for a future review: `gpt-6-astra`
- Correction rounds: at most 3 per gate
- Issue: [SoFa #49](https://github.com/juangcarmona/sofa/issues/49)

## Initial finding

The issue references active Product Change `CHG-AUTHENTICATED-OPERATIONS-001` and proposed ADR-0007/0008 paths that are absent from this starting `main`. The accepted baseline contains `FR-AUTHORIZED-ENTRY` and ADR-0006, which allow an authenticated adapter to submit explicit actions through GitHub while keeping SoFa out of execution authority. Product scope and architectural decisions remain human-owned. A later local-worktree audit found the complete draft change and both proposed ADRs as uncommitted files in `docs/48-product-operations`; the initial search of Git refs had missed them. They are now copied unchanged into [draft PR #82](https://github.com/juangcarmona/sofa/pull/82) for review, with the original dirty worktree preserved.

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
