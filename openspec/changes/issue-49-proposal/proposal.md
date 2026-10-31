# Proposal

## Why

The owner needs authenticated access to the operational workspace and a separately verified GitHub account before production identity is available. The existing anonymous read-only observer and empty Aspire AppHost do not provide that boundary.

## What Changes

- Compose a dedicated, pinned development Keycloak and the SoFa workspace in the existing Aspire solution, with separated test configuration and phone-reachable HTTPS endpoints.
- **BREAKING**: the operational workspace and observation API require an authenticated configured owner. The new workspace replaces anonymous operational serving in the supported development composition; it does not upgrade the legacy server with privileges.
- Add external-provider sign-in, an explicit MFA/session policy, server-held credentials, sign-out and fail-closed access explanations.
- Add an explicit GitHub user connection, visible acting account and server-side configured-repository permission checks. Connecting or reconnecting never requests repository work.
- Use the adopted Lighthouse identity guidance and retain actual provider, source-identity and real-device evidence during implementation.

## Capabilities

### New Capabilities

- `identity-environments`: isolated development identity composition, externally configured issuer compatibility and identity verification evidence.
- `owner-access`: owner-only operational reads and writes, bounded authenticated sessions and accessible private-network entry.
- `github-user-connection`: explicit source-account association, verified acting identity and current repository authority.

### Modified Capabilities

None. `/workspace/openspec` is absent in this checkout; no existing OpenSpec capability or active change was found to reuse. ProductShape remains the product authority; these specifications realize its applied authenticated-operations direction, not a replacement product model.

## Impact

Add a proposed `src/SoFa.Web` ASP.NET Core application and focused .NET tests. Change `src/SoFa.AppHost`, the supported startup documentation, solution references, verification scripts and CI. Preserve the observation response contract and source-evidence presentation from `src/web`/`src/observation`, but perform new-workspace collection through the connected user rather than operator GitHub CLI credentials. Retain legacy tools only as explicitly separate, non-privileged tools; do not start their anonymous listener in Aspire.

Use the existing .NET 10 / Aspire 13.6.1 baseline, OIDC middleware 10.0.14 and the owner-supplied Keycloak 26.8.1 digest. No execution database, supervisor, command journal, command dispatch or new scheduler is introduced. Development sessions and source connections are deliberately process-local and lost on restart. Production identity provisioning and durable secret recovery belong to #58.

Product basis: UC-ACCESS-FACTORY, FR-AUTHORIZED-ENTRY, QR-EXECUTION-SECURITY, QR-MOBILE-OPERATIONS, BR-LAN-PRIVILEGE-BOUNDARY and CON-TRUSTED-LAN under `docs/product/model/`, as applied by CHG-AUTHENTICATED-OPERATIONS-001. Preserve accepted ADR-0006. Juan accepted ADR-0007 on 2026-10-31; its status is recorded in this PR pending merge. ADR-0008 remains Proposed.

Issue: <https://github.com/juangcarmona/sofa/issues/49>. The `planned` assessment means a complete candidate for Juan's review, not implementation permission. The `future` hold remains in force.
