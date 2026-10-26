# Design

## Context

See proposal.md for motivation and the two capability deltas for behavior. This is a proposed design, not current architecture.

Current evidence: `src/web/server.mjs:15-48` serves fixed observations and rejects non-read methods; `src/observation/github.mjs:9-12` reads the latest 20 runs and first 100 open issue records without request correlation. `src/SoFa.AppHost/AppHost.cs` starts no application resources. The AppHost already targets .NET 10. ADR-0006 and arc42 views 05, 06 and 08 reserve authorization, concurrency, execution and publication to repository workflows.

The installed router's `workflow_dispatch` accepts issue and stage, checks `github.actor == github.repository_owner`, and uses issue-scoped GitHub concurrency. Its run name includes stage, issue and actor but no request ID. A dispatch acknowledgement is not proof of admission, execution or publication. The runbook identifies the reviewed Lighthouse source pin; generated workflows must not be hand-edited.

The verified owner answer chooses external OIDC and Aspire-managed development Keycloak, not GitHub delegation or acceptance of this boundary. `docs/product/changes/active/` has no active changes; `openspec/` is absent. Existing ProductShape permission for a separate authenticated adapter is sufficient for this candidate. Product intent and lifecycle judgements are not replaced by these artifacts.

## Goals / Non-Goals

**Goals:** isolate mutation credentials; use the actual GitHub user; make each intentional submission recoverable; preserve uncertain evidence; avoid migrating the observation UI.

**Non-Goals:** remote privileged control, a new identity server, multi-user administration, managing production Keycloak, guaranteed exactly-once GitHub dispatch, changing the shared router contract, executing stages or accepting product/security decisions. Phone usability concerns layout; it does not authorize phone-to-LAN privileged access.

## Decisions

### 1. Separate server-rendered control service

Propose a small ASP.NET Core Razor Pages service under `src/SoFa.Control/`, using maintained OIDC, cookie, OAuth and antiforgery middleware. It listens only on loopback at a distinct HTTPS origin, validates its configured host and rejects forwarded-host trust unless separately reviewed. It has no CORS permission for the observation origin. The existing Node observer remains independently runnable and has no route, proxy or shared session into controls. A normal navigation link can open controls; it cannot submit work. Reuse Lighthouse visual tokens without adopting a new SPA framework.

Why: .NET is already present and maintained authentication/antiforgery support avoids building sessions into the hand-written Node server. Alternative: add Node OIDC and control routes to the current listener; fewer processes, but it couples the LAN observer to privileged secrets. Replacing the entire UI adds unrelated migration. Framework and boundary are proposed for owner review, not accepted architecture.

### 2. OIDC session and separately verified GitHub user grant

Use authorization-code OIDC with PKCE, issuer/audience/nonce/state validation and exact callback URIs. Keep a host-only Secure HttpOnly session cookie, session identifiers rather than access tokens in the browser, server-side tickets and a 30-minute absolute session lifetime with no silent extension. Use middleware-compatible SameSite handling for protocol callbacks; callbacks are narrowly scoped and protected by state/nonce. All application mutations, logout and GitHub connection initiation require antiforgery tokens and exact origin checks; GET never dispatches. Set no-store, a restrictive CSP and frame protection on controls.

Propose a GitHub App web authorization using a user access token, never an installation token or app-authenticated dispatch. Register it only for selected repositories, with Actions write, Issues read, Contents read and required Metadata read. Actions write is the API's coarse dispatch permission, not a grant for arbitrary SoFa actions: the service exposes only a fixed allowlisted router/refine operation. No workflow-file write, issue write, organization administration or general GitHub proxy is needed. Contents read supports verification of the installed default-branch router contract. Explain these permissions at connection time.

For this single-owner scope, configure one immutable `(OIDC issuer, subject) -> GitHub numeric user ID` binding through operator-reviewed configuration. Do not equate emails or login strings, auto-link accounts, or offer account administration. GitHub OAuth uses maintained state/correlation handling tied to the existing OIDC session; it does not replace the primary principal. Verify `/user` with the user token after connection and before every mutation, match the configured numeric ID, and verify the selected repository's owner identity and capability. The existing router makes the final actor decision. Token refresh, where supported, uses the same user grant and identity recheck; revoked or expired access otherwise requires reconnection.

Keep tokens and refresh material server-side, encrypted using the platform data-protection mechanism with externally protected keys and restrictive local storage access. Store session data separately from non-secret receipts. Clear local tickets and token material at sign-out/expiry, and offer documented grant revocation in GitHub. Never use observation `gh` credentials, a bot, a PAT pasted into SoFa or an installation-token fallback. Log only whitelisted event codes and non-secret IDs; do not log request bodies, callback queries, headers, provider errors or token responses. Disable sensitive middleware/HTTP diagnostic output. Browser evidence must use isolated test accounts and sanitize captures.

