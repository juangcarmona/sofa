# Tasks

All tasks are future and unchecked. This candidate changes neither source nor product authority. Implement only after the independent gates below; stop for proposal revision if an accepted upstream contract cannot meet the specifications. Tests and documentation land with their owning behavior.

## 1. Establish independent authority and upstream prerequisites

- [ ] 1.1 Obtain owner review of this revised candidate and separate implementation authorization; verify the decision links PR #60's exact reviewed head and does not infer approval from planned, identity-provider feedback or green checks.
- [ ] 1.2 Prepare `chg-authenticated-phone-controls-001` through ProductShape using design.md's semantic draft; validate its overlay/reference closure and record human approval, application and merge acceptance before implementation. Verify `CON-TRUSTED-LAN` permits the accepted authenticated phone boundary and anonymous observation remains read-only.
- [ ] 1.3 Present the HTTPS origin/proxy trust, reachable callbacks, OIDC session/CSRF policy, owner binding, App permissions, token handling and uncertain-request policy to Juan García Carmona; verify explicit security acceptance before coding/exposure and document the selected test deployment profile.
- [ ] 1.4 Use `arcshape-adr` for Proposed durable control/identity/receipt decisions and obtain required maintainer acceptance. Verify managed ADR shape/status and map authorized architecture updates to arc42 views 03, 05–11 without describing proposed behavior as current.
- [ ] 1.5 Obtain accepted, delivered, packaged Lighthouse #119 and inspect its actual issue-actions/admission/publication contracts, versions and compatibility tests, including #70/#91/#95/#105 and freshness coordination with #100, without requiring all of #99. Produce a consumer mapping proving eligibility, payload-bound request identity, run/attempt/result, replay/conflict, lost-response reconciliation and comment-intent guarantees; verify direct GitHub entry remains supported. Do not invent an interface while this prerequisite is unmet.
- [ ] 1.6 In an independently authorized dependency upgrade, regenerate router/workers through the reviewed Lighthouse installer and document the accepted source/package version and direct-entry compatibility. Verify generated diff and upstream contract/consumer tests; do not hand-edit workflow locks or enable an incompatible current router.

## 2. Build the isolated phone-reachable identity boundary

- [ ] 2.1 Add the separate server-rendered control service disabled for mutation by default, canonical HTTPS/host checks and explicit direct-TLS or trusted-proxy configuration. Add tests rejecting HTTP, arbitrary forwarded origins and unapproved exposure; document phone DNS/certificate trust and reachable issuer/callback setup. Verify existing observation mutation-denial tests remain passing and no CORS/proxy bridge is added.
- [ ] 2.2 Add maintained OIDC/cookie middleware, server-side sessions, 30-minute absolute expiry, protected POST actions and logout. Add callback state/nonce/issuer/redirect, origin/CSRF, anonymous/expired and signed-out replay tests; document session/callback configuration and verify every negative case causes zero GitHub mutation.
- [ ] 2.3 Add Aspire-managed isolated persistent development Keycloak and reproducible non-destructive initialization. Add first-start/restart/reset tests and a development-only reset runbook; verify a changed identity survives restart and explicit confirmed reset cannot target production.
- [ ] 2.4 Add external production issuer/client configuration without bootstrap/admin resources. Test valid/invalid issuer settings, sign-in/out/expiry and production-no-admin behavior; document operator ownership and verify no development or password-recovery fallback exists.
- [ ] 2.5 Add separately consented GitHub App user authorization, stable configured owner binding, per-mutation user verification and encrypted server-side grant storage. Test missing/mismatched/revoked/expired grants and OAuth state; document least-privilege connection/revocation. Verify no bot, installation, PAT or observation-credential fallback and no raw token/header/callback data in browser/log/export fixtures.

## 3. Consume source eligibility and explicit reviewed requests

- [ ] 3.1 Add configured repository and paginated open-issue selection excluding pull requests with accessible review of repository/issue/refine. Test invalid/arbitrary selections, PR records and pagination bounds; document selection/source links and verify no unchecked browser workflow/stage parameter reaches dispatch.
- [ ] 3.2 Consume the delivered versioned source eligibility assessment, reasons, required evidence, input digest, actor and reservation information. Test denied/unavailable/incompatible contracts and document disabled states; verify preflight creates no reservation and never replaces source admission.
- [ ] 3.3 Add five-minute server-side review intents and confirmation-time identity, issue, digest, router/default-branch and contract freshness checks. Test closure, evidence change, reconfigured repository, simultaneous reservation and late admission rejection; document re-review behavior and verify invalid confirmations dispatch zero requests.
- [ ] 3.4 Add durable payload-bound immutable request identities using the accepted source canonicalization and entrypoint. Test source same-ID/same-payload replay, changed-payload conflict and delayed events across separate clients; document identity lifecycle and verify repository reservations remain authoritative with no exactly-once execution claim.

## 4. Dispatch with exact receipts and recover uncertainty

- [ ] 4.1 Implement transactional local receipt storage before network I/O and a bounded single dispatch attempt using API `2026-03-10` and the actual user's grant. Test repeated clicks, storage failure, crash boundaries, fixed refine scope and transport timeout; document receipt fields/retention and verify no mutation queue or automatic retry exists.
- [ ] 4.2 Parse the documented HTTP 200 workflow_run_id/run_url/html_url receipt and validate its repository/workflow/source association. Test malformed/missing/unexpected responses, unsafe links, router rejection and router/worker mappings; document run receipt versus admission and verify no response is treated as a passed stage.
- [ ] 4.3 Add read-only source reconciliation by durable request identity and payload binding after lost response/restart. Test acceptance-before-response-loss, replay, unavailable reconciliation, multiple attempts and contradictory evidence; document recovery and verify one exact source outcome is recovered without new identity or automatic redispatch.
- [ ] 4.4 Add source-authorized identical replay only where supported and explicitly confirmed, plus fresh-review rules for genuinely new intent. Test concurrent clients, reservation conflicts and journal cleanup; document unresolved retention/local-data-loss limits and verify uncertainty never triggers a replacement request or clears a source reservation.

