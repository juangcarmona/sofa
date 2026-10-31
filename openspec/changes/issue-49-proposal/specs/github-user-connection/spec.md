# GitHub user connection

## Purpose

Associate the signed-in owner with an explicitly authorized GitHub user and expose current repository authority without requesting repository execution.

## ADDED Requirements

### Requirement: Explicit actor-preserving connection
An authenticated owner SHALL explicitly initiate GitHub user authorization independently of provider login. SoFa MUST verify the source user, bind the connection to the initiating owner session and display the verified account. Email matching, operator credentials and installation bot tokens MUST NOT substitute for user authority.

#### Scenario: Explicit connection
- **WHEN** the owner initiates and completes separate GitHub authorization
- **THEN** SoFa verifies the user through GitHub, associates the returned stable user ID with that session and displays its account login and source profile link
- **AND** the provider subject and GitHub account remain distinct facts

#### Scenario: Callback belongs to another session
- **WHEN** a GitHub callback is unsolicited, expired, replayed or belongs to a different initiating session
- **THEN** SoFa rejects it and leaves the existing connection unchanged

#### Scenario: Provider login only
- **WHEN** the owner signs in through Keycloak, including with an email matching a GitHub account
- **THEN** no GitHub account is implicitly linked and no source authority is inferred

### Requirement: Current repository authority
SoFa SHALL check the actual connected user's permissions and application grants for the configured repository on the server. It MUST distinguish readable repository access from authority needed by a dependent operation and MUST NOT use cached observations as permission evidence.

#### Scenario: Designated repository is authorized
- **WHEN** live source checks establish the connected user's access and the required application grants
- **THEN** SoFa displays the verified actor and repository-specific available authority
- **AND** those checks do not approve any future command or bypass repository policy

#### Scenario: Insufficient or unknown permission
- **WHEN** repository authority is insufficient, the repository is outside configuration or the source cannot verify permission
- **THEN** the dependent operation is blocked with a safe explanation
- **AND** an unrelated or private repository is not exposed through arbitrary client selection

### Requirement: Connection lifecycle without execution
Missing, expired, revoked or disconnected source authority SHALL block dependent operations. Source authority MUST be rechecked before each dependent operation; connection status displayed as current MUST be revalidated at least every five minutes. Link, reconnect and disconnect MUST NOT request repository work.

#### Scenario: Source permission is lost
- **WHEN** the source rejects a token or a current permission check detects removal of required permission
- **THEN** SoFa disables the affected capability, discards invalid authority and explains whether reconnection or permission correction is required

#### Scenario: Reconnect or disconnect
- **WHEN** the owner explicitly reconnects or disconnects GitHub
- **THEN** the old local credential is replaced only after successful verified authorization or is removed on disconnect
- **AND** no issue comment, command request, label change, workflow dispatch or execution cancellation is sent

#### Scenario: Refresh or source failure
- **WHEN** refreshing an expiring source credential or checking authority fails
- **THEN** dependent operations remain blocked until authority is re-established
- **AND** SoFa does not silently switch to a bot or operator credential
