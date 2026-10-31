# Identity environments

## Purpose

Provide isolated development authentication and a consistent external-issuer boundary without requiring production identity infrastructure.

## ADDED Requirements

### Requirement: Isolated development composition

The development startup SHALL compose SoFa and a dedicated version-pinned identity provider with non-secret test realm/client configuration. It MUST NOT start production identity connectivity, the retired supervisor or an execution database.

#### Scenario: Development startup

- **WHEN** the configured developer starts the existing Aspire solution
- **THEN** the application and dedicated test provider start with recorded version pins and test realm/client settings
- **AND** no production provider, supervisor or execution database is started

#### Scenario: Missing development credential configuration

- **WHEN** required local test credentials have not been supplied
- **THEN** startup reports a safe configuration failure without falling back to production credentials or committing credentials into the realm seed

### Requirement: Reachable protected endpoints

The application and selected issuer SHALL expose stable HTTPS addresses reachable by the desktop and real phone on the permitted LAN/VPN. Redirects MUST use exact registered callback addresses and trusted certificates without disabling certificate validation.

#### Scenario: Phone follows redirects

- **WHEN** the owner signs in from a real phone on LAN/VPN
- **THEN** issuer, application and callback addresses resolve and are trusted on that phone
- **AND** redirects do not depend on desktop loopback addresses

#### Scenario: Callback origin is not registered

- **WHEN** an authorization request uses an unregistered callback or untrusted origin
- **THEN** authorization fails without creating an application session

### Requirement: External issuer configuration

The same application boundary SHALL accept an externally configured production issuer and client without provisioning that provider. Development accounts, clients, callbacks and credentials MUST remain separate; configuration validation MUST reject mixed environment settings.

#### Scenario: Alternate external issuer configuration

- **WHEN** an isolated configuration test supplies an external issuer profile
- **THEN** the application selects only that issuer and client and creates no provider resource
- **AND** the test requires no production credential or live production login

#### Scenario: Mixed environment settings

- **WHEN** development mode references production-profile client or callback settings
- **THEN** configuration validation rejects the combination before authentication is enabled

### Requirement: Shared identity guidance and runtime proof

Delivery SHALL record the adopted Lighthouse package and matching identity guidance, and verify the implemented boundary against that contract. Documentation, package restoration and provider discovery MUST NOT substitute for actual sign-in and source-identity evidence.

#### Scenario: Delivery evidence review

- **WHEN** the developer presents the completed authentication slice
- **THEN** evidence identifies the adopted package, matching guide revision/hash, runtime pins, real Keycloak sign-in and actual GitHub user connection
- **AND** owner-reported preliminary probes are distinguished from the implementation's own results
