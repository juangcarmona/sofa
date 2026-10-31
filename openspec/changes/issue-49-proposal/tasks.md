# Tasks

All tasks are future work. The candidate is `planned`, not approved implementation. Complete groups in order; Juan must accept the approach and explicitly release the `future` hold before group 2.

## 1. Confirm authority and shared contract

- [ ] 1.1 Record the adopted Lighthouse 0.12.0 identity guide revision/hash against the verified owner answer and check its installed copy when dependencies are available; verify a review matrix covers external identity, isolated development provider, code/PKCE, server sessions, issuer/subject, separate GitHub authorization and real-provider evidence (AC6).
- [ ] 1.2 Obtain Juan's explicit decision on this framework, MFA/session/revocation policy and development storage lifetime, and resolve ADR-0007 through `arcshape-adr` without accepting ADR-0008 by implication; verify linked owner decisions and explicit release of the implementation hold before any implementation.

## 2. Compose isolated identity and application

- [ ] 2.1 Add `SoFa.Web`, solution/test references, locked OIDC middleware 10.0.14 and the generic Aspire 13.6.1 container pinned to the exact Keycloak 26.8.1 digest; verify locked restore/build and actual development resources start with no supervisor or execution database (AC1, AC6).
- [ ] 2.2 Add non-secret realm/client/MFA configuration and isolated development/external issuer profiles; verify mixed-profile and missing-owner/credential settings fail safely, external mode provisions no provider, and no production credentials/connectivity are used (AC1, AC2).
- [ ] 2.3 Document operator-supplied development credentials, exact callbacks, HTTPS issuer/application addressing and phone trust setup without committing secrets; verify configuration tests and an actual browser reachability check for both endpoints, retaining safe startup evidence (AC1, AC2).

## 3. Enforce the owner session boundary

- [ ] 3.1 Add cookie/OIDC middleware, fallback authorization, exact issuer/subject and verified MFA/auth_time admission, single-use five-minute correlation and local-only return URLs; verify deterministic middleware tests deny anonymous/direct-route/wrong-owner/wrong-issuer/insufficient-assurance and forged, expired or replayed callbacks (AC3, AC4).
- [ ] 3.2 Add bounded server-side ticket/credential storage, opaque cookies, rotation, 15-minute idle/four-hour absolute expiry and five-minute provider refresh revalidation with atomic concurrent refresh; verify fake-clock boundary tests, polling does not renew, provider failure grants no stale access, and recognized revocation/restart destroys access (AC3, AC4).
- [ ] 3.3 Serve the observation successor behind owner authorization, port the source adapter to connected-user HTTP access and isolate caches by connection/repository; verify preserved observation JSON, bounded collection/coalescing, source links/times and stale/unavailable behavior, no operator credential fallback and no anonymous legacy listener in Aspire (AC3).
- [ ] 3.4 Implement anti-CSRF login/logout and safe error/retry states; document session deadlines, MFA, bounded revocation and local versus provider logout. Verify real pinned Keycloak MFA login/logout, expiry and fresh absolute-limit reauthentication, forged/replayed callbacks, CSRF rejection, provider outage and online-session revocation without any execution cancellation (AC4, AC6).

## 4. Connect the actual GitHub user

- [ ] 4.1 Implement session-bound explicit GitHub App user authorization, five-minute single-use state, server token exchange, verified `/user` and different-account confirmation; verify callback-session mismatch, replay, cancellation and failed replacement tests leave no forged association and no installation/operator token is used (AC5).
- [ ] 4.2 Implement configured-repository live permission/app-grant checks, safe source reads and expiring user-token refresh; verify deterministic insufficient/unknown/out-of-scope cases and actual GitHub user plus sufficient/insufficient access against the operator-designated real test repository, with redacted evidence (AC5, AC6).
- [ ] 4.3 Implement disconnect, permission-loss/revocation cache invalidation and connection-status freshness; document process-local lifetime and separate GitHub grant revocation. Verify recognized token loss blocks dependent operations, refresh never extends the SoFa session, logout removes local source credentials, and link/reconnect/disconnect send no issue mutation, label change, workflow request or execution cancellation (AC4, AC5).

## 5. Deliver accessible account and denial states

- [ ] 5.1 Add clear provider identity, distinct GitHub acting-account/repository authority, sign-in/out/link/disconnect controls and safe denial/retry messages; verify Playwright keyboard-only operation, visible focus, axe checks and redacted desktop/phone-width screenshots for signed-out, denied, unlinked, linked, insufficient and unavailable states (AC7).
- [ ] 5.2 Document the actual-device checklist and demonstrate sign-in, GitHub connection and sign-out on a real phone on LAN/VPN; verify trusted reachable issuer/application/callbacks and retain redacted device/browser/time, redirect outcome and safe identity evidence, not only emulated screenshots (AC7).

## 6. Integrate verification and review evidence

- [ ] 6.1 Wire new .NET build/tests and provider integration coverage into the repository verification/CI path; run `npm run verify` and the real-provider/source/device matrix from design.md, inspect browser storage/logs/errors/retained evidence for token leakage using controlled test values, and deliver an AC1-AC7 evidence index distinguishing mocks, owner probes and actual results. Verify all configured checks pass or stop without claiming completion.
- [ ] 6.2 Update supported startup/user/runbook documentation, tests documentation and affected arc42 views through their owning skills, preserving ProductShape authority and ADR statuses; verify `npm run lint:docs`, architecture/product checks, documented commands and explicit rollback behavior without anonymous fallback.
- [ ] 6.3 Present the implementation with CI links, redacted manual evidence, remaining #58 release limitations and a dimension-by-dimension Definition of Done review; verify Juan's human review is recorded and no implementation, integration, merge or release approval is inferred from this proposal.

## Workflow follow-up

- The trusted publisher validates this candidate with OpenSpec 1.15.0 strict validation before opening its proposal PR.
- Archive only after implementation and the repository's review requirements are satisfied; retain the owner decisions and evidence links.
