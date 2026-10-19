# Issue 6: Repository attention

## Scope and authority

Implement CHG-INITIAL and ADR-0001, approved by Juan on 2026-10-19. Juan additionally approved newest-release-target per mapped workflow with latest retry; unknown targets remain incomplete. Preserve owner merge approval. [Issue #6](https://github.com/juangcarmona/sofa/issues/6).

## Design

Use .NET SDK 10.0.112, ASP.NET Core Razor Pages, and Aspire Hosting 13.6.1 (installed CLI version; confirm package restore). Four source projects: Attention (pure rules and application service), GitHub (read adapter), Web (composition and one page), AppHost (orchestration). Latest snapshot lives in memory. A single-flight refresh is independent of browser disconnect; service shutdown cancels it. No database, background scheduler, execution adapter, or frontend toolchain.

Attention owns immutable observations, findings and per-category coverage. GitHub transport types and secrets never cross the port. Failed checks include failure/timed_out/action_required; pending, cancelled, skipped, neutral and unknown conclusions retain their distinct states. Check identity includes provider/context; use current PR head and its current merge revision, latest attempt only. Recheck PR head after collection. Deduplicate approval reasons, but never infer approval itself.

GitHub reads use bounded pagination and concurrency, per-request timeouts, bounded retry for transient responses, and explicit partial coverage when limits/errors occur. Failure in one category/repository cannot suppress others. Persist no secrets. HTML escapes source text; only allow GitHub HTTPS links. Loopback host, antiforgery POST, and host restrictions protect refresh.

## Verified mappings (2026-10-19)

| Repo | Blocker | Owner review | Release workflow | Target |
| --- | --- | --- | --- | --- |
| arcshape | Unconfigured | Unconfigured | publish.yml | published release tag; identify from release event/run metadata, otherwise incomplete |
| productshape | Unconfigured | Unconfigured | release.yml | actual publication revision is not reliably exposed (workflow may checkout newer main); incomplete until proven |
| domusmind | blocked | owner-review | release.yml | dispatch version not exposed by normal run metadata; incomplete until reliably identified |
| raccord | status:blocked | Unconfigured | release.yml | tag from workflow run metadata, otherwise incomplete |
| sofa | Unconfigured | Unconfigured | Unconfigured | none |

Mappings came from live label/workflow APIs and workflow source. No labels installed in monitored repos. SoFa’s six documented lifecycle labels were created for this work. Authored PR and explicit review-request checks work independently of label mappings. Workflow IDs/paths are explicit configuration, not name heuristics. Secondary publishing workflows outside the configured mapping are not claimed as covered.

## Ordered tasks

- [x] Record approved product/architecture baseline and generated citations.
- [x] Scaffold four projects, pinned toolchain, root solution and test projects.
- [x] Implement evidence records, classification, refresh coordination, immutable snapshots and metrics.
- [x] Implement GitHub adapter, verified mappings, pagination, error isolation and revision checks.
- [x] Implement Razor attention page, refresh, safe links, visible coverage and error/empty states.
- [x] Wire Aspire hosting and OTLP telemetry; document local credentials and startup.
- [x] Add deterministic domain, adapter, concurrency and web request tests.
- [x] Extend CI/verify, run full checks and exercise runtime through Aspire.
- [x] Obtain independent Done audit; corrections approved at `3ca7c95`.
- [ ] Confirm final-head CI and obtain Juan’s protected judgement and merge approval (tracked on PR #7).

## Verification and Definition of Done

Exercise authored PRs with no requests and duplicate signals; draft/closed exclusion; old revisions and latest retries; pending/skipped/cancelled checks; explicit blockers; newest release target; missing mapping and ambiguous target. Adapter fixtures cover HTTP failures, rate limits, pagination caps, exact source links, changed heads and partial responses. Service checks cover single-flight, isolated repository failure, atomic replacement and restart empty state. Web tests cover antiforgery, escaped content, invalid external links and all coverage states.

Run the existing npm verification plus dotnet restore/build/test and format verification; CI installs the pinned SDK. Runtime: launch AppHost, load page, refresh actual five repos using existing authorized access, inspect Aspire logs/traces and capture sanitized local evidence. No credentials in evidence. Tests will use fixtures; live reads are supplemental. Add a fixture where the dispatched ProductShape revision differs from the revision checked out for publication: run head_sha alone must not classify a target. Inject a distinctive synthetic credential and assert its absence from rendered responses, captured logs, and exported telemetry, including failure paths.

DoD dimensions: acceptance coverage, completeness, tests, deterministic checks, security, architecture conformance, documentation, product consistency, evidence, human review, and known limitations. Independent review prepares all judgement findings; none substitutes for Juan’s protected final judgement/merge gate. No deployment beyond local verification.

## Risks and limits

GitHub is not a transactional snapshot. Expose collection interval and incomplete outcomes. Bound all requests so refresh terminates. Newest release targets that cannot be resolved from authorized evidence remain incomplete. No claim of complete security/delivery metrics. npm install reports pre-existing dependency advisories; record and assess without unrequested major upgrades.

## Implementation notes

AppHost explicitly patches transitive MessagePack to 2.5.307 after restore audit rejected the vulnerable version. Application hosting URLs belong to launch profiles/Aspire, not appsettings; the endpoint regression test enforces this. No product scope changed.
