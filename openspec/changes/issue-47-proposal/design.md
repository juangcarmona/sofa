# Design

## Context

See proposal.md for motivation and the capability deltas for observable behavior. This is a revised candidate, not current architecture.

`src/web/server.mjs:15-48` exposes fixed read-only observations and rejects non-read methods. `src/observation/github.mjs:9-12` reads only 20 recent runs and the first 100 issue records; neither is request correlation. AppHost starts no application resources. ADR-0006 assigns routing, authorization, concurrency, execution and publication to repository workflows. Arc42 views 05–08 preserve that separation.

The installed `lighthouse-sdlc-router.yml` checks the repository-owner actor and accepts issue/stage plus later-stage parameters, but no client request identity or payload binding. The refine worker calls pinned `issue-actions.mjs` for preparation/reservation, validation and publication and retains result/runtime artifacts for 14 days. That external source is not present in this checkout: inspecting the generated call sites is not proof of a #119 implementation. The runbook identifies source pin `63c64b779ef18959a9c993902a5e5e1225a16947`; npm metadata still says 0.13.0. Upgrade via the shared installer, not manual workflow edits.

No `openspec/` inventory exists in this checkout and no active Product Change exists beyond .gitkeep. The supplied candidate is the artifact revision baseline. Lifecycle prose contains acknowledged historical drift (runbook lines 19–21); it is not silently repaired here.

Verified owner feedback confirms development Aspire/Keycloak, external production OIDC and separate GitHub user authorization. It also requires actual phone/LAN/VPN access and exact request attribution. `CON-TRUSTED-LAN` still specifies loopback-only controls: feedback authorizes preparing the change, not applying or accepting it.

