# Tasks

All tasks are future, unchecked work. Groups are dependency ordered; tests and documentation belong with the behavior they introduce. This proposal has no unresolved build-changing Open Questions: it deliberately retains uncertain dispatch-to-run attribution rather than depending on a router extension. Owner acceptance below is a gate before implementation, not approval supplied by this plan.

## 1. Record owner decisions before implementation

- [ ] 1.1 Present the full security design to Juan García Carmona and record explicit acceptance or rejection of loopback-only hosting, session/CSRF protections, static issuer/subject-to-GitHub-ID binding, user-token permissions, credential lifecycle and duplicate/uncertain policy; verify a linked owner decision exists before coding or enabling mutations (AC2, AC4, AC5, AC10).
- [ ] 1.2 Use `arcshape-adr` to draft the durable control/authentication and receipt decisions as Proposed, obtain required maintainer acceptance and map changes to arc42 views 03, 05, 06, 07, 08, 09, 10 and 11; verify the ADR workflow and that no candidate is represented as current architecture. Recheck product semantics and obtain a human-approved Product Change before any deviation; otherwise document why none is needed (AC5, AC10).

## 2. Establish isolated authentication and identity

- [ ] 2.1 Add the separate loopback HTTPS ASP.NET Core control service with fixed-host checks, no CORS/proxy from observations, disabled-by-default mutations and security headers; add listener/host/origin integration tests and startup documentation, verifying the existing Node observation tests still reject mutations (AC5, AC8).
- [ ] 2.2 Add maintained OIDC/cookie middleware, server-side sessions, the specified absolute expiry, antiforgery and logout behavior; add tests for anonymous access, expired sessions, callback state/nonce failure, cross-site POST and sign-out invalidation, and document callback/session configuration. Verify every negative case records zero dispatches (AC5, AC6, AC8).
- [ ] 2.3 Add separately consented GitHub App user authorization, configured stable identity binding, per-mutation `/user` verification, scoped repository permissions, encrypted server token storage and sanitized diagnostics; add grant mismatch/revocation/expiry, OAuth state and redaction tests and connection/revocation documentation. Verify no installation, bot, PAT or observation credential fallback exists and tokens never reach browser storage or reports (AC2, AC5, AC8).
- [ ] 2.4 Extend Aspire with isolated persistent development Keycloak resources and reproducible non-destructive realm/client/account bootstrap, plus explicit development-only reset and external-issuer production settings; add bootstrap/restart/reset and production-no-admin-call tests and a reproducible runbook. Verify development secrets are outside source and production configuration cannot activate bootstrap/reset (AC6, AC7, AC8).

## 3. Implement reviewed single-attempt refinement

- [ ] 3.1 Add configured-repository and paginated open-issue selection, excluding pull requests, with server-rendered review of repository/issue/refine and labelled keyboard-accessible controls; add selection/rendering tests and user guidance, verifying arbitrary repositories and PR records cannot reach confirmation (AC1, AC9).
- [ ] 3.2 Add supported-router capability verification, five-minute bound review intents and confirmation-time identity/issue/version/default-branch revalidation followed by the fixed user-token workflow dispatch; test disabled/unverifiable/missing entrypoints, stale selections, malicious payloads and least-privilege failures. Document the supported contract and verify every invalid case dispatches zero requests and only refine is accepted (AC1, AC2, AC10).
- [ ] 3.3 Add transactional SQLite receipts and single-attempt confirmation with the 15-second transport bound, replay recovery and unresolved request-key uniqueness; test concurrent tabs, duplicate clicks, storage failure, crash windows and definite versus ambiguous responses. Document receipt states and verify no background mutation retry or queue exists (AC1, AC4).
- [ ] 3.4 Add explicit potentially-duplicate new-attempt acknowledgement, receipt relationships, retention and development/operator recovery guidance; test restart after lost acknowledgement, unresolved retention, acknowledgement requirements and secret-free receipt export, verifying ordinary retries return the existing receipt without dispatch (AC4, AC8).

## 4. Implement honest source-linked progress

- [ ] 4.1 Add bounded router-specific run discovery with timestamps, actor/stage/issue/branch filters, ten-page limit, backoff and visible/manual refresh behavior; test delayed, truncated, single-candidate, multiple-candidate, stale and rate-limited responses. Document scope limits and verify even a single heuristic match remains a candidate (AC3).
- [ ] 4.2 Add selected-issue source-result collection and validation of issue, stage and workflowRunId before showing published gates; add fixtures for valid results, mismatches, malformed/missing publication metadata, router rejection and successful runs without publication. Document source/result versus receipt/run attribution and verify no workflow conclusion is presented as a published result (AC3, AC8).
- [ ] 4.3 Render independent submission/evidence states, candidate and result source links, errors, stale timestamps and recovery controls; add browser tests for all states, keyboard/live-region behavior and non-color statuses, plus sanitized output assertions. Document the uncertainty/retry flow and verify submission never waits for completion (AC3, AC8, AC9).

## 5. Prove integrated acceptance and delivery readiness

- [ ] 5.1 Run integrated Playwright/axe and manual keyboard review across selection, confirmation, rejection, pending discovery, ambiguous submission/correlation, stale evidence and published result; retain sanitized desktop and 390px-phone screenshots/reports, verifying readable layout, labels, focus and source navigation without LAN control exposure (AC1, AC3, AC9).
- [ ] 5.2 In an owner-approved disposable repository, demonstrate GitHub App user-token dispatch retains the owner's actual `github.actor`, the unchanged router denies non-owner authority, and valid OIDC without a bound GitHub grant cannot mutate; include expired/cross-site denial, repeated clicks, lost-response and duplicate acknowledgement tests. Verify linked source run and sanitized test evidence cover the accepted security/duplicate policy (AC2, AC4, AC5, AC8).
- [ ] 5.3 On an approved Docker-capable host, exercise documented Aspire first start, preserved identity change after restart, explicit reset, sign-in/out/expiry and external-issuer production configuration; verify evidence shows no production administration or reset calls and contains no secrets (AC6, AC7, AC8).
- [ ] 5.4 Stop SoFa after GitHub admits a disposable refine request and verify repository publication continues; restart and recover source evidence, verifying no automatic later-stage request, scheduler, execution queue or cancellation is introduced (AC10).
- [ ] 5.5 Add the control build/tests to `npm run verify` and CI, run the full updated verification on the proposed implementation head, and attach CI/test evidence and documented limitations. Verify all eleven repository DoD dimensions are explicitly reviewed by Juan García Carmona, accepted design deviations are recorded, and applicable documentation/spec/product checks pass before requesting integration.

## Workflow follow-up

- The separate publisher validates these proposal files with OpenSpec strict validation and opens a proposal PR. The engine performs no repository or GitHub writes.
- Owner review decides whether to accept this candidate. Request `implement` only after that decision and the independent security/product gates; this plan invokes no next stage.
- Integration, deployment and any later OpenSpec archive remain separate authorized actions; no tracked implementation task presumes an archive has occurred.
