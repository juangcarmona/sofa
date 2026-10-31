# Owner access

## Purpose

Protect operational factory content with explicit owner authentication and bounded sessions while supporting desktop and phone use on private networks.

## ADDED Requirements

### Requirement: Owner-only operational access

SoFa SHALL authenticate operational reads and writes on the server and admit only the configured exact issuer/subject pair. Network location, display name, email or provider sign-in alone MUST NOT grant access.

#### Scenario: Configured owner enters

- **WHEN** the trusted provider authenticates the configured issuer/subject with the required assurance
- **THEN** the owner can access protected content and see the signed-in identity and sign-out action

#### Scenario: Anonymous direct request

- **WHEN** an anonymous caller requests an operational page or directly calls its data or mutation route
- **THEN** a page offers sign-in and an API returns an authentication-required response without protected data or side effects

#### Scenario: Valid but different identity

- **WHEN** a valid callback contains a different subject or issuer, even with the owner's email
- **THEN** access is denied without issuing an owner session or disclosing protected content

### Requirement: Authentication ceremony

SoFa SHALL use authorization-code flow with PKCE S256 and verify issuer, audience, signature, lifetime, state and nonce. The candidate policy requires provider-enforced password plus TOTP, verified through its configured assurance claim and authentication time. No local password bypass SHALL exist.

#### Scenario: Fresh sufficiently assured login

- **WHEN** the configured owner completes the required provider flow with a fresh valid response
- **THEN** SoFa creates a new server-managed session and rotates pre-authentication session identifiers

#### Scenario: Invalid or replayed response

- **WHEN** a callback has forged or reused state, an invalid nonce, wrong issuer/audience/signature, expired correlation or reused authorization response
- **THEN** no authenticated session is created and only a safe retry explanation is shown

#### Scenario: Insufficient assurance

- **WHEN** the provider returns a valid identity without the configured assurance or acceptable authentication time
- **THEN** access is denied rather than treating password-only sign-in as sufficient

### Requirement: Bounded server sessions

The candidate session policy SHALL expire access after 15 minutes without authenticated user activity or four hours from provider authentication, whichever occurs first. Automated polling MUST NOT renew inactivity. Reauthentication at the absolute limit MUST require a fresh provider ceremony; refresh MUST NOT extend that limit.

#### Scenario: Idle and absolute expiry

- **WHEN** either boundary is reached, including while an unattended page continues polling
- **THEN** the next protected request is denied and the owner must authenticate again

#### Scenario: Activity before expiry

- **WHEN** the owner deliberately navigates or performs an authenticated UI action before expiry
- **THEN** the idle deadline can advance but never beyond the original four-hour boundary

### Requirement: Revocation and provider failure

SoFa SHALL revalidate provider session authority at least every five minutes during use and block protected requests when due validation cannot succeed. Recognized revocation MUST invalidate local access immediately. Provider failure MUST NOT extend a deadline or grant a bypass.

#### Scenario: Provider unavailable

- **WHEN** provider revalidation becomes due and the provider is unavailable
- **THEN** protected requests are blocked with a retry explanation without extending the idle or absolute deadlines

#### Scenario: Revoked provider session

- **WHEN** revalidation detects provider revocation or an operator invalidates the local session
- **THEN** subsequent protected requests are denied immediately
- **AND** undetected provider revocation has no stronger immediacy claim than the five-minute revalidation bound

### Requirement: Credential containment

Provider and GitHub tokens and session tickets SHALL remain server-side and MUST NOT appear in browser storage, page content, logs or retained evidence. Browser session references MUST use Secure, HttpOnly cookies; protected responses MUST prevent shared caching.

#### Scenario: Browser and evidence inspection

- **WHEN** login and GitHub linking succeed or fail
- **THEN** browser-visible storage contains no provider/GitHub token or session ticket
- **AND** logs, errors, screenshots and retained reports contain no credential, authorization code, callback query or token-bearing trace

### Requirement: Protected session mutations

Logout and application/source-connection mutations SHALL require server-enforced anti-CSRF checks. Logout MUST invalidate the server session before attempting provider logout. Session loss MUST NOT cancel independent repository execution.

#### Scenario: Cross-site request rejected

- **WHEN** logout, link initiation, disconnect or another mutation lacks valid same-session anti-CSRF proof
- **THEN** no session or connection mutation occurs

#### Scenario: Logout with provider failure

- **WHEN** the owner signs out and provider logout is unreachable
- **THEN** local access and session-linked credentials are already invalidated and the remaining provider-session limitation is explained
- **AND** no cancellation is sent to repository execution

### Requirement: Authenticated observation successor

The supported workspace SHALL protect observation pages and APIs and obtain repository data only within the connected user's configured source scope. It MUST NOT expose the legacy anonymous listener or fall back to operator GitHub CLI authority.

#### Scenario: Owner has no source connection

- **WHEN** the authenticated owner opens the workspace without sufficient source authority
- **THEN** the account shell explains the missing GitHub connection without disclosing repository observations

#### Scenario: Protected observation response

- **WHEN** the authenticated owner with sufficient repository read authority requests observations
- **THEN** source identities, timestamps, unavailable/stale states and source links are preserved
- **AND** stale observation data never proves current permission for a dependent action

### Requirement: Accessible desktop and phone entry

The owner SHALL be able to sign in, sign out and connect GitHub on desktop and a real phone over LAN/VPN. Desktop controls MUST be keyboard-accessible with visible focus, clear identity labels and understandable denial/retry messages.

#### Scenario: Desktop keyboard journey

- **WHEN** the owner completes the authentication and connection journey using only a keyboard
- **THEN** every control is reachable, focus is visible and the provider identity and separate GitHub actor are distinguishable

#### Scenario: Real phone journey

- **WHEN** the owner performs sign-in, GitHub connection and sign-out on a real phone
- **THEN** redirects and return navigation work on the permitted network and sign-out removes protected access
- **AND** delivery retains redacted device/network observations rather than relying on viewport emulation alone
