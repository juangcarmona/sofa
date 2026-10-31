---
name: shared-identity-custody-and-recovery
description: Proposed ownership and recovery boundary for SoFa's shared identity dependency.
status: Proposed
date: 2026-10-26
deciders: [Juan García Carmona]
tags: [identity, keycloak, infrastructure, recovery]
---

# ADR-0007: Shared identity custody and recovery

## Context

The owner selected shared Keycloak for SoFa sign-in and a separate authorized GitHub connection for repository actions. The initial Bastion placement was revised by the owner: node-home is production and Bastion is development. This proposed record now targets node-home for production identity. The active Product Change remains draft. This record proposes technical realization of that direction; it does not approve the product delta, deployment or current architecture changes. ADR-0006 remains applicable.

Read-only evidence on 2026-10-26:

- `juangcarmona/infra.homelab` already exists as a private repository. Its README describes Compose, WireGuard and an Nginx edge VPS, with no secrets in Git. This documents a topology; connectivity to those remote nodes was not tested.
- `/srv/stacks/README.md` assigns Compose definitions to `/srv/stacks/<stack>`, data to `/srv/data/<stack>` and root-only environment files to `/etc/bastion-stacks/<stack>`. Its Git repository has no remote configured. These are Bastion conventions, not a deployed Keycloak stack or verified node-home paths. Adapt the production paths to node-home during its infrastructure review.
- The inspected Docker daemon has one running container, `lighthouse-sofa-ghaw-runner`; no running Keycloak was observed there. Its configuration mounts `/var/run/docker.sock` and uses host networking. Docker security options do not report rootless mode. This is not evidence that an individual agent has the socket, but a compromised runner has host-daemon authority.
- No existing password manager or verified off-host backup arrangement has been established by this inspection. Secret values were not read.

## Decision proposed

### Ownership and deployment

Use the existing private infrastructure repository for pinned deployment definitions, non-secret realm/client policy, bootstrap procedures, upgrade instructions and recovery runbooks. Reuse upstream Keycloak images rather than forking Keycloak. Its local deployment is a projection of a reviewed repository revision. SoFa owns its OIDC integration, sessions and GitHub account connection; infrastructure owns the identity service, database, TLS, secret injection and backups. Lighthouse may supply reusable guidance but does not own personal credentials or operate Bastion.

Place a single production Keycloak service and its PostgreSQL database on node-home, without a cluster requirement. Check its actual capacity, storage, service inventory and administration before deployment; these were not verified remotely. Provision a dedicated SoFa client with narrow callbacks and permissions. Keep administrative credentials separate from application credentials. Use a stable HTTPS identity hostname reachable on LAN/VPN, independent of a physical machine name, so later recovery can preserve the issuer. Public edge exposure is not required for the original delivery.

Keep Bastion's development and CI execution outside the production host's management boundary. Verify firewall rules and absence of node-home administration keys, Docker API access, shared identity volumes, production deployment credentials and backup deletion credentials in runner contexts. Allow only required application traffic to the HTTPS identity endpoint. Physical separation removes the shared-host Docker authority problem for Keycloak only when these remote trust paths are also controlled.

Recommend placing the production SoFa service on node-home as well, so real GitHub user tokens and production sessions do not remain on a host controlled by development runners. This placement is a proposed consequence of the owner's environment split, not a deployed service. The owner explicitly selected a development Keycloak container composed by the existing Aspire solution. The development SoFa instance uses this separate provider with test accounts, its own OIDC client, distinct callbacks and test-repository credentials; it does not depend on production Keycloak being installed. Aspire is development composition, not a new execution scheduler or a production identity host. A separate client alone does not isolate production accounts, and development must not receive production administration privileges. A real-source delivery demonstration runs through the protected production service.

The previously proposed runner VM is no longer an identity-delivery prerequisite solely to protect Keycloak. Bastion runner hardening remains infrastructure work, including containment of development credentials. Production capacity protection and runner isolation must be verified at their respective hosts; do not disrupt the existing runner during this proposal.

### Secret custody

Keep service credentials out of Git, including private Git. Use an established password manager, accessible independently of this Keycloak, as owner-controlled custody for bootstrap/deployment secrets and emergency recovery material. The selected manager is unresolved. Retain an offline recovery copy independent of node-home, Bastion and the owner's phone.

Inject only the service-specific credentials needed at runtime, through protected files or supported secret mounts under the existing host convention. Restrict parent directories as well as files; adapt ownership to the actual service UID. Do not put credentials in command arguments, logs, issue bodies or build artifacts. File modes do not protect against host root or Docker-daemon control; production host authority must remain outside development and runner contexts.

