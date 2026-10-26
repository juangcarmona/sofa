# Spec Delta

## Purpose

Provide an isolated authenticated control boundary that preserves the requesting user's repository authority without granting privilege to the existing observation surface.

## ADDED Requirements

### Requirement: Isolate privileged controls
The system SHALL keep the existing observation surface read-only and SHALL expose controls only through a separately authenticated loopback boundary accepted by the owner before implementation. LAN reachability SHALL NOT grant authority.

#### Scenario: Observation remains non-privileged
- **WHEN** an observation endpoint receives a mutation request or a control-shaped URL
- **THEN** it rejects the request without dispatch or access to control credentials.

#### Scenario: Control boundary is not approved for LAN exposure
- **WHEN** control configuration requests a non-loopback listener
- **THEN** startup refuses that configuration and explains that a separate security decision is required.

### Requirement: Protect session-bound actions
The system SHALL require a valid authenticated session and anti-forgery protection for every control mutation, including GitHub connection and logout. It SHALL deny anonymous, expired, unauthorized and cross-site actions before dispatch and SHALL invalidate local access on sign-out.

#### Scenario: Invalid session or cross-site submission
- **WHEN** a control POST has no valid session, an expired session, an invalid anti-forgery token or an untrusted origin
- **THEN** the action is denied and no GitHub mutation occurs.

#### Scenario: Sign-out and expiry
- **WHEN** the owner signs out or the server-side session expires
- **THEN** subsequent control requests require fresh authentication and cannot reuse the previous session's GitHub connection.

### Requirement: Preserve the actual GitHub actor
The system SHALL require separately granted user-authorized GitHub access and verify its stable user identity against the configured owner binding for each mutation. OIDC alone SHALL NOT grant GitHub permission. Repository workflows SHALL retain their own actor and capability checks; shared or installation credentials SHALL NOT impersonate the user.

#### Scenario: Authorized user dispatch
- **WHEN** the authenticated owner's bound GitHub user has authorized the selected repository action
- **THEN** submission uses that user's delegated access and the unchanged repository router evaluates the actual user as its actor.

#### Scenario: Missing grant or identity mismatch
- **WHEN** OIDC is valid but GitHub access is absent, revoked, insufficient or identifies a different user
- **THEN** the action is denied without fallback to an operator, bot or installation credential.

### Requirement: Isolate development identity lifecycle
Local Aspire startup SHALL reproducibly initialize an isolated development realm, client and development accounts. Identity data SHALL survive normal restarts. Reset SHALL be explicit, documented and restricted to development identity data; development identities and secrets SHALL remain separate from production and source control.

#### Scenario: First startup and normal restart
- **WHEN** a developer starts the documented Aspire configuration and later restarts it normally
- **THEN** initial development identities are usable and subsequent startup preserves identity changes without reapplying destructive initialization.

#### Scenario: Explicit development reset
- **WHEN** a developer confirms the documented development reset
- **THEN** only the isolated development identity store is reset and its reproducible initialization is restored.

### Requirement: Connect to independently operated production identity
The system SHALL use standards-based external OIDC with maintained authentication support for sign-in, sign-out and expiry. Production issuer and client settings SHALL be configurable. SoFa SHALL NOT provision, reset or delete the production provider or accounts, or implement password storage or credential recovery.

#### Scenario: External issuer configuration
- **WHEN** production is configured with an independently operated issuer and registered client
- **THEN** the same OIDC integration supports authentication without starting development initialization or invoking provider administration.

#### Scenario: Invalid issuer settings
- **WHEN** production issuer validation or required configuration fails
- **THEN** controls fail closed without development-account or password-based fallback.

### Requirement: Keep credentials out of observable output
The system SHALL keep secrets, tokens and authorization headers out of page content, client storage, logs and published evidence. Authentication failures SHALL use sanitized messages. Observational data and receipt exports SHALL NOT contain control credentials.

#### Scenario: Provider or GitHub error contains sensitive data
- **WHEN** an authentication or GitHub client failure contains a token or authorization header
- **THEN** rendered errors, logs and exported test or review evidence contain only sanitized failure information.