## 5. Render exact source progress and preserve boundaries

- [ ] 5.1 Add distinct submission, exact-run, admission, stage, workflow-completion and publication states using the accepted request/run/attempt bindings. Test validated and mismatched result identities, missing publication and successful workflow without result; document evidence semantics and verify heuristic/latest-run matching cannot establish attribution.
- [ ] 5.2 Add visible bounded read refresh/backoff, timestamps, stale/unavailable indicators and issue/run/result links. Test delay, rate limit, truncation, reopening and unavailable source; document recovery and verify control POST does not wait for refinement completion.
- [ ] 5.3 Add keyboard/phone layouts, labels, visible focus, live text status and non-color error/recovery controls. Run browser/axe tests for selection, review and every progress/denial state; document accessible operation and retain sanitized desktop/390px screenshots as supplementary evidence.
- [ ] 5.4 Verify source compatibility tests for ordinary comments, explicit continuation and uncertain comment receipts in the packaged upstream consumer evidence. Add SoFa tests that viewing/recovery of needs-info never posts a comment, edits labels or continues work; document #48 as a separate change and verify refine completion cannot submit later stages.

## 6. Demonstrate integrated acceptance and delivery readiness

- [ ] 6.1 In an owner-approved disposable repository, run actual GitHub App user-token dispatch with API `2026-03-10`; capture safe HTTP receipt fields, actual github.actor, immutable request/payload binding, admission, exact run/attempt and publication links. Verify owner success, non-owner/source denial, OIDC-only denial, revoked grant, duplicate submissions, changed payload/evidence and conflicting reservation; retain evidence without token values.
- [ ] 6.2 In that packaged integration, inject response loss after source acceptance and stop/restart SoFa. Verify source reconciliation recovers the original exact run/result with no second dispatch, source replay/conflicts hold across clients, and unavailable evidence stays uncertain rather than choosing a nearby run; retain sanitized attribution evidence.
- [ ] 6.3 Use an actual phone over the accepted LAN/VPN route, not just emulation: verify trusted HTTPS, reachable issuer/OIDC and GitHub callbacks, sign-in/out/expiry, review/submit refine, duplicate tap and interruption/reopen with exact request/run/result links. Exercise the lost-response recovery path and record safe device/browser/network/origin details and screenshots; verify no LAN reachability alone grants authority.
- [ ] 6.4 On an approved capable host, exercise the documented Aspire first startup, changed development identity surviving restart, explicit development reset and external production issuer profile. Verify no production bootstrap/reset/admin calls and no secrets in retained evidence; no Docker-in-Docker is required.
- [ ] 6.5 Stop SoFa after source admission and verify repository execution/publication completes, direct GitHub entry still works and later SoFa recovery returns the exact source outcome. Retain links proving no control-service queue, cancellation or automatic later-stage action.
- [ ] 6.6 Run integrated Playwright/axe and manual keyboard/visual review across all accepted states and real-device findings. Verify labelled controls, focus/status clarity, source navigation and safe evidence capture; record remaining limitations rather than substituting viewport screenshots for phone access.
- [ ] 6.7 Add the control build/tests and applicable security/docs checks to `npm run verify` and CI, then run on the exact implementation head. Verify linked CI/tests and all eleven repository DoD dimensions with Juan García Carmona's judgement/self-review, accepted ProductShape/spec consistency, approved design conformance/deviations and current user/operator/developer documentation before requesting integration.

## Acceptance coverage

- Issue selection/explicit refine: authenticated-controls authority scenarios, refinement-requests current-selection scenarios; tasks 2.5, 3.1–3.4, 6.1.
- Actual-user authority/no bot or LAN trust: authenticated-controls GitHub/session scenarios; tasks 2.1–2.5, 6.1, 6.3.
- Distinct submitted/run/running/result and honest unavailable evidence: refinement-requests receipt/reconciliation/publication scenarios; tasks 4.1–5.2, 6.1–6.2.
- Duplicate handling and exact source attribution: immutable-request and recovery scenarios; tasks 3.4, 4.1–4.4, 6.1–6.3.
- No SoFa execution dependency or later approval: execution/gates and comment-scope scenarios; tasks 5.4, 6.5.
- Anonymous/cross-site denial and secret safety: authenticated-controls scenarios; tasks 2.1–2.5, 6.1, 6.3.
- Owner identity decision and independent product/security gates: identity lifecycle/HTTPS scenarios; tasks 1.1–1.4, 2.3–2.4, 6.4.
- Real phone/LAN/VPN plus keyboard accessibility: phone/accessible scenarios; tasks 2.1, 5.3, 6.3, 6.6.
- Accepted upstream dependency, documented API and future live proof: compatibility/receipt scenarios; tasks 1.5–1.6, 4.2–4.3, 6.1–6.2.

## Workflow follow-up

- The trusted publisher checks the file boundary, runs OpenSpec strict validation and updates existing PR #60 only if the expected head and feedback remain unchanged. No repository/GitHub write is performed by this engine.
- Owner review and each independent product/security/upstream gate remain necessary. No next stage is invoked.
- Integration, deployment and eventual OpenSpec archive are separate authorized actions, not completed tasks in this proposal.