Distinguish deployment secrets from runtime data. Keycloak users, credentials, signing state and client state live in its database. SoFa's GitHub user tokens belong in protected application storage with expiry/revocation handling, not in an operator's static environment file or the password-manager inventory. Protect encryption keys separately from encrypted token data. Reconnecting GitHub after disaster recovery is preferable to assuming restored tokens remain valid. A normal login never grants repository authority by itself.

SOPS/age is a possible later encrypted configuration workflow if Git-based secret distribution becomes necessary. It is not the initial default because the existing infrastructure policy excludes secrets from Git and an independent decryption-key recovery process would still be required. A private repository alone, GitHub Actions secrets alone and a new self-hosted vault with no recovery route do not solve owner custody.

### Backup and recovery

Use database-consistent PostgreSQL backups, encrypted before leaving the protected environment, with an off-host destination independent of node-home. Bastion must not be the sole backup destination or custodian; keep a protected copy outside both machines and preferably outside the home. Restic is the proposed backup transport and retention tool, not a database-consistency mechanism. Do not treat a live volume copy or realm export as a complete backup. Preserve the versioned deployment revision, required configuration and key material alongside the recoverable database state.

Keep backup decryption/recovery credentials outside the backup repository, node-home and Bastion. Give automated backup writers limited permissions; retain a protected copy or immutable history that compromised production or development writer credentials cannot delete. Confirm the destination, retention, acceptable data loss and recovery time with the owner before implementation.

Maintain an emergency route through host/VM administration and credential recovery that does not require Keycloak, SoFa or GitHub login. Remove temporary Keycloak bootstrap administrator accounts after permanent administration is established. The recovery runbook must cover issuer/DNS restoration, database restoration with compatible versions, administrative access, invalidating potentially stale sessions, secret rotation after suspected compromise and GitHub reconnection.

Acceptance evidence must include restoration into an isolated environment from off-host backups and independently held recovery material, followed by owner login, denial of an unauthorized account and separately verified GitHub authorization. Record the measured recovery time and restored backup age. No production readiness claim based solely on a successful backup command or local login.

## Alternatives considered

- Local-only setup: simple initially, but configuration and recovery knowledge can disappear with the machine.
- New Keycloak-only repository: unnecessary while the existing private infrastructure repository owns shared services.
- Plaintext secrets in a private repository: increases exposure through clones, history and automation and contradicts existing policy.
- SOPS/age now: valid if encrypted Git distribution is chosen, but adds key management without removing recovery obligations.
- Dedicated Vault deployment: defer until dynamic-secret or organizational requirements justify another critical service.
- Keycloak beside the Bastion runner, separated only by Compose: rejected as a security boundary because the runner controls that host daemon.
- Runner VM as an immediate prerequisite: the owner's separate production host removes that specific need; runner isolation can be assessed separately while excluding remote production authority.
- Entra instead: operationally viable, but superseded as the selected direction by the owner's explicit Keycloak choice.

## Consequences

Identity is reusable across applications and independently recoverable. Shared identity creates a shared availability dependency; existing admitted repository work continues when login is unavailable. The first authenticated delivery now has an explicit infrastructure prerequisite, separate from SoFa application code. Household onboarding, migration of other applications and public hosting remain later decisions.

Provider placement and the Aspire development container are owner-supplied decisions. Production readiness still requires confirmation of the independent password manager, backup destination and recovery objectives, plus validation of node-home capacity, the production/development trust boundary, production SoFa placement and the private HTTPS/DNS path. No infrastructure files, service registrations, secrets or host configuration were changed. The proposed ADR is intentionally absent from the current architecture index until acceptance.

## Proposal history

- 2026-10-26: proposed shared identity on Bastion and assessed a runner VM to separate host authority.
- 2026-10-26: revised before acceptance following the owner's node-home production / Bastion development direction. Keycloak targets node-home; a runner VM is no longer the prerequisite for this identity placement. Production SoFa placement and operational details remain proposed.

- 2026-10-26: owner closed provider discussion and selected an Aspire-composed development Keycloak container. Production infrastructure details do not block development or product specification.

## References

- [Draft product direction and interview](../../product/changes/active/chg-authenticated-operations-001/proposal.md)
- [Operational issue story](https://github.com/juangcarmona/sofa/issues/48)
- [Existing private infrastructure repository](https://github.com/juangcarmona/infra.homelab)
- [Independent repository execution](0006-observe-repository-owned-workflows.md)
- [Docker daemon authority](https://docs.docker.com/engine/security/)
- [Keycloak export limitations](https://www.keycloak.org/server/importExport)
- [Keycloak administrator recovery](https://www.keycloak.org/server/bootstrap-admin-recovery)
- [Restic repositories and recovery passwords](https://restic.readthedocs.io/en/stable/030_preparing_a_new_repo.html)
- [SOPS encryption formats and backends](https://getsops.io/docs/)