Why: this directly addresses the router's user-actor rule while separating identity from authority. Alternatives: installation tokens fail actor preservation; broad OAuth `repo` scope grants unnecessary access; user-supplied PAT management burdens the UI and credential lifecycle. The chosen App user token's actor behavior and effective permissions must be demonstrated in an owner-approved disposable repository before enabling real mutations. This is planned evidence, not an assertion that a live grant was tested.

### 3. Validate, review, then make one bounded dispatch attempt

Use a fixed configured repository list. Provide paginated open-issue selection excluding `pull_request` records and direct issue-number lookup within that list. Display pagination and incomplete collection honestly. Before review, read the exact issue, repository owner, default branch and configured router. Verify the installed contract supports owner-authored dispatch and refine; unsupported, disabled or unverifiable automation is unavailable, never an invitation to install or patch workflows. GitHub remains responsible for final enablement/admission; permission to query configuration must not be assumed.

Issue a server-side review intent bound to user, repository, issue number, issue `updated_at`, default-branch SHA, router path and stage=refine, expiring after five minutes. On confirmation, repeat identity and selection checks; changed issue version, branch/router evidence or repository configuration requires re-review. Send `POST /repos/{owner}/{repo}/actions/workflows/{router}/dispatches` with the reviewed default branch and only `issue` and `stage=refine`. A branch can advance after validation; do not claim atomic admission or source pinning. Repository worker rechecks remain authoritative. Never accept stage, repository URL or workflow path from an unchecked browser payload.

Control interfaces: authenticated GET selection/review/status pages; antiforgery-protected POST review, confirmation and explicit new-attempt acknowledgement. Responses expose a receipt identifier, sanitized submission state and source links, not credentials. Dispatch has a 15-second transport timeout and never waits for stage completion. Definite validation/permission rejection is distinct from a transport/5xx outcome that might have been accepted. UI and error documentation land with this slice.

### 4. Durable receipt journal, not a work queue

Use a single-instance local SQLite receipt journal with transactional uniqueness/locking for an unresolved `(GitHub user ID, repository, issue, refine)` request key. Store intent/request ID, reviewed evidence references, submission-start time, attempt state, sanitized response category, observed candidate IDs and source-result links. No tokens, issue bodies, transcripts or execution leases belong here. Write an attempting receipt before network I/O; only the winning confirmed request may call GitHub. A replay returns the existing receipt. If storage is unavailable, do not dispatch.

A crash after the write or a lost response becomes uncertain on restart. There is no background mutation worker, retry queue or automatic redispatch. A definite rejection can be reviewed again; acknowledged or uncertain attempts block ordinary resubmission. To intentionally try again, the owner must inspect linked GitHub evidence and separately acknowledge possible duplicates; preserve both attempts and their relationship. This cannot prevent independent direct GitHub requests or promise exactly-once delivery. Repository concurrency is unchanged. Keep receipts for 30 days, retain unresolved records until explicitly reviewed, and expose a documented owner-only cleanup operation that never submits, cancels or resumes work. Session/token storage is not part of receipt export.

Alternative: browser-only deduplication fails across tabs and restarts. An execution database or worker queue violates ADR-0006. This small receipt journal affects only optional controls; repository work never reads it.

### 5. Source evidence without invented correlation

Keep request state and evidence state independent. Request states are reviewed, attempting, acknowledged, rejected and uncertain. Evidence states include discovery pending, candidate run(s), source-linked stage activity, workflow completed, published result, unavailable and stale. A 204 means acknowledged only; a skipped router is not admitted work.

Query the selected router's runs rather than the global latest-20 list. Filter by workflow_dispatch, reviewed branch/revision where applicable, GitHub actor, stage/issue run title and submission window, retaining all matches. Paginate to a documented bound of ten pages of 100 records, then report incomplete discovery. Refresh visible receipts every ten seconds with conditional requests/backoff; after two minutes without useful evidence show delayed discovery and require manual refresh. Hiding or closing the page stops polling, never repository work. Rate limits and errors retain dated evidence and show staleness. These are control UX bounds, not throughput promises.

**Correlation limitation:** time, actor and title matching cannot prove which dispatch caused a run, even when exactly one matches. Current router inputs carry no request ID. All such matches remain labelled candidates. Let the owner open their GitHub source; manually choosing a candidate does not turn it into cryptographic or source-proven attribution. Do not add an unsupported request_id input, modify generated workflows or claim exact-once correlation.

Separately, collect source-published refinement records from the selected issue and the run's published evidence. Validate structured issue/stage/workflow-run identity against the source before displaying a stage gate; use the workflowRunId in the stage-result contract where available, not free-text resemblance. Show source-linked activity/results for the selected issue/run without claiming that an uncertain receipt caused them. If supported publication metadata is missing, malformed or contradictory, link the original evidence and show result unavailable/ambiguous. A successful Actions conclusion is never a published stage result. Future source-supported request-ID correlation would require a separate reviewed change to the shared adapter; it is not an unresolved prerequisite or hidden task here.

Alternative: select the newest run or infer result from workflow success; simpler but makes unsupported claims. Extending the shared router offers stronger attribution but expands this bounded change and cannot be silently assumed installed across repositories. Honest ambiguity is explicitly allowed by AC3.

