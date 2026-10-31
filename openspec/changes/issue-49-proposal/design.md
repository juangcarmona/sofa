# Design

## Context

See proposal.md for motivation and the three capability specifications for behavior. The current Node server serves fixed GET/HEAD assets and `/api/observations`, uses operator `gh` authentication and has no identity session. AppHost is empty; its project targets .NET 10 with Aspire 13.6.1. Existing tests cover bounded repository collection, cache coalescing, safe errors and a non-mutating observer.

ProductShape's applied CHG-AUTHENTICATED-OPERATIONS-001 and canonical access/security/mobile rules authorize this slice. Some model frontmatter still says `draft`; the host-supplied accepted product record and applied change are the authority evidence, not an inferred promotion of every file. ADR-0006 is Accepted; Juan accepted ADR-0007 on 2026-10-31, recorded in this PR pending merge. ADR-0008 and the corresponding application views remain Proposed. This document is a reviewable implementation candidate, not a current-runtime claim.

## Goals / Non-Goals

**Goals:** one server security boundary; provider-independent owner authorization; development-only credential lifetime; a separately verified GitHub actor; reuse observation presentation without inheriting operator authority.

**Non-Goals:** command entrypoints/journal, SQLite adoption under ADR-0008, scheduling/execution, production identity provisioning, durable source authorization across restarts, household identity administration, or the full mobile issue journey.

## Decisions

### 1. One ASP.NET Core workspace boundary

Propose `src/SoFa.Web` on .NET 10, serving the existing observation assets plus a small server-rendered account shell and JSON routes. Use framework cookie/OIDC middleware and an authorization fallback policy, with explicit anonymous exceptions only for a data-free landing/sign-in page, static assets, protocol callbacks and minimal liveness. APIs return 401/403 rather than HTML redirects; access-denied responses contain no operational data. Authentication runs before observation retrieval or any cache lookup.

Use a small server-side GitHub HTTP adapter to produce the existing observation JSON contract from the connected user. Preserve configured repositories, source identities/times, bounded collection, unavailable/stale explanations and safe rendering. Scope caches by owner session, GitHub user ID, repository and connection generation; disconnect, source rejection and logout invalidate them. Keep the existing Node collector/CLI as separate legacy tooling, never as a credential fallback. AppHost starts no legacy anonymous HTTP listener.

Alternatives: extending Node with another OIDC stack avoids a server migration but adds an identity stack outside the owner-tested middleware baseline; a reverse-proxy security perimeter leaves two route/configuration boundaries and risks exposing the old listener. A full SPA adds token/browser complexity without a requirement. The proposed migration is a larger adapter change but keeps identity, tokens and route authorization together.

### 2. Pin the development integration without claiming interoperability

Keep Aspire.AppHost.Sdk 13.6.1 and net10.0. Use its generic container resource API rather than introducing a separately versioned Keycloak hosting integration. Pin `quay.io/keycloak/keycloak:26.8.1@sha256:a78d6b79572168cb25886b80a8b5b8d4ed4390743e3eecb810538683e209dd6b`. Use `Microsoft.AspNetCore.Authentication.OpenIdConnect` 10.0.14, framework cookies/antiforgery and committed NuGet lock updates.

The verified owner answer reports exact-image startup/discovery with authorization code and S256, and successful restore/build of middleware 10.0.14 in a disposable .NET 10 app. These justify the candidate pins, not completed protocol compatibility. Reproduce build and demonstrate actual MFA login, callbacks, refresh/revocation and logout during implementation before claiming compatibility. If they fail, stop for a reviewed pin/design correction rather than floating a version.

Seed only a non-secret development realm, confidential client registration shape, exact redirect/logout URI templates and the client-specific MFA flow. Supply test credentials and TLS materials through operator-controlled local configuration, never committed realm users/passwords/client secrets. Start no PostgreSQL/execution store or supervisor. Test provider data is disposable; loss of the realm requires reconfirming the configured owner subject rather than email-based reassociation.

Use fixed development HTTPS application/issuer hostnames and ports registered by the operator. Both browser and application must resolve the same issuer; do not substitute an internal container hostname as issuer. Configure Keycloak's HTTPS listener and hostname explicitly, using operator-supplied trusted development certificates. Configure separate named development/external issuer profiles, explicit mode selection and validation preventing mixed profile credentials/callbacks. External mode composes only SoFa, never provisions Keycloak, and is tested with synthetic configuration rather than a live production connection.

Alternative: a dedicated Aspire Keycloak package can be evaluated later but adds a compatibility pin not covered by the supplied probes. Floating tags and disabled HTTPS validation are rejected. Concrete DNS names and certificate paths are setup values, not unmade product decisions.

### 3. Owner and assurance policy for review

Identify the owner by an exact configured `(issuer, subject)` pair after full OIDC validation, never email. Fail startup on missing owner or issuer configuration. Use code flow, S256 PKCE, exact callbacks, state/nonce correlation and a five-minute authorization-transaction lifetime. Restrict return URLs to local allowlisted paths. Rotate the local session reference after authentication and account transitions.

