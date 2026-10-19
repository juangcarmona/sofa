# Issue 6 retrospective: Repository attention

Date: 2026-10-19

## Outcome and evidence

- [Issue #6](https://github.com/juangcarmona/sofa/issues/6): closed, `status:done`.
- [PR #7](https://github.com/juangcarmona/sofa/pull/7): merged as `dcb0d6459ba34a59e6eff7cf9d1945786c0cafbe` after Juan explicitly approved integration and this retrospective.
- [Final PR CI](https://github.com/juangcarmona/sofa/actions/runs/39262037959): passed on `0780580`. The fresh Done audit passed all eleven dimensions, including the final plan archive metadata; no unresolved review threads.
- [Post-merge main CI](https://github.com/juangcarmona/sofa/actions/runs/39262357315): passed on merge commit `dcb0d64` (verify, 1m19s).
- Bastion's existing Aspire session was preserved. HTTP 200 observed through `http://192.168.0.54:5168/`; dashboard remains on loopback. This is a running session, not an installed boot service. No claim of a remote-client or reboot test.
- [FF evidence](../delivery/ff/6/20261019-attention/summary.md) records runtime collection, independent review and corrections. The plan is archived in place to preserve links.

## What worked

The small modular application delivered useful attention across five repositories without adding persistence, scheduling or an execution platform. Explicit incomplete coverage avoided invented release provenance. Real Aspire spans and browser observations complemented 28 application tests and 16 Node tests. Independent review found concrete gaps while Juan retained product, exposure and merge authority.

## What did not

Plan review exposed ambiguous publishing targets. Done review exposed lost partial observations and insufficient failure reasons; both were corrected before acceptance. A fixed host URL initially defeated Aspire endpoint assignment, and an unsampled ambient activity hid collection traces. The first access instructions assumed Juan was browsing on Bastion; direct trusted-LAN access was added after his correction. GitHub briefly returned HTTP 502 during CI polling; a fresh read confirmed success before merge.

## What changes next time

- Establish where the operator's browser runs during deployment planning; verify the address the operator will use. Routed to this cycle's running guide and deployment view; no new mandatory lifecycle gate warranted for this single access mismatch.
- Preserve partial evidence and explain uncertainty. Already routed into adapter regression tests and bounded coverage reasons.
- Verify exported telemetry and hosting behavior, not just successful requests. Already routed into endpoint and trace-export regression tests.
- Keep the fresh final-head audit and CI check: archive metadata changed the head after approval, so both were rechecked before merge.

The executor's reflections agree with the independent review suggestions; no unresolved retrospective disagreement.

## Effort and telemetry gaps

Human active time, agent active time, tokens and cost: **not captured for this issue**. The repository has no configured session-to-work-item usage mapping. The FF record identifies review rounds and models but cannot supply attributable effort totals. Application collection latency is runtime evidence, not development effort, and is not substituted for it.

## Proposed amendment — not applied

The lifecycle's deterministic-check table and scaffold-only note predate the .NET implementation. Route this documentation drift to a maintainer-approved amendment of `docs/engineering-lifecycle.md`. The actual CI already runs the checks, so no implementation guard is missing. The lifecycle requires an explicit amendment diff and approval; this proposal leaves that contract unchanged.

```diff
-**Note**: This scaffold has no application build or product behavior yet. Add checks when implementation tooling is adopted; do not report absent checks as passing.
+**Note**: `npm run verify` includes `verify:app`: .NET solution build, application tests and format verification. CI installs the SDK pinned in `global.json`.
```

Add corresponding rows for `dotnet build SoFa.slnx -m:1 --disable-build-servers`, `dotnet test tests/SoFa.Tests/SoFa.Tests.csproj --no-build --no-restore`, and `dotnet format SoFa.slnx --verify-no-changes --no-restore` when the amendment is approved. No other lifecycle amendment is proposed.
