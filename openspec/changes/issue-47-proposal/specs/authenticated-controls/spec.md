# Spec Delta

## Purpose

Provide separately authenticated control access from the owner's phone over LAN/VPN while keeping anonymous factory observation non-privileged and repository authorization independent.

## ADDED Requirements

### Requirement: Preserve the anonymous observation boundary
The system SHALL keep anonymous observation read-only and SHALL isolate its routes, sessions and credentials from privileged controls. LAN or VPN reachability SHALL NOT confer control authority.

#### Scenario: Anonymous mutation through observations
- **WHEN** an anonymous client submits a mutation or control-shaped URL to the observation service
- **THEN** no dispatch occurs and the service exposes neither control credentials nor a mutation proxy.

### Requirement: Support approved phone access over HTTPS
The system SHALL support the separately approved control origin over authenticated HTTPS from a real phone on LAN/VPN. It SHALL accept only configured origins and callback destinations. Deployment SHALL remain disabled until the Product Change and independent security boundary are accepted.

#### Scenario: Phone signs in over LAN or VPN
- **WHEN** the owner opens the accepted HTTPS control origin on a real phone over the configured LAN/VPN route
- **THEN** sign-in uses reachable issuer and callback URLs, returns to that origin and permits review of a refine request without desktop loopback access.

#### Scenario: Unapproved exposure or host
- **WHEN** configuration lacks exposure acceptance, uses HTTP for controls or supplies an unapproved host or forwarded origin
- **THEN** controls fail closed and no mutation becomes available.

### Requirement: Protect session-bound actions
Every control mutation SHALL require an authenticated, unexpired session and CSRF protection. Sign-out SHALL invalidate local access. Protocol callbacks SHALL validate their correlation data without allowing arbitrary redirects or application mutation by GET.

#### Scenario: Cross-site or expired submission
- **WHEN** a mutation has no valid session, an expired session, invalid antiforgery data or an untrusted origin
- **THEN** it is denied before GitHub mutation, including when sent from a device on the trusted LAN.

#### Scenario: Invalid callback or signed-out replay
- **WHEN** callback state or nonce fails, a redirect is unregistered, or a prior session is reused after sign-out
- **THEN** authentication or control access is rejected without dispatch.

### Requirement: Preserve actual GitHub authority
The system SHALL require a separate GitHub user grant and verify its stable user identity against the configured owner binding for each mutation. OIDC authentication alone SHALL NOT grant repository permission. Repository actor and capability checks SHALL remain authoritative, with no shared bot or installation-token fallback.

#### Scenario: Owner dispatches using a separate user grant
- **WHEN** the signed-in owner's verified GitHub user grant permits the selected action
- **THEN** the source workflow evaluates the actual user as actor and independently admits or rejects the request.

#### Scenario: Valid OIDC without GitHub authority
- **WHEN** OIDC is valid but the GitHub grant is absent, mismatched, revoked, expired or insufficient
- **THEN** dispatch is denied without falling back to observation, operator or application credentials.

### Requirement: Isolate development identity lifecycle
Development startup SHALL reproducibly initialize isolated development identity resources and SHALL preserve identity changes across normal restarts. Reset SHALL require explicit confirmation and affect only development resources. Development identities and secrets SHALL remain isolated from production.

#### Scenario: Startup restart and explicit reset
- **WHEN** the documented Aspire environment is started, an account changes, the environment restarts, and the developer later confirms reset
- **THEN** normal restart preserves the change and only the explicit reset restores the isolated initial identity fixture.

### Requirement: Use independently operated production identity
Production SHALL use configured standards-based external OIDC for sign-in, sign-out and session expiry. SoFa SHALL NOT provision, reset or delete the production provider or accounts, store passwords or implement credential recovery. Invalid production configuration SHALL fail closed.

#### Scenario: External issuer and invalid configuration
- **WHEN** production is configured with an external issuer or its required settings fail validation
- **THEN** valid settings use that issuer without development bootstrap or provider administration, and invalid settings deny controls without a development fallback.

### Requirement: Keep credentials out of observable output
The system SHALL keep credentials and tokens out of page content, browser storage, logs, receipts and published evidence. Authentication and provider errors SHALL expose only sanitized information.

#### Scenario: Sensitive provider error
- **WHEN** an identity or GitHub response includes sensitive error data
- **THEN** rendered errors, diagnostic events, exports and acceptance captures omit that data and retain only a safe failure category.