Candidate policy: require provider-enforced password plus TOTP, represented by Keycloak client LoA `2` with an `acr=2` mapping configured and verified for that actual flow. Request that assurance explicitly and reject absent/lower assurance; a claim label alone is not proof that the realm enforces MFA. Require valid `auth_time`, tolerate at most 60 seconds of clock skew, and reject authentication older than four hours. Initial and absolute-limit sign-in uses `max_age=0` with the required assurance so an unattended provider SSO cookie cannot simply create a new four-hour application session. No local password handling or claim of a formally certified assurance level is introduced.

Use a server ticket store with cryptographically random opaque cookie references, not a serialized ticket in the browser. Candidate cookie policy: `__Host-` prefix, Secure, HttpOnly, Path=/, no Domain and SameSite=Lax. Framework OIDC correlation/nonce cookies can use SameSite=None plus Secure where the callback protocol requires it; they contain no provider token.

Sessions expire at 15 minutes idle or four hours from provider `auth_time`, whichever is first. Only validated user navigation or explicit anti-CSRF-protected interaction can renew idle time; observation polling and background status requests cannot. Keep a non-renewable absolute deadline in the server record. Test deadlines with a controllable clock. Reject requests at or after the deadline without a grace session.

Revalidate the provider session synchronously when its last successful validation is five minutes old, using the confidential client's online refresh grant without offline access. Keep provider access/refresh tokens only in the server record, rotate refresh tokens atomically and serialize concurrent refresh per session. Validate refreshed identity consistently; never move the original authentication/absolute deadline. Invalid-grant or recognized revocation destroys the local session. Provider timeout/network failure blocks protected requests while due and preserves the original deadlines for a bounded retry, never granting access from stale validation. Before validation becomes due an otherwise valid session can continue. This is bounded detection, not instantaneous remote logout; enforce and demonstrate the actual Keycloak online-session/refresh behavior.

Logout is an anti-CSRF-protected POST that removes the server ticket, connection credentials and caches before optional provider end-session redirect. An unavailable provider cannot preserve local access. Show that the provider may still hold SSO and distinguish local logout from global logout; any subsequent sign-in requires the fresh ceremony. An operator session invalidation is immediate.

Alternatives: a long-lived encrypted browser ticket simplifies storage but prevents reliable server invalidation; password-only assurance and sliding sessions without an absolute cap are rejected for unattended phone use. More restrictive five-minute sessions or mandatory MFA on every refresh add friction without making provider revocation instantaneous. These chosen values are explicit proposal decisions for Juan's review, not previously approved product constants.

### 4. Explicit GitHub App user authorization

Use a separately registered development GitHub App with user authorization, expiring user access tokens and server-held refresh tokens. Install it only on designated test repositories. Request metadata/read, issues/read-write and Actions/read grants to bound the intended foundation and later issue-entry slice; issue-writing remains unimplemented and unreachable in this story. Do not request administration, contents-write or workflow-dispatch grants. User tokens preserve the actual user and intersect app installation and user authority; never use an installation token to impersonate that user.

A protected anti-CSRF POST starts linking. Store single-use random callback state for at most five minutes, bound to the current owner session and connection generation; reject callbacks after logout, expiry, reuse or session mismatch. Exchange the code server-side with the app's confidential credentials. Verify `/user`, recording numeric ID and login rather than email. Show the verified profile and repository permission result; require explicit owner confirmation before replacing an already connected different account. A cancelled/failed replacement leaves the previous still-valid connection intact.

Use only the user's token for configured-repository reads and source permission checks. Before each dependent data/action request verify current repository visibility and the permission needed by that request; expose write readiness only when user write/maintain/admin access and required app grants are both confirmed. No write endpoint is added. Recheck a displayed connection status at least every five minutes while active and label the check time. An unknown check is unavailable, not sufficient. Refresh an expiring user token server-side only within the original SoFa session; refresh cannot extend application lifetime. On revoked credentials, remove them; on reduced repository permission, disable affected access and invalidate related cached data.

Disconnect deletes local authority and caches; local logout drops the session-linked connection. Document GitHub's separate authorization-revocation UI: local disconnect does not claim to revoke the provider-wide grant. Restart requires explicit relinking, even if GitHub reuses an existing consent. No callback, connection operation or page load may post an issue comment, change labels or request a workflow. Future commands must still use Lighthouse/source policy; connection does not establish command eligibility.

Alternatives: a classic OAuth App with broad `repo` scope is simpler but less repository-bounded; a PAT or operator `gh` credential loses the explicit ceremony; an installation bot loses the actual actor. The GitHub App registration and designated test repository are operator setup, not authorization to provision them from this engine.

### 5. Data lifetime and interface boundary

Maintain process-local owner sessions, provider refresh state, GitHub connections and single-use correlation records in the new application. Store only stable identities, needed protected credentials, deadlines, validation timestamps and connection generation. Use bounded capacity and purge expired records; process restart invalidates all of them and requires fresh login/linking. Never write tokens into logs, fixtures or browser storage. This deliberate development lifetime avoids introducing ADR-0008 persistence as a hidden dependency; production protected persistence, keys, backups and recovery remain #58 release work.