The host-supplied [GitHub documentation excerpt](https://github.com/juangcarmona/sofa/pull/60#issuecomment-6429302951), citing [dispatch documentation](https://docs.github.com/en/rest/actions/workflows#create-a-workflow-dispatch-event), selects API `2026-03-10`, App user access tokens with Actions write and HTTP 200 carrying `workflow_run_id`, `run_url`, `html_url`. This is documentary evidence, not a live token/actor test. [Lighthouse #119](https://github.com/juangcarmona/lighthouse/issues/119) is open and proposed; its delivered, accepted contract remains a prerequisite.

## Goals / Non-Goals

**Goals:** isolate privileged access from observation; allow actual phone control; preserve the actual GitHub actor; correlate every request with source identity; recover uncertainty without dispatch retries; keep all approval boundaries visible.

**Non-Goals:** public hosting, multi-user administration, comment/reply UI (#48), new identity server, production provider administration, a SoFa execution store/queue, later-stage actions, replacing Lighthouse policy, exactly-once execution claims or accepting this candidate. Narrow screenshots alone do not prove phone access.

## Decisions

### 1. Separate HTTPS control origin reachable over LAN/VPN

Retain the Node observation service unchanged. Propose a small ASP.NET Core Razor Pages service under `src/SoFa.Control/`, using maintained OIDC, cookie, OAuth and antiforgery middleware rather than implementing these in the handwritten Node server. Use the shared visual tokens, not a new SPA. The control service has no anonymous mutation routes, observation credential reuse, cross-origin CORS permission or proxy from observation. An ordinary navigation link to controls is safe; navigation cannot dispatch.

Expose one configured canonical HTTPS control origin on the owner's LAN/VPN, either direct TLS or behind an explicitly configured TLS reverse proxy. In the proxy profile the backend binds loopback/private transport and accepts forwarded scheme/host only from configured proxy identities; arbitrary Forwarded/X-Forwarded-* headers cannot alter redirects, cookies or origin checks. Validate allowed hosts, canonical origin and exact callback URLs. No wildcard redirects, HTTP control fallback or internet exposure. Provide certificate trust/DNS setup for the actual phone, not localhost aliases usable only on the host. The phone must reach the OIDC issuer's browser endpoints and the HTTPS callback origin; the server must reach issuer discovery/token endpoints. GitHub authorization callbacks use that same registered reachable control origin. Operators select host names and certificates before the acceptance test; this does not select or deploy a production identity host.

Alternative: loopback-only controls reduce exposure but fail the explicit phone use case. Adding mutations to the existing anonymous listener couples observation to secrets and contradicts its contract. A full UI replacement adds unrelated migration. The separate reachable service is the candidate, contingent on Product Change and security acceptance.

### 2. Session security and separate GitHub user authority

Use OIDC authorization code with PKCE, issuer/audience/state/nonce validation and exact redirects. Store session tickets server-side, with host-only Secure HttpOnly cookies, a proposed 30-minute absolute lifetime and no silent extension. Use middleware-supported SameSite correlation handling for callbacks; callback exceptions are narrowly scoped, not general CSRF exemptions. All application mutations, connection initiation and logout use POST, antiforgery tokens and exact origin checks. GET never submits work. Apply no-store, restrictive CSP and frame protection. Invalidate local session/token access on sign-out/expiry; protocol logout cannot become an open redirect.

Bind the single configured owner by immutable `(OIDC issuer, subject) -> GitHub numeric user ID`, reviewed by the operator. No email matching, automatic account linking or account-management UI. A separate GitHub App web authorization obtains the user's access token; verify `/user` after connection and before mutation, repository ownership and the supported source capability. The source router retains the final actor check. Refresh only within the same grant with identity recheck; otherwise require reconnection. OIDC alone never authorizes GitHub mutation.

Proposed App access is limited to selected repositories: Actions write for dispatch, Issues read for issue/publication evidence, Contents read for installed-contract evidence and required Metadata read. Revalidate the final minimum against the accepted source contract; additional permission requires separate review, not silent broadening. Expose only the fixed refine operation, never an arbitrary GitHub proxy. Reject installation/bot/operator/PAT fallbacks and never reuse observer `gh` credentials. A disposable-repository live test must establish actual `github.actor`, effective user permissions and denial behavior.

Keep grant/refresh material encrypted server-side using maintained data protection with externally protected keys and restricted storage. Separate tokens/tickets from the non-secret receipt journal. Whitelist diagnostic event fields; omit request bodies, callback queries, authorization headers, raw provider errors and token responses. Disable sensitive HTTP/middleware logging; redact browser evidence at capture. Document grant revocation and session teardown.

Alternative: installation tokens cannot preserve the owner's actor; broad OAuth scopes or pasted PATs increase authority and credential-management burden. Building custom sessions/password recovery adds avoidable security scope.

### 3. Development identity is not production identity ownership

Aspire manages maintained Keycloak development resources, a dedicated persistent identity volume/database and reproducible dedicated realm/client/accounts. Bootstrap is idempotent and does not overwrite changed identities on restart. Keep development credentials outside source, isolated from production, and avoid printing them. Document explicit named development reset with confirmation; production issuer configuration is never a reset target. Persist required local data-protection keys across normal restart. Development phone tests need trusted HTTPS and reachable issuer/callback configuration too.

Production uses the same OIDC middleware against an operator-configured external issuer/client. It starts no bootstrap or identity administration resources and needs no provider admin credentials. SoFa does not provision, reset or delete shared Keycloak/accounts. Container-dependent evidence runs on an approved capable host; this proposal engine does not attempt Docker-in-Docker.

Alternative: transient dev accounts violate restart acceptance; managing shared production Keycloak violates the owner decision.

### 4. Source-owned eligibility and immutable request lifecycle

Consume the accepted versioned #119 contract for Refine eligibility, required evidence, input digest/revision, actor authority and active reservations. Source preflight and admission must share semantics. Preflight is neither a reservation nor implementation permission. Unsupported/missing versions disable submission with GitHub source links. Do not reconstruct eligibility from labels or enable against the current ID-free router. Direct GitHub entry stays supported under source checks.

Use paginated configured-repository/open-issue selection excluding PRs, with exact issue lookup. Build a five-minute server-side review intent bound to owner, repository, issue, refine, source-assessed input digest, default-branch/router evidence and compatible contract version. Confirmation repeats identity, selection, freshness and contract checks. Changed evidence requires new review; source admission still rechecks races. Browser parameters cannot choose a workflow path, arbitrary repository or another stage.

Persist a random immutable request identity and canonical payload binding before outbound I/O. Carry those via the delivered source-defined entrypoint through admission, run/attempt and publication. The field names, canonical encoding, endpoints and version negotiation are deliberately not invented here: implement the accepted packaged interface and prove this behavioral contract before enabling. If the delivered contract cannot meet these requirements, keep controls unavailable and return for proposal revision; do not replace correlation with candidates.

The shared dependency covers Refine and Propose; SoFa #47 exposes Refine only. #119 builds on #70/#91/#95 and completion #105, coordinates freshness with #100 and does not require all of #99. Its ordinary-comment versus explicit-continuation semantics and attributable comment reconciliation support #48, not a new comment UI in this change. The accepted upstream package must retain those guarantees; SoFa refine does not post comments or edit routing labels.

Alternative: duplicating Lighthouse rules creates competing policy; passing an unsupported request_id to the current workflow is not integration. A shared source contract is necessary for cross-client and lost-response guarantees.

### 5. Exact receipt first, source reconciliation always available

Pin `X-GitHub-Api-Version: 2026-03-10` and verify the documented dispatch behavior with an actual GitHub App user access token before enabling repositories. For the documented HTTP 200, preserve `workflow_run_id`, `run_url` and `html_url`; validate identity via the selected repository/workflow and allowlisted GitHub links, not arbitrary returned URLs. A receipt proves an exact dispatch run, not successful source admission. Follow the accepted contract's explicit mapping if router and worker identities differ; retain run attempts, never substitute a newer attempt silently.

After any lost response, timeout, process crash, malformed receipt or unexpected response shape, retain uncertain submission and query source reconciliation by immutable request identity and payload binding. The same mechanism recovers admission/rejection, exact run/attempt and published outcome after restart or local receipt loss when identity can be restored. Source records, not a client-local journal, enforce replay/conflict and reservation guarantees. Do not automatically retry mutations, issue a new identity or bypass an active reservation. Reconciliation retries are reads with bounded backoff. Where the source explicitly allows an identical replay, an owner-confirmed retry reuses the same identity/payload and must resolve the original admission or explicit rejection. A genuinely new request requires a fresh eligible review and a distinct explicit intent, not an uncertainty escape hatch.

Use a local single-instance SQLite receipt journal for UI recovery, not execution: request identity, actor ID, repository/issue/stage, canonical payload binding, contract/input revision, attempt phase, sanitized receipt category, exact source identities and timestamps. Do not store tokens, issue bodies, execution leases or transcripts there. Transactional review-intent uniqueness prevents repeated clicks in one instance; source idempotency/reservations cover multiple clients, delayed events and restarts. A journal write failure prevents dispatch. Crash after recording an attempting state is uncertain, not failed. Retain resolved receipts for a proposed 30 days and unresolved receipts until explicit review; cleanup never mutates GitHub or erases source reservations. Explain local data-loss limits; do not promise recovery without retained/restorable request identity.

No exactly-once execution claim follows from client deduplication or a 200 response. Never choose the latest issue/stage run or use time/actor/title matching as normal attribution. Diagnostic candidates, if shown at all, are labelled uncorrelated and cannot satisfy acceptance.

Alternative: heuristic discovery fails exact attribution even with one candidate. Browser-only deduplication fails restart/cross-client races. A SoFa work queue violates ADR-0006.

### 6. Asynchronous progress and validated publication

Keep separate facts: reviewed; submission attempting/acknowledged/rejected/uncertain; exact run discovered; source admitted/rejected; stage running; workflow completed; source result published. Preserve timestamps and stale/unavailable states. Return the control POST after the bounded submission attempt (proposed 15-second transport timeout), never after refinement completes. Poll source reads every ten seconds while visible with conditional requests/backoff; after two minutes show delayed evidence and manual refresh. Use paginated bounded source queries and disclose incomplete results; source contract—not a global latest-20 list—provides reconciliation. Closing the page stops polling, not repository work.

Display a result only from validated source publication bound to request, issue, refine, run/attempt and the actual gate/status. Reuse accepted completion/publication semantics (#105 and #119), not a new parser inferred from prose. Missing or contradictory metadata stays unavailable with links. Workflow success is not publication. The service never starts propose, implementation or integration after Ready. Keyboard flow, labels, visible focus, live text statuses and non-color distinctions apply to every state.

## Proposed Product Change for review

This section is a prepared semantic draft, not a ProductShape overlay or approval. File boundaries permit only this OpenSpec change; formal ProductShape artifacts must be prepared through its workflow in the separately authorized product step.

**Candidate:** `chg-authenticated-phone-controls-001`. **Reason:** make the explicit owner phone/LAN/VPN journey possible without granting anonymous network clients privilege.

**Target:** `docs/product/model/requirements/constraints/con-trusted-lan.md` (`CON-TRUSTED-LAN`). Preserve its first paragraph, including the read-only anonymous surface and separately accepted boundary. Replace its loopback-only planning paragraph with the proposed wording:

> A separately authenticated control surface MAY be reachable from the owner's phone over an explicitly accepted LAN/VPN HTTPS boundary. Reachability MUST NOT grant identity, GitHub authority or approval. The control origin, reachable identity callbacks, session lifecycle, CSRF protections and actual-user GitHub delegation MUST be accepted before implementation or exposure. Public hosting is not authorized.

Replace its final consequence about a future LAN requirement with:

> Control routes, secrets and authorization MUST remain isolated from anonymous observation. Repository workflows retain admission, concurrency, execution, publication and continuation authority. HTTPS phone access requires the separately accepted deployment/security profile and evidence from an actual phone session.

Preserve `BR-LAN-PRIVILEGE-BOUNDARY`, `BR-WORK-AUTHORIZATION`, `CON-FACTORY-RUNTIME` and `FR-AUTHORIZED-ENTRY`: they already allow a separately accepted adapter without an alternate execution authority. Validate the formal overlay's affected-reference closure rather than changing those rules speculatively. Human Product Change approval, application and merge acceptance remain distinct under `docs/product/README.md`; the feedback does not perform any of them. Record durable decisions through `arcshape-adr` as Proposed and obtain maintainer acceptance; update arc42 views via their managed workflow only when authorized.

## Risks / Trade-offs

- [Remote authenticated origin expands exposure] → independent security acceptance; strict origin/proxy trust, trusted TLS, CSRF/session defenses and real-device denial tests.
- [#119 remains an open proposal] → explicit upstream prerequisite and disabled incompatible controls; no final wire shape or completion claim.
- [Documented API differs from real user-token behavior] → disposable-repository live test with API version, actor, receipt and source outcome; fail closed with no service-token substitute.
- [Lost responses or multiple clients] → immutable payload-bound source identity, durable receipts, explicit conflicts and read reconciliation; no automatic redispatch or exactly-once execution promise.
- [Source publication retention or outage] → retain dated safe receipt evidence, disclose unavailable/expired source results and document retention; never infer success.
- [Extra service and identity dependencies] → optional independent controls; observation and admitted repository execution continue when controls fail.

## Migration Plan

1. Review this candidate. Before implementation, formalize/approve the Product Change, security boundary and durable architecture decisions; owner confirms implementation separately.
2. Accept/deliver/package #119 and inspect its actual issue-actions/publication interface and compatibility. Upgrade repository workflows through the reviewed installer in a separate authorized change; retain direct GitHub entry. If behavior differs materially, revise this proposal.
3. Build controls disabled for mutation by default; add identity/session/security tests and docs with their slices. Configure development identity, then delegated GitHub user authorization.
4. Add source-contract consumer, journal and evidence UI. Enable only an owner-approved disposable repository for live tests, including exact receipt and lost-response recovery.
5. Complete real phone, identity, accessibility, CI and DoD evidence before enabling configured real repositories. Operator chooses accepted DNS/TLS/issuer settings; SoFa never provisions shared production identity.
6. Roll back by disabling controls, withdrawing the control listener/proxy route and revoking the user grant. Preserve receipts according to retention. Keep observation and repository execution running; do not cancel admitted work, delete production accounts or restore the retired supervisor.

## Validation and Visual Evidence Plan

Proposal publication: the trusted publisher validates authorized paths and OpenSpec strict format and updates PR #60 against its unchanged expected head/feedback. This engine performed no live API, browser, CLI, Docker or implementation checks. Prose is reviewed directly; optional Humanizer is not listed among installed skills.

Unit/integration tests accompany each slice: host/proxy/origin/TLS configuration, sessions, callbacks, expiry/logout, GitHub identity binding and revocation, redaction, review freshness, immutable payload canonicalization under the delivered contract, transactional duplicates, crash windows, source conflicts, version incompatibility, uncertain recovery and result identity. Keep Node observation tests proving no mutations. Include same-ID/same-payload replay, changed payload conflict, independent clients, changed evidence and reservation races. Verify the packaged upstream ordinary-comment/explicit-continuation and uncertain-comment receipt tests as compatibility evidence; no comment publication is added here.

A live disposable-repository test records API `2026-03-10`, user-token type/permissions without token values, actual `github.actor`, HTTP status/receipt fields, request ID, payload binding, source admission, exact run/attempt and validated published result. Test non-owner and OIDC-only denial, revoked grants, duplicated submissions, missing/ambiguous responses and restart. Inject lost response after source acceptance and demonstrate read reconciliation to the same identity and outcome without another dispatch. Stop SoFa after admission and prove independent publication. Successful workflow without result remains result unavailable.

Use Playwright plus existing axe tooling for all desktop/narrow selection, review, denial, pending, uncertain, running, stale and result states; manual keyboard/focus/status review is also required. Capture sanitized screenshots/reports. Separately use an actual phone over the accepted LAN/VPN route: trusted HTTPS, reachable OIDC and GitHub callbacks, sign-in, explicit review/submit, exact source links, duplicate tap, interruption/reopen and lost-response reconciliation. Record device/browser, network profile, configured origin, request/run/result IDs and safe screenshots; never capture credentials. A 390px screenshot is supplementary, not real-phone evidence.

Identity checks on an approved capable host cover first Aspire startup, a changed account surviving restart, explicit development reset, sign-in/out/expiry and external production issuer configuration without bootstrap/reset/admin calls. Add control build/tests to `npm run verify` and CI and execute on the exact implementation head.

## Definition of Done

Apply all eleven dimensions in `docs/engineering-lifecycle.md:44-64`: demonstrated acceptance criteria; complete implementation/tasks or justified removals; adequate passing tests; updated verification/CI passing; secret-free security checks and human exposure review; approved architecture conformance with deviations recorded; current user/operator/developer docs; ProductShape/spec consistency; linked exact-head CI/test evidence; Juan García Carmona's judgement/self-review; explicit known limitations. Planned does not satisfy Done or authorize integration/deployment.

## Deferred operational choices

The operator selects exact host names, certificate source, proxy/direct-TLS profile and external production issuer location within the accepted boundary before deployment tests. These do not permit HTTP, public hosting, weaker callbacks or broader authority. Upstream delivery and human approvals are prerequisites, not unanswered design choices disguised as operational configuration.