### 6. Development identity versus external production issuer

Extend Aspire development orchestration with maintained Keycloak hosting support, a dedicated development database/data volume, realm/client initialization and documented development accounts. First-start bootstrap is idempotent and non-destructive on subsequent starts; do not reimport over changed identities. Keep development credentials outside source, require developer provisioning where needed and document recovery without printing secrets. Use development HTTPS and exact redirect URIs. Persist identity data and data-protection material through normal restarts. A named, explicitly confirmed reset touches only development identity resources and restores the initial fixture; it cannot accept an arbitrary production issuer as a reset target.

The same control service uses configurable authority/client settings for the external production provider. Production configuration excludes Keycloak bootstrap/reset resources and administration credentials. An operator owns provider availability, accounts, TLS and client registration. No SoFa password store, custom recovery or provider lifecycle is added.

Why: implements the verified owner decision. Alternatives of transient development identities or SoFa-managed shared production Keycloak contradict it. Docker-dependent startup evidence must run on an approved capable host; Docker-in-Docker is not required or attempted by this proposal engine.

## Risks / Trade-offs

- [User-token permission and actor behavior] → verify actual github.actor in an isolated owner-authorized integration test; fail closed if unsupported, with no service-token substitute.
- [Cross-site attacks despite loopback] → exact host/origin validation, antiforgery, protocol state/nonce, no CORS bridge and secure host-only cookies; test both HTTP and browser paths.
- [Lost acknowledgement and independent direct requests] → durable uncertain receipts, explicit duplicate acknowledgement and unchanged GitHub concurrency; no exactly-once claim.
- [Imperfect run attribution] → candidate labels even for one time-matched run; preserve source-result identity separately.
- [New service and local receipt persistence] → small server-rendered boundary, encrypted session secrets and non-secret export; optional service failure never blocks repository execution.
- [Policy/document drift] → maintainer-reviewed amendments only; use executable source for implementation facts. No source or generated policy is edited in propose.

## Migration Plan

1. Obtain owner acceptance of this candidate and its security boundary; use arcshape-adr to record Proposed durable decisions and obtain required maintainer acceptance before implementation. No accepted ADR is superseded. Reassess ProductShape semantics; any deviation requires a human-approved Product Change first.
2. Build the control boundary disabled for mutation by default. Add tests and documentation with each slice, then isolated development identity and delegated GitHub connection.
3. Add validated submission, receipts and evidence views. Enable only a configured disposable test repository after the security decision; prove actor preservation and denial without changing its router.
4. Capture acceptance evidence, pass updated verification/CI and complete owner DoD judgements before enabling configured real repositories. Production issuer location is an operator configuration choice, not a new privileged hosting approval.
5. Roll back by disabling/stopping the optional control service and revoking its GitHub user grant. Preserve receipts for diagnosis under the retention policy. Keep the Node observer and repository workflows running; never cancel repository work, delete production identities or restore the retired supervisor.

## Validation and Visual Evidence Plan

Unit tests cover request state transitions, transactional replay, uncertain restart, selection/version validation, bounded candidate filtering and source-result identity. Integration tests cover middleware sessions, expiry/logout, OAuth state, fixed identity binding, user-token revocation, CSRF/origin denial, sanitized diagnostics and zero dispatch on invalid requests. Keep `tests/web.test.mjs` and observation tests proving the LAN surface remains read-only.

A disposable-repository test must show authorized dispatch as the actual owner and router rejection for a non-owner, including a valid OIDC user without the bound GitHub grant. Inject delayed discovery, multiple and single candidates without request IDs, missing publication metadata, pagination truncation, rate limits and successful workflows with no result. Capture evidence that closing SoFa after admission does not stop publication. Test commands and fixture setup must be documented as part of their owning slice.

Use Playwright plus existing axe tooling for desktop and 390px-phone selection, review, confirmation, errors and every progress state. Assert keyboard focus order, labels, visible focus, non-color status and live status announcements; manually review screenshots and keyboard flow. Use only sanitized test-account data in screenshots/reports. Phone layout is tested locally and does not require LAN control exposure.

Identity evidence covers first Aspire startup, repeat startup preserving a changed development account, explicit reset, sign-in/out/expiry and production-mode external-issuer configuration without bootstrap/admin calls. This engine has not run those checks. The publisher runs OpenSpec strict validation; implementation runs `npm run verify` with newly added .NET control tests/build and CI coverage on a Docker-capable host.

## Definition of Done

Apply all eleven dimensions in `docs/engineering-lifecycle.md:44-64`: acceptance criteria demonstrated with tests where feasible; complete implementation and accounted-for tasks; adequate relevant tests; updated `npm run verify` and CI passing; no secrets and explicit exposure review; conformance to the owner-accepted design with recorded deviations; updated user/operator/developer documentation; ProductShape/spec consistency; linked exact-head CI and test evidence; Juan García Carmona's self-review of all judgement items; and explicit known limitations. Planned does not satisfy Done, authorize deployment or approve integration.
