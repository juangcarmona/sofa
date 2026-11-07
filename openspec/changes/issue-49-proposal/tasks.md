# Tasks

<!-- pdac-scope: cited -->

The owner authorized this development proposal and necessary dependencies on 2026-11-07, explicitly lifting the `future` hold (issue comment 6706085004). Build the independently testable identity/session foundations first, then compose and wire the runtime; complete each group's listed verification before marking it done. Preserve actual provider/source/device verification; production remains separate. Unchecked tasks are not complete. Deterministic source tests and host-only provider/UI evidence are recorded under `docs/delivery/evidence/issue-49/`. Actual GitHub consent/repository and physical-phone evidence remain open; no full issue acceptance or production readiness is claimed.

## 1. Confirm authority and shared contract

- [x] 1.1 Record the adopted Lighthouse 0.12.53 / SDLC 0.21.4 identity guide revision/hash against the verified owner answer and check its installed copy when dependencies are available; verify a review matrix covers external identity, isolated development provider, code/PKCE, server sessions, issuer/subject, separate GitHub authorization and real-provider evidence (AC6).
- [x] 1.2 Record Juan's acceptance of ADR-0007 through `arcshape-adr` (accepted on 2026-10-31 in this PR, pending merge). Record his acceptance of the .NET/Aspire application direction in ADR-0009. The 2026-11-07 owner grant covers this reviewed development proposal, including its concrete session/assurance/revalidation and process-local storage choices, and releases the development implementation hold (issue comment 6706085004). Preserve that linked decision; no production authorization is inferred. ADR-0008 remains Proposed.

- [x] 1.3 Assess the existing application against the selected Lighthouse `dotnet-backend` profile. Adopt its relevant pinned analyzers and CQRS guidance without overwriting SoFa; implement real `npm run verify:dotnet` and applicable .NET tests, connect them to CI, and record any justified differences. The official creator is only for an empty target, not this existing repository. Profile selection alone is not implementation evidence.

## 2. Compose isolated identity and application

- [x] 2.1 Add `SoFa.Web`, solution/test references, locked OIDC middleware 10.0.14 and the generic Aspire 13.6.1 container pinned to the exact Keycloak 26.8.1 digest; verify locked restore/build and actual development resources start with no supervisor or execution database (AC1, AC6).
- [x] 2.2 Add non-secret realm/client/MFA configuration and isolated development/external issuer profiles; verify mixed-profile and missing-owner/credential settings fail safely, external mode provisions no provider, and no production credentials/connectivity are used (AC1, AC2).
- [x] 2.3 Document operator-supplied development credentials, exact callbacks, HTTPS issuer/application addressing and phone trust setup without committing secrets; verify configuration tests and an actual browser reachability check for both endpoints, retaining safe startup evidence (AC1, AC2).

## 3. Enforce the owner session boundary

- [x] 3.1 Add cookie/OIDC middleware, fallback authorization, exact issuer/subject and verified MFA/auth_time admission, single-use five-minute correlation and local-only return URLs; verify deterministic middleware tests deny anonymous/direct-route/wrong-owner/wrong-issuer/insufficient-assurance and forged, expired or replayed callbacks (AC3, AC4).
- [x] 3.2 Add bounded server-side ticket/credential storage, opaque cookies, rotation, 15-minute idle/four-hour absolute expiry and five-minute provider refresh revalidation with atomic concurrent refresh; verify fake-clock boundary tests, polling does not renew, provider failure grants no stale access, and recognized revocation/restart destroys access (AC3, AC4).
- [x] 3.3 Serve the observation successor behind owner authorization, port the source adapter to connected-user HTTP access and isolate caches by connection/repository; verify preserved observation JSON, bounded collection/coalescing, source links/times and stale/unavailable behavior, no operator credential fallback and no anonymous legacy listener in Aspire (AC3).
- [ ] 3.4 Implement anti-CSRF login/logout and safe error/retry states; document session deadlines, MFA, bounded revocation and local versus provider logout. Verify real pinned Keycloak MFA login/logout, expiry and fresh absolute-limit reauthentication, forged/replayed callbacks, CSRF rejection, provider outage and online-session revocation without any execution cancellation (AC4, AC6).

## 4. Connect the actual GitHub user

- [x] 4.1 Implement session-bound explicit GitHub App user authorization, five-minute single-use state, server token exchange, verified `/user` and different-account confirmation; verify callback-session mismatch, replay, cancellation and failed replacement tests leave no forged association and no installation/operator token is used (AC5).
- [ ] 4.2 Implement configured-repository live permission/app-grant checks, safe source reads and expiring user-token refresh; verify deterministic insufficient/unknown/out-of-scope cases and actual GitHub user plus sufficient/insufficient access against the operator-designated real test repository, with redacted evidence (AC5, AC6).
- [x] 4.3 Implement disconnect, permission-loss/revocation cache invalidation and connection-status freshness; document process-local lifetime and separate GitHub grant revocation. Verify recognized token loss blocks dependent operations, refresh never extends the SoFa session, logout removes local source credentials, and link/reconnect/disconnect send no issue mutation, label change, workflow request or execution cancellation (AC4, AC5).

## 5. Deliver accessible account and denial states

- [x] 5.1 Add clear provider identity, distinct GitHub acting-account/repository authority, sign-in/out/link/disconnect controls and safe denial/retry messages; verify Playwright keyboard-only operation, visible focus, axe checks and redacted desktop/phone-width screenshots for signed-out, denied, unlinked, linked, insufficient and unavailable states (AC7).
- [ ] 5.2 Document the actual-device checklist and demonstrate sign-in, GitHub connection and sign-out on a real phone on LAN/VPN; verify trusted reachable issuer/application/callbacks and retain redacted device/browser/time, redirect outcome and safe identity evidence, not only emulated screenshots (AC7).

## 6. Integrate verification and review evidence

- [ ] 6.1 Wire new .NET build/tests and provider integration coverage into the repository verification/CI path; run `npm run verify` and the real-provider/source/device matrix from design.md, inspect browser storage/logs/errors/retained evidence for token leakage using controlled test values, and deliver an AC1-AC7 evidence index distinguishing mocks, owner probes and actual results. Verify all configured checks pass or stop without claiming completion.
- [x] 6.2 Update supported startup/user/runbook documentation, tests documentation and affected arc42 views through their owning skills, preserving ProductShape authority and ADR statuses; verify `npm run lint:docs`, architecture/product checks, documented commands and explicit rollback behavior without anonymous fallback.
- [ ] 6.3 Present the implementation with CI links, redacted runtime evidence, remaining #58 production limitations and an independent dimension-by-dimension Done review under the owner's explicit implementation/merge delegation. Retain the real authorization, never fabricate a human forge vote, and stop at a remaining owner-only or protected gate.

## Workflow follow-up

- The trusted publisher validates this candidate with OpenSpec 1.15.0 strict validation before opening its proposal PR.
- Archive only after implementation and the repository's review requirements are satisfied; retain the owner decisions and evidence links.

## Product grounding

<!-- pdac:cite id="UC-ACCESS-FACTORY" digest="sha256:0b58747905a775c38f02b058ea90e2a63481cf69e4708e9b14aabf79d1335170" -->

<!-- pdac:cite id="QR-EXECUTION-SECURITY" digest="sha256:9f39dd08a392db995a75f26c9a30ed4d4ec43f73ad0ccafbdfaa3d0eabc1528a" -->

<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->