Proposed routes: data-free GET `/`; anti-CSRF POST `/auth/login` and `/auth/logout`; middleware callbacks `/signin-oidc` and `/signout-callback-oidc`; protected GET `/workspace`, `/api/session` and `/api/observations`; anti-CSRF POST `/connections/github/start`, `/connections/github/confirm` and `/connections/github/disconnect`; protocol-only GET `/connections/github/callback`. A callback exception never grants access to other routes. Public static assets contain no identities or repository payload. Minimal liveness reveals no provider/repository configuration. Unknown routes remain denied/not found, not a proxy or command surface.

Apply no-store to protected content, restrictive CSP with only the required same-origin form actions, origin/host validation and framework antiforgery. Keep token exchange and source requests server-side with fixed destinations and bounded timeouts. Disable body/query logging on protocol routes, token logging and sensitive identity-library diagnostics. Suppress or sanitize callback URLs before request logging. Never retain token-bearing HARs, traces or authorization-code screenshots; manual evidence uses safe post-callback screens and a redacted event checklist.

## Risks / Trade-offs

- [New server/adapter migration can regress source observation] → Preserve response-shape, configured-repository, coalescing, stale/unavailable and safe-rendering regression tests alongside authorization tests. No operator fallback is allowed.
- [MFA/refresh semantics differ from discovery] → Demonstrate the real configured realm, assurance enforcement, logout/revocation and concurrent refresh; owner probes are not substitutes.
- [Phone trusts different DNS/certificates] → Use one stable issuer and actual phone trust/reachability checks, never certificate-validation bypasses.
- [Broad installation or insufficient user grants] → Select only test repositories, display current effective authority and fail closed on uncertainty.
- [Process-local credentials force reauthentication after restart] → Make this explicit in UI/runbooks and test it; do not introduce an unreviewed persistent store.
- [Remote provider revocation is not instantaneous] → Publish the five-minute maximum revalidation interval, deny when due validation fails, and invalidate recognized revocation immediately.
- [Proposed architecture could be mistaken for approval] → Preserve ADR status and require Juan's explicit architecture/session decision and release of the `future` hold before implementation.

## Migration Plan

After explicit owner approval only: add the new workspace and tests, compose it with isolated Keycloak, then switch the documented supported Aspire entrypoint to the authenticated successor. Do not launch both public surfaces. Keep Node collector and observer tests clearly identified as legacy compatibility coverage until intentionally retired; adjust browser tests to target the new protected entrypoint and retain authenticated source presentation coverage.

Rollback stops the new development workspace and discards process-local sessions/connections. It must not silently restore anonymous network access or change repository execution. A separately invoked historical observer remains non-privileged and is not an authenticated fallback. No source mutation or execution migration is required. Update canonical architecture views using their owning skill as implementation progresses, clearly separating implemented facts from proposed work. ADR-0008 is not a prerequisite.

## Verification and evidence plan

- Unit/component: exact owner/issuer and assurance checks, deadlines with fake clock, non-renewing polling, callback single use, atomic refresh, revocation/failure handling, cache isolation and credential-safe errors.
- Server integration: route inventory with anonymous/wrong-owner/owner requests, real middleware validation through a local test issuer, antiforgery failure and no mutation, restart/logout invalidation, repository allowlist and permission-loss cases. Test doubles establish deterministic coverage, not provider interoperability.
- Provider integration: actual pinned Keycloak startup, configured MFA sign-in/logout, forged/replayed/expired callbacks, absolute reauthentication, online refresh failure/revocation and provider outage. Retain sanitized results, not token-bearing traces.
- Source integration: actual GitHub App user `/user`, designated real repository permission success and insufficient/revoked cases; record account/repository identifiers only with operator consent. Assert no issue mutation or workflow request during link/reconnect/disconnect.
- Visual/accessibility: Playwright desktop and phone-width states for signed out, owner, wrong owner, unlinked, linked, insufficient access and provider/source failure. Run axe and keyboard/focus checks. Redact screenshots and supplement them with an actual phone LAN/VPN redirect/login/link/logout checklist, device/browser and time, endpoint reachability and safe account confirmation. Emulation alone is not acceptance.
- Delivery: add .NET build/test and authentication integration commands to `npm run verify` and `.github/workflows/verify.yml`, keeping existing deterministic checks. Review all repository Definition of Done dimensions with Juan; retain CI links, test results, redacted manual evidence and known limitations. The publisher runs OpenSpec strict validation before publishing this candidate; the engine has not run a CLI or runtime.

## Review boundary and setup values

There is no unresolved design choice hidden in the task list. Juan may accept or request changes to the candidate framework, MFA/session values and ephemeral development storage before implementation; none is accepted here. Operator-selected DNS names/ports, certificates, exact configured subject, development GitHub App registration and designated test repository can be supplied later without changing these contracts or slices. Never request credentials in a public issue or proposal. Production credential storage and provider provisioning remain separately unresolved under #58, not claims of this proposal.
